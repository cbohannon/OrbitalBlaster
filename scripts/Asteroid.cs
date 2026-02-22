using Godot;

public partial class Asteroid : Area2D
{
    [Export] public float Speed      = 150f;
    [Export] public int   HitPoints  = 1;
    [Export] public int   PointValue = 100;

    private static readonly PackedScene ExplosionScene =
        GD.Load<PackedScene>("res://scenes/Explosion.tscn");

    private Main _main;

    public override void _Ready()
    {
        _main = GetTree().Root.GetNode<Main>("Main");
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

    public void TakeHit()
    {
        HitPoints--;
        if (HitPoints <= 0)
        {
            SoundManager.Instance.PlayExplosion();
            SpawnExplosion();
            _main.AddScore(PointValue);
            QueueFree();
        }
        else
        {
            SoundManager.Instance.PlayHit();
            FlashHit();
        }
    }

    private void FlashHit()
    {
        Modulate = new Color(1f, 0.2f, 0.2f);
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate", Colors.White, 0.15f);
    }

    private void SpawnExplosion()
    {
        var explosion = ExplosionScene.Instantiate<Node2D>();
        explosion.Position = Position;
        GetTree().Root.GetNode<Node2D>("Main/GameWorld").AddChild(explosion);
    }
}
