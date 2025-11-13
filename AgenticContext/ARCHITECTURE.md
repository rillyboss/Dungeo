# TestRPGGame - Unified Architecture

**Last Updated**: 2025-11-10
**Status**: Production Ready

---

## Overview

TestRPGGame now uses a **unified, interface-driven architecture** where all game experiences flow through the same core engine (`GameCore`) with pluggable interfaces.

```
┌─────────────────────────────────────────────────────────┐
│                     GameCore                            │
│  (Pure game logic - combat, progression, dungeons)     │
└───────────────┬─────────────────────────────────────────┘
                │
                │ IGameInterface
                │
    ┌───────────┼──────────────┬────────────────┐
    │           │              │                │
┌───▼────┐ ┌───▼────┐  ┌──────▼──────┐  ┌──────▼──────┐
│Console │ │Automated│  │   Web API   │  │    Unity    │
│   UI   │ │   AI    │  │   (future)  │  │   (future)  │
└────────┘ └─────────┘  └─────────────┘  └─────────────┘
```

---

## How to Run

### Default: Console Experience
```bash
dotnet run
```
Uses: `GameCore + ConsoleInterface`

### AI Playthrough
```bash
dotnet run -- --automated
# or
dotnet run -- -a
```
Uses: `GameCore + AutomatedInterface`

### Legacy Mode (Deprecated)
```bash
dotnet run -- --old
# or
dotnet run -- -o
```
Uses: Old `Game.cs` (pre-refactor code)
⚠️ **Not recommended** - Only for backwards compatibility

---

## Architecture Layers

### 1. GameCore (Engine)

**Location**: `GameCore.cs`

**Responsibilities:**
- Game state management
- Combat orchestration
- Progression (leveling, gold, XP)
- Dungeon progression
- Save/Load system
- Event publishing

**Key Feature**: Zero Console dependencies - all I/O goes through `IGameInterface`

**What it does**:
- Publishes events: `OnEvent(CombatStartedEvent)`, `OnEvent(PlayerLeveledUp)`, etc.
- Requests input: `RequestCombatAction()`, `RequestMainMenuChoice()`, etc.
- Manages state: player, dungeons, save slots, combat system

### 2. IGameInterface (Contract)

**Location**: `Interfaces/IGameInterface.cs`

**Purpose**: Defines how game logic communicates with the outside world

**Two-way contract:**

#### A. Events OUT (Game → Interface)
```csharp
void OnEvent<T>(T gameEvent) where T : class;
```

Examples:
- `OnEvent(new CombatStartedEvent { EnemyName = "Goblin", ... })`
- `OnEvent(new DamageDealtEvent { Attacker = "Player", Damage = 50 })`
- `OnEvent(new PlayerLeveledUpEvent { NewLevel = 5 })`

#### B. Input IN (Interface → Game)
```csharp
MainMenuChoice RequestMainMenuChoice(...);
CombatAction RequestCombatAction(...);
bool RequestConfirmation(string message);
```

### 3. Implementations

#### ConsoleInterface (Default)

**Location**: `Interfaces/ConsoleInterface.cs`

**What it does:**
- Displays events with colors and ASCII art
- Prompts user for input via Console
- Shows menus, combat UI, character art
- Handles keyboard input

**Event Handling Example:**
```csharp
case GameEvents.CombatStartedEvent e:
    Console.Clear();
    AsciiArt.DrawEnemy(e.EnemyName);
    UIHelper.PrintColoredLine($"BATTLE: {e.EnemyName}", ConsoleColor.Yellow);
    break;
```

**Input Example:**
```csharp
public MainMenuChoice RequestMainMenuChoice(...)
{
    Console.WriteLine("1. ⚔️  Enter Combat");
    Console.WriteLine("2. 🏪 Visit Shop");
    // ...
    string choice = Console.ReadLine();
    return ParseChoice(choice);
}
```

#### AutomatedInterface (AI)

**Location**: `Interfaces/AutomatedInterface.cs`

**What it does:**
- Makes decisions programmatically (no human input)
- Logs all events to a string
- Uses strategy pattern for AI behavior
- Perfect for testing and analysis

**Strategy Examples:**
- `DefaultStrategy` - Cautious play, stops after 3 combats
- `AggressiveStrategy` - Always attack, use high-damage abilities
- `DefensiveStrategy` - Prioritize survival, use potions early
- `SpeedrunStrategy` - Optimize for fastest completion

**Event Logging:**
```csharp
case GameEvents.DamageDealtEvent e:
    Log($"{e.Attacker} → {e.Target}: {e.Damage} damage");
    break;
```

**Decision Making:**
```csharp
public override CombatAction ChooseCombatAction(CombatState state)
{
    if (state.PlayerCurrentHP < state.PlayerMaxHP * 0.3)
        return UsePotion();

    return UseStrongestAbility();
}
```

---

## System Integration

### Fully Integrated Systems

