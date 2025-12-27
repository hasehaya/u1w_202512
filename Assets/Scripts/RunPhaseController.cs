using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RunPhaseController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Text timerText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private GameObject gabaText; // 「ガバッ」

    [Header("Obstacle")]
    [SerializeField] private ObstacleController obstacleController;

    private float currentRemainingTime;
    private float progress = 0;
    private bool isRunning = false;
    
    // 障害物関連
    private List<float> obstacleTriggers = new List<float>();
    private bool isInSafeTime = false;
    private const float SAFE_TIME_DURATION = 0.5f;

    // 設定
    private const float REQUIRED_CLICKS = 45f;
    private float progressIncrement;

    private void Start()
    {
        // イベント購読
        InputManager.Instance.OnTap += HandleTap;
        InputManager.Instance.OnSwipe += HandleSwipe;
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnTap -= HandleTap;
            InputManager.Instance.OnSwipe -= HandleSwipe;
        }
    }

    public void Initialize(float remainingTime)
    {
        currentRemainingTime = remainingTime;
        progress = 0;
        progressBar.value = 0;
        isRunning = true;
        
        // 進捗の増分計算
        progressIncrement = 1f / REQUIRED_CLICKS;

        // 障害物トリガーの生成 (3~5回)
        GenerateObstacleTriggers();

        // 演出
        gabaText.SetActive(true);
        Invoke(nameof(HideGaba), 1f);
    }

    private void HideGaba() => gabaText.SetActive(false);

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

    private void Update()
    {
        if (!isRunning || GameManager.Instance.CurrentState != GameState.Run) return;

        currentRemainingTime -= Time.deltaTime;
        timerText.text = Mathf.Max(0, currentRemainingTime).ToString("F2");

        if (currentRemainingTime <= 0)
        {
            isRunning = false;
            GameManager.Instance.GameOver();
        }
    }

    // InputManagerから呼ばれるタップ処理
    private void HandleTap()
    {
        if (!isRunning || GameManager.Instance.CurrentState != GameState.Run) return;

        // 障害物が出ている時
        if (obstacleController.IsActive)
        {
            if (isInSafeTime) return; // 猶予期間は無視
            
            // 障害物に激突（ゲームオーバー）
            isRunning = false;
            GameManager.Instance.GameOver();
            return;
        }

        // 進行
        progress += progressIncrement;
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
            isRunning = false;
            GameManager.Instance.GameClear(currentRemainingTime);
        }
    }

    // InputManagerから呼ばれるスワイプ処理
    private void HandleSwipe(SwipeDirection swipeDir)
    {
        if (!isRunning || !obstacleController.IsActive) return;

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