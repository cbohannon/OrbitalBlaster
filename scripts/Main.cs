using Godot;
using System.Collections.Generic;

public partial class Main : Node2D
{
    [Export] public int StartingLives = 3;

    private int  _score    = 0;
    private int  _lives;
    private int  _wave     = 1;
    private bool _gameOver = false;

    private Label        _scoreLabel;
    private Label        _livesLabel;
    private Label        _waveLabel;
    private Label        _waveAnnounceLabel;
    private CanvasLayer  _gameOverScreen;
    private Label        _finalScoreLabel;
    private Label        _highScoreLabel;

    private Node2D _gameWorld;
    private Timer  _spawnTimer;

    // -------------------------------------------------------------------------
    // Object pools — pre-allocated to avoid per-spawn GC pressure
    // -------------------------------------------------------------------------

    private const int AsteroidPoolSize  = 20;
    private const int ExplosionPoolSize = 15;

    private readonly List<Asteroid>  _asteroidPool  = new List<Asteroid>(AsteroidPoolSize);
    private readonly List<Explosion> _explosionPool = new List<Explosion>(ExplosionPoolSize);

    private PackedScene _asteroidScene;
    private PackedScene _explosionScene;

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    public override void _Ready()
    {
        _lives = StartingLives;
        _wave  = GameSettings.StartingWave;

        _scoreLabel        = GetNode<Label>("HUD/ScoreLabel");
        _livesLabel        = GetNode<Label>("HUD/LivesLabel");
        _waveLabel         = GetNode<Label>("HUD/WaveLabel");
        _waveAnnounceLabel = GetNode<Label>("HUD/WaveAnnounceLabel");
        _gameOverScreen    = GetNode<CanvasLayer>("GameOverScreen");
        _finalScoreLabel   = GetNode<Label>("GameOverScreen/FinalScoreLabel");
        _highScoreLabel    = GetNode<Label>("GameOverScreen/HighScoreLabel");

        GetNode<Button>("GameOverScreen/PlayAgainButton").Pressed += OnPlayAgainPressed;
        GetNode<Button>("GameOverScreen/QuitButton").Pressed      += OnQuitPressed;

        _gameWorld  = GetNode<Node2D>("GameWorld");
        _spawnTimer = GetNode<Timer>("SpawnTimer");

        _asteroidScene  = GD.Load<PackedScene>("res://scenes/Asteroid.tscn");
        _explosionScene = GD.Load<PackedScene>("res://scenes/Explosion.tscn");

        _spawnTimer.Timeout                 += SpawnAsteroid;
        GetNode<Timer>("WaveTimer").Timeout += OnWaveTimerTimeout;

        _spawnTimer.WaitTime = Mathf.Max(0.5f, 2.0f - (_wave - 1) * 0.25f);

        // Stop autostart timers; PrewarmPools will start them once pools are ready
        _spawnTimer.Stop();
        GetNode<Timer>("WaveTimer").Stop();

        UpdateHUD();
        PrewarmPools();
    }

