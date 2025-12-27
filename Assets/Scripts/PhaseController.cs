using UnityEngine;
using System;

/// <summary>
/// フェーズ基本クラス
/// 全てのゲームフェーズはこのクラスを継承します
/// 責務: 個々のフェーズのロジック管理
/// </summary>
public abstract class PhaseController : MonoBehaviour
{
    [SerializeField] protected CanvasGroup canvasGroup;
    
    // フェーズの状態管理
    protected bool isActive = false;
    protected bool isPaused = false;

    /// <summary>
    /// このフェーズのタイプ（サブクラスでオーバーライド）
    /// </summary>
    public virtual GameState PhaseType => GameState.Title;

    // イベント（必要に応じて外部で購読可能）
    public event Action OnPhaseStarted;
    public event Action OnPhaseEnded;
    public event Action OnPhasePaused;
    public event Action OnPhaseResumed;

    #region Unity Lifecycle

    protected virtual void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    #endregion

    #region Phase Lifecycle

    /// <summary>
    /// フェーズ開始時に呼ばれる
    /// </summary>
    public virtual void OnPhaseEnter()
    {
        isActive = true;
        isPaused = false;
        SetVisible(true);
        OnPhaseStarted?.Invoke();
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
        OnPhaseEnded?.Invoke();
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

    #endregion

    #region Pause Control

    /// <summary>
    /// フェーズの一時停止
    /// </summary>
    public virtual void Pause()
    {
        if (isActive && !isPaused)
        {
            isPaused = true;
            OnPhasePaused?.Invoke();
            OnPauseImpl();
        }
    }

    /// <summary>
    /// フェーズの再開
    /// </summary>
    public virtual void Resume()
    {
        if (isActive && isPaused)
        {
            isPaused = false;
            OnPhaseResumed?.Invoke();
            OnResumeImpl();
        }
    }

    /// <summary>
    /// サブクラス用：Pause()時の処理
    /// </summary>
    protected virtual void OnPauseImpl() { }

    /// <summary>
    /// サブクラス用：Resume()時の処理
    /// </summary>
    protected virtual void OnResumeImpl() { }

    #endregion

    #region Visibility

    /// <summary>
    /// フェーズの表示/非表示切り替え
    /// </summary>
    protected virtual void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = visible;
            canvasGroup.interactable = visible;
        }
    }

    #endregion

    #region Properties

    /// <summary>
    /// フェーズがアクティブかつ非停止中かどうか
    /// </summary>
    public bool IsActive => isActive && !isPaused;

    /// <summary>
    /// フェーズが一時停止中かどうか
    /// </summary>
    public bool IsPaused => isPaused;

    /// <summary>
    /// フェーズがアクティブかどうか（停止中含む）
    /// </summary>
    public bool IsPhaseActive => isActive;

    #endregion

    #region Phase Transition Helper

    /// <summary>
    /// 次のフェーズへ遷移をリクエスト
    /// GameManagerへイベント経由で通知
    /// </summary>
    protected void RequestTransitionTo(GameState nextState)
    {
        PhaseEvents.RequestPhaseTransition(nextState);
    }

    /// <summary>
    /// ゲームクリアをリクエスト
    /// </summary>
    protected void RequestGameClear(float remainingTime)
    {
        PhaseEvents.RequestGameClear(remainingTime);
    }

    /// <summary>
    /// ゲームオーバーをリクエスト
    /// </summary>
    protected void RequestGameOver()
    {
        PhaseEvents.RequestGameOver();
    }

    #endregion
}
