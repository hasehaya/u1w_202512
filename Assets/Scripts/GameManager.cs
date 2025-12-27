using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Managers")]
    [SerializeField] private UIManager uiManager;
    [SerializeField] private SleepPhaseController sleepController;
    [SerializeField] private RunPhaseController runController;

    [Header("Game Settings")]
    public float MinTime = 15f;
    public float MaxTime = 25f;

    // Game Data
    public float TotalTimeLimit { get; private set; }
    public float StartTime { get; private set; }
    public float SleepDuration { get; private set; } // 寝ていた時間
    public float RemainingTime { get; private set; } // 残り時間

    public GameState CurrentState { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        ChangeState(GameState.Title);
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        uiManager.SwitchPanel(newState);

        switch (newState)
        {
            case GameState.Sleep:
                StartSleepPhase();
                break;
            case GameState.Run:
                StartRunPhase();
                break;
            case GameState.Result:
                // リザルト表示処理はResultView側で行うか、ここで計算して渡す
                uiManager.ShowResult(SleepDuration, RemainingTime, CalculateScore());
                break;
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
}