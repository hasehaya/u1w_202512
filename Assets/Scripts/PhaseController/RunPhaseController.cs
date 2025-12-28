using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;

/// <summary>
/// ラン（移動）フェーズコントローラー
/// 責務: プレイヤーが障害物を避けながら進むフェーズの管理
/// </summary>
public class RunPhaseController : PhaseController
{
    [Header("UI")]
    [SerializeField] private RectTransform roadObject;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Button tapButton;

    [Header("Player")]
    [SerializeField] private Player player;

    [Header("Obstacle")]
    [SerializeField] private ObstacleManager obstacleManager;

    [Header("Fade")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 3f;

    [Header("Settings")]
    [SerializeField] private float requiredClicks;
    [SerializeField] private float safeTimeDuration;
    [SerializeField] private int minObstacles;
    [SerializeField] private int maxObstacles;
    [SerializeField] private float collisionPenaltyDuration;

    private float progress;
    private float progressIncrement;
    private List<float> obstacleTriggers = new List<float>();
    private bool isInSafeTime;
    private bool isInputLocked;
    private float inputLockTimer;
    private bool isTransitioning;
    private Tween roadShakeTween;

    public override GameState PhaseType => GameState.Run;

    private void Start()
    {
        SubscribeToInputEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromInputEvents();
    }

    private void SubscribeToInputEvents()
    {
        if (tapButton != null)
        {
            tapButton.onClick.AddListener(HandleTap);
        }
        
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnSwipe += HandleSwipe;
        }
        
        if (obstacleManager != null)
        {
            obstacleManager.OnObstacleDisplayed += HandleObstacleDisplayed;
        }
    }

    private void UnsubscribeFromInputEvents()
    {
        if (tapButton != null)
        {
            tapButton.onClick.RemoveListener(HandleTap);
        }
        
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnSwipe -= HandleSwipe;
        }
        
