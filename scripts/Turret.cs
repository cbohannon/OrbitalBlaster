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

    private Vector2 _barrelDir = Vector2.Up;

    public override void _Process(double delta)
    {
        var mousePos = GetViewport().GetMousePosition();

        // Angle from turret center toward mouse; clamp to upper hemisphere only
        float angle = (mousePos - GlobalPosition).Angle();
        angle = Mathf.Clamp(angle, Mathf.DegToRad(-170f), Mathf.DegToRad(-10f));
        _barrelDir = Vector2.Right.Rotated(angle);

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
    }
}
