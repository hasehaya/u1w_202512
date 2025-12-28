using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// チュートリアルフェーズコントローラー
/// 責務: ゲームの操作方法を教えるフェーズの管理
/// </summary>
public class TutorialPhaseController : PhaseController
{
    [SerializeField] private Sprite[] tutorialImages;
    [SerializeField] private Sprite[] playImages;
    [SerializeField] private Image tutorialImage;
    [SerializeField] private Image playImage;
    private int currentIndex;

    public override GameState PhaseType => GameState.Tutorial;

    protected override void OnEnterImpl()
    {   
        InputManager.Instance.OnTap += OnScreenTapped;
        currentIndex = 0;
        tutorialImage.sprite = tutorialImages[currentIndex];
        playImage.sprite = playImages[currentIndex];
    }

    public override void UpdatePhase()
    {
        
    }

    protected override void OnExitImpl()
    {
        InputManager.Instance.OnTap -= OnScreenTapped;
    }

    private void OnScreenTapped()
    {
        currentIndex++;
        if (currentIndex < tutorialImages.Length)
        {
            tutorialImage.sprite = tutorialImages[currentIndex];
            playImage.sprite = playImages[currentIndex];
            
        }
        else
        {
            CompleteTutorial();
        }
    }

    private void CompleteTutorial()
    {
        RequestTransitionTo(GameState.Sleep);
    }
    
}


