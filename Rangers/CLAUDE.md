# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Rangers** is a Unity 2D game project created for Ludum Dare 57 (game jam). The project uses Unity 2022+ with a 2D template and is designed for rapid prototyping and game jam development.

**Product Name:** Rangers
**Unity Version:** 2022.x (2D Template)
**Target Platforms:** Standalone (Windows primary), Android support configured
**Project GUID:** c4260de35d2b37345b7756c4f3e876d2

## Key Dependencies

The project uses several third-party plugins and packages:

- **Odin Inspector** (Sirenix) - Advanced Unity inspector and serialization
  - Scripting define: `ODIN_INSPECTOR` (and versioned variants for 3.x)
  - Used for enhanced editor features like `[ReadOnly]`, `[CreateAssetMenu]` attributes
- **DOTween** (Demigiant) - Animation and tweening library
  - Scripting define: `DOTWEEN`
  - Located in `Assets/Plugins/Demigiant/DOTween/`
- **Rainbow Folders** (Borodar) - Editor organization tool for colored folders
- **TextMeshPro** (Unity Package) - Advanced text rendering

## Architecture

### Core Systems Structure

The project follows a modular **systems-based architecture** under `Assets/Systems/`:

```
Assets/
├── Systems/
│   ├── Creatures/           # Creature data and management
│   │   ├── Scripts/
│   │   │   ├── CreatureData.cs    # ScriptableObject for creature definitions
│   │   │   └── ShapeData.cs       # ScriptableObject for shape definitions
│   │   └── Assets/                # Runtime creature assets
│   └── Helper Functions/
│       └── Extensions.cs          # Global extension methods and utilities
├── Plugins/                 # Third-party assets (Odin, DOTween, Rainbow Folders)
├── Resources/               # Unity Resources folder
└── Scenes/
    └── SampleScene.unity    # Main scene
```

### ScriptableObject Pattern

The game uses Unity's **ScriptableObject pattern** for data-driven design:

- **CreatureData** (`Assets/Systems/Creatures/Scripts/CreatureData.cs`): Defines creatures with:
  - Sprite representation
  - Description text
  - `shapePool`: List of available shapes for this creature
  - `currentShapePool`: Runtime-tracked active shapes (marked `[ReadOnly]` for debugging)
  - Create via: `Assets > Create > RANGER/Creature`

- **ShapeData** (`Assets/Systems/Creatures/Scripts/ShapeData.cs`): Minimal shape definition
  - Currently a placeholder for future shape properties
  - Create via: `Assets > Create > RANGER/Shape`

### Extension Methods

`Extensions.cs` provides global utility methods:
- **Random selection**: `GetRandom<T>()` for arrays and lists
- **Transform utilities**: `DestroyAllChildren()`
- **Vector manipulation**: `XOZ()`, `OYZ()`, `XYO()` projection methods
- **Number formatting**: `FormatNumber()` for ordinal numbers (1st, 2nd, 3rd...)
- **Math comparisons**: `MeetsEquation()` for dynamic inequality checks
- **Array operations**: `GetAverage()` for float collections

## Development Workflow

### Opening the Project

1. Open Unity Hub
2. Add the `Rangers` folder as a Unity project
3. Open with Unity 2022.x or later

### Building the Game

Unity Editor:
- File > Build Settings > Build (Windows Standalone by default)
- Default resolution: 1920x1080

### Running in Editor

- Open `Assets/Scenes/SampleScene.unity`
- Press Play button in Unity Editor

### Working with ScriptableObjects

When creating new data assets:
- Right-click in Project window > Create > RANGER > [Creature/Shape]
- Store creature assets in `Assets/Systems/Creatures/Assets/`
- All ScriptableObjects use the `RANGER` menu prefix for consistency

### Code Conventions

- **Scripting Defines**: When adding platform-specific code, note that DOTween and Odin Inspector defines are globally available
- **Namespace**: Project does not use namespaces (game jam convention)
- **Odin Attributes**: Use Odin Inspector attributes (`[ReadOnly]`, `[Button]`, etc.) for enhanced editor experience where applicable
- **Extensions**: Add new utility methods to `Extensions.cs` rather than creating helper MonoBehaviours

## Platform Configuration

- **Standalone (Primary Target)**:
  - Scripting defines: `ODIN_INSPECTOR;ODIN_INSPECTOR_3;ODIN_INSPECTOR_3_1;ODIN_INSPECTOR_3_2;ODIN_INSPECTOR_3_3;DOTWEEN`
  - Color Space: Linear
  - Graphics API: Auto

- **Android (Secondary)**:
  - Min SDK: 22
  - Scripting defines: `DOTWEEN`

## Project Context

This is a **Ludum Dare 57 game jam project**, which means:
- Fast iteration is prioritized over architectural perfection
- ScriptableObjects are used for quick data editing without recompiling
- Third-party tools (Odin, DOTween) are used to accelerate development
- Code is kept simple and in the global namespace for rapid prototyping
