using System;

/// <summary>
/// フェーズ遷移イベントを定義
/// GameManagerとPhaseController間の疎結合を実現
/// </summary>
public static class PhaseEvents
{
    /// <summary>
    /// 次のフェーズへ遷移するリクエスト
    /// PhaseControllerから発火し、GameManagerが購読
    /// </summary>
    public static event Action<GameState> OnPhaseTransitionRequested;

    /// <summary>
    /// ゲームクリア通知
    /// </summary>
    public static event Action<float> OnGameClearRequested;

    /// <summary>
    /// ゲームオーバー通知
    /// </summary>
    public static event Action OnGameOverRequested;

    /// <summary>
    /// フェーズ遷移をリクエスト
    /// </summary>
    public static void RequestPhaseTransition(GameState newState)
    {
        OnPhaseTransitionRequested?.Invoke(newState);
    }

    /// <summary>
    /// ゲームクリアをリクエスト
    /// </summary>
    public static void RequestGameClear(float remainingTime)
    {
        OnGameClearRequested?.Invoke(remainingTime);
    }

    /// <summary>
    /// ゲームオーバーをリクエスト
    /// </summary>
    public static void RequestGameOver()
    {
        OnGameOverRequested?.Invoke();
    }
}

