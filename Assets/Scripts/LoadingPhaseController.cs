using UnityEngine;

/// <summary>
/// ローディング画面フェーズコントローラー
/// </summary>
public class LoadingPhaseController : PhaseController
{
    [SerializeField] private float loadingDuration = 2f;
    private float loadingTimer = 0f;

    private void Start()
    {
        phaseType = GameState.Loading;
    }

    public override void Initialize()
    {
        SetVisible(true);
        loadingTimer = 0f;
    }

    public override void UpdatePhase()
    {
        if (!IsActive) return;

        loadingTimer += Time.deltaTime;

        if (loadingTimer >= loadingDuration)
        {
            CompleteLoading();
        }
    }

    public override void Cleanup()
    {
        SetVisible(false);
        loadingTimer = 0f;
    }

    /// <summary>
    /// ローディング完了時の処理
    /// </summary>
    private void CompleteLoading()
    {
        GameManager.Instance.ChangeState(GameState.Prologue);
    }

    /// <summary>
    /// ローディング進捗の取得 (0~1)
    /// </summary>
    public float GetLoadingProgress()
    {
        return Mathf.Clamp01(loadingTimer / loadingDuration);
    }
}

