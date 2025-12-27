using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ゲーム全体の状態管理とフェーズ遷移を担当
/// 責務: フェーズの切り替え、ゲームデータの管理、UIパネル管理
/// </summary>
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

    [Header("Game Settings")]
    [SerializeField] private float minTime = 15f;
    [SerializeField] private float maxTime = 25f;

    // ゲームデータ
    private GameData gameData;
    public GameData Data => gameData;

    // フェーズ管理
    public GameState CurrentState { get; private set; }
    private PhaseController currentPhaseController;
    private Dictionary<GameState, PhaseController> phaseControllers;
    private Dictionary<GameState, GameObject> panelMap;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        InitializeGameData();
        InitializePhaseControllers();
    }
    
    private void Start()
    {
        ChangeState(GameState.Title);
    }

    private void Update()
    {
        UpdateCurrentPhase();
    }

    private void InitializeGameData()
    {
        gameData = new GameData
        {
            MinTime = minTime,
            MaxTime = maxTime
        };
    }

    private void InitializePhaseControllers()
    {
        phaseControllers = new Dictionary<GameState, PhaseController>
        {
            { GameState.Title, titleController },
            { GameState.Loading, loadingController },
            { GameState.Prologue, prologueController },
            { GameState.Tutorial, tutorialController },
            { GameState.Sleep, sleepController },
            { GameState.Run, runController },
            { GameState.Result, resultController }
        };
    }

    private void UpdateCurrentPhase()
    {
        if (currentPhaseController != null && currentPhaseController.IsActive)
        {
            currentPhaseController.UpdatePhase();
        }
    }

    /// <summary>
    /// ゲームステートを変更
    /// </summary>
    public void ChangeState(GameState newState)
    {
        ExitCurrentPhase();

        CurrentState = newState;

        // UIパネルを切り替え
        SwitchPanel(newState);

        // フェーズ固有のデータ処理
        ProcessPhaseData(newState);

        // 新しいフェーズを開始
        EnterNewPhase(newState);
    }

    private void ExitCurrentPhase()
    {
        if (currentPhaseController != null)
        {
            currentPhaseController.OnPhaseExit();
        }
    }

    private void EnterNewPhase(GameState newState)
    {
        if (phaseControllers.TryGetValue(newState, out PhaseController controller))
        {
            currentPhaseController = controller;
            currentPhaseController?.OnPhaseEnter();
        }
    }

    /// <summary>
    /// UIパネルを切り替え
    /// </summary>
    private void SwitchPanel(GameState state)
    {
        // すべてのパネルを非表示
        foreach (var panel in panelMap.Values)
        {
            if (panel != null)
                panel.SetActive(false);
        }

        // 対象のパネルのみ表示
        if (panelMap.TryGetValue(state, out GameObject targetPanel) && targetPanel != null)
        {
            targetPanel.SetActive(true);
        }
    }

    /// <summary>
    /// フェーズ固有のデータ処理
    /// </summary>
    private void ProcessPhaseData(GameState newState)
    {
        switch (newState)
        {
            case GameState.Sleep:
                gameData.StartSleep();
                sleepController.SetTimeLimit(gameData.TotalTimeLimit);
                break;

            case GameState.Run:
                gameData.EndSleep();
                if (gameData.IsOverslept)
                {
                    // 寝過ごした場合は即リザルトへ
                    gameData.SetGameOver();
                    ChangeState(GameState.Result);
                    return;
                }
                runController.SetRemainingTime(gameData.RemainingTime);
                break;

            case GameState.Result:
                resultController.SetResultData(gameData.SleepDuration, gameData.RemainingTime, gameData.Score);
                break;
        }
    }

    /// <summary>
    /// ゲームクリア処理
    /// </summary>
    public void HandleGameClear(float finalRemainingTime)
    {
        gameData.SetGameClear(finalRemainingTime);
        ChangeState(GameState.Result);
    }

    /// <summary>
    /// ゲームオーバー処理
    /// </summary>
    public void HandleGameOver()
    {
        gameData.SetGameOver();
        ChangeState(GameState.Result);
    }

    /// <summary>
    /// フェーズ遷移をリクエスト
    /// </summary>
    public void RequestPhaseTransition(GameState newState)
    {
        ChangeState(newState);
    }
}