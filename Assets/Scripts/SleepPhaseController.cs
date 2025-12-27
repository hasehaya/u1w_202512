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
    [SerializeField] private Button backSleepButton;

    [SerializeField] private GameObject dream;
    [SerializeField] private GameObject checkWatch;

    [Header("Settings")]
    [SerializeField] private int maxCheckCount = 3;
    [SerializeField] private float timerDisplayDuration = 1.5f;

    private int checkCount;
    private float totalTime;
    private float phaseStartTime;
    
    private CheckWatchAnimationController checkWatchController;

    public override GameState PhaseType => GameState.Sleep;
    
    private SleepGameState sleepGameState = SleepGameState.Dream;


    private enum SleepGameState
    {
        Dream,
        CheckWatch
    }

    /// <summary>
    /// 時間制限を設定（GameManagerから呼び出される）
    /// </summary>
    public void SetTimeLimit(float limit)
    {
        totalTime = limit;
    }

    protected override void OnEnterImpl()
    {
        checkWatchController = checkWatch.GetComponent<CheckWatchAnimationController>();
        checkButton.onClick.AddListener(() => ChangeState(SleepGameState.CheckWatch));
        wakeUpButton.onClick.AddListener(() => RequestTransitionTo(GameState.Run));
        backSleepButton.onClick.AddListener(() =>
        {
            ChangeState(SleepGameState.Dream);
        });

        checkWatchController.OnEnterAnimationComplete += () =>
        {
            wakeUpButton.enabled = true;
            backSleepButton.enabled = true;
        };
        checkWatchController.OnExitAnimationComplete += () =>
        {
            checkWatch.SetActive(false);
            dream.SetActive(true);
            checkButton.enabled = true;
        };
        
        Initialize();
        sleepGameState = SleepGameState.Dream;
        dream.SetActive(true);
        checkButton.enabled = true;
    }

    public override void UpdatePhase()
    {
    }

    protected override void OnExitImpl()
    {
        StopAllCoroutines();
    }


    private void Initialize()
    {
        checkButton.enabled = false;
        wakeUpButton.enabled = false;
        backSleepButton.enabled = false;
        
        dream.SetActive(false);
        checkWatch.SetActive(false);
    }
    
    private void ChangeState(SleepGameState newState)
    {
        sleepGameState = newState;
        switch (sleepGameState)
        {
            case SleepGameState.Dream:
                backSleepButton.enabled = false;
                wakeUpButton.enabled = false;
                checkWatchController.PlayExitAnimation();
                break;
            case SleepGameState.CheckWatch:
                dream.SetActive(false);
                checkWatch.SetActive(true);
                checkButton.enabled = false;
                break;
        }
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
