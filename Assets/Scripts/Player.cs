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

    private PlayerPosition currentPosition = PlayerPosition.Center;
    private Tween moveTween;
    private float spriteChangeTimer = 0f;
    private bool useFirstSprite = true;
    private float autoReturnTimer = 0f;
    private bool shouldAutoReturn = false;

    public PlayerPosition CurrentPosition => currentPosition;

    private void Start()
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
        }

        SubscribeToInputEvents();
    }

    private void Update()
    {
        // スプライトの切り替え処理
        if (playerImage != null && sprite1 != null && sprite2 != null)
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

    private void OnDestroy()
    {
        UnsubscribeFromInputEvents();
        moveTween?.Kill();
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

