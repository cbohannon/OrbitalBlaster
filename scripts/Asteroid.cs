using Godot;

public enum AsteroidSize { Small, Medium, Large }

public partial class Asteroid : Area2D
{
    [Export] public float Speed      = 150f;
    [Export] public int   HitPoints  = 1;
    [Export] public int   PointValue = 100;

    public bool IsActive { get; private set; } = false;
    public AsteroidSize Size { get; private set; } = AsteroidSize.Medium;

    private Main  _main;
    private float _rotationSpeed;
    private float _flashTimer = 0f;
    private const float FlashDuration = 0.15f;

    public override void _Ready()
    {
        _main = GetTree().Root.GetNode<Main>("Main");
    }

    // -------------------------------------------------------------------------
    // Pool interface
    // -------------------------------------------------------------------------

    private static (float scale, float speedMult, int hpBonus, float pointsMult) SizeData(AsteroidSize size) =>
        size switch
        {
            AsteroidSize.Small  => (0.6f, 1.3f, 0, 0.6f),
            AsteroidSize.Large  => (1.5f, 0.7f, 1, 2.0f),
            _                   => (1.0f, 1.0f, 0, 1.0f),
        };

    public void Activate(Vector2 position, float speed, int hitPoints, int pointValue, AsteroidSize size = AsteroidSize.Medium)
    {
        var (scaleFactor, speedMult, hpBonus, pointsMult) = SizeData(size);

        Position    = position;
        Speed       = speed * speedMult;
        HitPoints   = hitPoints + hpBonus;
        PointValue  = Mathf.RoundToInt(pointValue * pointsMult);
        Scale       = Vector2.One * scaleFactor;
        Size        = size;
        Modulate    = Colors.White;
        RotationDegrees = 0f;

        _rotationSpeed = (float)GD.RandRange(30.0, 90.0)
                         * (GD.Randf() > 0.5f ? 1f : -1f);
        _flashTimer = 0f;

        IsActive    = true;
        Visible     = true;
        ProcessMode = ProcessModeEnum.Inherit;
    }

    public void Deactivate()
    {
        _flashTimer = 0f;
        IsActive    = false;
        Visible     = false;
        ProcessMode = ProcessModeEnum.Disabled;
    }

    // -------------------------------------------------------------------------
    // Gameplay
    // -------------------------------------------------------------------------

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        Position        += Vector2.Down * Speed * dt;
        RotationDegrees += _rotationSpeed * dt;

        // Drive the hit-flash without allocating a Tween object
        if (_flashTimer > 0f)
        {
            _flashTimer -= dt;
            if (_flashTimer <= 0f)
            {
                _flashTimer = 0f;
                Modulate    = Colors.White;
            }
            else
            {
                float t  = 1f - (_flashTimer / FlashDuration);  // 0 → 1
                Modulate = new Color(1f, 0.2f + 0.8f * t, 0.2f + 0.8f * t);
            }
        }

        if (Position.Y > 680f)
        {
            _main.LoseLife();
            _main.ReturnAsteroidToPool(this);
        }
    }

    public void TakeHit()
    {
        HitPoints--;
        if (HitPoints <= 0)
        {
            SoundManager.Instance.PlayExplosion();
            _main.SpawnExplosion(Position);
            _main.AddScore(PointValue);
            _main.ReturnAsteroidToPool(this);
        }
        else
        {
            SoundManager.Instance.PlayHit();
            FlashHit();
        }
    }

    private void FlashHit()
    {
        Modulate    = new Color(1f, 0.2f, 0.2f);
        _flashTimer = FlashDuration;
    }
}
