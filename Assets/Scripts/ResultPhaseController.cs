using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// リザルト画面フェーズコントローラー
/// 責務: ゲーム結果を表示するフェーズの管理
/// </summary>
public class ResultPhaseController : PhaseController
{
    [Header("UI References")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text resultText;
    [SerializeField] private Text sleepTimeText;
    [SerializeField] private Text remainingTimeText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button titleButton;

    private float sleepDuration;
    private float remainingTime;
    private int score;

    public override GameState PhaseType => GameState.Result;

    /// <summary>
    /// リザルトデータを設定（GameManagerから呼び出される）
    /// </summary>
    public void SetResultData(float sleepDuration, float remainingTime, int score)
    {
        this.sleepDuration = sleepDuration;
        this.remainingTime = remainingTime;
        this.score = score;
    }

    protected override void OnEnterImpl()
    {
        SetupButtons();
        DisplayResult();
    }

    public override void UpdatePhase()
    {
        // リザルト画面の更新処理（必要に応じて実装）
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
        bool isCleared = score > 0;

        if (scoreText != null)
            scoreText.text = $"Score: {score}";

        if (resultText != null)
        {
            resultText.text = isCleared ? "Success!" : "Failed!";
            resultText.color = isCleared ? Color.green : Color.red;
        }

        if (sleepTimeText != null)
            sleepTimeText.text = $"{sleepDuration:F2}s";

        if (remainingTimeText != null)
            remainingTimeText.text = isCleared ? $"{remainingTime:F2}s" : "Time Up";
    }

    private void OnRetry()
    {
        RequestTransitionTo(GameState.Sleep);
    }

    private void OnBackToTitle()
    {
        RequestTransitionTo(GameState.Title);
    }

    #region Properties

    /// <summary>
    /// ゲームをクリアしたかどうか
    /// </summary>
    public bool IsCleared => score > 0;

    /// <summary>
    /// スコア
    /// </summary>
    public int Score => score;

    #endregion
}
