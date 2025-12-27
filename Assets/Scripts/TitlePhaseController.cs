using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// タイトル画面フェーズコントローラー
/// 責務: タイトル画面のロジック管理
/// </summary>
public class TitlePhaseController : PhaseController
{
    [SerializeField] private Button startButton;

    public override GameState PhaseType => GameState.Title;

    protected override void OnEnterImpl()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnClickStart);
    }

    public override void UpdatePhase()
    {
        // タイトル画面での更新処理
    }

    protected override void OnExitImpl()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(OnClickStart);
    }

    /// <summary>
    /// スタートボタンクリック時
    /// </summary>
    private void OnClickStart()
    {
        RequestTransitionTo(GameState.Prologue);
    }
}

