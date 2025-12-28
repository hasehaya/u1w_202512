using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// タイトル画面フェーズコントローラー
/// 責務: タイトル画面のロジック管理
/// </summary>
public class TitlePhaseController : PhaseController
{
    public override GameState PhaseType => GameState.Title;

    protected override void OnEnterImpl()
    {
        InputManager.Instance.OnTap += OnScreenTapped;
    }

    public override void UpdatePhase()
    {
     
    }

    protected override void OnExitImpl()
    {
        InputManager.Instance.OnTap -= OnScreenTapped;
    }

    private void OnScreenTapped()
    {
        Start();
    }
    
    /// <summary>
    /// スタートボタンクリック時
    /// </summary>
    private void Start()
    {
        RequestTransitionTo(GameState.Loading);
    }
}

