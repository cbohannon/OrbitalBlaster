using Godot;

public partial class SoundManager : Node
{
    public static SoundManager Instance { get; private set; }

    private const float SampleRate = 22050f;

    private AudioStreamPlayer _explosionPlayer;
    private AudioStreamPlayer _hitPlayer;
    private AudioStreamPlayer _lifeLostPlayer;
    private AudioStreamPlayer _wavePlayer;

    public override void _Ready()
    {
        Instance = this;

        _explosionPlayer = CreatePlayer(0.6f);
        _hitPlayer       = CreatePlayer(0.2f);
        _lifeLostPlayer  = CreatePlayer(0.8f);
        _wavePlayer      = CreatePlayer(0.5f);
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void PlayExplosion()   => PlaySound(_explosionPlayer, BuildExplosion());
    public void PlayHit()         => PlaySound(_hitPlayer,       BuildHit());
    public void PlayLifeLost()    => PlaySound(_lifeLostPlayer,  BuildLifeLost());
    public void PlayWaveAdvance() => PlaySound(_wavePlayer,      BuildWaveAdvance());

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static void PlaySound(AudioStreamPlayer player, Vector2[] buffer)
    {
        player.Stop();
        player.Play();
        ((AudioStreamGeneratorPlayback)player.GetStreamPlayback()).PushBuffer(buffer);
    }

    private AudioStreamPlayer CreatePlayer(float bufferLength)
    {
        var stream = new AudioStreamGenerator();
        stream.MixRate      = SampleRate;
        stream.BufferLength = bufferLength;

        var player = new AudioStreamPlayer();
        player.Stream = stream;
        AddChild(player);
        return player;
    }

    // -------------------------------------------------------------------------
    // Sound builders
    // -------------------------------------------------------------------------

    // White noise burst with exponential decay
    private static Vector2[] BuildExplosion()
    {
        int n   = (int)(SampleRate * 0.4f);
        var buf = new Vector2[n];
        for (int i = 0; i < n; i++)
        {
            float t      = i / SampleRate;
            float env    = Mathf.Exp(-t * 10f);
            float sample = (GD.Randf() * 2f - 1f) * env;
            buf[i] = new Vector2(sample, sample);
        }
        return buf;
    }

    // Short high-pitched pip
    private static Vector2[] BuildHit()
    {
        float dur = 0.06f;
        int n     = (int)(SampleRate * dur);
        var buf   = new Vector2[n];
        for (int i = 0; i < n; i++)
        {
            float t      = i / SampleRate;
            float env    = 1f - (t / dur);
            float sample = Mathf.Sin(2f * Mathf.Pi * 900f * t) * env * 0.4f;
            buf[i] = new Vector2(sample, sample);
        }
        return buf;
    }

    // Descending tone — conveys losing a life
    private static Vector2[] BuildLifeLost()
    {
        float dur = 0.6f;
        int n     = (int)(SampleRate * dur);
        var buf   = new Vector2[n];
        float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t    = i / SampleRate;
            float freq = 400f - (t / dur) * 250f;   // sweeps 400Hz → 150Hz
            float env  = 1f - (t / dur);
            phase += 2f * Mathf.Pi * freq / SampleRate;
            buf[i] = new Vector2(Mathf.Sin(phase) * env * 0.5f,
                                 Mathf.Sin(phase) * env * 0.5f);
        }
        return buf;
    }

    // Two-tone ascending beep — signals a new wave
    private static Vector2[] BuildWaveAdvance()
    {
        float dur = 0.3f;
        int n     = (int)(SampleRate * dur);
        int half  = n / 2;
        var buf   = new Vector2[n];
        for (int i = 0; i < n; i++)
        {
            float t    = i / SampleRate;
            float freq = i < half ? 440f : 660f;
            float env  = i < half
                ? 1f - (float)i / half * 0.2f
                : 1f - (float)(i - half) / half;
            float sample = Mathf.Sin(2f * Mathf.Pi * freq * t) * env * 0.4f;
            buf[i] = new Vector2(sample, sample);
        }
        return buf;
    }
}
