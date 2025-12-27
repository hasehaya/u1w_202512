using UnityEngine;

public static class Utilities
{
    public static int ConvertSec2Min(this float seconds)
    {
        return Mathf.FloorToInt(seconds / 60f);
    }
}
