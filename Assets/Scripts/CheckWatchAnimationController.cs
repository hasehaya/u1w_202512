using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CheckWatchAnimationController : MonoBehaviour
{
    // ============================================
    // アニメーション順序設定
    // ============================================
    [System.Serializable]
    public class AnimationStep
    {
        [Header("基本設定")]
        public string stepName;                         // ステップの名前（識別用）
        public GameObject targetObject;                 // アニメーション対象
        public AnimationType type;                      // アニメーションの種類
        
        [Header("タイミング設定")]
        [Range(0f, 5f)] public float delay;            // 前のアニメーションからの待機時間
        [Range(0f, 3f)] public float duration;         // アニメーション時間
        public Ease easeType = Ease.OutQuad;           // イージングタイプ
        
        [Header("移動アニメーション設定")]
        public Vector3 startPosition;                   // 開始位置
        public Vector3 endPosition;                     // 終了位置
        public bool useCurrentPositionAsStart;          // 現在の位置を開始位置として使用
        public bool useCurrentPositionAsEnd;            // 現在の位置を終了位置として使用
        
        [Header("スケールアニメーション設定")]
        public Vector3 startScale = Vector3.one;        // 開始スケール
        public Vector3 endScale = Vector3.one;          // 終了スケール
        public bool useCurrentScaleAsStart;             // 現在のスケールを開始として使用
        public bool useCurrentScaleAsEnd;               // 現在のスケールを終了として使用
        
        [Header("フェードアニメーション設定")]
        [Range(0f, 1f)] public float startAlpha = 1f;   // 開始アルファ
        [Range(0f, 1f)] public float endAlpha = 1f;     // 終了アルファ
        
        // 実行時に保存される値
        [HideInInspector] public Vector3 runtimeStartPosition;
        [HideInInspector] public Vector3 runtimeEndPosition;
        [HideInInspector] public Vector3 runtimeStartScale;
        [HideInInspector] public Vector3 runtimeEndScale;
        [HideInInspector] public CanvasGroup canvasGroup;
        [HideInInspector] public Image image;
        [HideInInspector] public bool isInitialized;
    }
    
    public enum AnimationType
    {
        SlideIn,        // スライドイン（位置移動）
        ScaleIn,        // スケールイン（拡大）
        Fade,           // フェード（アルファ変更）
        SlideAndFade,   // スライド＋フェード
        ScaleAndFade    // スケール＋フェード
    }
    
    [Header("入場アニメーションステップ（上から順に実行）")]
    [SerializeField] private AnimationStep[] enterAnimationSteps = new AnimationStep[0];
    
    [Header("退場アニメーションステップ（上から順に実行）")]
    [SerializeField] private AnimationStep[] exitAnimationSteps = new AnimationStep[0];

    private Coroutine _currentAnimationCoroutine;
    
    public event System.Action OnEnterAnimationCompleted;
    public event System.Action OnExitAnimationCompleted;

    private void Awake()
    {
        InitializeAnimationSteps(enterAnimationSteps);
        InitializeAnimationSteps(exitAnimationSteps);
    }

    private void OnEnable()
    {
        PlayEnterAnimation();
    }

    /// <summary>
    /// アニメーションステップの初期化
    /// </summary>
    private void InitializeAnimationSteps(AnimationStep[] steps)
    {
        foreach (var step in steps)
        {
            if (step.targetObject == null)
            {
                Debug.LogWarning($"AnimationStep '{step.stepName}' のtargetObjectが設定されていません。");
                continue;
            }
            
            // 現在の位置・スケールを保存
            var transform = step.targetObject.transform;
            
            // 開始位置の設定
            step.runtimeStartPosition = step.useCurrentPositionAsStart 
                ? transform.localPosition 
                : step.startPosition;
            
            // 終了位置の設定
            step.runtimeEndPosition = step.useCurrentPositionAsEnd 
                ? transform.localPosition 
                : step.endPosition;
            
            // 開始スケールの設定
            step.runtimeStartScale = step.useCurrentScaleAsStart 
                ? transform.localScale 
                : step.startScale;
            
            // 終了スケールの設定
            step.runtimeEndScale = step.useCurrentScaleAsEnd 
                ? transform.localScale 
                : step.endScale;
            
            // フェード用コンポーネントの取得
            if (step.type == AnimationType.Fade || 
                step.type == AnimationType.SlideAndFade || 
                step.type == AnimationType.ScaleAndFade)
            {
                step.canvasGroup = step.targetObject.GetComponent<CanvasGroup>();
                if (step.canvasGroup == null)
                {
                    // CanvasGroupがなければ追加
                    step.canvasGroup = step.targetObject.AddComponent<CanvasGroup>();
                }
                
                // Imageコンポーネントも取得（オプション）
                step.image = step.targetObject.GetComponent<Image>();
            }
            
            step.isInitialized = true;
        }
    }

    /// <summary>
    /// 入場アニメーションを再生
    /// </summary>
    public void PlayEnterAnimation()
    {
        if (_currentAnimationCoroutine != null)
        {
            StopCoroutine(_currentAnimationCoroutine);
        }
        
        ResetToStartState(enterAnimationSteps);
        _currentAnimationCoroutine = StartCoroutine(PlayEnterSequence());
    }
    
    /// <summary>
    /// 退場アニメーションを再生
    /// </summary>
    public void PlayExitAnimation()
    {
        if (_currentAnimationCoroutine != null)
        {
            StopCoroutine(_currentAnimationCoroutine);
        }
        
        ResetToStartState(exitAnimationSteps);
        _currentAnimationCoroutine = StartCoroutine(PlayExitSequence());
    }

    /// <summary>
    /// すべての要素を開始状態にリセット
    /// </summary>
    private void ResetToStartState(AnimationStep[] steps)
    {
        foreach (var step in steps)
        {
            if (step.targetObject == null || !step.isInitialized) continue;
            
            var transform = step.targetObject.transform;
            
            // 位置・スケール関連のアニメーション
            if (step.type == AnimationType.SlideIn || 
                step.type == AnimationType.SlideAndFade)
            {
                transform.localPosition = step.runtimeStartPosition;
            }
            
            if (step.type == AnimationType.ScaleIn || 
                step.type == AnimationType.ScaleAndFade)
            {
                transform.localScale = step.runtimeStartScale;
            }
            
            // フェード関連のアニメーション
            if (step.type == AnimationType.Fade || 
                step.type == AnimationType.SlideAndFade || 
                step.type == AnimationType.ScaleAndFade)
            {
                if (step.canvasGroup != null)
                {
                    step.canvasGroup.alpha = step.startAlpha;
                }
            }
        }
    }
    
    /// <summary>
    /// 入場アニメーションシーケンス
    /// </summary>
    private IEnumerator PlayEnterSequence()
    {
        yield return PlayAnimationSequence(enterAnimationSteps);
        OnEnterAnimationCompleted?.Invoke();
    }
    
    /// <summary>
    /// 退場アニメーションシーケンス
    /// </summary>
    private IEnumerator PlayExitSequence()
    {
        yield return PlayAnimationSequence(exitAnimationSteps);
        OnExitAnimationCompleted?.Invoke();
    }
    
    /// <summary>
    /// アニメーションシーケンスを順番に再生
    /// </summary>
    private IEnumerator PlayAnimationSequence(AnimationStep[] steps)
    {
        Tween lastTween = null;
        
        foreach (var step in steps)
        {
            if (step.targetObject == null || !step.isInitialized)
            {
                Debug.LogWarning($"Skipping step '{step.stepName}' - target object is null or not initialized");
                continue;
            }
            
            // 待機
            yield return new WaitForSeconds(step.delay);
            
            // アニメーション再生
            var stepTransform = step.targetObject.transform;
            
            switch (step.type)
            {
                case AnimationType.SlideIn:
                    lastTween = stepTransform.DOLocalMove(step.runtimeEndPosition, step.duration)
                        .SetEase(step.easeType);
                    break;
                    
                case AnimationType.ScaleIn:
                    lastTween = stepTransform.DOScale(step.runtimeEndScale, step.duration)
                        .SetEase(step.easeType);
                    break;
                    
                case AnimationType.Fade:
                    if (step.canvasGroup != null)
                    {
                        lastTween = step.canvasGroup.DOFade(step.endAlpha, step.duration)
                            .SetEase(step.easeType);
                    }
                    break;
                    
                case AnimationType.SlideAndFade:
                    stepTransform.DOLocalMove(step.runtimeEndPosition, step.duration)
                        .SetEase(step.easeType);
                    if (step.canvasGroup != null)
                    {
                        lastTween = step.canvasGroup.DOFade(step.endAlpha, step.duration)
                            .SetEase(step.easeType);
                    }
                    break;
                    
                case AnimationType.ScaleAndFade:
                    stepTransform.DOScale(step.runtimeEndScale, step.duration)
                        .SetEase(step.easeType);
                    if (step.canvasGroup != null)
                    {
                        lastTween = step.canvasGroup.DOFade(step.endAlpha, step.duration)
                            .SetEase(step.easeType);
                    }
                    break;
            }
        }
        
        // 最後のTweenが完了するまで待機
        if (lastTween != null)
        {
            yield return lastTween.WaitForCompletion();
        }
        
        _currentAnimationCoroutine = null;
    }
    
    /// <summary>
    /// 入場アニメーションの終了状態に即座に移行（デバッグ用）
    /// </summary>
    public void SkipToEnterEnd()
    {
        if (_currentAnimationCoroutine != null)
        {
            StopCoroutine(_currentAnimationCoroutine);
            _currentAnimationCoroutine = null;
        }
        
        DOTween.Kill(transform);
        SetToEndState(enterAnimationSteps);
    }
    
    /// <summary>
    /// 退場アニメーションの終了状態に即座に移行（デバッグ用）
    /// </summary>
    public void SkipToExitEnd()
    {
        if (_currentAnimationCoroutine != null)
        {
            StopCoroutine(_currentAnimationCoroutine);
            _currentAnimationCoroutine = null;
        }
        
        DOTween.Kill(transform);
        SetToEndState(exitAnimationSteps);
    }
    
    /// <summary>
    /// すべての要素を終了状態にセット
    /// </summary>
    private void SetToEndState(AnimationStep[] steps)
    {
        foreach (var step in steps)
        {
            if (step.targetObject == null || !step.isInitialized) continue;
            
            var transform = step.targetObject.transform;
            
            if (step.type == AnimationType.SlideIn || 
                step.type == AnimationType.SlideAndFade)
            {
                transform.localPosition = step.runtimeEndPosition;
            }
            
            if (step.type == AnimationType.ScaleIn || 
                step.type == AnimationType.ScaleAndFade)
            {
                transform.localScale = step.runtimeEndScale;
            }
            
            if (step.type == AnimationType.Fade || 
                step.type == AnimationType.SlideAndFade || 
                step.type == AnimationType.ScaleAndFade)
            {
                if (step.canvasGroup != null)
                {
                    step.canvasGroup.alpha = step.endAlpha;
                }
            }
        }
    }
    
    /// <summary>
    /// 入場アニメーション完了イベントをクリア
    /// </summary>
    public void ClearEnterAnimationCompletedEvent()
    {
        OnEnterAnimationCompleted = null;
    }
    
    /// <summary>
    /// 退場アニメーション完了イベントをクリア
    /// </summary>
    public void ClearExitAnimationCompletedEvent()
    {
        OnExitAnimationCompleted = null;
    }
    
    /// <summary>
    /// すべてのアニメーション完了イベントをクリア
    /// </summary>
    public void ClearAllAnimationEvents()
    {
        OnEnterAnimationCompleted = null;
        OnExitAnimationCompleted = null;
    }
}
