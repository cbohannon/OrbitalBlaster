# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Orbital Blaster is a 2D action game built with **Godot 4.6.1 (.NET)** and **C# targeting .NET 8**. The player uses the mouse to destroy objects falling from space before they cross the defense line at the bottom of the screen.

## Engine & Build

- **Engine:** Godot 4.6.1 .NET — must be launched via the Godot executable (not `dotnet` directly)
- **C# SDK:** .NET 8
- **IDE:** Visual Studio — open `OrbitalBlaster.sln` to edit scripts
- **Build:** Godot compiles C# automatically on run. To build the C# solution manually: `dotnet build OrbitalBlaster.sln`
- **Run:** Press **F5** in Godot, or use **Project → Run**
- **Main scene:** `res://scenes/Main.tscn`

## Project Structure

```
scenes/      — Godot scene files (.tscn); one scene per major game object/screen
scripts/     — C# scripts; each script is attached to a scene node
assets/      — Sprites, sounds, fonts (not yet populated)
project.godot — Project settings (viewport 1280x720, stretch mode: canvas_items/keep)
```

## Architecture

The game uses Godot's **node/scene composition** model. Each game object (asteroid, projectile, etc.) will be its own scene with an attached C# script, instanced into the `GameWorld` node at runtime.

### Scene Tree (Main.tscn)
```
Main (Node2D) [Main.cs]
├── Background (ColorRect)      — dark space fill, 1280x720
├── BaseLine (ColorRect)        — defense line at y=650; objects crossing here cost a life
├── GameWorld (Node2D)          — runtime parent for all spawned objects
└── HUD (CanvasLayer)
    ├── LivesLabel              — top left
    ├── WaveLabel               — top center
    └── ScoreLabel              — top right
```

### Main.cs — Game State Manager
`Main.cs` is the central game controller. Other scripts call into it via:
- `AddScore(int points)` — adds to score and refreshes HUD
- `LoseLife()` — decrements lives, triggers `GameOver()` at zero
- `StartNextWave()` — increments wave counter and refreshes HUD

Scripts that need to call these methods should get a reference via `GetTree().Root.GetNode<Main>("Main")` or via a signal.

## Conventions

- Scene files go in `scenes/`, scripts go in `scripts/` — keep them paired by name (e.g. `Asteroid.tscn` / `Asteroid.cs`)
- C# partial classes are required by Godot: `public partial class Foo : Node`
- Node references are resolved in `_Ready()` via `GetNode<T>("NodePath")`
- Exported fields (`[Export]`) are used for designer-tunable values (speeds, counts, etc.)
- `GD.Print()` is used for debug output (Godot's equivalent of `Console.WriteLine`)
- When creating `.tscn` files manually, omit the `uid=` field from the header (`[gd_scene format=3]`) — Godot will generate a valid UID on first load. Hand-crafted UIDs cause an `Unrecognized UID` error at startup.
