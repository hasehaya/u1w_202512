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
    [SerializeField] private Button retryButton;
    [SerializeField] private CheckWatchAnimationController animationController;
    
    [Header("Rank Display")]
    [SerializeField] private Image rankImage;
    [SerializeField] private Sprite rankSSprite;
    [SerializeField] private Sprite rankASprite;
    [SerializeField] private Sprite rankBSprite;
    [SerializeField] private Sprite rankCSprite;

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
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetry);
            retryButton.enabled = false;
        }
            
        if (titleButton != null)
        {
            titleButton.onClick.AddListener(OnBackToTitle);
            titleButton.enabled = false;

            animationController.OnEnterAnimationCompleted += () =>
            {
                if (retryButton != null)
                    retryButton.enabled = true;
                titleButton.enabled = true;
            };
        }
        
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetry);
            retryButton.enabled = false;

            animationController.OnEnterAnimationCompleted += () =>
            {
                retryButton.enabled = true;
            };
        }
    }

    private void CleanupButtons()
    {
        if (retryButton != null)
            retryButton.onClick.RemoveListener(OnRetry);
        if (titleButton != null)
            titleButton.onClick.RemoveListener(OnBackToTitle);
        
        if (retryButton != null)
            retryButton.onClick.RemoveListener(OnRetry);
        
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
            int sleepTime = (int)GameManager.Instance.Data.SleepTime;
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
        
        // ランク表示
        if (rankImage != null)
        {
            string rank = GameManager.Instance.Data.Rank();
            Sprite rankSprite = rank switch
            {
                "S" => rankSSprite,
                "A" => rankASprite,
                "B" => rankBSprite,
                "C" => rankCSprite,
                _ => rankCSprite
            };
            
            if (rankSprite != null)
                rankImage.sprite = rankSprite;
        }
    }

    private void OnBackToTitle()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySe(SeType.ButtonClick);
        }
        RequestTransitionTo(GameState.Title);
    }
    
    private void OnRetry()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySe(SeType.ButtonClick);
        }
        RequestTransitionTo(GameState.SetTimer);
    }
}
