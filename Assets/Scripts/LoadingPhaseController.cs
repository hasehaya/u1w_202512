using UnityEngine;

/// <summary>
/// ローディング画面フェーズコントローラー
/// 責務: ローディングの進行管理
/// </summary>
public class LoadingPhaseController : PhaseController
{
    [SerializeField] private float loadingDuration = 2f;
    
    private float loadingTimer = 0f;

    public override GameState PhaseType => GameState.Loading;

    protected override void OnEnterImpl()
    {
        loadingTimer = 0f;
    }

    public override void UpdatePhase()
    {
        loadingTimer += Time.deltaTime;

        if (loadingTimer >= loadingDuration)
        {
            CompleteLoading();
        }
    }

    protected override void OnExitImpl()
    {
        loadingTimer = 0f;
    }

    private void CompleteLoading()
    {
        RequestTransitionTo(GameState.Prologue);
    }

    /// <summary>
    /// ローディング進捗の取得 (0~1)
    /// </summary>
    public float GetLoadingProgress()
    {
        return Mathf.Clamp01(loadingTimer / loadingDuration);
    }
}

