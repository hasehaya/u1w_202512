using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// チュートリアルフェーズコントローラー
/// 責務: ゲームの操作方法を教えるフェーズの管理
/// </summary>
public class TutorialPhaseController : PhaseController
{
    [SerializeField] private int tutorialStageCount = 3;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button startGameButton;
    
    private int currentStage = 0;

    public override GameState PhaseType => GameState.Tutorial;

    protected override void OnEnterImpl()
    {
        currentStage = 0;
        SetupButtons();
        // デバッグ用
        RequestTransitionTo(GameState.Sleep);
    }

    public override void UpdatePhase()
    {
        // チュートリアルの更新ロジック（必要に応じて実装）
    }

    protected override void OnExitImpl()
    {
        currentStage = 0;
        CleanupButtons();
    }

    private void SetupButtons()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(OnClickNext);
        if (skipButton != null)
            skipButton.onClick.AddListener(OnClickSkip);
        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnClickStartGame);
    }

    private void CleanupButtons()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(OnClickNext);
        if (skipButton != null)
            skipButton.onClick.RemoveListener(OnClickSkip);
        if (startGameButton != null)
            startGameButton.onClick.RemoveListener(OnClickStartGame);
    }

    /// <summary>
    /// 次へボタンクリック時
    /// </summary>
    private void OnClickNext()
    {
        NextTutorialStep();
    }

    /// <summary>
    /// スキップボタンクリック時
    /// </summary>
    private void OnClickSkip()
    {
        CompleteTutorial();
    }

    /// <summary>
    /// ゲーム開始ボタンクリック時
    /// </summary>
    private void OnClickStartGame()
    {
        CompleteTutorial();
    }

    /// <summary>
    /// 次のチュートリアルステップに進む
    /// </summary>
    public void NextTutorialStep()
    {
        currentStage++;

        if (currentStage >= tutorialStageCount)
        {
            CompleteTutorial();
        }
    }

    private void CompleteTutorial()
    {
        RequestTransitionTo(GameState.Sleep);
    }


    /// <summary>
    /// 現在のステージを取得
    /// </summary>
    public int CurrentStage => currentStage;

    /// <summary>
    /// チュートリアルの進捗度を取得 (0~1)
    /// </summary>
    public float GetTutorialProgress()
    {
        return (float)currentStage / tutorialStageCount;
    }
}

