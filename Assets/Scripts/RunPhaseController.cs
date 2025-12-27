using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// ラン（移動）フェーズコントローラー
/// 責務: プレイヤーが障害物を避けながら進むフェーズの管理
/// </summary>
public class RunPhaseController : PhaseController
{
    [Header("UI")]
    [SerializeField] private Text timerText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private GameObject gabaText;

    [Header("Obstacle")]
    [SerializeField] private ObstacleController obstacleController;

    [Header("Settings")]
    [SerializeField] private float requiredClicks = 45f;
    [SerializeField] private float safeTimeDuration = 0.5f;
    [SerializeField] private int minObstacles = 3;
    [SerializeField] private int maxObstacles = 6;

    private float currentRemainingTime;
    private float progress;
    private float progressIncrement;
    private List<float> obstacleTriggers = new List<float>();
    private bool isInSafeTime;

    public override GameState PhaseType => GameState.Run;

    /// <summary>
    /// 残り時間を設定（GameManagerから呼び出される）
    /// </summary>
    public void SetRemainingTime(float remainingTime)
    {
        currentRemainingTime = remainingTime;
    }

    #region Unity Lifecycle

    private void Start()
    {
        SubscribeToInputEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromInputEvents();
    }

    private void SubscribeToInputEvents()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnTap += HandleTap;
            InputManager.Instance.OnSwipe += HandleSwipe;
        }
    }

    private void UnsubscribeFromInputEvents()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnTap -= HandleTap;
            InputManager.Instance.OnSwipe -= HandleSwipe;
        }
    }

    #endregion

    #region Phase Lifecycle

    protected override void OnEnterImpl()
    {
        progress = 0;
        isInSafeTime = false;
        progressIncrement = 1f / requiredClicks;

        if (progressBar != null)
            progressBar.value = 0;

        GenerateObstacleTriggers();
        ShowGabaEffect();
    }

    public override void UpdatePhase()
    {
        currentRemainingTime -= Time.deltaTime;
        
        UpdateTimerUI();

        if (currentRemainingTime <= 0)
        {
            RequestGameOver();
        }
    }

    protected override void OnExitImpl()
    {
        progress = 0;
        currentRemainingTime = 0;
        obstacleTriggers.Clear();
        CancelInvoke();
    }

    protected override void OnPauseImpl()
    {
        Time.timeScale = 0f;
    }

    protected override void OnResumeImpl()
    {
        Time.timeScale = 1f;
    }

    #endregion

    #region Input Handling

    private void HandleTap()
    {
        if (!IsActive) return;

        if (obstacleController != null && obstacleController.IsActive)
        {
            if (isInSafeTime) return;
            
            // 障害物に激突
            RequestGameOver();
            return;
        }

        AddProgress();
        CheckObstacleTrigger();
        CheckClear();
    }

    private void HandleSwipe(SwipeDirection swipeDir)
    {
        if (!IsActive || obstacleController == null || !obstacleController.IsActive) return;

        bool isCorrect = CheckSwipeDirection(swipeDir);

        if (isCorrect)
        {
            obstacleController.Hide(true);
        }
    }

    private bool CheckSwipeDirection(SwipeDirection swipeDir)
    {
        return obstacleController.CurrentPosition switch
        {
            ObstaclePosition.Top => swipeDir == SwipeDirection.Up,
            ObstaclePosition.Bottom => swipeDir == SwipeDirection.Down,
            ObstaclePosition.Left => swipeDir == SwipeDirection.Left,
            ObstaclePosition.Right => swipeDir == SwipeDirection.Right,
            _ => false
        };
    }

    #endregion

    #region Game Logic

    private void AddProgress()
    {
        progress += progressIncrement;
        
        if (progressBar != null)
            progressBar.value = progress;
    }

    private void CheckObstacleTrigger()
    {
        if (obstacleTriggers.Count > 0 && progress >= obstacleTriggers[0])
        {
            SpawnObstacle();
            obstacleTriggers.RemoveAt(0);
        }
    }

    private void CheckClear()
    {
        if (progress >= 1f)
        {
            RequestGameClear(currentRemainingTime);
        }
    }

    private void GenerateObstacleTriggers()
    {
        obstacleTriggers.Clear();
        int count = Random.Range(minObstacles, maxObstacles);
        float start = 0.1f;
        float end = 0.9f;
        float step = (end - start) / count;

        for (int i = 0; i < count; i++)
        {
            float triggerPoint = start + (step * i) + Random.Range(0, step * 0.8f);
            obstacleTriggers.Add(triggerPoint);
        }
    }

    private void SpawnObstacle()
    {
        if (obstacleController == null) return;

        ObstaclePosition pos = (ObstaclePosition)Random.Range(0, 4);
        obstacleController.Spawn(pos);

        isInSafeTime = true;
        Invoke(nameof(EndSafeTime), safeTimeDuration);
    }

    private void EndSafeTime()
    {
        isInSafeTime = false;
    }

    #endregion

    #region UI

    private void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = Mathf.Max(0, currentRemainingTime).ToString("F2");
    }

    private void ShowGabaEffect()
    {
        if (gabaText != null)
        {
            gabaText.SetActive(true);
            Invoke(nameof(HideGabaEffect), 1f);
        }
    }

    private void HideGabaEffect()
    {
        if (gabaText != null)
            gabaText.SetActive(false);
    }

    #endregion

    #region Properties

    /// <summary>
    /// 現在の進捗 (0~1)
    /// </summary>
    public float Progress => progress;

    /// <summary>
    /// 現在の残り時間
    /// </summary>
    public float RemainingTime => currentRemainingTime;

    #endregion
}
