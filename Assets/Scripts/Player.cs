using UnityEngine;
using DG.Tweening;

/// <summary>
/// プレイヤーの左右移動を管理
/// </summary>
public class Player : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;

    [Header("Position Settings")]
    [SerializeField] private float leftPositionX = -500f;
    [SerializeField] private float rightPositionX = 500f;
    [SerializeField] private float centerPositionY = 0f;

    [Header("Animation Settings")]
    [SerializeField] private float moveDuration = 0.3f;

    private PlayerPosition currentPosition = PlayerPosition.Center;
    private Tween moveTween;

    public PlayerPosition CurrentPosition => currentPosition;

    private void Start()
    {
        // 初期位置を中央に設定
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(0f, centerPositionY);
        }

        SubscribeToInputEvents();
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
                MoveToLeft();
                break;
            case SwipeDirection.Right:
                MoveToRight();
                break;
        }
    }

    public void MoveToLeft()
    {
        if (currentPosition == PlayerPosition.Left) return;

        currentPosition = PlayerPosition.Left;
        AnimateToPosition(new Vector2(leftPositionX, centerPositionY));
    }

    public void MoveToRight()
    {
        if (currentPosition == PlayerPosition.Right) return;

        currentPosition = PlayerPosition.Right;
        AnimateToPosition(new Vector2(rightPositionX, centerPositionY));
    }

    public void MoveToCenter()
    {
        currentPosition = PlayerPosition.Center;
        AnimateToPosition(new Vector2(0f, centerPositionY));
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

