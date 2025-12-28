using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// ローディング画面フェーズコントローラー
/// 責務: ローディングの進行管理
/// </summary>
public class LoadingPhaseController : PhaseController
{
    [SerializeField] private Sprite[] images;
    [SerializeField] private Image loadingImage;
    [SerializeField] private float interval = 0.5f; // 画像切り替え間隔（秒）

    private float elapsedTime;
    private int currentImageIndex;

    public override GameState PhaseType => GameState.Loading;

    protected override void OnEnterImpl()
    {
        loadingImage.sprite = images[0];
        elapsedTime = 0f;
        currentImageIndex = 0;
    }

    public override void UpdatePhase()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= interval)
        {
            elapsedTime = 0f;
            currentImageIndex = (currentImageIndex + 1) % images.Length;
            loadingImage.sprite = images[currentImageIndex];
        }
    }

    protected override void OnExitImpl()
    {
        
    }

    private void CompleteLoading()
    {
        RequestTransitionTo(GameState.Prologue);
    }
    
}
