using Godot;

public partial class Main : Node2D
{
    [Export] public int StartingLives = 3;

    private int _score = 0;
    private int _lives;
    private int _wave = 1;

    private Label _scoreLabel;
    private Label _livesLabel;
    private Label _waveLabel;

    public override void _Ready()
    {
        _lives = StartingLives;

        _scoreLabel = GetNode<Label>("HUD/ScoreLabel");
        _livesLabel = GetNode<Label>("HUD/LivesLabel");
        _waveLabel  = GetNode<Label>("HUD/WaveLabel");

        UpdateHUD();
    }

    public void AddScore(int points)
    {
        _score += points;
        UpdateHUD();
    }

    public void LoseLife()
    {
        _lives--;
        UpdateHUD();

        if (_lives <= 0)
            GameOver();
    }

    public void StartNextWave()
    {
        _wave++;
        UpdateHUD();
    }

    private void GameOver()
    {
        GD.Print($"Game Over! Final score: {_score}");
        // TODO: Show game over screen
    }

    private void UpdateHUD()
    {
        _scoreLabel.Text = $"Score: {_score}";
        _livesLabel.Text = $"Lives: {_lives}";
        _waveLabel.Text  = $"Wave: {_wave}";
    }
}
