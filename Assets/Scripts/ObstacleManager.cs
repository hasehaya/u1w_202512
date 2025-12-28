using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 障害物のUI表示（ヒントテキスト、矢印）とPrefabからの生成を管理
/// </summary>
public class ObstacleManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject attentionObject; // Attention表示オブジェクト
    [SerializeField] private GameObject leftArrowObject; // 左向き矢印オブジェクト
    [SerializeField] private GameObject rightArrowObject; // 右向き矢印オブジェクト
    
    [Header("Obstacle Prefab")]
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private Transform obstacleParent;
    
    [Header("Animation Settings")]
    [SerializeField] private float attentionBlinkDuration = 1.0f; // Attention点滅時間
    [SerializeField] private int attentionBlinkCount = 3; // Attention点滅回数
    
    private Obstacle currentObstacle;
    private Coroutine spawnCoroutine;
    private Queue<ObstacleSpawnRequest> spawnQueue = new Queue<ObstacleSpawnRequest>();
    private bool isSpawning;

    public ObstaclePosition CurrentPosition => currentObstacle != null ? currentObstacle.CurrentPosition : ObstaclePosition.Left;
    public bool IsActive => currentObstacle != null && currentObstacle.IsActive;
    
    /// <summary>
    /// 障害物が実際に画面に表示された時に発火するイベント
    /// </summary>
    public event Action OnObstacleDisplayed;

    /// <summary>
    /// 障害物生成リクエストを保持する構造体
    /// </summary>
    private struct ObstacleSpawnRequest
    {
        public ObstaclePosition pos;

        public ObstacleSpawnRequest(ObstaclePosition pos)
        {
            this.pos = pos;
        }
    }

    /// <summary>
    /// 障害物をPrefabから生成して表示
    /// </summary>
    /// <param name="pos">左右の位置</param>
    public void Spawn(ObstaclePosition pos)
    {
        if (obstaclePrefab == null) return;

        // 障害物生成中の場合はキューに追加
        if (isSpawning || currentObstacle != null)
        {
            spawnQueue.Enqueue(new ObstacleSpawnRequest(pos));
            return;
        }

        // 生成を開始
        StartSpawn(pos);
    }

    /// <summary>
    /// 実際に障害物を生成する内部メソッド
    /// </summary>
    private void StartSpawn(ObstaclePosition pos)
    {
        isSpawning = true;
        
        // Attention表示→矢印表示→Obstacleを生成するコルーチンを開始
        spawnCoroutine = StartCoroutine(SpawnSequence(pos));
    }

    /// <summary>
    /// Attention表示→点滅→消去→矢印表示→Obstacle生成のシーケンス
    /// </summary>
    private IEnumerator SpawnSequence(ObstaclePosition pos)
    {
        // Attention表示
        ShowArrow(pos);
        attentionObject.SetActive(true);
        
        // 点滅アニメーション
        CanvasGroup attentionCanvasGroup = attentionObject.GetComponent<CanvasGroup>();
        if (attentionCanvasGroup == null)
        {
            attentionCanvasGroup = attentionObject.AddComponent<CanvasGroup>();
        }

        float blinkInterval = attentionBlinkDuration / (attentionBlinkCount * 2);
        for (int i = 0; i < attentionBlinkCount; i++)
        {
            attentionCanvasGroup.alpha = 0f;
            yield return new WaitForSeconds(blinkInterval);
            attentionCanvasGroup.alpha = 1f;
            yield return new WaitForSeconds(blinkInterval);
        }
        
        // Attentionを消去
        attentionObject.SetActive(false);


        // Obstacleを生成してスライドイン
        GameObject obstacleObj = Instantiate(obstaclePrefab, obstacleParent);
        currentObstacle = obstacleObj.GetComponent<Obstacle>();

        if (currentObstacle == null)
        {
            Debug.LogError("ObstaclePrefabにObstacleコンポーネントが見つかりません");
            Destroy(obstacleObj);
            isSpawning = false;
            ProcessNextInQueue();
            yield break;
        }

        // Obstacleをスライドインさせる（初期位置が始点）
        currentObstacle.Spawn(pos);
        
        // 生成完了
        isSpawning = false;
        
        // 障害物が実際に表示されたことを通知
        OnObstacleDisplayed?.Invoke();
    }
    
    /// <summary>
    /// 現在の障害物を非表示にする
    /// </summary>
    public void Hide()
    {
        // コルーチンをキャンセル
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        // Attentionを非表示
        if (attentionObject != null)
        {
            attentionObject.SetActive(false);
        }

        // 矢印を非表示
        HideArrow();

        if (currentObstacle != null)
        {
            currentObstacle.Despawn();
            Destroy(currentObstacle.gameObject);
            currentObstacle = null;
        }
        
        // フラグをリセット
        isSpawning = false;
        
        // キューにある次のリクエストを処理
        ProcessNextInQueue();
    }
    
    /// <summary>
    /// キューにある次の障害物生成リクエストを処理
    /// </summary>
    private void ProcessNextInQueue()
    {
        if (spawnQueue.Count > 0 && !isSpawning && currentObstacle == null)
        {
            ObstacleSpawnRequest request = spawnQueue.Dequeue();
            StartSpawn(request.pos);
        }
    }

    /// <summary>
    /// 左右の矢印を表示
    /// </summary>
    private void ShowArrow(ObstaclePosition pos)
    {
        if (leftArrowObject != null)
        {
            leftArrowObject.SetActive(pos == ObstaclePosition.Right);
        }
        
        if (rightArrowObject != null)
        {
            rightArrowObject.SetActive(pos == ObstaclePosition.Left);
        }
    }

    /// <summary>
    /// 矢印を非表示にする
    /// </summary>
    private void HideArrow()
    {
        if (leftArrowObject != null)
        {
            leftArrowObject.SetActive(false);
        }
        
        if (rightArrowObject != null)
        {
            rightArrowObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // クリーンアップ
        if (currentObstacle != null)
        {
            Destroy(currentObstacle.gameObject);
            currentObstacle = null;
        }
        
        // キューをクリア
        spawnQueue.Clear();
    }
}

