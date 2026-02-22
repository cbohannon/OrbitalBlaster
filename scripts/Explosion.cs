using Godot;

public partial class Explosion : Node2D
{
    public override void _Ready()
    {
        var particles = GetNode<CpuParticles2D>("Particles");

        // Auto-free once the burst is finished
        particles.Finished += QueueFree;
    }
}
