﻿using UnityEngine;

/// <summary>
/// ゲームデータを管理するクラス
/// 各フェーズ間でのデータ共有を担当
/// </summary>
public class GameData
{
    public const int TotalTimeLimit = 40;
    public int CheckCount = 0;
    public float RemainingTime = TotalTimeLimit;
    
    public int Score()
    {
        if (RemainingTime <= 0) return 0;
        return CheckCount + (int)RemainingTime;
    }
}

