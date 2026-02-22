using Godot;

public partial class Starfield : Node2D
{
    private struct Star
    {
        public float X;
        public float Y;
        public float Speed;
        public float Size;
        public float Brightness;
    }

    private const int   StarCount     = 120;
    private const float ScreenWidth   = 1280f;
    private const float ScreenHeight  = 720f;

    private readonly Star[] _stars = new Star[StarCount];
    private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        _rng.Randomize();

        // Spread stars across the full screen on startup
        for (int i = 0; i < StarCount; i++)
            _stars[i] = SpawnStar(distributed: true);
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        for (int i = 0; i < StarCount; i++)
        {
            _stars[i].Y += _stars[i].Speed * dt;

            // Wrap off-screen stars back to the top
            if (_stars[i].Y > ScreenHeight + 4f)
                _stars[i] = SpawnStar(distributed: false);
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        for (int i = 0; i < StarCount; i++)
        {
            ref Star s = ref _stars[i];
            // Slightly blue-white tint for a space feel
            DrawCircle(new Vector2(s.X, s.Y), s.Size,
                       new Color(s.Brightness, s.Brightness, s.Brightness + 0.08f));
        }
    }

    // -------------------------------------------------------------------------

    private Star SpawnStar(bool distributed)
    {
        float roll = _rng.Randf();

        float speed, size, brightness;

        if (roll < 0.55f)       // far layer  — slow, dim, tiny
        {
            speed      = _rng.RandfRange(15f, 28f);
            size       = 1.0f;
            brightness = _rng.RandfRange(0.25f, 0.45f);
        }
        else if (roll < 0.85f)  // mid layer  — medium
        {
            speed      = _rng.RandfRange(35f, 55f);
            size       = 1.5f;
            brightness = _rng.RandfRange(0.50f, 0.70f);
        }
        else                    // near layer — fast, bright, larger
        {
            speed      = _rng.RandfRange(65f, 90f);
            size       = 2.0f;
            brightness = _rng.RandfRange(0.75f, 1.00f);
        }

        return new Star
        {
            X          = _rng.RandfRange(0f, ScreenWidth),
            Y          = distributed ? _rng.RandfRange(0f, ScreenHeight) : -4f,
            Speed      = speed,
            Size       = size,
            Brightness = brightness
        };
    }
}
