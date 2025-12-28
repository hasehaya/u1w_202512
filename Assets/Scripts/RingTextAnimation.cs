using UnityEngine;
using DG.Tweening;

public class RingTextAnimation : MonoBehaviour
{
    [SerializeField] private Ease easeType = Ease.InOutFlash;
    [SerializeField] private float moveY = 100f;
    [SerializeField] private float duration = 1f;
    
    private float initialY;
    private float time;
    private bool isAnimating;
    
    private void OnEnable()
    {
        initialY = transform.localPosition.y;
        time = 0f;
        isAnimating = true;
    }
    
    private void Update()
    {
        if (!isAnimating) return;
        
        time += Time.deltaTime;
        
        // ループするための時間計算（往復で duration * 2）
        float loopTime = time % (duration * 2);
        float t = loopTime / duration;
        
        // 往復の計算（0→1→0）
        if (t > 1f)
        {
            t = 2f - t; // 1を超えたら反転
        }
        
        // Easeを適用
        float easedT = DOVirtual.EasedValue(0f, 1f, t, easeType);
        
        // Y座標のみを更新（localPositionを使用）
        Vector3 pos = transform.localPosition;
        pos.y = initialY + (moveY * easedT);
        transform.localPosition = pos;
    }
    
    private void OnDisable()
    {
        isAnimating = false;
        Vector3 pos = transform.localPosition;
        pos.y = initialY;
        transform.localPosition = pos;
    }
}
