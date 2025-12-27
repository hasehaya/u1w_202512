using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲームオーバー画面フェーズコントローラー
/// 責務: ゲームオーバー結果を表示するフェーズの管理
/// </summary>
public class GameOverPhaseController : PhaseController
{
    [Header("UI References")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text resultText;
    [SerializeField] private Text sleepTimeText;
    [SerializeField] private Text reasonText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button titleButton;

    private float sleepDuration;
    private int score;

    public override GameState PhaseType => GameState.GameOver;

    protected override void OnEnterImpl()
    {
        // デバッグ用
        RequestTransitionTo(GameState.Title);
        
        // GameManagerから直接データを取得
        score = GameManager.Instance.Data.Score();
        
        SetupButtons();
        DisplayResult();
    }

    public override void UpdatePhase()
    {
        // ゲームオーバー画面の更新処理（必要に応じて実装）
    }

    protected override void OnExitImpl()
    {
        CleanupButtons();
    }

    private void SetupButtons()
    {
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetry);
        if (titleButton != null)
            titleButton.onClick.AddListener(OnBackToTitle);
    }

    private void CleanupButtons()
    {
        if (retryButton != null)
            retryButton.onClick.RemoveListener(OnRetry);
        if (titleButton != null)
            titleButton.onClick.RemoveListener(OnBackToTitle);
    }

    private void DisplayResult()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";

        if (resultText != null)
        {
            resultText.text = "Game Over";
            resultText.color = Color.red;
        }

        if (sleepTimeText != null)
            sleepTimeText.text = $"{sleepDuration:F2}s";

        if (reasonText != null)
            reasonText.text = "Time Up!";
    }

    private void OnRetry()
    {
        RequestTransitionTo(GameState.Sleep);
    }

    private void OnBackToTitle()
    {
        RequestTransitionTo(GameState.Title);
    }

    /// <summary>
    /// スコア
    /// </summary>
    public int Score => score;
}

