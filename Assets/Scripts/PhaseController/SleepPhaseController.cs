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
    
    private float initialRemainingTime;


    private enum SleepGameState
    {
        Dream,
        CheckWatch,
        WakeUp
    }

    protected override void OnEnterImpl()
    {
        remainingCheckCount = checkButtonSprites.Length;
        gameData = GameManager.Instance.Data;
        initialRemainingTime = gameData.RemainingTime; // 寝る前の初期時間を記録
        
        checkWatchController = checkWatch.GetComponent<CheckWatchAnimationController>();
        wakeUpController = wakeUp.GetComponent<CheckWatchAnimationController>();
        checkButton.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySe(SeType.ButtonClick);
            }
            ChangeState(SleepGameState.CheckWatch);
        });
        wakeUpButton.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySe(SeType.ButtonClick);
            }
            ChangeState(SleepGameState.WakeUp);
        });
        backSleepButton.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySe(SeType.ButtonClick);
            }
            ChangeState(SleepGameState.Dream);
        });

        checkWatchController.OnEnterAnimationCompleted += () =>
        {
            wakeUpButton.enabled = true;
            backSleepButton.enabled = true;
            
            if(remainingCheckCount == 0)
                backSleepButton.interactable = false;
        };
        checkWatchController.OnExitAnimationCompleted += () =>
        {
            checkWatch.SetActive(false);
            dream.SetActive(true);
            checkButton.image.sprite = checkButtonSprites[remainingCheckCount - 1];
            checkButton.enabled = true;
            timeDecreaseRate = Random.Range(decreaseRateRange.x, decreaseRateRange.y);
            sleepGameState = SleepGameState.Dream;
        };
        wakeUpController.OnEnterAnimationCompleted += () =>
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
        checkWatchController.ClearAllAnimationEvents();
        wakeUpController.ClearAllAnimationEvents();
        checkButton.onClick.RemoveAllListeners();
        wakeUpButton.onClick.RemoveAllListeners();
        backSleepButton.onClick.RemoveAllListeners();
        StopAllCoroutines();
    }


    private void Initialize()
    {
        checkButton.enabled = false;
        wakeUpButton.enabled = false;
        backSleepButton.enabled = false;
        backSleepButton.interactable = true;
        
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
                AudioManager.Instance.PlaySe(SeType.Gaba);
                wakeUp.SetActive(true);
                sleepGameState = SleepGameState.WakeUp;
                // 寝た時間を計算して保存（初期時間 - 現在の残り時間 = 経過した時間）
                gameData.SleepTime = initialRemainingTime - gameData.RemainingTime;
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
