using UnityEngine;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public event Action OnTap;
    public event Action<SwipeDirection> OnSwipe;

    private Vector2 startPos;
    private bool isTouching = false;
    private const float SWIPE_THRESHOLD = 80f; // ピクセル単位の閾値

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isTouching = true;
            startPos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0) && isTouching)
        {
            isTouching = false;
            Vector2 endPos = Input.mousePosition;
            Vector2 diff = endPos - startPos;

            if (diff.magnitude < SWIPE_THRESHOLD)
            {
                // 移動量が少なければタップとみなす
                OnTap?.Invoke();
            }
            else
            {
                // スワイプ判定
                float x = diff.x;
                float y = diff.y;
                SwipeDirection direction = SwipeDirection.None;

                if (Mathf.Abs(x) > Mathf.Abs(y))
                {
                    direction = x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
                }
                else
                {
                    direction = y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
                }

                OnSwipe?.Invoke(direction);
            }
        }
    }
}