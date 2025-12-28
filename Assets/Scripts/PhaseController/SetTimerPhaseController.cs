using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// チュートリアルフェーズコントローラー
/// 責務: ゲームの操作方法を教えるフェーズの管理
/// </summary>
public class SetTimerPhaseController : PhaseController
{
    [SerializeField] private GameObject slideSet;
    [SerializeField] private GameObject alarmRing;
    [SerializeField] private TMPro.TextMeshProUGUI remainingTimeText;
    [SerializeField] private TMPro.TextMeshProUGUI goSleepText;
    
    private CheckWatchAnimationController slideSetController;
    private CheckWatchAnimationController alarmRingController;
    private TextRevealAnimation textRevealAnimation;
    
    [SerializeField] private float delayBeforeAlarmRing = 1.0f;
    [SerializeField] private float alarmRingDuration = 2.0f;
    

    public override GameState PhaseType => GameState.SetTimer;

    protected override void OnEnterImpl()
    {
        
        slideSetController = slideSet.GetComponent<CheckWatchAnimationController>();
        alarmRingController = alarmRing.GetComponent<CheckWatchAnimationController>();
        textRevealAnimation = goSleepText.gameObject.GetComponent<TextRevealAnimation>();
        
        slideSetController.OnEnterAnimationCompleted += () =>
        {
            textRevealAnimation.StartReveal();
            textRevealAnimation.OnRevealCompleted += () =>
            {
                InputManager.Instance.OnSwipe += OnSwiped;
            };
        };
        slideSetController.OnExitAnimationCompleted += () =>
        {
            StartCoroutine(ChangeStateToAlarmRing());
        };
        alarmRingController.OnEnterAnimationCompleted += () =>
        {
            StartCoroutine(DelayExitAnimationStart());
        };
        alarmRingController.OnExitAnimationCompleted += () =>
        {
            GameManager.Instance.ChangeState(GameState.Sleep);
        };
        
        
        slideSet.SetActive(true);
        alarmRing.SetActive(false);
    }

    public override void UpdatePhase()
    {
        // チュートリアルの更新ロジック（必要に応じて実装）
    }

    protected override void OnExitImpl()
    {
        slideSet.SetActive(false);
        alarmRing.SetActive(false);
        slideSetController.ClearAllAnimationEvents();
        alarmRingController.ClearAllAnimationEvents();
        textRevealAnimation.ClearEvents();
        StopAllCoroutines();
    }
    
    private void OnSwiped(SwipeDirection direction)
    {
        if (direction == SwipeDirection.Right)
        {
            slideSetController.PlayExitAnimation();
            InputManager.Instance.OnSwipe -= OnSwiped;
        }
    }
    
    private IEnumerator ChangeStateToAlarmRing()
    {
        yield return new WaitForSeconds(delayBeforeAlarmRing);
        remainingTimeText.text = GameManager.Instance.Data.RemainingTime.ConvertSec2Min().ToString();
        slideSet.SetActive(false);
        alarmRing.SetActive(true);
    }

    private IEnumerator DelayExitAnimationStart()
    {
        yield return new WaitForSeconds(alarmRingDuration);
        alarmRingController.PlayExitAnimation();
    }
}

