using UnityEngine;

/// <summary>
/// ゲームデータを管理するクラス
/// 各フェーズ間でのデータ共有を担当
/// </summary>
public class GameData
{
    public float MinTime { get; set; } = 15f;
    public float MaxTime { get; set; } = 25f;
    public float TotalTimeLimit { get; private set; }
    public float SleepStartTime { get; private set; }
    public float SleepDuration { get; private set; }
    public float RemainingTime { get; private set; }
    public int Score { get; private set; }

    /// <summary>
    /// Sleepフェーズ開始時のデータ初期化
    /// </summary>
    public void StartSleep()
    {
        TotalTimeLimit = Random.Range(MinTime, MaxTime);
        SleepStartTime = Time.time;
        SleepDuration = 0f;
        RemainingTime = TotalTimeLimit;
        Score = 0;
    }

    /// <summary>
    /// Sleepフェーズ終了時のデータ更新
    /// </summary>
    public void EndSleep()
    {
        SleepDuration = Time.time - SleepStartTime;
        float elapsed = Time.time - SleepStartTime;
        RemainingTime = Mathf.Max(0, TotalTimeLimit - elapsed);
    }

    /// <summary>
    /// ゲームクリア時のデータ更新
    /// </summary>
    public void SetGameClear(float finalRemainingTime)
    {
        RemainingTime = finalRemainingTime;
        Score = CalculateScore();
    }

    /// <summary>
    /// ゲームオーバー時のデータ更新
    /// </summary>
    public void SetGameOver()
    {
        RemainingTime = 0f;
        Score = 0;
    }

    /// <summary>
    /// スコア計算
    /// </summary>
    private int CalculateScore()
    {
        if (RemainingTime <= 0) return 0;
        return Mathf.FloorToInt((SleepDuration * 100) + (RemainingTime * 500));
    }

    /// <summary>
    /// 寝過ぎたかどうか
    /// </summary>
    public bool IsOverslept => RemainingTime <= 0;
}