✅ **Combat** - 100% interface-driven
- Uses `InterfacedCombatSystem`
- All combat events published
- Player decisions requested through interface
- No Console dependencies

✅ **Main Menu** - Interface-driven
- Publishes player stats
- Requests menu choices
- Handles all navigation

✅ **Dungeons** - Mostly interface-driven
- Dungeon selection through interface
- Combat encounters use InterfacedCombatSystem
- Progression events published

✅ **Save/Load** - Interface-driven
- Save slot selection through interface
- Save events published
- Auto-save notifications

✅ **Ability Unlocking** - Interface-driven
- Lists locked abilities
- Requests unlock decisions
- Publishes unlock events

### Temporarily Hybrid Systems

⚠️ **Shop** - Hybrid approach
- Accessible through GameCore
- Uses existing `Shop.Enter(player)` with embedded Console UI
- Auto-saves after shop visit
- **Future**: Extract to interface methods

⚠️ **Inventory** - Hybrid approach
- Accessible through GameCore
- Uses existing `player.Inventory.DisplayInventory()`
- Has embedded Console UI
- **Future**: Extract to interface methods

⚠️ **Character Sheet** - Hybrid approach
- Uses existing `player.DisplayCharacterSheet()`
- Has embedded Console UI
- **Future**: Extract to interface methods

**Why hybrid?**
These systems already work perfectly. Rather than rewrite them immediately, we pragmatically reuse the existing UI code. Future refactoring can extract them to pure events.

---

## Data Flow Example

### Combat Flow

```
1. GameCore.EnterCombat()
   ↓
2. InterfacedCombatSystem.StartBattle(player, enemy)
   ↓
3. OnEvent(CombatStartedEvent { EnemyName: "Goblin" })
   ↓
4. ConsoleInterface displays enemy art and stats
   ↓
5. OnEvent(CombatTurnStartEvent { TurnNumber: 1, ... })
   ↓
6. ConsoleInterface displays turn info
   ↓
7. RequestCombatAction(state)
   ↓
8. ConsoleInterface shows menu, gets user choice
   ↓
9. Returns: CombatAction { UseAbility, abilityIndex: 2 }
   ↓
10. Execute ability, OnEvent(AbilityUsedEvent)
    ↓
11. ConsoleInterface displays ability animation
    ↓
12. OnEvent(DamageDealtEvent { Attacker: "Player", Damage: 50 })
    ↓
13. ConsoleInterface shows damage
    ↓
14. Enemy turn, OnEvent(DamageDealtEvent { Attacker: "Goblin", Damage: 5 })
    ↓
15. OnEvent(CombatEndedEvent { PlayerVictory: true, GoldEarned: 30 })
    ↓
16. ConsoleInterface shows victory screen
```

---

## Event System

### 30+ Event Types

**Lifecycle**:
- GameStartedEvent
- GameEndedEvent
- GameSavedEvent
- GameLoadedEvent

**Character**:
- CharacterCreatedEvent
- PlayerLeveledUpEvent
- PlayerStatsChangedEvent
- AbilityUnlockedEvent

**Combat**:
- CombatStartedEvent
- CombatTurnStartEvent
- AbilityUsedEvent
- DamageDealtEvent
- EffectAppliedEvent
- PotionUsedEvent
- CombatEndedEvent

**Items**:
- ItemReceivedEvent
- ItemEquippedEvent
- ItemPurchasedEvent
- ItemSoldEvent

**Dungeons**:
- DungeonEnteredEvent
- DungeonEncounterEvent
- DungeonCompletedEvent

**System**:
- InfoMessageEvent (Info, Success, Warning, Error)

### Event Properties

Events are **rich data structures**:

```csharp
public class DamageDealtEvent
{
    public string Attacker { get; set; }      // Who dealt damage
    public string Target { get; set; }        // Who received damage
    public int Damage { get; set; }           // Amount
    public bool IsCritical { get; set; }      // Was it a crit?
    public string AttackType { get; set; }    // Physical/Magic
}
```

This allows interfaces to:
- Display damage with context
- Show critical hit animations
- Color-code by attack type
- Log structured data for analysis

---

## Extension Points

### Adding a New Interface

1. **Create class** implementing `IGameInterface`
2. **Implement OnEvent<T>()** - handle game events
3. **Implement Request*()** methods - provide player input
4. **Add to Program.cs** - wire up with command-line flag

**Example: Web API Interface**

```csharp
public class WebAPIInterface : IGameInterface
{
    private readonly HttpClient client;

    public void OnEvent<T>(T gameEvent)
    {
        // POST event to web client
        var json = JsonSerializer.Serialize(gameEvent);
        client.PostAsync("/api/game/events", json);
    }

    public CombatAction RequestCombatAction(CombatState state)
    {
        // Wait for client HTTP request
        var response = WaitForClientAction();
        return JsonSerializer.Deserialize<CombatAction>(response);
    }

    // ... implement other Request methods
}
```

**Usage:**
```bash
dotnet run -- --api
```

### Adding New Events

