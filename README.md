# Conway's Game of Life (Unity Edition)

A retro-inspired implementation of Conway's Game of Life using Unity. Built to introduce the topic
to my kids. Improved it for the sake of keeping my skills on C# + Unity sharp.

## Features

- Cell simulation using Tilemaps
- Custom patterns using ScriptableObjects
- Pattern preview inside Unity Editor
- Random pattern spawning with `R` key
- Toggleable runtime grid overlay with `G` key
- Pixel Perfect Camera setup for crisp visuals
- Dead cells outside camera view (screen-culling mode)
- Live FPS and cell counter

## Controls

| Key | Action                       |
|-----|------------------------------|
| `R` | Spawn a new pattern randomly |
| `G` | Toggle grid overlay          |

## Folder Structure

```
Assets/
├── Fonts/            # Google Fonts (e.g. Pixelify Sans)
├── Patterns/         # ScriptableObject assets for patterns
├── Prefabs/
├── Scripts/
│   ├── GameManager.cs
│   ├── GridLineDrawer.cs
│   ├── UIManager.cs
│   └── Pattern.cs     # ScriptableObject definition
├── Editor/
│   └── PatternEditor.cs # Inspector preview for patterns
```

## Patterns

Patterns are stored as `ScriptableObject` assets and support live preview in the Unity Inspector.

You can easily create new patterns or import classic ones like:
- Glider
- R-pentomino
- Acorn
- Penta-decathlon
- TBD

## Grid Overlay

A `GridLineDrawer` component dynamically draws grid lines to match the camera view using `LineRenderer`s. Toggle it on/off at runtime with the `G` key.

## Requirements

- Unity 6
- TextMeshPro 
- Pixel Perfect Camera component 

## TODO / Ideas

- [ ] Choose which Pattern to spawn from a visual inventory
- [ ] Better UI
- [ ] Configuration menu to control colors

## Credits

Inspired by Conway's classic cellular automaton, rip John Conway

