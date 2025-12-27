using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 睡眠フェーズコントローラー
/// 責務: プレイヤーが目を覚ます時間を決めるフェーズの管理
/// </summary>
public class SleepPhaseController : PhaseController
{
    [Header("UI References")]
    [SerializeField] private Text timerText;
    [SerializeField] private Text checkCountText;
    [SerializeField] private Button checkButton;
    [SerializeField] private Button wakeUpButton;
    [SerializeField] private GameObject sheepImage;

    [Header("Settings")]
    [SerializeField] private int maxCheckCount = 3;
    [SerializeField] private float timerDisplayDuration = 1.5f;

    private int checkCount;
    private float totalTime;
    private float phaseStartTime;

    public override GameState PhaseType => GameState.Sleep;

    /// <summary>
    /// 時間制限を設定（GameManagerから呼び出される）
    /// </summary>
    public void SetTimeLimit(float limit)
    {
        totalTime = limit;
    }

    protected override void OnEnterImpl()
    {
        // デバッグ用
        RequestTransitionTo(GameState.Run);
        
        checkCount = maxCheckCount;
        phaseStartTime = Time.time;
        
        if (checkButton != null)
        {
            checkButton.onClick.AddListener(OnCheckTime);
            checkButton.interactable = true;
        }
        
        if (wakeUpButton != null)
            wakeUpButton.onClick.AddListener(OnWakeUp);
        
        UpdateUI();
        StartCoroutine(ShowTimerBriefly());
    }

    public override void UpdatePhase()
    {
        float elapsed = Time.time - phaseStartTime;
        float remaining = totalTime - elapsed;

        if (remaining <= 0)
        {
            // 寝過ごした場合は即リザルトへ
            GameManager.Instance.Data.EndSleep();
            if (GameManager.Instance.Data.IsOverslept)
            {
                GameManager.Instance.Data.SetGameOver();
                if (checkButton != null)
                    checkButton.onClick.RemoveListener(OnCheckTime);
                if (wakeUpButton != null)
                    wakeUpButton.onClick.RemoveListener(OnWakeUp);
                RequestTransitionTo(GameState.Result);
                return;
            }
        
            // 時間内に起きた場合はRunフェーズへ
            if (checkButton != null)
                checkButton.onClick.RemoveListener(OnCheckTime);
            if (wakeUpButton != null)
                wakeUpButton.onClick.RemoveListener(OnWakeUp);
            RequestTransitionTo(GameState.Run);
        }
    }

    protected override void OnExitImpl()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// 時間チェックボタン処理
    /// </summary>
    public void OnCheckTime()
    {
        if (checkCount > 0)
        {
            checkCount--;
            UpdateUI();
            StartCoroutine(ShowTimerBriefly());
            
            if (checkCount <= 0 && checkButton != null)
                checkButton.interactable = false;
        }
    }

    /// <summary>
    /// 起床ボタン処理
    /// </summary>
    public void OnWakeUp()
    {
        RequestTransitionTo(GameState.Run);
    }

    private void UpdateUI()
    {
        if (checkCountText != null)
            checkCountText.text = $"{checkCount}";
    }

    private IEnumerator ShowTimerBriefly()
    {
        float elapsed = Time.time - phaseStartTime;
        float currentRemaining = Mathf.Max(0, totalTime - elapsed);
        
        if (timerText != null)
        {
            timerText.text = currentRemaining.ToString("F2");
            timerText.gameObject.SetActive(true);
        }
        
        yield return new WaitForSeconds(timerDisplayDuration);
        
        if (timerText != null)
            timerText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 残り時間を取得
    /// </summary>
    public float GetRemainingTime()
    {
        float elapsed = Time.time - phaseStartTime;
        return Mathf.Max(0, totalTime - elapsed);
    }
}