1. **Define event class** in `GameEvents.cs`
2. **Publish from GameCore** or combat system
3. **Handle in interfaces** (optional - interfaces can ignore unknown events)

**Example:**
```csharp
// 1. Define
public class EquipmentBrokeEvent
{
    public string ItemName { get; set; }
    public string Slot { get; set; }
}

// 2. Publish
gameInterface.OnEvent(new EquipmentBrokeEvent {
    ItemName = "Iron Sword",
    Slot = "Weapon"
});

// 3. Handle (ConsoleInterface)
case GameEvents.EquipmentBrokeEvent e:
    UIHelper.PrintColoredLine($"💔 Your {e.ItemName} broke!", ConsoleColor.Red);
    break;
```

---

## Benefits of This Architecture

### 🎯 Separation of Concerns
- **Game logic** in GameCore
- **UI logic** in interfaces
- **Data** in JSON files

### 🧪 Testability
- Run automated playthroughs
- Simulate thousands of games
- Balance testing via AI
- Unit test game logic without UI

### 🔌 Extensibility
- Add new interfaces easily
- Multiple UIs for same game
- Community can create interfaces
- Modding support

### 📊 Analytics
- Log all events
- Analyze player behavior
- Track balance metrics
- Generate gameplay reports

### 🤖 AI Integration
- AI agents can play the game
- Training data generation
- Automated QA testing
- Difficulty tuning

### 🌐 Multi-Platform
- Console, Web, Mobile, Unity
- Same game logic everywhere
- UI customized per platform
- Consistent game behavior

---

## Future Enhancements

### Short Term
1. ✅ ~~Fix ability selection~~ (DONE)
2. ✅ ~~Data-driven enemy art~~ (DONE)
3. ✅ ~~Unify console to use GameCore~~ (DONE)
4. ⏳ Extract Shop UI to interface methods
5. ⏳ Extract Inventory UI to interface methods
6. ⏳ Extract Character Sheet UI to interface methods

### Medium Term
1. Web API interface for browser play
2. Event replay system (record/playback)
3. More AI strategies (aggressive, defensive, speedrun)
4. Multiplayer PvP foundation

### Long Term
1. Unity 3D client
2. Mobile app interfaces
3. Twitch integration
4. Community mod support

---

## Code Organization

```
TestRPGGame/
├── GameCore.cs                  # Main game engine
├── Program.cs                   # Entry point & interface selection
├── Game.cs                      # DEPRECATED - old monolithic code
│
├── Interfaces/
│   ├── IGameInterface.cs        # Interface contract
│   ├── GameEvents.cs            # All event definitions
│   ├── ConsoleInterface.cs      # Default console UI
│   └── AutomatedInterface.cs    # AI player
│
├── Combat/
│   ├── CombatSystem.cs          # Old combat (used by Game.cs)
│   └── InterfacedCombatSystem.cs # New combat (used by GameCore)
│
├── Systems/
│   ├── Shop.cs                  # Shop logic (has embedded UI)
│   ├── SaveSystem.cs            # Save/load system
│   └── GameConfig.cs            # Configuration
│
├── Entities/
│   ├── Player/                  # Player character
│   ├── Enemy/                   # Enemies and AI
│   └── Dungeon/                 # Dungeon system
│
├── DataLoading/
│   ├── DataLoader.cs            # Central data loading
│   ├── EnemyArtData.cs          # Enemy ASCII art
│   └── [Data classes]
│
├── UI/
│   ├── AsciiArt.cs             # ASCII art rendering
│   └── UIHelper.cs             # Console helpers
│
└── Data/
    ├── abilities.json
    ├── enemies.json
    ├── classes.json
    ├── Enemies/
    │   └── enemy-art.json       # Data-driven enemy art
    └── Items/
```

---

## Compatibility

### Backwards Compatibility

✅ **Old saves work** - Save format unchanged
✅ **Old game accessible** - Use `--old` flag
✅ **All data files compatible** - No JSON changes needed
✅ **Tests still pass** - Unit tests unaffected

### Migration Path

If you have custom modifications to `Game.cs`:

1. **Short term**: Continue using `--old` flag
2. **Medium term**: Port changes to `GameCore.cs`
3. **Long term**: Contribute to interface extraction

---

## Summary

TestRPGGame now has a **production-ready, unified architecture**:

✅ **Single code path** - Everyone uses GameCore
✅ **Pluggable interfaces** - Console, AI, future: Web, Unity
✅ **Event-driven** - Rich, structured events
✅ **Testable** - AI can play and analyze
✅ **Extensible** - Easy to add features
✅ **Pragmatic** - Reuses existing working code

**The console experience now eats its own dog food** - it uses the same GameCore architecture that enables AI playthrough, future web interfaces, and more.

---

**Want to contribute?**
- Add new interfaces
- Extract Shop/Inventory UI
- Create new AI strategies
- Design new event types
- Improve documentation

---

Built with ❤️ using interface-driven architecture
