﻿using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

/// <summary>
/// 障害物の移動ロジックを管理
/// </summary>
public class Obstacle : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image obstacleImage;

    // 左右それぞれの始点・終点の設定
    [Header("Position Settings")]
    [SerializeField] private Vector2 leftStartPos = new Vector2(-1000f, 300f);
    [SerializeField] private Vector2 leftEndPos = new Vector2(-1000f, -300f);
    [SerializeField] private Vector2 rightStartPos = new Vector2(1000f, 300f);
    [SerializeField] private Vector2 rightEndPos = new Vector2(1000f, -300f);

    [Header("Animation Settings")]
    [SerializeField] private float moveDuration = 2f; // 始点から終点への移動時間
    
    [Header("Obstacle Sprites")]
    [SerializeField] private Sprite[] obstacleSprites; // 障害物のSpriteリスト

    private ObstaclePosition currentPos;
    private Vector2 startPos; // 始点
    private Vector2 endPos; // 終点
    private Tween currentTween;
    
    private static List<Sprite> shuffledSprites = new List<Sprite>(); // シャッフルされたSpriteリスト（static）
    private static int currentSpriteIndex = 0; // 現在のSpriteインデックス（static）
    private static bool isInitialized = false; // 初期化フラグ（static）

    public ObstaclePosition CurrentPosition => currentPos;
    public bool IsActive { get; private set; }

    private void Awake()
    {
        // 最初のインスタンスでSpriteリストをシャッフル
        if (!isInitialized)
        {
            InitializeSprites();
            isInitialized = true;
        }
    }

    /// <summary>
    /// Spriteリストを初期化してシャッフル
    /// </summary>
    private void InitializeSprites()
    {
        shuffledSprites.Clear();
        
        if (obstacleSprites == null || obstacleSprites.Length == 0)
        {
            Debug.LogWarning("Obstacle: obstacleSpritesが設定されていません");
            return;
        }
        
        // 配列をリストにコピー
        shuffledSprites.AddRange(obstacleSprites);
        
        // Fisher-Yatesアルゴリズムでシャッフル
        for (int i = shuffledSprites.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Sprite temp = shuffledSprites[i];
            shuffledSprites[i] = shuffledSprites[randomIndex];
            shuffledSprites[randomIndex] = temp;
        }
        
        currentSpriteIndex = 0;
    }
    
    /// <summary>
    /// 次のSpriteを取得（循環）
    /// </summary>
    private Sprite GetNextSprite()
    {
        if (shuffledSprites.Count == 0)
        {
            return null;
        }
        
        Sprite sprite = shuffledSprites[currentSpriteIndex];
        currentSpriteIndex = (currentSpriteIndex + 1) % shuffledSprites.Count;
        
        return sprite;
    }

    /// <summary>
    /// 障害物のSpriteを設定
    /// </summary>
    public void SetSprite(Sprite sprite)
    {
        if (obstacleImage != null && sprite != null)
        {
            obstacleImage.sprite = sprite;
        }
    }

    public void Spawn(ObstaclePosition pos)
    {
        // 既存のTweenをキャンセル
        currentTween?.Kill();

        currentPos = pos;
        IsActive = true;
        gameObject.SetActive(true);
        
        // 次のSpriteを自動的に設定
        Sprite nextSprite = GetNextSprite();
        if (nextSprite != null)
        {
            SetSprite(nextSprite);
        }

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

