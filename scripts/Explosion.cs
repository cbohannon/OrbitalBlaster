using Godot;

public partial class Explosion : Node2D
{
    private Main           _main;
    private CpuParticles2D _particles;

    public override void _Ready()
    {
        _main      = GetTree().Root.GetNode<Main>("Main");
        _particles = GetNode<CpuParticles2D>("Particles");

        _particles.Finished += OnParticlesFinished;
    }

    // -------------------------------------------------------------------------
    // Pool interface
    // -------------------------------------------------------------------------

    public void Activate(Vector2 position)
    {
        Position    = position;
        Visible     = true;
        ProcessMode = ProcessModeEnum.Inherit;
        _particles.Restart();
    }

    public void Deactivate()
    {
        Visible     = false;
        ProcessMode = ProcessModeEnum.Disabled;
    }

    // -------------------------------------------------------------------------

    private void OnParticlesFinished()
    {
        _main.ReturnExplosionToPool(this);
    }
}
