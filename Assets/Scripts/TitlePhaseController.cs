using UnityEngine;

/// <summary>
/// タイトル画面フェーズコントローラー
/// </summary>
public class TitlePhaseController : PhaseController
{
    private void Start()
    {
        phaseType = GameState.Title;
    }

    public override void Initialize()
    {
        SetVisible(true);
    }

    public override void UpdatePhase()
    {
        // タイトル画面での入力処理はUIManager側で行う
    }

    public override void Cleanup()
    {
        SetVisible(false);
    }

    /// <summary>
    /// ゲーム開始時の遷移
    /// </summary>
    public void StartGame()
    {
        GameManager.Instance.ChangeState(GameState.Loading);
    }
}

