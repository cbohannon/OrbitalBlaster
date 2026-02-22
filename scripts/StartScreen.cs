using Godot;

public partial class StartScreen : Control
{
    public override void _Ready()
    {
        GetNode<Button>("DefaultButton").Pressed  += () => StartGame(1);
        GetNode<Button>("AdvancedButton").Pressed += () => StartGame(5);
        GetNode<Button>("HardcoreButton").Pressed += () => StartGame(10);
    }

    private void StartGame(int startingWave)
    {
        GameSettings.StartingWave = startingWave;
        GetTree().ChangeSceneToFile("res://scenes/Main.tscn");
    }
}
