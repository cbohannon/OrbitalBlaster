using Godot;

public partial class Turret : Node2D
{
    // Colors — dome/line blue palette
    private static readonly Color ColorBody   = new Color(0.10f, 0.28f, 0.58f);
    private static readonly Color ColorDome   = new Color(0.18f, 0.52f, 1.00f);
    private static readonly Color ColorBarrel = new Color(0.62f, 0.78f, 0.92f);
    private static readonly Color ColorMuzzle = new Color(0.88f, 0.96f, 1.00f);

    private const float DomeCenterY  = -20f;
    private const float DomeRadius   = 18f;
    private const float BarrelLength = 48f;
    private const float BarrelWidth  = 8f;

    // Built once — body trapezoid in local coords (origin = center of defense line)
    private static readonly Vector2[] BodyPoly =
    {
        new Vector2(-36f,  2f),   // bottom-left (2px below line so it overlaps slightly)
        new Vector2( 36f,  2f),   // bottom-right
        new Vector2( 18f, DomeCenterY),
        new Vector2(-18f, DomeCenterY),
    };

    private const float FlashDuration = 0.12f;

    private Vector2 _barrelDir  = Vector2.Up;
    private float   _flashTimer = 0f;

    public void TriggerMuzzleFlash() => _flashTimer = FlashDuration;

    public override void _Process(double delta)
    {
        var mousePos = GetViewport().GetMousePosition();

        // Angle from turret center toward mouse; clamp to upper hemisphere only
        float angle = (mousePos - GlobalPosition).Angle();
        angle = Mathf.Clamp(angle, Mathf.DegToRad(-170f), Mathf.DegToRad(-10f));
        _barrelDir = Vector2.Right.Rotated(angle);

        if (_flashTimer > 0f)
            _flashTimer -= (float)delta;

        QueueRedraw();
    }

    public override void _Draw()
    {
        var domeCenter = new Vector2(0f, DomeCenterY);
        var barrelTip  = domeCenter + _barrelDir * BarrelLength;

        // 1. Body trapezoid
        DrawPolygon(BodyPoly, new Color[] { ColorBody });

        // 2. Dome circle — sits on top of body
        DrawCircle(domeCenter, DomeRadius, ColorDome);

        // 3. Barrel shaft with round caps (circles at each end)
        DrawLine(domeCenter, barrelTip, ColorBarrel, BarrelWidth, true);
        DrawCircle(domeCenter, BarrelWidth * 0.5f, ColorBarrel);
        DrawCircle(barrelTip,  BarrelWidth * 0.5f + 1f, ColorMuzzle);

        // 4. Muzzle flash — expanding ring + bright core, both fade out
        if (_flashTimer > 0f)
        {
            float t = _flashTimer / FlashDuration;             // 1 → 0
            float outerRadius = 4f + 16f * (1f - t);          // expands as it fades
            DrawCircle(barrelTip, outerRadius, new Color(1f, 0.85f, 0.4f, t * 0.9f));
            DrawCircle(barrelTip, 6f,          new Color(1f, 1.00f, 0.9f, t));
        }
    }
}
