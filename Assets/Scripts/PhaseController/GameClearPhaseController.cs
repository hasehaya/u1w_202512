using UnityEngine;
using UnityEngine.UI;
using TMPro;
using unityroom.Api;

/// <summary>
/// ゲームクリア画面フェーズコントローラー
/// 責務: ゲームクリア結果を表示するフェーズの管理
/// </summary>
public class GameClearPhaseController : PhaseController
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI sleepTimeMinText;
    [SerializeField] private TextMeshProUGUI sleepTimeSecText;
    [SerializeField] private GameObject sleepTimeMinObject;
    [SerializeField] private TextMeshProUGUI remainingTimeMinText;
    [SerializeField] private TextMeshProUGUI remainingTimeSecText;
    [SerializeField] private GameObject remainingTimeMinObject;
    [SerializeField] private TextMeshProUGUI awakeCountText;
    [SerializeField] private Button titleButton;
    [SerializeField] private CheckWatchAnimationController animationController;

    public override GameState PhaseType => GameState.GameClear;

    protected override void OnEnterImpl()
    {
        UnityroomApiClient.Instance.SendScore(1, GameManager.Instance.Data.Score(), ScoreboardWriteMode.HighScoreDesc);
        SetupButtons();
        DisplayResult();
    }

    public override void UpdatePhase()
    {
        // ゲームクリア画面の更新処理（必要に応じて実装）
    }

    protected override void OnExitImpl()
    {
        CleanupButtons();
    }

    private void SetupButtons()
    {
        if (titleButton != null)
        {
            titleButton.onClick.AddListener(OnBackToTitle);
            titleButton.enabled = false;

            animationController.OnEnterAnimationCompleted += () =>
            {
                titleButton.enabled = true;
            };
        }
            
    }

    private void CleanupButtons()
    {
        if (titleButton != null)
            titleButton.onClick.RemoveListener(OnBackToTitle);
        
        // eventに登録されたすべてのデリゲートをクリア
        if (animationController != null)
            animationController.ClearEnterAnimationCompletedEvent();
    }

    private void DisplayResult()
    {
        if (scoreText != null)
            scoreText.text = $"{GameManager.Instance.Data.Score()}";

        if (remainingTimeMinText != null && remainingTimeSecText != null)
        {
            int remainingTime = (int)GameManager.Instance.Data.RemainingTime;
            int minutes = remainingTime / 60;
            int seconds = remainingTime % 60;
            remainingTimeMinText.text = $"{minutes:D2}";
            remainingTimeSecText.text = $"{seconds:D2}";
            
            // 分が0の時は分のGameObjectを非表示
            if (remainingTimeMinObject != null)
                remainingTimeMinObject.SetActive(minutes > 0);
        }
        
        if (sleepTimeMinText != null && sleepTimeSecText != null)
        {
            int sleepTime = GameData.TotalTimeLimit - (int)GameManager.Instance.Data.RemainingTime;
            int minutes = sleepTime / 60;
            int seconds = sleepTime % 60;
            sleepTimeMinText.text = $"{minutes:D2}";
            sleepTimeSecText.text = $"{seconds:D2}";
            
            // 分が0の時は分のGameObjectを非表示
            if (sleepTimeMinObject != null)
                sleepTimeMinObject.SetActive(minutes > 0);
        }
        
        if (awakeCountText != null)
            awakeCountText.text = $"{GameManager.Instance.Data.CheckCount}";
    }

    private void OnBackToTitle()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySe(SeType.ButtonClick);
        }
        RequestTransitionTo(GameState.Title);
    }
}
