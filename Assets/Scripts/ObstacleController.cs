using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 障害物のUI表示（ヒントテキスト、矢印）とPrefabからの生成を管理
/// </summary>
public class ObstacleManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject attentionObject; // Attention表示オブジェクト
    [SerializeField] private Image hintImage; // 左右のヒント画像
    [SerializeField] private Sprite leftHintSprite; // 左向き矢印画像
    [SerializeField] private Sprite rightHintSprite; // 右向き矢印画像
    
    [Header("Obstacle Prefab")]
    [SerializeField] private GameObject obstaclePrefab;
    
    [Header("Spawn Parent")]
    [SerializeField] private Transform obstacleParent;
    
    [Header("Animation Settings")]
    [SerializeField] private float attentionBlinkDuration = 1.0f; // Attention点滅時間
    [SerializeField] private int attentionBlinkCount = 3; // Attention点滅回数
    
    private Obstacle currentObstacle;
    private Coroutine spawnCoroutine;

    public ObstaclePosition CurrentPosition => currentObstacle != null ? currentObstacle.CurrentPosition : ObstaclePosition.Left;
    public bool IsActive => currentObstacle != null && currentObstacle.IsActive;

    /// <summary>
    /// 障害物をPrefabから生成して表示
    /// </summary>
    /// <param name="pos">左右の位置</param>
    /// <param name="posIndex">0: Left Start, 1: Left End, 2: Right Start, 3: Right End</param>
    public void Spawn(ObstaclePosition pos, int posIndex)
    {
        if (obstaclePrefab == null) return;

        // 既存のコルーチンをキャンセル
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        // 既存の障害物があれば破棄
        if (currentObstacle != null)
        {
            Destroy(currentObstacle.gameObject);
            currentObstacle = null;
        }

        UpdateUI(pos);
        gameObject.SetActive(true);
        
        // Attention表示後にObstacleを生成するコルーチンを開始
        spawnCoroutine = StartCoroutine(SpawnSequence(pos, posIndex));
    }

    /// <summary>
    /// Attention表示→点滅→消去→Obstacle生成のシーケンス
    /// </summary>
    private IEnumerator SpawnSequence(ObstaclePosition pos, int posIndex)
    {
        // Attention表示
        if (attentionObject != null)
        {
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
        }

        // Obstacleを生成してスライドイン
        GameObject obstacleObj = Instantiate(obstaclePrefab, obstacleParent != null ? obstacleParent : transform);
        currentObstacle = obstacleObj.GetComponent<Obstacle>();

        if (currentObstacle == null)
        {
            Debug.LogError("ObstaclePrefabにObstacleコンポーネントが見つかりません");
            Destroy(obstacleObj);
            yield break;
        }

        // Obstacleをスライドインさせる（初期位置が始点）
        currentObstacle.Spawn(pos, posIndex);
    }

    /// <summary>
    /// 現在の障害物を削除（RunPhaseControllerから呼び出される）
    /// </summary>
    public void DespawnCurrentObstacle()
    {
        if (currentObstacle != null)
        {
            currentObstacle.Despawn();
            Destroy(currentObstacle.gameObject);
            currentObstacle = null;
        }
        
        gameObject.SetActive(false);
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

        if (currentObstacle != null)
        {
            currentObstacle.Despawn();
            Destroy(currentObstacle.gameObject);
            currentObstacle = null;
        }
        
        gameObject.SetActive(false);
    }

    private void UpdateUI(ObstaclePosition pos)
    {
        if (hintImage == null) return;

        switch (pos)
        {
            case ObstaclePosition.Left:
                hintImage.sprite = leftHintSprite;
                break;
                
            case ObstaclePosition.Right:
                hintImage.sprite = rightHintSprite;
                break;
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
    }
}