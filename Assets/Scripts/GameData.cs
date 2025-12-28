public class GameData
{
    public const int TotalTimeLimit = 1800; // 30分
    public int CheckCount = 0;
    public float RemainingTime = TotalTimeLimit;
    
    public int Score()
    {
        if (RemainingTime <= 0) return 0;
        return 1000 + (int)RemainingTime * 2 - CheckCount * 300;
    }
    
    public string Rank()
    {
        int score = Score();
        if (score >= 3000) return "S";
        if (score >= 2500) return "A";
        if (score >= 2000) return "B";
        return "C";
    }
    
    public void Reset()
    {
        CheckCount = 0;
        RemainingTime = TotalTimeLimit;
    }
}

