using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// 睡眠フェーズコントローラー
/// 責務: プレイヤーが目を覚ます時間を決めるフェーズの管理
/// </summary>
public class SleepPhaseController : PhaseController
{
    [Header("UI References")]
    [SerializeField] private TMPro.TMP_Text remainingTimeText;
    [SerializeField] private Button checkButton;
    [SerializeField] private Button wakeUpButton;
    [SerializeField] private Button backSleepButton;

    [SerializeField] private GameObject dream;
    [SerializeField] private GameObject checkWatch;

    [SerializeField] private Sprite[] checkButtonSprites;

    [Header("Settings")] //GameManagerに置き換える？
    [SerializeField] private Vector2 decreaseRateRange = new Vector2(3f, 10f);
    [SerializeField] private Vector2 popUpStateThreshold = new Vector2(300f, 100f);
    
    private int currenCheckCount;
    
    private float timeDecreaseRate = 1.0f;
    
    // GameManagerに置き換える予定
    private float remainingTime = 600.0f;
    
    private CheckWatchAnimationController checkWatchController;

    public override GameState PhaseType => GameState.Sleep;
    
    private SleepGameState sleepGameState = SleepGameState.Dream;


    private enum SleepGameState
    {
        Dream,
        CheckWatch
    }

    protected override void OnEnterImpl()
    {
        currenCheckCount = checkButtonSprites.Length;
        
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
            
            if(currenCheckCount == 0)
                backSleepButton.interactable = false;
        };
        checkWatchController.OnExitAnimationComplete += () =>
        {
            checkWatch.SetActive(false);
            dream.SetActive(true);
            checkButton.image.sprite = checkButtonSprites[currenCheckCount - 1];
            checkButton.enabled = true;
            timeDecreaseRate = Random.Range(decreaseRateRange.x, decreaseRateRange.y);
            sleepGameState = SleepGameState.Dream;
        };
        
        Initialize();
        sleepGameState = SleepGameState.Dream;
        dream.SetActive(true);
        checkButton.image.sprite = checkButtonSprites[currenCheckCount - 1];
        timeDecreaseRate = Random.Range(decreaseRateRange.x, decreaseRateRange.y);
        checkButton.enabled = true;
    }

    public override void UpdatePhase()
    {
        if (sleepGameState != SleepGameState.Dream) return;
        
        remainingTime -= timeDecreaseRate * Time.deltaTime;
            
        if (remainingTime <= 0)
        {
            RequestTransitionTo(GameState.GameOver);
        }
    }

    private void Update()
    {
        if (sleepGameState != SleepGameState.Dream) return;
        
        remainingTime -= timeDecreaseRate * Time.deltaTime;
            
        if (remainingTime <= 0)
        {
            RequestTransitionTo(GameState.GameOver);
        }
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
        switch (newState)
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
                remainingTimeText.text = remainingTime.ConvertSec2Min().ToString();
                currenCheckCount--;
                sleepGameState = SleepGameState.CheckWatch;
                break;
        }
    }
}
