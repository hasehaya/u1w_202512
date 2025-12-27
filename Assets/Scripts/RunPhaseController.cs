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

    [Header("Player")]
    [SerializeField] private Player player;

    [Header("Obstacle")]
    [SerializeField] private ObstacleManager obstacleManager;

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

    protected override void OnEnterImpl()
    {
        progress = 0;
        isInSafeTime = false;
        progressIncrement = 1f / requiredClicks;

        if (progressBar != null)
            progressBar.value = 0;

        if (player != null)
            player.ResetPosition();

        GenerateObstacleTriggers();
        ShowGabaEffect();
    }

    public override void UpdatePhase()
    {
        currentRemainingTime -= Time.deltaTime;
        
        UpdateTimerUI();
        CheckCollision();

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

    private void HandleTap()
    {
        if (!IsActive) return;

        AddProgress();
        CheckObstacleTrigger();
        CheckClear();
    }

    private void HandleSwipe(SwipeDirection swipeDir)
    {
        if (!IsActive) return;
        
        // プレイヤーの移動はPlayerControllerが自動で処理
        // ここでは何もしない（衝突判定はUpdatePhaseで行う）
    }

    private void CheckCollision()
    {
        if (player == null || obstacleManager == null) return;
        if (!obstacleManager.IsActive || isInSafeTime) return;

        // プレイヤーと障害物の位置を比較
        PlayerPosition playerPos = player.CurrentPosition;
        ObstaclePosition obstaclePos = obstacleManager.CurrentPosition;

        bool isCollision = false;

        // 左側の障害物に左側にいる場合は衝突
        if (obstaclePos == ObstaclePosition.Left && playerPos == PlayerPosition.Left)
        {
            isCollision = true;
        }
        // 右側の障害物に右側にいる場合は衝突
        else if (obstaclePos == ObstaclePosition.Right && playerPos == PlayerPosition.Right)
        {
            isCollision = true;
        }

        if (isCollision)
        {
            RequestGameOver();
        }
    }

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
        if (obstacleManager == null) return;

        // 0: Left Start, 1: Left End, 2: Right Start, 3: Right End
        int posIndex = Random.Range(0, 4);
        ObstaclePosition pos = (posIndex < 2) ? ObstaclePosition.Left : ObstaclePosition.Right;
        
        obstacleManager.Spawn(pos, posIndex);

        // セーフタイムを開始
        isInSafeTime = true;
        
        // アニメーション完了後のセーフタイム経過後に削除とセーフタイム終了
        // 注: 障害物のアニメーション時間(moveDuration)はObstacleで管理されているため、
        // ここではセーフタイムのみを考慮して削除タイミングを決定
        Invoke(nameof(DespawnObstacle), safeTimeDuration);
    }

    /// <summary>
    /// 障害物を削除してセーフタイムを終了
    /// </summary>
    private void DespawnObstacle()
    {
        if (obstacleManager != null)
        {
            obstacleManager.DespawnCurrentObstacle();
        }
        
        // セーフタイムを終了
        isInSafeTime = false;
    }

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

    /// <summary>
    /// 現在の進捗 (0~1)
    /// </summary>
    public float Progress => progress;

    /// <summary>
    /// 現在の残り時間
    /// </summary>
    public float RemainingTime => currentRemainingTime;
}
