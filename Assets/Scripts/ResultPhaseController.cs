using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// リザルト画面フェーズコントローラー
/// ゲーム結果を表示するフェーズ
/// </summary>
public class ResultPhaseController : PhaseController
{
    [SerializeField] private Text scoreText;
    [SerializeField] private Text resultText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button titleButton;

    private void Start()
    {
        phaseType = GameState.Result;
    }

    public override void Initialize()
    {
        SetVisible(true);

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetry);
        if (titleButton != null)
            titleButton.onClick.AddListener(OnBackToTitle);
    }

    public override void UpdatePhase()
    {
        // リザルト画面の更新処理
    }

    public override void Cleanup()
    {
        if (retryButton != null)
            retryButton.onClick.RemoveListener(OnRetry);
        if (titleButton != null)
            titleButton.onClick.RemoveListener(OnBackToTitle);

        SetVisible(false);
    }

    /// <summary>
    /// リザルトを表示
    /// </summary>
    public void DisplayResult(float sleepDuration, float remainingTime, int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";

        if (resultText != null)
        {
            string result = remainingTime > 0 ? "Success!" : "Failed!";
            resultText.text = result;
        }
    }

    /// <summary>
    /// リトライボタン
    /// </summary>
    private void OnRetry()
    {
        GameManager.Instance.ChangeState(GameState.Sleep);
    }

    /// <summary>
    /// タイトルに戻る
    /// </summary>
    private void OnBackToTitle()
    {
        GameManager.Instance.ChangeState(GameState.Title);
    }

    /// <summary>
    /// ゲームをクリアしたかどうか
    /// </summary>
    public bool IsClear { get; set; }
}

