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
├── Scenes/
│   ├── MainScene.unity          # Primary game scene
│   └── SampleScene.unity        # Legacy/sample scene
└── SerializableDictionary/      # Third-party serializable dictionary utility
```

### Game Architecture

The game is a **grid-based tactical battle system** with these core managers:

- **GameManager** (`Assets/Systems/GameManager.cs`): Singleton managing game flow
  - Controls screen navigation (main menu, character select, battle, victory/defeat)
  - Manages wave progression and owned creatures collection
  - Orchestrates battle initialization with selected creatures

- **BattleManager** (`Assets/Systems/Battle/BattleManager.cs`): Turn-based battle controller
  - Manages health, damage calculation, and turn flow
  - Enemy turn: Spawns a random shape on the grid from enemy's shape pool
  - Player turn: Each ally gets 2 random attack shapes to choose from
  - Damage calculation: Allies deal 1 damage per grid cell they control; enemies deal base damage + bonus for adjacent enemy cells

- **GridManager** (`Assets/Systems/Battle/Grid_Shapes/GridManager.cs`): Grid state management
  - Manages the shared grid battlefield where shapes are placed
  - Tracks cell states (empty, ally, enemy, contested)
  - Handles shape placement validation and overlap resolution

### ScriptableObject Pattern

The game uses Unity's **ScriptableObject pattern** for data-driven design:

- **CreatureData** (`Assets/Systems/Creatures/Scripts/CreatureData.cs`): Defines creatures with:
  - Basic properties: `creatureName`, `sprite`, `desc`, `uniqueId`
  - Health values: `healthMaxAlly` and `healthMaxEnemy` (creatures have different stats as ally vs enemy)
  - Shape pools: `allyShapePool` and `enemyShapePool` - separate attack patterns for each role
  - `currentShapePool`: Runtime pool tracking which attacks are still available this battle
  - Methods: `GetRandomAttack(isEnemy)` draws from pool, `EmptyPool()` resets between battles
  - Create via: `Assets > Create > RANGER/Creature`

- **ShapeData** (`Assets/Systems/Creatures/Scripts/ShapeData.cs`): Grid-based attack pattern definition
  - `uniqueID`: Identifier for this shape
  - `gridSize`: Configurable grid dimensions (default 8x8)
  - `currentColor`: Active color for painting cells in the editor
  - `GridWrapper`: Nested serializable structure containing 2D grid of colors
  - **Interactive Editor**: Custom Odin Inspector GUI allows clicking cells to paint attack patterns
    - Left-click: Paint with `currentColor`
    - Right-click: Erase (set to `Color.clear`)
  - The colored grid defines which cells are "active" when this attack shape is placed on the battlefield
  - Create via: `Assets > Create > RANGER/Shape`

### Extension Methods

`Extensions.cs` (`Assets/Systems/Helper Functions/Extensions.cs`) provides global utility methods:
- **Random selection**: `GetRandom<T>()` for arrays and lists
- **Transform utilities**: `DestroyAllChildren()` - destroys all child GameObjects
- **Vector manipulation**: `XOZ()`, `OYZ()`, `XYO()` projection methods for zeroing specific components
- **Number formatting**: `FormatNumber()` for ordinal numbers (1st, 2nd, 3rd...)
- **Math comparisons**: `MeetsEquation()` with `MathEquation` enum for dynamic inequality checks
- **Array operations**: `GetAverage()` for float collections
- **String utilities**: `IsNullOrEmpty()` wrapper
- **Coroutine utilities**: `SafeStopCoroutine()` null-safe coroutine stopping

### Additional Data Structures

- **SerializableDictionary** (`Assets/SerializableDictionary/`): Third-party utility enabling Unity-serializable dictionaries
  - Standard C# dictionaries don't serialize in Unity Inspector
  - Use this when you need inspector-editable key-value pairs

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

- Open `Assets/Scenes/MainScene.unity` (primary game scene)
- Press Play button in Unity Editor
- Note: `SampleScene.unity` exists but `MainScene.unity` is the active game scene

### Working with ScriptableObjects

When creating new data assets:
- Right-click in Project window > Create > RANGER > [Creature/Shape]
- Store creature assets in `Assets/Systems/Creatures/Assets/`
- Store shape patterns organized by creature in `Assets/Systems/Creatures/Assets/Shapes/[CreatureName] - Attacks/`
- All ScriptableObjects use the `RANGER` menu prefix for consistency

**ShapeData Editor Workflow:**
1. Create new Shape asset via RANGER menu
2. Set `uniqueID` (important for pool management)
3. Adjust `gridSize` if needed (triggers automatic grid resize)
4. Select `currentColor` for painting
5. Click cells in the visual grid to design attack pattern
6. Left-click paints, right-click erases
7. Save the asset
8. Add to a CreatureData's `allyShapePool` or `enemyShapePool`

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

## Game Loop and Mechanics

**High-Level Flow:**
1. **Main Menu** → Player clicks "Start Game"
2. **Character Select** → Player chooses up to 3 creatures from owned collection for battle
3. **Battle Phase** → Turn-based grid combat
   - Enemy places a random shape on the shared grid (from `enemyShapePool`)
   - Player selects attack shapes for each ally (2 options per ally, from `allyShapePool`)
   - Player places selected shapes on grid to contest enemy territory
   - Click "GO" to resolve turn
4. **Resolution** → Damage calculated, health updated
5. **Victory/Defeat** → Win adds enemy to collection, defeat returns to menu
6. **Next Wave** → Face stronger enemies with expanded creature roster

**Core Mechanic - Grid Territory Control:**
- Shared 8x8 grid battlefield where both sides place colored shape patterns
- When shapes overlap, cells become contested
- Ally damage = number of grid cells under ally control
- Enemy damage = base damage per enemy cell + bonus for adjacent enemy cells
- Strategic placement to maximize territory and minimize damage taken

## Project Context

This is a **Ludum Dare 57 game jam project**, which means:
- Fast iteration is prioritized over architectural perfection
- ScriptableObjects are used for quick data editing without recompiling
- Third-party tools (Odin, DOTween) are used to accelerate development
- Code is kept simple and in the global namespace for rapid prototyping

# important-instruction-reminders
Do what has been asked; nothing more, nothing less.
NEVER create files unless they're absolutely necessary for achieving your goal.
ALWAYS prefer editing an existing file to creating a new one.
NEVER proactively create documentation files (*.md) or README files. Only create documentation files if explicitly requested by the User.
