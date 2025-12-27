using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 睡眠フェーズコントローラー
/// プレイヤーが目を覚ます時間を決めるフェーズ
/// </summary>
public class SleepPhaseController : PhaseController
{
    [SerializeField] private Text timerText;
    [SerializeField] private Text checkCountText;
    [SerializeField] private Button checkButton;
    [SerializeField] private GameObject sheepImage; // 演出用

    private int checkCount = 3;
    private float totalTime;
    private float phaseStartTime;

    private void Start()
    {
        phaseType = GameState.Sleep;
    }

    public override void Initialize()
    {
        // 外部から時間制限が渡される必要があります
        // GameManager.Instance.StartSleepPhase()で設定してください
    }

    public override void UpdatePhase()
    {
        if (!IsActive) return;

        float elapsed = Time.time - phaseStartTime;
        float remaining = totalTime - elapsed;

        if (remaining <= 0)
        {
            GameManager.Instance.ChangeState(GameState.Run); // 強制起床
        }
    }

    public override void Cleanup()
    {
        checkCount = 0;
        StopAllCoroutines();
    }

    /// <summary>
    /// 時間制限を指定して初期化
    /// </summary>
    public void InitializeWithTimeLimit(float limit)
    {
        totalTime = limit;
        checkCount = 3;
        phaseStartTime = Time.time;
        
        SetVisible(true);
        checkButton.interactable = true;
        UpdateUI();

        // 最初だけ時間を表示
        StartCoroutine(ShowTimerBriefly());
    }

    /// <summary>
    /// 互換性維持用メソッド
    /// </summary>
    public void Initialize(float timeLimit)
    {
        totalTime = timeLimit;
        checkCount = 3;
        phaseStartTime = Time.time;
        
        SetVisible(true);
        checkButton.interactable = true;
        UpdateUI();

        StartCoroutine(ShowTimerBriefly());
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
            
            if (checkCount <= 0) checkButton.interactable = false;
        }
    }

    /// <summary>
    /// 起床ボタン処理
    /// </summary>
    public void OnWakeUp()
    {
        GameManager.Instance.ChangeState(GameState.Run);
    }

    private void UpdateUI()
    {
        if (checkCountText != null)
            checkCountText.text = $"{checkCount}";
    }

    private IEnumerator ShowTimerBriefly()
    {
        float elapsed = Time.time - phaseStartTime;
        float currentRem = Mathf.Max(0, totalTime - elapsed);
        
        if (timerText != null)
        {
            timerText.text = currentRem.ToString("F2");
            timerText.gameObject.SetActive(true);
        }
        
        yield return new WaitForSeconds(1.5f);
        
        if (timerText != null)
            timerText.gameObject.SetActive(false);
    }
}

