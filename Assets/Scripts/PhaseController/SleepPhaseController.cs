using System.Collections;
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
    [SerializeField] private GameObject wakeUp;

    [SerializeField] private Sprite[] checkButtonSprites;
    [SerializeField] private GameObject[] popUpStates;

    [Header("Settings")]
    [SerializeField] private Vector2 decreaseRateRange = new Vector2(3f, 10f);
    [SerializeField] private Vector2 popUpStateThreshold = new Vector2(300f, 100f);
    [SerializeField] private float transitionDurationToRunPhase = 2.0f;
    
    private int remainingCheckCount;
    
    private float timeDecreaseRate = 1.0f;
    
    private GameData gameData;
    
    private CheckWatchAnimationController checkWatchController;
    private CheckWatchAnimationController wakeUpController;

    public override GameState PhaseType => GameState.Sleep;
    
    private SleepGameState sleepGameState = SleepGameState.Dream;


    private enum SleepGameState
    {
        Dream,
        CheckWatch,
        WakeUp
    }

    protected override void OnEnterImpl()
    {
        remainingCheckCount = checkButtonSprites.Length;
        GameManager.Instance.Data.Reset();
        gameData = GameManager.Instance.Data;
        
        checkWatchController = checkWatch.GetComponent<CheckWatchAnimationController>();
        wakeUpController = wakeUp.GetComponent<CheckWatchAnimationController>();
        checkButton.onClick.AddListener(() => ChangeState(SleepGameState.CheckWatch));
        wakeUpButton.onClick.AddListener(() => ChangeState(SleepGameState.WakeUp));
        backSleepButton.onClick.AddListener(() =>
        {
            ChangeState(SleepGameState.Dream);
        });

        checkWatchController.OnEnterAnimationComplete += () =>
        {
            wakeUpButton.enabled = true;
            backSleepButton.enabled = true;
            
            if(remainingCheckCount == 0)
                backSleepButton.interactable = false;
        };
        checkWatchController.OnExitAnimationComplete += () =>
        {
            checkWatch.SetActive(false);
            dream.SetActive(true);
            checkButton.image.sprite = checkButtonSprites[remainingCheckCount - 1];
            checkButton.enabled = true;
            timeDecreaseRate = Random.Range(decreaseRateRange.x, decreaseRateRange.y);
            sleepGameState = SleepGameState.Dream;
        };
        wakeUpController.OnEnterAnimationComplete += () =>
        {
            StartCoroutine(TransitionToRun());
        };
        
        
        Initialize();
        sleepGameState = SleepGameState.Dream;
        dream.SetActive(true);
        checkButton.image.sprite = checkButtonSprites[remainingCheckCount - 1];
        timeDecreaseRate = Random.Range(decreaseRateRange.x, decreaseRateRange.y);
        checkButton.enabled = true;
    }

    public override void UpdatePhase()
    {
        if (sleepGameState != SleepGameState.Dream) return;
        
        gameData.RemainingTime -= timeDecreaseRate * Time.deltaTime;
            
        if (gameData.RemainingTime <= 0)
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
        wakeUp.SetActive(false);
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
                remainingTimeText.text = gameData.RemainingTime.ConvertSec2Min().ToString();
                remainingCheckCount--;
                gameData.CheckCount++;
                SetPopUpState(gameData.RemainingTime, remainingCheckCount == 0);
                
                sleepGameState = SleepGameState.CheckWatch;
                break;
            case SleepGameState.WakeUp:
                wakeUp.SetActive(true);
                sleepGameState = SleepGameState.WakeUp;
                break;
        }
    }
    
    private void SetPopUpState(float time, bool forceHighState = false)
    {
        if(time <= popUpStateThreshold.y || forceHighState)
        {
            popUpStates[0].SetActive(false);
            popUpStates[1].SetActive(false);
            popUpStates[2].SetActive(true);
        }
        else if(time <= popUpStateThreshold.x)
        {
            popUpStates[0].SetActive(false);
            popUpStates[1].SetActive(true);
            popUpStates[2].SetActive(false);
        }
        else
        {
            popUpStates[0].SetActive(true);
            popUpStates[1].SetActive(false);
            popUpStates[2].SetActive(false);
        }
    }

    private IEnumerator TransitionToRun()
    {
        yield return new WaitForSeconds(transitionDurationToRunPhase);
        RequestTransitionTo(GameState.Run);
    }
}
