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

    public ObstaclePosition CurrentPosition => currentPos;
    public bool IsActive { get; private set; }

    public void Spawn(ObstaclePosition pos)
    {
        // 既存のTweenをキャンセル
        currentTween?.Kill();

        currentPos = pos;
        IsActive = true;
        gameObject.SetActive(true);

        SetupPosition(pos);
        AnimateObstacle();
    }

    public void Despawn()
    {
        IsActive = false;
        currentTween?.Kill();
        gameObject.SetActive(false);
    }

    private void SetupPosition(ObstaclePosition pos)
    {
        switch (pos)
        {
            case ObstaclePosition.Left:
                startPos = leftStartPos;
                endPos = leftEndPos;
                rectTransform.anchoredPosition = startPos;
                break;
                
            case ObstaclePosition.Right:
                startPos = rightStartPos;
                endPos = rightEndPos;
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

