using Godot;

public partial class StartScreen : Control
{
    public override void _Ready()
    {
        GetNode<Button>("DefaultButton").Pressed  += () => StartGame(1);
        GetNode<Button>("AdvancedButton").Pressed += () => StartGame(5);
        GetNode<Button>("HardcoreButton").Pressed += () => StartGame(10);

        var highScoreLabel = GetNode<Label>("HighScoreLabel");
        if (GameSettings.HighScore > 0)
        {
            highScoreLabel.Text    = $"Best Score: {GameSettings.HighScore}";
            highScoreLabel.Visible = true;
        }
    }

    private void StartGame(int startingWave)
    {
        GameSettings.StartingWave = startingWave;
        GetTree().ChangeSceneToFile("res://scenes/Main.tscn");
    }
}
