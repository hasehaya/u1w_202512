using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Phase Controllers")]
    [SerializeField] private TitlePhaseController titleController;
    [SerializeField] private LoadingPhaseController loadingController;
    [SerializeField] private ProloguePhaseController prologueController;
    [SerializeField] private TutorialPhaseController tutorialController;
    [SerializeField] private SleepPhaseController sleepController;
    [SerializeField] private RunPhaseController runController;
    [SerializeField] private ResultPhaseController resultController;

    [Header("Managers")]
    [SerializeField] private UIManager uiManager;

    [Header("Game Settings")]
    public float MinTime = 15f;
    public float MaxTime = 25f;

    // Game Data
    public float TotalTimeLimit { get; private set; }
    public float StartTime { get; private set; }
    public float SleepDuration { get; private set; } // 寝ていた時間
    public float RemainingTime { get; private set; } // 残り時間

    public GameState CurrentState { get; private set; }
    private PhaseController currentPhaseController;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        ChangeState(GameState.Title);
    }

    private void Update()
    {
        if (currentPhaseController != null && currentPhaseController.IsActive)
        {
            currentPhaseController.UpdatePhase();
        }
    }

    public void ChangeState(GameState newState)
    {
        // 現在のフェーズを終了
        if (currentPhaseController != null)
        {
            currentPhaseController.OnPhaseExit();
        }

        CurrentState = newState;
        if (uiManager != null)
            uiManager.SwitchPanel(newState);

        switch (newState)
        {
            case GameState.Title:
                currentPhaseController = titleController;
                break;
            case GameState.Loading:
                currentPhaseController = loadingController;
                break;
            case GameState.Prologue:
                currentPhaseController = prologueController;
                break;
            case GameState.Tutorial:
                currentPhaseController = tutorialController;
                break;
            case GameState.Sleep:
                currentPhaseController = sleepController;
                StartSleepPhase();
                break;
            case GameState.Run:
                currentPhaseController = runController;
                StartRunPhase();
                break;
            case GameState.Result:
                currentPhaseController = resultController;
                resultController.DisplayResult(SleepDuration, RemainingTime, CalculateScore());
                break;
        }

        // 新しいフェーズを開始
        if (currentPhaseController != null)
        {
            currentPhaseController.OnPhaseEnter();
        }
    }

    private void StartSleepPhase()
    {
        TotalTimeLimit = Random.Range(MinTime, MaxTime);
        StartTime = Time.time;
        sleepController.Initialize(TotalTimeLimit);
    }

    private void StartRunPhase()
    {
        // 寝ていた時間を確定
        SleepDuration = Time.time - StartTime;
        
        // 残り時間を計算 (もし寝過ごしていたら0)
        float elapsed = Time.time - StartTime;
        RemainingTime = Mathf.Max(0, TotalTimeLimit - elapsed);

        if (RemainingTime <= 0)
        {
            // 即ゲームオーバー（または強制失敗のリザルトへ）
            ChangeState(GameState.Result);
            return;
        }

        runController.Initialize(RemainingTime);
    }

    public void GameClear(float finalRemainingTime)
    {
        RemainingTime = finalRemainingTime;
        ChangeState(GameState.Result);
    }

    public void GameOver()
    {
        RemainingTime = 0;
        ChangeState(GameState.Result);
    }

    private int CalculateScore()
    {
        if (RemainingTime <= 0) return 0;
        return Mathf.FloorToInt((SleepDuration * 100) + (RemainingTime * 500));
    }

    /// <summary>
    /// 現在のフェーズの一時停止
    /// </summary>
    public void PauseGame()
    {
        if (currentPhaseController != null)
        {
            currentPhaseController.Pause();
        }
    }

    /// <summary>
    /// 現在のフェーズの再開
    /// </summary>
    public void ResumeGame()
    {
        if (currentPhaseController != null)
        {
            currentPhaseController.Resume();
        }
    }
}