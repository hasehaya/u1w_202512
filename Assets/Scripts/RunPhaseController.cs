using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// ラン（移動）フェーズコントローラー
/// プレイヤーが障害物を避けながら進むフェーズ
/// </summary>
public class RunPhaseController : PhaseController
{
    [Header("UI")]
    [SerializeField] private Text timerText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private GameObject gabaText; // 「ガバッ」

    [Header("Obstacle")]
    [SerializeField] private ObstacleController obstacleController;

    private float currentRemainingTime;
    private float progress = 0;
    
    // 障害物関連
    private List<float> obstacleTriggers = new List<float>();
    private bool isInSafeTime = false;
    private const float SAFE_TIME_DURATION = 0.5f;
    private const float REQUIRED_CLICKS = 45f;
    private float progressIncrement;

    private void Start()
    {
        phaseType = GameState.Run;
        
        // イベント購読
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnTap += HandleTap;
            InputManager.Instance.OnSwipe += HandleSwipe;
        }
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnTap -= HandleTap;
            InputManager.Instance.OnSwipe -= HandleSwipe;
        }
    }

    public override void Initialize()
    {
        // パラメータなし初期化版
    }

    /// <summary>
    /// 残り時間を指定して初期化
    /// </summary>
    public void Initialize(float remainingTime)
    {
        currentRemainingTime = remainingTime;
        progress = 0;
        
        if (progressBar != null)
            progressBar.value = 0;
        
        progressIncrement = 1f / REQUIRED_CLICKS;
        GenerateObstacleTriggers();
        
        SetVisible(true);
        
        // 演出
        if (gabaText != null)
        {
            gabaText.SetActive(true);
            Invoke(nameof(HideGaba), 1f);
        }
    }

    public override void UpdatePhase()
    {
        if (!IsActive) return;

        currentRemainingTime -= Time.deltaTime;
        
        if (timerText != null)
            timerText.text = Mathf.Max(0, currentRemainingTime).ToString("F2");

        if (currentRemainingTime <= 0)
        {
            GameManager.Instance.GameOver();
        }
    }

    public override void Cleanup()
    {
        progress = 0;
        currentRemainingTime = 0;
        obstacleTriggers.Clear();
        SetVisible(false);
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

    private void GenerateObstacleTriggers()
    {
        obstacleTriggers.Clear();
        int count = Random.Range(3, 6); // 3 to 5
        float start = 0.1f;
        float end = 0.9f;
        float step = (end - start) / count;

        for (int i = 0; i < count; i++)
        {
            float triggerPoint = start + (step * i) + Random.Range(0, step * 0.8f);
            obstacleTriggers.Add(triggerPoint);
        }
    }

    private void HideGaba()
    {
        if (gabaText != null)
            gabaText.SetActive(false);
    }

    /// <summary>
    /// InputManagerから呼ばれるタップ処理
    /// </summary>
    private void HandleTap()
    {
        if (!IsActive) return;

        // 障害物が出ている時
        if (obstacleController.IsActive)
        {
            if (isInSafeTime) return; // 猶予期間は無視
            
            // 障害物に激突（ゲームオーバー）
            GameManager.Instance.GameOver();
            return;
        }

        // 進行
        progress += progressIncrement;
        
        if (progressBar != null)
            progressBar.value = progress;

        // 障害物出現チェック
        if (obstacleTriggers.Count > 0 && progress >= obstacleTriggers[0])
        {
            SpawnObstacle();
            obstacleTriggers.RemoveAt(0);
        }

        // クリアチェック
        if (progress >= 1f)
        {
            GameManager.Instance.GameClear(currentRemainingTime);
        }
    }

    /// <summary>
    /// InputManagerから呼ばれるスワイプ処理
    /// </summary>
    private void HandleSwipe(SwipeDirection swipeDir)
    {
        if (!IsActive || !obstacleController.IsActive) return;

        // 「押し返す」ロジック: 上から来たら(Top)、上にスワイプ(Up)で正解
        bool isCorrect = false;
        switch (obstacleController.CurrentPosition)
        {
            case ObstaclePosition.Top: isCorrect = (swipeDir == SwipeDirection.Up); break;
            case ObstaclePosition.Bottom: isCorrect = (swipeDir == SwipeDirection.Down); break;
            case ObstaclePosition.Left: isCorrect = (swipeDir == SwipeDirection.Left); break;
            case ObstaclePosition.Right: isCorrect = (swipeDir == SwipeDirection.Right); break;
        }

        if (isCorrect)
        {
            obstacleController.Hide(true);
        }
    }

    private void SpawnObstacle()
    {
        // ランダムな位置から出現
        ObstaclePosition pos = (ObstaclePosition)Random.Range(0, 4);
        obstacleController.Spawn(pos);

        // 猶予時間の開始
        isInSafeTime = true;
        Invoke(nameof(EndSafeTime), SAFE_TIME_DURATION);
        
        // 画面を揺らすなどの演出を入れるならここ
    }

    private void EndSafeTime()
    {
        isInSafeTime = false;
    }
}

