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
    
    public void Reset()
    {
        CheckCount = 0;
        RemainingTime = TotalTimeLimit;
    }
}

