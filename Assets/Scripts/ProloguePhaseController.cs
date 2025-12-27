using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プロローグフェーズコントローラー
/// 責務: ゲームのストーリー導入部分の管理
/// </summary>
public class ProloguePhaseController : PhaseController
{
    [SerializeField] private float displayDuration = 5f;
    [SerializeField] private Button skipButton;
    
    private float displayTimer = 0f;

    public override GameState PhaseType => GameState.Prologue;

    protected override void OnEnterImpl()
    {
        displayTimer = 0f;
        
        if (skipButton != null)
            skipButton.onClick.AddListener(OnClickSkip);
        
        // デバッグ用
        RequestTransitionTo(GameState.Tutorial);
    }

    public override void UpdatePhase()
    {
        displayTimer += Time.deltaTime;

        if (displayTimer >= displayDuration)
        {
            CompletePrologue();
        }
    }

    protected override void OnExitImpl()
    {
        displayTimer = 0f;
        
        if (skipButton != null)
            skipButton.onClick.RemoveListener(OnClickSkip);
    }

    private void CompletePrologue()
    {
        RequestTransitionTo(GameState.Tutorial);
    }

    /// <summary>
    /// スキップボタンクリック時
    /// </summary>
    private void OnClickSkip()
    {
        CompletePrologue();
    }
}

