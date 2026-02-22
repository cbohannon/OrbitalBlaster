using Godot;

/// <summary>
/// Autoload singleton that carries player-selected settings across scene changes
/// and manages persistent high score storage.
/// </summary>
public partial class GameSettings : Node
{
    public static int StartingWave { get; set; } = 1;
    public static int HighScore    { get; private set; } = 0;

    private const string SavePath = "user://highscore.dat";

    public override void _Ready()
    {
        LoadHighScore();
    }

    public static bool TrySaveHighScore(int score)
    {
        if (score <= HighScore) return false;

        HighScore = score;
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
        file?.Store32((uint)HighScore);
        return true; // signals that a new record was set
    }

    private static void LoadHighScore()
    {
        if (!FileAccess.FileExists(SavePath)) return;
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
        if (file != null)
            HighScore = (int)file.Get32();
    }
}
