using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// ゲームオーバー画面フェーズコントローラー
/// 責務: ゲームオーバー結果を表示するフェーズの管理
/// </summary>
public class GameOverPhaseController : PhaseController
{
    [Header("UI References")] 
    [SerializeField] private GameObject characterObject;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button titleButton;
    
    [Header("Animation Settings")]
    [SerializeField] private float moveDuration = 2.0f; // 移動アニメーションの時間
    [SerializeField] private Vector2 startPosition = new Vector2(1200f, -103f); // 開始位置（画面外右側）
    [SerializeField] private Vector2 targetPosition = new Vector2(531f, -103f); // 最終着地位置
    [SerializeField] private float initialBounceHeight = 300f; // 最初のバウンドの高さ
    [SerializeField] private int bounceCount = 4; // バウンドの回数
    
    public override GameState PhaseType => GameState.GameOver;

    protected override void OnEnterImpl()
    {
        SetupButtons();
        PlayThrowAnimation();
        
        AudioManager.Instance.PlaySe(SeType.GameOver);
    }

    public override void UpdatePhase()
    {
        // ゲームオーバー画面の更新処理（必要に応じて実装）
    }

    protected override void OnExitImpl()
    {
        CleanupButtons();
        // アニメーションをクリーンアップ
        if (characterObject != null)
        {
            characterObject.transform.DOKill();
        }
        AudioManager.Instance.StopAllSe();
    }

    /// <summary>
    /// キャラクターが右から左へ移動しながら、減衰するバウンドアニメーションを再生
    /// </summary>
    private void PlayThrowAnimation()
    {
        if (characterObject == null) return;

        RectTransform rectTransform = characterObject.GetComponent<RectTransform>();
        if (rectTransform == null) return;

        // 初期位置を設定（画面外右側）
        rectTransform.anchoredPosition = startPosition;

        // X軸: 右から左へ滑らかに移動（OutSineで減速）
        rectTransform.DOAnchorPosX(targetPosition.x, moveDuration)
            .SetEase(Ease.OutSine);

        // Y軸: 減衰するバウンドアニメーション
        Sequence bounceSequence = DOTween.Sequence();
        float timePerBounce = moveDuration / bounceCount;

        for (int i = 0; i < bounceCount; i++)
        {
            // 各バウンドの高さを減衰させる（指数的に減少）
            float bounceHeight = initialBounceHeight * Mathf.Pow(0.5f, i);
            
            // 上昇
            bounceSequence.Append(
                rectTransform.DOAnchorPosY(targetPosition.y + bounceHeight, timePerBounce / 2)
                    .SetEase(Ease.OutQuad)
            );
            
            // 下降
            bounceSequence.Append(
                rectTransform.DOAnchorPosY(targetPosition.y, timePerBounce / 2)
                    .SetEase(Ease.InQuad)
            );
        }
    }

    private void SetupButtons()
    {
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetry);
        if (titleButton != null)
            titleButton.onClick.AddListener(OnBackToTitle);
    }

    private void CleanupButtons()
    {
        if (retryButton != null)
            retryButton.onClick.RemoveListener(OnRetry);
        if (titleButton != null)
            titleButton.onClick.RemoveListener(OnBackToTitle);
    }

    private void OnRetry()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySe(SeType.ButtonClick);
        }
        RequestTransitionTo(GameState.SetTimer);
    }

    private void OnBackToTitle()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySe(SeType.ButtonClick);
        }
        RequestTransitionTo(GameState.Title);
    }
}

