using UnityEngine;

/// <summary>
/// フェーズ基本クラス
/// 全てのゲームフェーズはこのクラスを継承します
/// 責務: 個々のフェーズのロジック管理
/// </summary>
public abstract class PhaseController : MonoBehaviour
{
    public bool IsActive => isActive;
    protected bool isActive = false;

    /// <summary>
    /// このフェーズのタイプ（サブクラスでオーバーライド）
    /// </summary>
    public virtual GameState PhaseType => GameState.Title;

    /// <summary>
    /// フェーズ開始時に呼ばれる
    /// </summary>
    public virtual void OnPhaseEnter()
    {
        isActive = true;
        SetVisible(true);
        OnEnterImpl();
    }

    /// <summary>
    /// フェーズ終了時に呼ばれる
    /// </summary>
    public virtual void OnPhaseExit()
    {
        isActive = false;
        OnExitImpl();
        SetVisible(false);
    }

    /// <summary>
    /// フェーズの更新（毎フレーム呼び出される）
    /// </summary>
    public abstract void UpdatePhase();

    /// <summary>
    /// サブクラス用：OnPhaseEnter()時の処理
    /// </summary>
    protected virtual void OnEnterImpl() { }

    /// <summary>
    /// サブクラス用：OnPhaseExit()時の処理
    /// </summary>
    protected virtual void OnExitImpl() { }

    /// <summary>
    /// フェーズの表示/非表示切り替え
    /// </summary>
    protected virtual void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    /// <summary>
    /// 次のフェーズへ遷移をリクエスト
    /// </summary>
    protected void RequestTransitionTo(GameState nextState)
    {
        GameManager.Instance.RequestPhaseTransition(nextState);
    }
}
