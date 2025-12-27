using UnityEngine;
using System;

/// <summary>
/// フェーズ基本クラス
/// 全てのゲームフェーズはこのクラスを継承します
/// </summary>
public abstract class PhaseController : MonoBehaviour
{
    [SerializeField] protected CanvasGroup canvasGroup;
    
    // フェーズの状態管理
    protected bool isActive = false;
    protected bool isPaused = false;
    protected GameState phaseType;

    // イベント
    public event Action<GameState> OnPhaseStarted;
    public event Action<GameState> OnPhaseEnded;
    public event Action OnPhasePaused;
    public event Action OnPhaseResumed;

    protected virtual void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// フェーズの初期化
    /// Initialize()を呼び出す前に必ずこのメソッドを実行します
    /// </summary>
    public virtual void OnPhaseEnter()
    {
        isActive = true;
        isPaused = false;
        OnPhaseStarted?.Invoke(phaseType);
        Initialize();
    }

    /// <summary>
    /// フェーズの終了処理
    /// </summary>
    public virtual void OnPhaseExit()
    {
        isActive = false;
        Cleanup();
        OnPhaseEnded?.Invoke(phaseType);
    }

    /// <summary>
    /// フェーズの初期化（サブクラスでオーバーライド）
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// フェーズの更新（毎フレーム呼び出される）
    /// </summary>
    public abstract void UpdatePhase();

    /// <summary>
    /// フェーズの終了処理（サブクラスでオーバーライド）
    /// </summary>
    public abstract void Cleanup();

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
    /// サブクラス用：Pause()時の処理をオーバーライド
    /// </summary>
    protected virtual void OnPauseImpl() { }

    /// <summary>
    /// サブクラス用：Resume()時の処理をオーバーライド
    /// </summary>
    protected virtual void OnResumeImpl() { }

    /// <summary>
    /// フェーズの表示/非表示切り替え
    /// </summary>
    public virtual void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = visible;
        }
    }

    /// <summary>
    /// フェーズがアクティブかどうか
    /// </summary>
    public bool IsActive => isActive && !isPaused;

    /// <summary>
    /// フェーズが一時停止中かどうか
    /// </summary>
    public bool IsPaused => isPaused;

    /// <summary>
    /// このフェーズのタイプを取得
    /// </summary>
    public GameState PhaseType => phaseType;
}

