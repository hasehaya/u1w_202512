﻿using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// タイトル画面フェーズコントローラー
/// 責務: タイトル画面のロジック管理
/// </summary>
public class TitlePhaseController : PhaseController
{
    [SerializeField] private Button startButton;

    public override GameState PhaseType => GameState.Title;

    protected override void OnEnterImpl()
    {
        AudioManager.Instance.PlayBGM(BGMType.Title);
        
        if (startButton != null)
        {
            startButton.onClick.AddListener(GameStart);
        }
    }

    public override void UpdatePhase()
    {
     
    }

    protected override void OnExitImpl()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(GameStart);
        }
    }
    
    /// <summary>
    /// スタートボタンクリック時
    /// </summary>
    private void GameStart()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySe(SeType.ButtonClick);
        }
        
        RequestTransitionTo(GameState.Loading);
    }
}
