using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ゲームオーバー画面フェーズコントローラー
/// 責務: ゲームオーバー結果を表示するフェーズの管理
/// </summary>
public class GameOverPhaseController : PhaseController
{
    [Header("UI References")] 
    [SerializeField] private GameObject trainPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button titleButton;
    
    public override GameState PhaseType => GameState.GameOver;

    protected override void OnEnterImpl()
    {
        // デバッグ用
        RequestTransitionTo(GameState.Title);
        
        SetupButtons();
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

    private void OnRetry()
    {
        RequestTransitionTo(GameState.Sleep);
    }

    private void OnBackToTitle()
    {
        RequestTransitionTo(GameState.Title);
    }
}

