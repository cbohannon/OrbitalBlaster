using Godot;

public partial class Asteroid : Area2D
{
    [Export] public float Speed     = 150f;
    [Export] public int   HitPoints = 1;
    [Export] public int   PointValue = 100;

    private Main _main;

    public override void _Ready()
    {
        _main = GetTree().Root.GetNode<Main>("Main");
        InputEvent += OnInputEvent;
    }

    public override void _Process(double delta)
    {
        Position += Vector2.Down * Speed * (float)delta;

        // Crossed the base line — player loses a life
        if (Position.Y > 680f)
        {
            _main.LoseLife();
            QueueFree();
        }
    }

    private void OnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event is InputEventMouseButton mouseEvent &&
            mouseEvent.ButtonIndex == MouseButton.Left &&
            mouseEvent.Pressed)
        {
            TakeHit();
        }
    }

    private void TakeHit()
    {
        HitPoints--;
        if (HitPoints <= 0)
        {
            _main.AddScore(PointValue);
            QueueFree();
        }
    }
}
