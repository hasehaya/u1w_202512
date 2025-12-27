using UnityEngine;

/// <summary>
/// チュートリアルフェーズコントローラー
/// ゲームの操作方法を教えるフェーズ
/// </summary>
public class TutorialPhaseController : PhaseController
{
    [SerializeField] private int tutorialStageCount = 3;
    private int currentStage = 0;

    private void Start()
    {
        phaseType = GameState.Tutorial;
    }

    public override void Initialize()
    {
        SetVisible(true);
        currentStage = 0;
    }

    public override void UpdatePhase()
    {
        // チュートリアルロジックはUIManager側で管理される
    }

    public override void Cleanup()
    {
        SetVisible(false);
        currentStage = 0;
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

    /// <summary>
    /// チュートリアル完了時の処理
    /// </summary>
    private void CompleteTutorial()
    {
        GameManager.Instance.ChangeState(GameState.Sleep);
    }

    /// <summary>
    /// チュートリアルをスキップ
    /// </summary>
    public void SkipTutorial()
    {
        CompleteTutorial();
    }

    /// <summary>
    /// 現在のステージを取得
    /// </summary>
    public int GetCurrentStage() => currentStage;

    /// <summary>
    /// チュートリアルの進捗度を取得
    /// </summary>
    public float GetTutorialProgress()
    {
        return (float)currentStage / tutorialStageCount;
    }
}

