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
    [SerializeField] private SetTimerPhaseController setTimerController;
    [SerializeField] private SleepPhaseController sleepController;
    [SerializeField] private RunPhaseController runController;
    [SerializeField] private GameClearPhaseController gameClearController;
    [SerializeField] private GameOverPhaseController gameOverController;

    // ゲームデータ
    private GameData gameData;
    public GameData Data => gameData;

    // フェーズ管理
    public GameState CurrentState { get; private set; }
    private PhaseController currentPhaseController;
    private Dictionary<GameState, PhaseController> phaseControllers;

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
        gameData = new GameData();
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

    private void InitializePhaseControllers()
    {
        phaseControllers = new Dictionary<GameState, PhaseController>
        {
            { GameState.Title, titleController },
            { GameState.Loading, loadingController },
            { GameState.Prologue, prologueController },
            { GameState.Tutorial, tutorialController },
            { GameState.SetTimer, setTimerController },
            { GameState.Sleep, sleepController },
            { GameState.Run, runController },
            { GameState.GameClear, gameClearController },
            { GameState.GameOver, gameOverController }
        };
        
        HideAllPhaseControllers();
    }

    private void HideAllPhaseControllers()
    {
        foreach (var controller in phaseControllers.Values)
        {
            if (controller != null)
            {
                controller.gameObject.SetActive(false);
            }
        }
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
    /// フェーズ遷移をリクエスト
    /// </summary>
    public void RequestPhaseTransition(GameState newState)
    {
        ChangeState(newState);
    }
}