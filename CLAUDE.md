# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Orbital Blaster is a 2D arcade game built with **Godot 4.6.1 (.NET)** and **C# targeting .NET 8**. The player clicks asteroids to destroy them before they cross the defense line at y=650. Waves advance every 30 seconds, scaling speed, HP, and point value.

## Engine & Build

- **Renderer:** GL Compatibility (OpenGL) — Forward+ caused first-run D3D12 shader compilation spikes; Compatibility has no render device pipeline and is correct for 2D
- **IDE:** Visual Studio — open `OrbitalBlaster.sln` to edit scripts
- **Run:** Press **F5** in Godot, or **Project → Run**
- **Manual build:** `dotnet clean OrbitalBlaster.sln -c Debug && dotnet build OrbitalBlaster.sln -c Debug`
- After external file edits: use **Project → Reload Current Project** in Godot before running. MSBuild skips CoreCompile if it thinks files haven't changed — `dotnet clean` forces a full recompile.

## Architecture

### Scene flow
`StartScreen.tscn` → sets `GameSettings.StartingWave` → loads `Main.tscn` → on game over, returns to `StartScreen.tscn`

### Autoloads (singletons)
- **SoundManager** (`scripts/SoundManager.cs`) — procedural audio via `AudioStreamGenerator` at 22050 Hz. All sound buffers are `static readonly Vector2[]` built once at startup; `PlaySound()` pushes a cached buffer to the player. No per-play heap allocation.
- **GameSettings** (`scripts/GameSettings.cs`) — holds `StartingWave` (set by difficulty button) and `HighScore` (persisted to `user://highscore.dat` via `FileAccess`).

### Main.tscn scene tree
```
Main (Node2D) [Main.cs]
├── Background (ColorRect, mouse_filter=2)   — dark space fill
├── Starfield (Node2D) [Starfield.cs]         — 120 stars, 3 parallax layers, zero per-frame alloc
├── BaseLine (ColorRect, mouse_filter=2)      — defense line at y=650
├── GameWorld (Node2D)                        — runtime parent for pooled asteroids/explosions
├── SpawnTimer (Timer, autostart=true)        — held by PrewarmPools until pool is ready
├── WaveTimer (Timer, autostart=true, 30s)    — held by PrewarmPools until pool is ready
├── HUD (CanvasLayer layer=1)
│   ├── LivesLabel, WaveLabel, ScoreLabel
│   └── WaveAnnounceLabel                    — fades in/out on wave advance via Tween
└── GameOverScreen (CanvasLayer layer=2, visible=false)
    ├── Overlay, GameOverLabel, FinalScoreLabel, HighScoreLabel
    ├── PlayAgainButton                       — returns to StartScreen
    └── QuitButton
```

### Object pooling
Both `Asteroid` (pool size 20) and `Explosion` (pool size 15) are pre-allocated in `GameWorld` and toggled active/inactive rather than Instantiate/QueueFree. The pool grows automatically if exhausted.

- **Activate(...)** / **Deactivate()** on each type set `Visible`, `ProcessMode`, and reset state
- **PrewarmPools()** in `Main.cs` is `async void` — creates 5 nodes per frame to avoid a single-frame startup spike, then starts SpawnTimer and WaveTimer
- `Main._UnhandledInput` iterates `_asteroidPool` directly (no `GetChildren()` allocation)

### Asteroid.cs
- Moves `Vector2.Down * Speed * delta` each frame; rotates at a random speed set on `Activate`
- Hit flash: driven by `_flashTimer` float in `_Process` — no `CreateTween()` allocation
- Calls `Main.LoseLife()` when `Position.Y > 680`, then returns itself to pool

### Explosion.cs
- `CPUParticles2D` one-shot burst; `Finished` signal calls `Main.ReturnExplosionToPool()`
- `emitting = false` in the scene; `Activate()` calls `_particles.Restart()`

## Conventions

- Scene files in `scenes/`, scripts in `scripts/` — paired by name (`Asteroid.tscn` / `Asteroid.cs`)
- `public partial class Foo : Node` required by Godot for all script classes
- Node references resolved in `_Ready()` via `GetNode<T>("path")`
- `[Export]` for designer-tunable values
- **Never hand-craft `uid=` values** in `.tscn` files — use `[gd_scene format=3]` with no uid field; Godot generates a valid UID on first load. Hand-crafted UIDs cause `Unrecognized UID` errors.
- **Background ColorRects must have `mouse_filter = 2` (Ignore)** — the default (Stop) consumes all mouse clicks before they reach game objects
- **Click detection lives in `Main._UnhandledInput`** with a `Position.DistanceTo(clickPos) <= 35f` check — `Area2D._InputEvent` is unreliable for this use case

## Git Workflow

The user handles all branching, PRs, and merges. Claude commits and pushes only during active development work on the current branch.
