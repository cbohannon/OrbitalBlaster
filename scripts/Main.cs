using Godot;

public partial class Main : Node2D
{
    [Export] public int StartingLives = 3;

    private int _score = 0;
    private int _lives;
    private int _wave = 1;
    private bool _gameOver = false;

    private Label _scoreLabel;
    private Label _livesLabel;
    private Label _waveLabel;
    private Label _waveAnnounceLabel;
    private CanvasLayer _gameOverScreen;
    private Label _finalScoreLabel;

    private PackedScene _asteroidScene;
    private Node2D _gameWorld;
    private Timer _spawnTimer;

    public override void _Ready()
    {
        _lives = StartingLives;

        _scoreLabel         = GetNode<Label>("HUD/ScoreLabel");
        _livesLabel         = GetNode<Label>("HUD/LivesLabel");
        _waveLabel          = GetNode<Label>("HUD/WaveLabel");
        _waveAnnounceLabel  = GetNode<Label>("HUD/WaveAnnounceLabel");
        _gameOverScreen     = GetNode<CanvasLayer>("GameOverScreen");
        _finalScoreLabel    = GetNode<Label>("GameOverScreen/FinalScoreLabel");

        GetNode<Button>("GameOverScreen/PlayAgainButton").Pressed += OnPlayAgainPressed;

        _asteroidScene = GD.Load<PackedScene>("res://scenes/Asteroid.tscn");
        _gameWorld     = GetNode<Node2D>("GameWorld");
        _spawnTimer    = GetNode<Timer>("SpawnTimer");

        _spawnTimer.Timeout                    += SpawnAsteroid;
        GetNode<Timer>("WaveTimer").Timeout    += OnWaveTimerTimeout;

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
    // Wave management
    // -------------------------------------------------------------------------

    private void OnWaveTimerTimeout()
    {
        if (_gameOver) return;
        StartNextWave();
    }

    public void StartNextWave()
    {
        _wave++;
        UpdateHUD();

        // Shrink spawn interval each wave, floor at 0.5s
        _spawnTimer.WaitTime = Mathf.Max(0.5f, _spawnTimer.WaitTime - 0.25f);

        ShowWaveAnnouncement();
    }

    private async void ShowWaveAnnouncement()
    {
        _waveAnnounceLabel.Text     = $"Wave {_wave}";
        _waveAnnounceLabel.Modulate = Colors.White;
        _waveAnnounceLabel.Visible  = true;

        // Fade out over 1.5s after a short pause
        var tween = CreateTween();
        tween.TweenProperty(_waveAnnounceLabel, "modulate:a", 0f, 1.5f)
             .SetDelay(0.5f);

        await ToSignal(tween, Tween.SignalName.Finished);
        _waveAnnounceLabel.Visible = false;
    }

    // -------------------------------------------------------------------------
    // Spawning
    // -------------------------------------------------------------------------

    private void SpawnAsteroid()
    {
        var asteroid = _asteroidScene.Instantiate<Asteroid>();
        asteroid.Position = new Vector2((float)GD.RandRange(50, 1230), -30f);

        // Scale difficulty with wave number
        asteroid.Speed      = 150f + (_wave - 1) * 25f;
        asteroid.HitPoints  = 1 + (_wave - 1) / 3;       // +1 HP every 3 waves
        asteroid.PointValue = 100 + (_wave - 1) * 50;    // +50 pts per wave

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
        if (_gameOver) return;

        _lives--;
        UpdateHUD();

        if (_lives <= 0)
            ShowGameOver();
    }

    // -------------------------------------------------------------------------
    // Internal
    // -------------------------------------------------------------------------

    private void ShowGameOver()
    {
        _gameOver = true;
        _spawnTimer.Stop();
        GetNode<Timer>("WaveTimer").Stop();
        _finalScoreLabel.Text   = $"Final Score: {_score}";
        _gameOverScreen.Visible = true;
    }

    private void OnPlayAgainPressed()
    {
        GetTree().ReloadCurrentScene();
    }

    private void UpdateHUD()
    {
        _scoreLabel.Text = $"Score: {_score}";
        _livesLabel.Text = $"Lives: {_lives}";
        _waveLabel.Text  = $"Wave: {_wave}";
    }
}