    // Builds pools over several frames (5 nodes/frame) to avoid a single-frame
    // allocation spike on load.  Starts gameplay timers once pools are ready.
    private async void PrewarmPools()
    {
        for (int i = 0; i < AsteroidPoolSize; i++)
        {
            var a = _asteroidScene.Instantiate<Asteroid>();
            _gameWorld.AddChild(a);
            a.Deactivate();
            _asteroidPool.Add(a);
            if (i % 5 == 4)
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        for (int i = 0; i < ExplosionPoolSize; i++)
        {
            var e = _explosionScene.Instantiate<Explosion>();
            _gameWorld.AddChild(e);
            e.Deactivate();
            _explosionPool.Add(e);
            if (i % 5 == 4)
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        _spawnTimer.Start();
        GetNode<Timer>("WaveTimer").Start();
    }

    // -------------------------------------------------------------------------
    // Input
    // -------------------------------------------------------------------------

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent &&
            mouseEvent.ButtonIndex == MouseButton.Left &&
            mouseEvent.Pressed)
        {
            // Iterate pool directly — no GetChildren() allocation
            for (int i = 0; i < _asteroidPool.Count; i++)
            {
                var asteroid = _asteroidPool[i];
                if (asteroid.IsActive &&
                    asteroid.Position.DistanceTo(mouseEvent.Position) <= 35f * asteroid.Scale.X)
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
        SoundManager.Instance.PlayWaveAdvance();
        UpdateHUD();

        _spawnTimer.WaitTime = Mathf.Max(0.5f, _spawnTimer.WaitTime - 0.25f);

        ShowWaveAnnouncement();
    }

    private async void ShowWaveAnnouncement()
    {
        _waveAnnounceLabel.Text     = $"Wave {_wave}";
        _waveAnnounceLabel.Modulate = Colors.White;
        _waveAnnounceLabel.Visible  = true;

        var tween = CreateTween();
        tween.TweenProperty(_waveAnnounceLabel, "modulate:a", 0f, 1.5f)
             .SetDelay(0.5f);

        await ToSignal(tween, Tween.SignalName.Finished);
        _waveAnnounceLabel.Visible = false;
    }

    // -------------------------------------------------------------------------
    // Spawning
    // -------------------------------------------------------------------------

    private static AsteroidSize PickRandomSize()
    {
        int roll = (int)GD.RandRange(0, 3);
        return roll switch { 0 => AsteroidSize.Small, 2 => AsteroidSize.Large, _ => AsteroidSize.Medium };
    }

    private void SpawnAsteroid()
    {
        // Find an inactive asteroid in the pool
        Asteroid asteroid = null;
        for (int i = 0; i < _asteroidPool.Count; i++)
        {
            if (!_asteroidPool[i].IsActive)
            {
                asteroid = _asteroidPool[i];
                break;
            }
        }

        // Pool exhausted — grow it rather than drop the spawn
        if (asteroid == null)
        {
            asteroid = _asteroidScene.Instantiate<Asteroid>();
            _gameWorld.AddChild(asteroid);
            _asteroidPool.Add(asteroid);
        }

        asteroid.Activate(
            new Vector2((float)GD.RandRange(50, 1230), -30f),
            150f + (_wave - 1) * 25f,
            1    + (_wave - 1) / 3,
            100  + (_wave - 1) * 50,
            PickRandomSize()
        );
    }

    public void SpawnExplosion(Vector2 position)
    {
        Explosion explosion = null;
        for (int i = 0; i < _explosionPool.Count; i++)
        {
            if (!_explosionPool[i].Visible)
            {
                explosion = _explosionPool[i];
                break;
            }
        }

        if (explosion == null)
        {
            explosion = _explosionScene.Instantiate<Explosion>();
            _gameWorld.AddChild(explosion);
            _explosionPool.Add(explosion);
        }

        explosion.Activate(position);
    }

    // -------------------------------------------------------------------------
    // Pool returns — called by Asteroid and Explosion when done
    // -------------------------------------------------------------------------

    public void ReturnAsteroidToPool(Asteroid asteroid)  => asteroid.Deactivate();
    public void ReturnExplosionToPool(Explosion explosion) => explosion.Deactivate();

    // -------------------------------------------------------------------------
    // Public API — called by Asteroid
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
        SoundManager.Instance.PlayLifeLost();
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

        bool newBest = GameSettings.TrySaveHighScore(_score);

        _finalScoreLabel.Text = $"Final Score: {_score}";
        _highScoreLabel.Text  = newBest
            ? "New Best!"
            : $"Best: {GameSettings.HighScore}";

        _gameOverScreen.Visible = true;
    }

    private void OnPlayAgainPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/StartScreen.tscn");
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }

    private void UpdateHUD()
    {
        _scoreLabel.Text = $"Score: {_score}";
        _livesLabel.Text = $"Lives: {_lives}";
        _waveLabel.Text  = $"Wave: {_wave}";
    }
}
