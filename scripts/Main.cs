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

    private PackedScene _asteroidScene;
    private Node2D _gameWorld;

    public override void _Ready()
    {
        _lives = StartingLives;

        _scoreLabel = GetNode<Label>("HUD/ScoreLabel");
        _livesLabel = GetNode<Label>("HUD/LivesLabel");
        _waveLabel  = GetNode<Label>("HUD/WaveLabel");

        _asteroidScene = GD.Load<PackedScene>("res://scenes/Asteroid.tscn");
        _gameWorld     = GetNode<Node2D>("GameWorld");

        GetNode<Timer>("SpawnTimer").Timeout += SpawnAsteroid;

        UpdateHUD();
    }

    // -------------------------------------------------------------------------
    // Input — handle clicks centrally to avoid Control node interference
    // -------------------------------------------------------------------------

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent &&
            mouseEvent.ButtonIndex == MouseButton.Left &&
            mouseEvent.Pressed)
        {
            foreach (var child in _gameWorld.GetChildren())
            {
                if (child is Asteroid asteroid &&
                    asteroid.Position.DistanceTo(mouseEvent.Position) <= 35f)
                {
                    asteroid.TakeHit();
                    return;
                }
            }
        }
    }

    // -------------------------------------------------------------------------
    // Spawning
    // -------------------------------------------------------------------------

    private void SpawnAsteroid()
    {
        var asteroid = _asteroidScene.Instantiate<Asteroid>();
        asteroid.Position = new Vector2((float)GD.RandRange(50, 1230), -30f);
        _gameWorld.AddChild(asteroid);
    }

    // -------------------------------------------------------------------------
    // Public API — called by game objects (Asteroid, etc.)
    // -------------------------------------------------------------------------

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

    // -------------------------------------------------------------------------
    // Internal
    // -------------------------------------------------------------------------

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
