using Godot;

public partial class PowerUp : Node2D
{
    public const float ClickRadius = 22f;

    private const float DriftSpeed = 65f;

    private static readonly Color ColorGlow  = new Color(1.00f, 0.65f, 0.10f, 0.35f);
    private static readonly Color ColorOrb   = new Color(1.00f, 0.80f, 0.20f);
    private static readonly Color ColorCore  = new Color(1.00f, 1.00f, 0.80f);
    private static readonly Color ColorSpark = new Color(1.00f, 1.00f, 0.70f);

    public bool IsActive { get; private set; } = false;

    private Main  _main;
    private float _pulseTimer = 0f;

    public override void _Ready()
    {
        _main = GetTree().Root.GetNode<Main>("Main");
    }

    // -------------------------------------------------------------------------
    // Pool interface
    // -------------------------------------------------------------------------

    public void Activate(Vector2 position)
    {
        Position    = position;
        _pulseTimer = 0f;
        IsActive    = true;
        Visible     = true;
        ProcessMode = ProcessModeEnum.Inherit;
    }

    public void Deactivate()
    {
        IsActive    = false;
        Visible     = false;
        ProcessMode = ProcessModeEnum.Disabled;
    }

    // -------------------------------------------------------------------------
    // Behaviour
    // -------------------------------------------------------------------------

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        Position    += Vector2.Down * DriftSpeed * dt;
        _pulseTimer += dt;

        if (Position.Y > 690f)
            _main.ReturnPowerUpToPool(this);
        else
            QueueRedraw();
    }

    public override void _Draw()
    {
        float pulse   = 0.7f + 0.3f * Mathf.Sin(_pulseTimer * 5f);   // 0.4–1.0
        float starRot = _pulseTimer * 0.6f;                            // slow rotation

        // Outer glow — pulsing radius
        DrawCircle(Vector2.Zero, 20f * pulse, new Color(ColorGlow, ColorGlow.A * pulse));

        // Main orb
        DrawCircle(Vector2.Zero, 12f, new Color(ColorOrb, pulse));

        // Inner core
        DrawCircle(Vector2.Zero, 5f,  new Color(ColorCore, pulse));

        // Rotating sparkle — two main axes + two shorter diagonal axes
        float len1 = 9f  * pulse;
        float len2 = 6f  * pulse;
        var   ax1  = Vector2.Right.Rotated(starRot);
        var   ax2  = Vector2.Right.Rotated(starRot + Mathf.Pi * 0.5f);
        var   ax3  = Vector2.Right.Rotated(starRot + Mathf.Pi * 0.25f);
        var   ax4  = Vector2.Right.Rotated(starRot + Mathf.Pi * 0.75f);

        var sparkMain = new Color(ColorSpark, pulse * 0.95f);
        var sparkDiag = new Color(ColorSpark, pulse * 0.55f);

        DrawLine(-ax1 * len1, ax1 * len1, sparkMain, 2f, true);
        DrawLine(-ax2 * len1, ax2 * len1, sparkMain, 2f, true);
        DrawLine(-ax3 * len2, ax3 * len2, sparkDiag, 1.5f, true);
        DrawLine(-ax4 * len2, ax4 * len2, sparkDiag, 1.5f, true);
    }
}
