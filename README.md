# Orbital Blaster

A fast-paced 2D arcade game built with Godot 4.6.1 and C#. Asteroids fall from space — click to destroy them before they cross the defense line at the bottom of the screen. Survive as long as you can across increasingly difficult waves.

## Gameplay

- **Click** an asteroid to hit it. Some asteroids require multiple hits.
- **Lose a life** every time an asteroid crosses the blue defense line.
- **Waves** advance every 30 seconds — each wave spawns faster asteroids worth more points.
- **Game over** when all lives are lost. Your high score is saved between sessions.

### Asteroid sizes

| Size   | Speed    | Hits to kill | Points  |
|--------|----------|--------------|---------|
| Small  | Faster   | 1            | Fewer   |
| Medium | Normal   | 1+           | Normal  |
| Large  | Slower   | 2+           | More    |

### Power-up

Occasionally a **golden orb** drops from a destroyed asteroid and drifts down the screen. Click it before it reaches the defense line to trigger an **Area Blast** — every asteroid on screen is instantly destroyed.

### Difficulty modes (Start Screen)

| Mode | Starting Wave |
|------|--------------|
| Default | 1 |
| Advanced | 5 |
| Hardcore | 10 |

## Tech Stack

- **Engine:** Godot 4.6.1 .NET (GL Compatibility renderer)
- **Language:** C# targeting .NET 8
- **IDE:** Visual Studio (`OrbitalBlaster.sln`)

## Building & Running

1. Open the project in Godot 4.6.1 .NET.
2. Press **F5** (or **Project → Run**) to build and run.

To build the C# solution manually (e.g. to check for errors without launching Godot):

```
dotnet clean OrbitalBlaster.sln -c Debug
dotnet build OrbitalBlaster.sln -c Debug
```

> After editing scripts outside Godot, use **Project → Reload Current Project** before running.

## Project Structure

```
scenes/       Godot scene files (.tscn)
scripts/      C# scripts (one per scene)
project.godot Project settings (1280x720 viewport, GL Compatibility)
```
