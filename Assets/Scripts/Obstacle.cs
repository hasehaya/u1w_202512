using UnityEngine;
using DG.Tweening;
using System;

/// <summary>
/// 障害物の移動ロジックを管理
/// </summary>
public class Obstacle : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;

    // 左右それぞれの始点・終点の設定
    [Header("Position Settings")]
    [SerializeField] private Vector2 leftStartPos = new Vector2(-1000f, 300f);
    [SerializeField] private Vector2 leftEndPos = new Vector2(-1000f, -300f);
    [SerializeField] private Vector2 rightStartPos = new Vector2(1000f, 300f);
    [SerializeField] private Vector2 rightEndPos = new Vector2(1000f, -300f);

    [Header("Animation Settings")]
    [SerializeField] private float moveDuration = 2f; // 始点から終点への移動時間

    private ObstaclePosition currentPos;
    private Vector2 startPos; // 始点
    private Vector2 endPos; // 終点
    private Tween currentTween;
    private int positionIndex; // 0: Left Start, 1: Left End, 2: Right Start, 3: Right End

    public ObstaclePosition CurrentPosition => currentPos;
    public bool IsActive { get; private set; }

    // posIndex: 0=Left Start, 1=Left End, 2=Right Start, 3=Right End
    public void Spawn(ObstaclePosition pos, int posIndex)
    {
        // 既存のTweenをキャンセル
        currentTween?.Kill();

        currentPos = pos;
        positionIndex = posIndex;
        IsActive = true;
        gameObject.SetActive(true);

        SetupPosition(pos, posIndex);
        AnimateObstacle();
    }

    public void Despawn()
    {
        IsActive = false;
        currentTween?.Kill();
        gameObject.SetActive(false);
    }

    private void SetupPosition(ObstaclePosition pos, int posIndex)
    {
        switch (pos)
        {
            case ObstaclePosition.Left:
                if (posIndex == 0) // Left Start (上)
                {
                    startPos = leftStartPos;
                    endPos = leftEndPos;
                }
                else // Left End (下)
                {
                    startPos = leftEndPos;
                    endPos = leftStartPos;
                }
                
                // 初期位置を始点に設定
                rectTransform.anchoredPosition = startPos;
                break;
                
            case ObstaclePosition.Right:
                if (posIndex == 2) // Right Start (上)
                {
                    startPos = rightStartPos;
                    endPos = rightEndPos;
                }
                else // Right End (下)
                {
                    startPos = rightEndPos;
                    endPos = rightStartPos;
                }
                
                // 初期位置を始点に設定
                rectTransform.anchoredPosition = startPos;
                break;
        }
    }

    private void AnimateObstacle()
    {
        // 始点から終点へ直接移動
        currentTween = rectTransform.DOAnchorPos(endPos, moveDuration)
            .SetEase(Ease.Linear);
    }

    private void OnDestroy()
    {
        currentTween?.Kill();
    }
}

