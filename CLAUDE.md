# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity 6000.3.11f1 dungeon-crawler action RPG. C# scripts live in `Assets/Scripts/`. The project uses the Universal Render Pipeline, Cinemachine for cameras, Unity Input System for input, NavMesh for pathfinding, and CsvHelper for loading game data from CSV files.

## Commands

**Code formatting:**
```
dotnet csharpier Assets/Scripts
```

**Build and run tests:** Done through the Unity Editor — no CLI build scripts exist.  
**Test files:** `Assets/Scripts/Csv/DataTableManagerTest.cs`, `Assets/Scripts/UI/Dungeon/Inventory/ItemSlotListTest.cs`

## Architecture

### Module Layout (`Assets/Scripts/`)

| Folder | Responsibility |
|--------|----------------|
| `CombatSystem/` | Shared damage/health base classes and status effects |
| `Player/` | Player input, movement, attack combo, skills, inventory |
| `Monster/` | Enemy AI state machines and boss implementations |
| `Map/` | Procedural generation, spawning, chest/portal logic |
| `UI/` | Shop, inventory slots, health/mana display, title screen |
| `Csv/` | Singleton `DataTableManager` that loads CSV data tables |
| `SceneManager/` | Scene loading helpers and menu UI |

### Core Inheritance Chain

```
LivingEntity (CombatSystem)
├── PlayerStatus (Player)
└── BaseMonster (Monster)
```

`LivingEntity` owns `Health`, `OnDamage()`, `OnDead` event, and invincibility logic. All damageable objects implement `IDamagable`.

### Player System

- **PlayerInput** — reads Unity Input System actions; exposes booleans/axes consumed by sibling components
- **PlayerMovement** — applies velocity to `Rigidbody` based on input; handles rotation via `PlayerRotation`
- **PlayerAttack** — 3-hit combo; drives Animator and spawns `HitBox` colliders per swing
- **PlayerSkill** — 4 skills mapped to Q/W/E/R; each slot has a mana cost and cooldown; reads mana from `PlayerStatus`
- **PlayerInventory** / **PlayerInteractive** — inventory management and chest/portal interaction (key: `Tab` / `F`)

### Monster AI State Machine

All monsters extend `BaseMonster` which implements:
**Idle → Alert → Trace → Attack → Return → Dead**

- `SimpleMonster` — sphere-range detection
- `SensoryMonster` — field-of-view angle detection
- `AnubisMonster`, `DragonBoss`, `Mushroom`, `RockMonster` — concrete bosses/enemies overriding attack behavior

Pathfinding uses Unity `NavMeshAgent`.

### Map Generation

`RandomMapGenerator` creates a grid of tiles; `MonsterRandomSpawner` and `RandomItemGenerator` place enemies and loot after the map is built. `ExitPortal` triggers scene transition via `SceneLoader`.

### Data Tables

`DataTableManager` (singleton) reads CSV files from `Assets/Resources/DataTables/` using CsvHelper and exposes typed tables (e.g., `ItemTable`). Access via `DataTableManager.Instance.GetTable<T>()`.

### Status Effects

Burn, electric, and frostbite are tracked in `StatusEffectData` and applied via coroutines in `LivingEntity`. Each effect has tick damage and a duration.

## Key Constraints

- `.meta`, `.unity`, `.csproj`, `.slnx` files must not be edited manually — managed by Unity. `.claude/settings.json` enforces read/write blocks on these at the tool level.
- `Library/`, `Temp/`, `Logs/`, `obj/` directories are generated; `.claude/settings.json` blocks tool access to them.
- Code may contain a mix of English and Korean comments.
