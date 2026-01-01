# Conway's Game of Life (Unity Edition)

A retro-inspired implementation of Conway's Game of Life using Unity. Built to introduce the topic  
to my kids. Improved it for the sake of keeping my skills on C# + Unity sharp.

![Game of Life demo](Assets/Preview/game.gif)

## Features

- Cell simulation using Tilemaps  
- Custom patterns using ScriptableObjects  
- Pattern preview inside Unity Editor  
- Random pattern spawning with `R` key  
- Numeric modifiers (e.g. `3-R`, `5-C-P-R`) for bulk spawning  
- Toggleable runtime grid overlay with `G` key  
- Pixel Perfect Camera setup for crisp visuals  
- Dead cells outside camera view (screen-culling mode)  
- Live FPS and cell counter  
- Colorful Patterns  
- Wrap around toggle  

## Quick Start

1. Open the project in Unity `6000.0.45f1` (Unity 6).
2. Open the scene at `Assets/Scenes/GameOfLife.unity`.
3. Press Play, then use the controls below to spawn patterns.

## Controls

```
| Key Combo       | Action                                                                 |
|-----------------|------------------------------------------------------------------------|
| `R`             | Spawn a new pattern with the currently selected pattern & color        |
| `C`             | Use a random color for the next pattern only                           |
| `P`             | Use a random pattern from the library for the next spawn only          |
| `Z`             | Select **previous pattern** in library                                 |
| `U`             | Select **next pattern** in library                                     |
| `H`             | Select **previous color** in palette                                   |
| `J`             | Select **next color** in palette                                       |
| `G`             | Toggle grid overlay                                                    |
| `W`             | Toggle wrap-around mode                                                |
| `V`             | Toggle UI elements visibility                                          |
| `K`             | Visually create a pattern. Mouse select where, Left click to create it | 
| `5-R`           | Spawn 5 patterns with the **current** pattern & color                  |
| `2-C-R`         | Spawn 2 random-colored patterns (next only)                            |
| `5-C-P-R`       | Spawn 5 random-colored, random-pattern instances (next only)           |
```

> You can chain keys: a number modifier (`2`) applies to the next `R`, and keys like `C`, `P` act as flags.

## Folder Structure

```
Assets/
├── Fonts/            # Google Fonts (e.g. Pixelify Sans)
├── Patterns/         # ScriptableObject assets for patterns
├── Prefabs/
├── Scripts/
│   ├── GameManager.cs
│   ├── GridLines.cs
|   ├── InputManager.cs 
│   └── Pattern.cs        # ScriptableObject definition
│   ├── UIManager.cs
├── Editor/
│   └── PatternEditor.cs  # Inspector preview for patterns
```

## Patterns

Patterns are stored as `ScriptableObject` assets and support live preview in the Unity Inspector.

You can easily create new patterns or import classic ones like:
- Glider  
- R-pentomino  
- Diehard  
- Blinker  
- Penta-decathlon  
- 1-2-3
- Spaceship
- TBD

## Grid Overlay

A `GridLineDrawer` component dynamically draws grid lines to match the camera view using `LineRenderer`. Toggle it on/off at runtime with the `G` key.

## Requirements

- Unity 6 (`6000.0.45f1`)  
- TextMeshPro  
- Pixel Perfect Camera component  

## Troubleshooting

- UI not updating: make sure `UIManager` has `Grid` assigned in the inspector, and the `Grid` has `GameManager` attached.
- Grid lines missing or offset: `GridLineDrawer` expects an orthographic camera.
- Pattern preview missing: check `UISelectionController` has `patternLibrary`, `patternColors`, and `previewCellPrefab` assigned.
- UI text missing: confirm TextMeshPro is imported and the UI text references are wired.

## TODO / Ideas

- [ ] Choose which Pattern to spawn from a visual inventory  
- [ ] Better UI  
- [ ] Configuration menu to control colors  
- [ ] Toggle simulation state (pause/resume/seed)  
- [ ] Export/import custom pattern JSON  
- [ ] Cellular orbit detection UI feedback  

## Credits

Inspired by Conway's classic cellular automaton. RIP John Conway.
