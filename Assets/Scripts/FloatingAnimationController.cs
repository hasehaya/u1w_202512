using UnityEngine;

public class FloatingAnimationController : MonoBehaviour
{
    [System.Serializable]
    public class FloatingObjectData
    {
        [Header("基本設定")]
        public Transform targetTransform;
        
        [Header("回転アニメーション（Z軸）")]
        public bool enableRotation = true;
        [Range(0f, 10f)] public float rotationSpeed = 2f;
        [Range(0f, 360f)] public float rotationAmplitude = 15f;
        [Range(0f, 10f)] public float rotationPhaseOffset;
        
        [Header("位置アニメーション（X軸）")]
        public bool enablePositionX = false;
        [Range(0f, 10f)] public float positionXSpeed = 1f;
        [Range(0f, 500f)] public float positionXAmplitude = 50f;
        [Range(0f, 10f)] public float positionXPhaseOffset;
        
        [Header("位置アニメーション（Y軸）")]
        public bool enablePositionY = false;
        [Range(0f, 10f)] public float positionYSpeed = 1.5f;
        [Range(0f, 500f)] public float positionYAmplitude = 30f;
        [Range(0f, 10f)] public float positionYPhaseOffset;
        
        [Header("スケールアニメーション（X軸）")]
        public bool enableScaleX = false;
        [Range(0f, 10f)] public float scaleXSpeed = 1f;
        [Range(0f, 2f)] public float scaleXAmplitude = 0.1f;
        [Range(0f, 10f)] public float scaleXPhaseOffset;
        
        [Header("スケールアニメーション（Y軸）")]
        public bool enableScaleY = false;
        [Range(0f, 10f)] public float scaleYSpeed = 1f;
        [Range(0f, 2f)] public float scaleYAmplitude = 0.1f;
        [Range(0f, 10f)] public float scaleYPhaseOffset;
        
        // 初期値を保存
        [HideInInspector] public Vector3 initialPosition;
        [HideInInspector] public Vector3 initialScale;
        [HideInInspector] public Quaternion initialRotation;
        [HideInInspector] public bool isInitialized;
    }
    
    [Header("子オブジェクトの設定")]
    [SerializeField] private FloatingObjectData[] floatingObjects = new FloatingObjectData[4];
    
    [Header("スクロール設定")]
    [SerializeField] private bool enableScroll = true;
    [SerializeField] private float scrollSpeed = 100f;
    [SerializeField] private float resetPositionX = 1920f;
    [SerializeField] private float removePositionX = -1920f;
    
    private RectTransform rectTransform;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (rectTransform == null)
        {
            Debug.LogError("RectTransformが見つかりません。UIオブジェクトにアタッチしてください。");
        }
        
        // 各オブジェクトの初期値を保存
        for (int i = 0; i < floatingObjects.Length; i++)
        {
            if (floatingObjects[i].targetTransform != null)
            {
                floatingObjects[i].initialPosition = floatingObjects[i].targetTransform.localPosition;
                floatingObjects[i].initialScale = floatingObjects[i].targetTransform.localScale;
                floatingObjects[i].initialRotation = floatingObjects[i].targetTransform.localRotation;
                floatingObjects[i].isInitialized = true;
            }
            else
            {
                Debug.LogWarning($"FloatingObject[{i}]のTransformが設定されていません。");
            }
        }
    }

    void Update()
    {
        // 各オブジェクトの浮遊感アニメーション
        AnimateFloatingObjects();
        
        // 親オブジェクトのスクロール
        if (enableScroll)
        {
            ScrollParent();
        }
    }
    
    /// <summary>
    /// 各オブジェクトに浮遊感のあるアニメーションを適用
    /// </summary>
    private void AnimateFloatingObjects()
    {
        for (int i = 0; i < floatingObjects.Length; i++)
        {
            var obj = floatingObjects[i];
            
            if (obj.targetTransform == null || !obj.isInitialized)
                continue;
            
            // 回転アニメーション
            if (obj.enableRotation)
            {
                float rotationTime = Time.time * obj.rotationSpeed + obj.rotationPhaseOffset;
                float zRotation = Mathf.Sin(rotationTime) * obj.rotationAmplitude;
                // 初期回転に対して相対的に回転を適用
                obj.targetTransform.localRotation = obj.initialRotation * Quaternion.Euler(0f, 0f, zRotation);
            }
            
            // 位置アニメーション
            Vector3 newPosition = obj.initialPosition;
            
            if (obj.enablePositionX)
            {
                float posXTime = Time.time * obj.positionXSpeed + obj.positionXPhaseOffset;
                newPosition.x += Mathf.Sin(posXTime) * obj.positionXAmplitude;
            }
            
            if (obj.enablePositionY)
            {
                float posYTime = Time.time * obj.positionYSpeed + obj.positionYPhaseOffset;
                newPosition.y += Mathf.Sin(posYTime) * obj.positionYAmplitude;
            }
            
            obj.targetTransform.localPosition = newPosition;
            
            // スケールアニメーション
            Vector3 newScale = obj.initialScale;
            
            if (obj.enableScaleX)
            {
                float scaleXTime = Time.time * obj.scaleXSpeed + obj.scaleXPhaseOffset;
                newScale.x += Mathf.Sin(scaleXTime) * obj.scaleXAmplitude;
            }
            
            if (obj.enableScaleY)
            {
                float scaleYTime = Time.time * obj.scaleYSpeed + obj.scaleYPhaseOffset;
                newScale.y += Mathf.Sin(scaleYTime) * obj.scaleYAmplitude;
            }
            
            obj.targetTransform.localScale = newScale;
        }
    }
    
    /// <summary>
    /// 親オブジェクトを左方向にスクロール
    /// </summary>
    private void ScrollParent()
    {
        if (rectTransform != null)
        {
            // 左方向に移動
            Vector3 currentPos = rectTransform.anchoredPosition;
            currentPos.x -= scrollSpeed * Time.deltaTime;
            rectTransform.anchoredPosition = currentPos;
            
            // 画面外（左側）に出たら右側にリセット
            if (currentPos.x < removePositionX)
            {
                currentPos.x = resetPositionX;
                rectTransform.anchoredPosition = currentPos;
            }
        }
    }
    
    /// <summary>
    /// 位置を手動でリセット（デバッグ用）
    /// </summary>
    public void ResetPosition()
    {
        if (rectTransform != null)
        {
            Vector3 currentPos = rectTransform.anchoredPosition;
            currentPos.x = resetPositionX;
            rectTransform.anchoredPosition = currentPos;
        }
    }
    
    /// <summary>
    /// 初期値を再取得（ランタイムで位置を変更した後に呼ぶ）
    /// </summary>
    public void RefreshInitialValues()
    {
        for (int i = 0; i < floatingObjects.Length; i++)
        {
            if (floatingObjects[i].targetTransform != null)
            {
                floatingObjects[i].initialPosition = floatingObjects[i].targetTransform.localPosition;
                floatingObjects[i].initialScale = floatingObjects[i].targetTransform.localScale;
                floatingObjects[i].initialRotation = floatingObjects[i].targetTransform.localRotation;
            }
        }
    }
}
