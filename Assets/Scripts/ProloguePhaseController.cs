using UnityEngine;

/// <summary>
/// プロローグフェーズコントローラー
/// ゲームのストーリー導入部分
/// </summary>
public class ProloguePhaseController : PhaseController
{
    [SerializeField] private float displayDuration = 5f;
    private float displayTimer = 0f;

    private void Start()
    {
        phaseType = GameState.Prologue;
    }

    public override void Initialize()
    {
        SetVisible(true);
        displayTimer = 0f;
    }

    public override void UpdatePhase()
    {
        if (!IsActive) return;

        displayTimer += Time.deltaTime;

        if (displayTimer >= displayDuration)
        {
            CompletePrologue();
        }
    }

    public override void Cleanup()
    {
        SetVisible(false);
        displayTimer = 0f;
    }

    /// <summary>
    /// プロローグ完了時の処理
    /// </summary>
    private void CompletePrologue()
    {
        GameManager.Instance.ChangeState(GameState.Tutorial);
    }

    /// <summary>
    /// プロローグをスキップ
    /// </summary>
    public void SkipPrologue()
    {
        CompletePrologue();
    }
}

