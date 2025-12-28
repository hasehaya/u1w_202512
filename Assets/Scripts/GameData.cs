public class GameData
{
    public const int TotalTimeLimit = 1800; // 30分
    public int CheckCount = 0;
    public float SleepTime = 0;
    public float RemainingTime = TotalTimeLimit;
    
    public int Score()
    {
        if (RemainingTime <= 0) return 0;
        return 1000 + (int)SleepTime * 2 + (int)RemainingTime - CheckCount * 300;
    }
    
    public string Rank()
    {
        int score = Score();
        if (score >= 2700) return "S";
        if (score >= 2300) return "A";
        if (score >= 1800) return "B";
        return "C";
    }
    
    public void Reset()
    {
        CheckCount = 0;
        SleepTime = 0;
        RemainingTime = TotalTimeLimit;
    }
}

