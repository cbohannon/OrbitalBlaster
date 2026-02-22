using Godot;

/// <summary>
/// Autoload singleton that carries player-selected settings across scene changes.
/// </summary>
public partial class GameSettings : Node
{
    public static int StartingWave { get; set; } = 1;
}