        if (obstacleManager != null)
        {
            obstacleManager.OnObstacleDisplayed -= HandleObstacleDisplayed;
        }
    }

    protected override void OnEnterImpl()
    {
        progress = 0;
        isInSafeTime = false;
        isInputLocked = false;
        inputLockTimer = 0f;
        isTransitioning = false;
        progressIncrement = 1f / requiredClicks;

        // フェードImageを透明にして非表示
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
            fadeImage.gameObject.SetActive(false);
        }

        if (progressBar != null)
            progressBar.value = 0;

        if (player != null)
        {
            player.Initialize();
            player.ResetPosition();
        }
            

        if (tapButton != null)
            tapButton.interactable = true;

        GenerateObstacleTriggers();
        StartRoadShake();
    }
    
    /// <summary>
    /// 道路オブジェクトを小刻みに揺らすアニメーションを開始
    /// </summary>
    private void StartRoadShake()
    {
        if (roadObject == null) return;
        
        // 既存のアニメーションがあればキャンセル
        roadShakeTween?.Kill();
        
        roadObject.anchoredPosition = new Vector2(0, 809);
        
        // 小刻みな揺れ（RectTransformにはDOShakeAnchorPosを使用）
        roadShakeTween = roadObject
            .DOShakeAnchorPos(
                duration: 2f,
                strength: 10f, // 揺れの強さ
                vibrato: 5, // 揺れの頻度（高いほど細かく揺れる）
                randomness: 90, // ランダム性
                snapping: false,
                fadeOut: false
            )
            .SetLoops(-1, LoopType.Restart) // 無限ループ
            .SetUpdate(true);
    }
    
    /// <summary>
    /// 道路オブジェクトの揺れアニメーションを停止
    /// </summary>
    private void StopRoadShake()
    {
        roadShakeTween?.Kill();
        roadShakeTween = null;
        
        if (roadObject != null)
        {
            roadObject.anchoredPosition = Vector2.zero;
        }
    }

    public override void UpdatePhase()
    {
        if(player != null) 
            player.UpdatePlayer();
        
        // 遷移中はタイマーを停止
        if (!isTransitioning)
        {
            GameManager.Instance.Data.RemainingTime -= 60 * Time.deltaTime;
        }
        
        UpdateTimerUI();
        CheckCollision();
        UpdateInputLock();

        if (!isTransitioning && GameManager.Instance.Data.RemainingTime <= 0)
        {
            TransitionToGameOver();
        }
    }

    protected override void OnExitImpl()
    {
        if(player != null)
            player.OnExit();
        
        StopRoadShake();
        
        progress = 0;
        obstacleTriggers.Clear();
        CancelInvoke();
    }


    private void HandleTap()
    {
        if (!IsActive || isInputLocked || isTransitioning) return;

        // ランニングSEを再生
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySe(SeType.Run);
        }

        // プレイヤーの画像を切り替え
        if (player != null)
        {
            player.SwitchSprite();
        }

        AddProgress();
        CheckObstacleTrigger();
        CheckClear();
    }

    private void HandleSwipe(SwipeDirection swipeDir)
    {
        if (!IsActive) return;
        
        // プレイヤーの移動はPlayerControllerが自動で処理
        // ここでは何もしない（衝突判定はUpdatePhaseで行う）
    }
    
    /// <summary>
    /// 障害物が実際に画面に表示された時に呼ばれる
    /// </summary>
    private void HandleObstacleDisplayed()
    {
        // セーフタイムを開始
        isInSafeTime = true;
        
        // 一定時間後にセーフタイムを終了
        Invoke(nameof(EndSafeTime), safeTimeDuration);
        
        // 障害物の削除は別途管理（セーフタイムとは独立）
        // 必要に応じて障害物のアニメーション時間を考慮した削除を追加可能
        Invoke(nameof(DespawnObstacle), safeTimeDuration + 0.5f);
    }

    private void CheckCollision()
    {
        if (player == null || obstacleManager == null) return;
        if (!obstacleManager.IsActive || isInSafeTime || isInputLocked) return;

        // プレイヤーと障害物の位置を比較
        PlayerPosition playerPos = player.CurrentPosition;
        ObstaclePosition obstaclePos = obstacleManager.CurrentPosition;

        bool isCollision = false;

        if (obstaclePos == ObstaclePosition.Left && playerPos != PlayerPosition.Right)
        {
            isCollision = true;
        }
        else if (obstaclePos == ObstaclePosition.Right && playerPos != PlayerPosition.Left)
        {
            isCollision = true;
        }

        if (isCollision)
        {
            HandleCollision();
        }
    }

    private void AddProgress()
    {
        progress += progressIncrement;
        
        if (progressBar != null)
            progressBar.value = progress;
    }

    private void CheckObstacleTrigger()
    {
        if (obstacleTriggers.Count > 0 && progress >= obstacleTriggers[0])
        {
            SpawnObstacle();
            obstacleTriggers.RemoveAt(0);
        }
    }

    private void CheckClear()
    {
        if (progress >= 1f)
        {
            TransitionToGameClear();
        }
    }

    private void GenerateObstacleTriggers()
    {
        obstacleTriggers.Clear();
        int count = Random.Range(minObstacles, maxObstacles);
        float start = 0.1f;
        float end = 0.9f;
        float step = (end - start) / count;

        for (int i = 0; i < count; i++)
        {
            float triggerPoint = start + (step * i) + Random.Range(0, step * 0.8f);
            obstacleTriggers.Add(triggerPoint);
        }
    }

    private void SpawnObstacle()
    {
        if (obstacleManager == null) return;

        // 0: Left Start, 1: Left End, 2: Right Start, 3: Right End
        int posIndex = Random.Range(0, 4);
        ObstaclePosition pos = (posIndex < 2) ? ObstaclePosition.Left : ObstaclePosition.Right;
        
        // 障害物生成をリクエスト（キューに追加される可能性がある）
        obstacleManager.Spawn(pos);
        
        // セーフタイムの設定は HandleObstacleDisplayed() で行う
        // 障害物の削除タイミングの設定もそちらで行う
    }

    /// <summary>
    /// セーフタイムを終了
    /// </summary>
    private void EndSafeTime()
    {
        isInSafeTime = false;
    }

    /// <summary>
    /// 障害物を削除
    /// </summary>
    private void DespawnObstacle()
    {
        if (obstacleManager != null)
        {
            obstacleManager.Hide();
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = Mathf.Max(0, (int)GameManager.Instance.Data.RemainingTime / 60).ToString();
    }

    /// <summary>
    /// 衝突時の処理：ボタンを無効化し、プレイヤーを赤く明滅
    /// </summary>
    private void HandleCollision()
    {
        // 衝突SEを再生
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySe(SeType.Collision);
        }
        
        // 入力をロック
        isInputLocked = true;
        inputLockTimer = collisionPenaltyDuration;
        
        // ボタンを無効化
        if (tapButton != null)
        {
            tapButton.interactable = false;
        }

        // プレイヤーの明滅エフェクト
        if (player != null)
        {
            player.PlayDamageEffect();
        }

        // セーフタイムを設定（連続で当たり判定が発生しないようにする）
        isInSafeTime = true;
        
        // 障害物を削除
        if (obstacleManager != null)
        {
            obstacleManager.Hide();
        }
    }

    /// <summary>
    /// 入力ロックタイマーを更新し、時間経過でロック解除
    /// </summary>
    private void UpdateInputLock()
    {
        if (isInputLocked)
        {
            inputLockTimer -= Time.deltaTime;

            if (inputLockTimer <= 0)
            {
                isInputLocked = false;
                inputLockTimer = 0f;
                
                // セーフタイムも解除
                isInSafeTime = false;

                // ボタンを再有効化
                if (tapButton != null)
                {
                    tapButton.interactable = true;
                }
            }
        }
    }

    /// <summary>
    /// ゲームクリア時のフェード付き遷移
    /// </summary>
    private void TransitionToGameClear()
    {
        if (isTransitioning) return;
        
        isTransitioning = true;
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySe(SeType.Train);
        }
        // ボタンを無効化
        if (tapButton != null)
        {
            tapButton.interactable = false;
        }

        // フェードアウトしてから遷移
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
            fadeImage.DOFade(1f, fadeDuration).OnComplete(() =>
            {
                GameManager.Instance.RequestPhaseTransition(GameState.GameClear);
            });
        }
        else
        {
            // フェードImageが設定されていない場合は即座に遷移
            GameManager.Instance.RequestPhaseTransition(GameState.GameClear);
        }
    }

    /// <summary>
    /// ゲームオーバー時のフェード付き遷移
    /// </summary>
    private void TransitionToGameOver()
    {
        if (isTransitioning) return;
        
        isTransitioning = true;
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySe(SeType.Train);
        }
        // ボタンを無効化
        if (tapButton != null)
        {
            tapButton.interactable = false;
        }

        // フェードアウトしてから遷移
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
            fadeImage.DOFade(1f, fadeDuration).OnComplete(() =>
            {
                GameManager.Instance.RequestPhaseTransition(GameState.GameOver);
            });
        }
        else
        {
            // フェードImageが設定されていない場合は即座に遷移
            GameManager.Instance.RequestPhaseTransition(GameState.GameOver);
        }
    }
}
