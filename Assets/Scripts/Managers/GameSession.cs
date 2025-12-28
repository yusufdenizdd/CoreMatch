using UnityEngine;

public class GameSession : Singleton<GameSession>
{
    public static bool HasSelection { get; set; } = false;
    public static bool IsEndless { get; set; } = false;
    public static int StartLevelIndex { get; set; } = 0;

    protected override void Init()
    {
        DontDestroyOnLoad(gameObject);
    }
}
