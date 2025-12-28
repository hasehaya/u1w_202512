public enum GameState
{
    Title,
    Loading,
    Prologue,
    Tutorial,
    Sleep,
    Run,
    GameClear,
    GameOver
}

public enum SwipeDirection
{
    None,
    Up,
    Down,
    Left,
    Right
}

public enum ObstaclePosition
{
    Left,
    Right
}

public enum BGMType
{
    None = 0,
    Title = 1,
    Prologue = 2,
    Tutorial = 3,
    Sleep = 4,
    Run = 5,
    GameClear = 6,
    GameOver = 7
}

public enum SeType
{
    None = 0,
    ButtonClick = 1,
    Swipe = 2,
    Collision = 3,
    Attention = 4,
    Run = 5,
    Train = 6,
    Gaba = 7,
    Alarm = 8,
}

