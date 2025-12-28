using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// プレイヤーの左右移動を管理
/// </summary>
public class Player : MonoBehaviour
{
    [SerializeField] private Image playerImage;
    [SerializeField] private RectTransform rectTransform;

    [Header("Position Settings")]
    [SerializeField] private float leftPositionX = -500f;
    [SerializeField] private float rightPositionX = 500f;
    [SerializeField] private float centerPositionY = 0f;

    [Header("Animation Settings")]
    [SerializeField] private float moveDuration = 0.3f;

    [Header("Sprite Animation Settings")]
    [SerializeField] private Sprite sprite1;
    [SerializeField] private Sprite sprite2;
    [SerializeField] private float spriteChangeInterval = 0.5f;

    [Header("Auto Return Settings")]
    [SerializeField] private float autoReturnDelay = 1.0f;

    [Header("Damage Effect Settings")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float blinkDuration = 0.2f;
    [SerializeField] private int blinkCount = 5;

    private PlayerPosition currentPosition = PlayerPosition.Center;
    private Tween moveTween;
    private Tween blinkTween;
    private float spriteChangeTimer = 0f;
    private bool useFirstSprite = true;
    private float autoReturnTimer = 0f;
    private bool shouldAutoReturn = false;
    private Color originalColor;
    private float idleTimer = 0f;
    private float idleThreshold = 0.5f; // 入力がない時間がこれを超えたら自動切り替え開始

    public PlayerPosition CurrentPosition => currentPosition;

    public void UpdatePlayer()
    {
        if(GameManager.Instance.CurrentState != GameState.Run) return;
        
        // アイドルタイマーを更新
        idleTimer += Time.deltaTime;

        // アイドル状態（入力がない状態が続いている）の場合のみ、スプライトを自動切り替え
        if (idleTimer >= idleThreshold && playerImage != null && sprite1 != null && sprite2 != null)
        {
            spriteChangeTimer += Time.deltaTime;

            if (spriteChangeTimer >= spriteChangeInterval)
            {
                spriteChangeTimer = 0f;
                useFirstSprite = !useFirstSprite;
                playerImage.sprite = useFirstSprite ? sprite1 : sprite2;
            }
        }

        // 自動復帰処理
        if (shouldAutoReturn)
        {
            autoReturnTimer += Time.deltaTime;

            if (autoReturnTimer >= autoReturnDelay)
            {
                shouldAutoReturn = false;
                autoReturnTimer = 0f;
                MoveToCenter();
            }
        }
    }
    
    public void Initialize()
    {
        // 初期位置を中央に設定
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(0f, centerPositionY);
        }

        // 初期スプライトを設定
        if (playerImage != null && sprite1 != null)
        {
            playerImage.sprite = sprite1;
            originalColor = playerImage.color;
        }

        spriteChangeTimer = 0;
        useFirstSprite = true;
        autoReturnTimer = 0f;
        shouldAutoReturn = false;
        idleTimer = 0f;

        SubscribeToInputEvents();
    }

    public void OnExit()
    {
        UnsubscribeFromInputEvents();
        moveTween?.Kill();
        blinkTween?.Kill();
        
        // 色を元に戻す
        if (playerImage != null)
        {
            playerImage.color = originalColor;
        }
    }

    private void SubscribeToInputEvents()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnSwipe += HandleSwipe;
        }
    }

    private void UnsubscribeFromInputEvents()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnSwipe -= HandleSwipe;
        }
    }

    private void HandleSwipe(SwipeDirection direction)
    {
        // スワイプSEを再生
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySe(SeType.Swipe);
        }
        
        switch (direction)
        {
            case SwipeDirection.Left:
                // 右にいる場合は中央へ、それ以外は左へ
                if (currentPosition == PlayerPosition.Right)
                {
                    MoveToCenter();
                }
                else if (currentPosition == PlayerPosition.Center)
                {
                    MoveToLeft();
                }
                break;
            case SwipeDirection.Right:
                // 左にいる場合は中央へ、それ以外は右へ
                if (currentPosition == PlayerPosition.Left)
                {
                    MoveToCenter();
                }
                else if (currentPosition == PlayerPosition.Center)
                {
                    MoveToRight();
                }
                break;
        }
    }

    public void MoveToLeft()
    {
        if (currentPosition == PlayerPosition.Left) return;

        currentPosition = PlayerPosition.Left;
        AnimateToPosition(new Vector2(leftPositionX, centerPositionY));
        
        // 自動復帰タイマーを開始
        autoReturnTimer = 0f;
        shouldAutoReturn = true;
    }

    public void MoveToRight()
    {
        if (currentPosition == PlayerPosition.Right) return;

        currentPosition = PlayerPosition.Right;
        AnimateToPosition(new Vector2(rightPositionX, centerPositionY));
        
        // 自動復帰タイマーを開始
        autoReturnTimer = 0f;
        shouldAutoReturn = true;
    }

    public void MoveToCenter()
    {
        currentPosition = PlayerPosition.Center;
        AnimateToPosition(new Vector2(0f, centerPositionY));
        
        // 自動復帰タイマーを停止
        shouldAutoReturn = false;
        autoReturnTimer = 0f;
    }

    private void AnimateToPosition(Vector2 targetPosition)
    {
        moveTween?.Kill();
        moveTween = rectTransform.DOAnchorPos(targetPosition, moveDuration)
            .SetEase(Ease.OutQuad);
    }

    public void ResetPosition()
    {
        moveTween?.Kill();
        currentPosition = PlayerPosition.Center;
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(0f, centerPositionY);
        }
    }

    /// <summary>
    /// 手動でスプライトを切り替える（ボタン押下時などに使用）
    /// </summary>
    public void SwitchSprite()
    {
        if (playerImage == null || sprite1 == null || sprite2 == null) return;

        // スプライトを切り替え
        useFirstSprite = !useFirstSprite;
        playerImage.sprite = useFirstSprite ? sprite1 : sprite2;

        // アイドルタイマーとスプライト切り替えタイマーをリセット
        idleTimer = 0f;
        spriteChangeTimer = 0f;
    }

    /// <summary>
    /// ダメージを受けた際の赤く明滅するエフェクト
    /// </summary>
    public void PlayDamageEffect()
    {
        if (playerImage == null) return;

        // 既存の明滅を停止
        blinkTween?.Kill();

        // 明滅シーケンスを作成
        Sequence blinkSequence = DOTween.Sequence();

        for (int i = 0; i < blinkCount; i++)
        {
            // 赤に変化
            blinkSequence.Append(playerImage.DOColor(damageColor, blinkDuration));
            // 元の色に戻る
            blinkSequence.Append(playerImage.DOColor(originalColor, blinkDuration));
        }

        blinkTween = blinkSequence;
    }
}

/// <summary>
/// プレイヤーの位置
/// </summary>
public enum PlayerPosition
{
    Left,
    Center,
    Right
}

