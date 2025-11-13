# Interface-Driven Architecture Refactor

## Overview

This document describes the major architectural refactor that separates game logic from presentation, allowing the game to be played through multiple interfaces (Console, Automated AI, future: Web API, Unity, Slack bots, etc.).

## Architecture

### Before

```
Game.cs → Console I/O directly embedded
CombatSystem.cs → Console I/O directly embedded
Shop.cs → Console I/O directly embedded
```

**Problem**: Tightly coupled to Console, impossible to test or use alternative interfaces.

### After

```
┌─────────────────────────────────────────────────────────────┐
│                       GameCore                              │
│          (Pure game logic, no UI dependencies)              │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           │ Uses IGameInterface
                           │
        ┌──────────────────┼──────────────────┬───────────────┐
        │                  │                  │               │
┌───────▼────────┐ ┌───────▼────────┐ ┌──────▼──────┐ ┌──────▼──────┐
│    Console     │ │   Automated    │ │  Web API    │ │    Unity    │
│   Interface    │ │   Interface    │ │  (future)   │ │   (future)  │
└────────────────┘ └────────────────┘ └─────────────┘ └─────────────┘
```

## Key Components

### 1. IGameInterface

The contract between game logic and presentation.

**Responsibilities:**
- Receive events from the game (via `OnEvent<T>()`)
- Provide player input when requested by the game

**Location**: `TestRPGGame/Interfaces/IGameInterface.cs`

```csharp
public interface IGameInterface
{
    // Events from game
    void OnEvent<T>(T gameEvent) where T : class;

    // Input requests from game
    (string name, PlayerClass playerClass) RequestCharacterCreation();
    CombatAction RequestCombatAction(CombatState state);
    MainMenuChoice RequestMainMenuChoice(...);
    // ... more input requests
}
```

### 2. GameEvents

All possible events the game can publish.

**Location**: `TestRPGGame/Interfaces/GameEvents.cs`

**Event Categories:**
- **Lifecycle**: GameStarted, GameEnded
- **Character**: CharacterCreated, PlayerLeveledUp, PlayerStatsChanged
- **Combat**: CombatStarted, CombatTurnStart, DamageDealt, AbilityUsed, CombatEnded
- **Items**: ItemReceived, ItemEquipped, PotionUsed
- **Dungeons**: DungeonEntered, DungeonCompleted
- **System**: GameSaved, GameLoaded, InfoMessage

### 3. GameCore

Pure game logic engine that orchestrates everything.

**Location**: `TestRPGGame/GameCore.cs`

**Key Features:**
- No Console dependencies
- Publishes events for all state changes
- Requests input through interface
- Manages game flow and state

### 4. InterfacedCombatSystem

Combat logic separated from UI.

**Location**: `TestRPGGame/Combat/InterfacedCombatSystem.cs`

**Features:**
- Turn-based combat logic
- Publishes granular combat events
- Requests player actions through interface
- No direct Console output

### 5. ConsoleInterface

The original console UI experience, refactored.

**Location**: `TestRPGGame/Interfaces/ConsoleInterface.cs`

**Features:**
- Displays events with colors and ASCII art
- Handles console input
- Maintains original game feel

### 6. AutomatedInterface

AI-driven interface for testing and playthrough.

**Location**: `TestRPGGame/Interfaces/AutomatedInterface.cs`

**Features:**
- Programmatic decision making
- Logs all events and decisions
- Configurable strategies
- Perfect for AI playthroughs and testing

## Usage

### Running with Console Interface (Default)

```bash
dotnet run
```

### Running with Automated Interface

```bash
dotnet run -- --automated
# or
dotnet run -- -a
```

### Running Original Game (Backwards Compatibility)

```bash
dotnet run -- --old
# or
dotnet run -- -o
```

## Automated Playthrough Example

```bash
$ dotnet run -- --automated
```

**Output:**
```
=== GAME STARTED ===

Decision: Create new character in slot 2
Decision: Create 'AIHero' as Warrior
Character Created: AIHero the Warrior
  HP: 152, Mana: 80

=== COMBAT 1: Goblin (Lvl 1) ===
--- Turn 1 ---
  Player: 152/152 HP, 80/80 Mana
  Enemy: 51/51 HP
  Player Action: Use Ability: Battle Rage
  Player used Battle Rage! (30 mana)
  Goblin → Player: 2 Physical damage

...

*** VICTORY! ***
  Gained: 28 gold, 50 XP
  Loot: [Common] Leather Mitts

🎉 LEVEL UP! → Level 2
```

## Benefits

### 1. **Testability**
- Can now write unit tests with mock interfaces
- Can replay scenarios programmatically
- Can validate game balance automatically

### 2. **Multiple Frontends**
Easy to add new interfaces:
- **Web API** → RESTful endpoints for browser-based play
- **Unity Client** → 3D visualization
- **Slack Bot** → Play through chat
- **Discord Bot** → Multiplayer social experience
- **Terminal UI** → Rich terminal with ncurses

### 3. **AI & Analytics**
- AI agents can play the game
- Automated balance testing
- Generate gameplay analytics
- Simulate thousands of playthroughs

### 4. **Debugging**
- Can log full game sessions
- Replay exact sequences of events
- Step through game logic without UI noise

## Strategy Pattern

The AutomatedInterface uses Strategy pattern for decision-making:

```csharp
public abstract class AutomatedStrategy
{
    public abstract (string name, PlayerClass playerClass) ChooseCharacterClass();
    public abstract MainMenuChoice ChooseMainMenuAction(...);
    public abstract CombatAction ChooseCombatAction(CombatState state);
}
```

**Example Strategies:**
- `DefaultStrategy` - Cautious, methodical approach
- `AggressiveStrategy` - Always attack, use abilities frequently
- `DefensiveStrategy` - Prioritize survival, use potions early
- `SpeedrunStrategy` - Optimize for fastest completion

## Future Enhancements

### Short Term
1. Complete Shop and Inventory interfaces
2. Add more event types for status effects
3. Implement full dungeon system
4. Create additional AI strategies

### Medium Term
1. Build REST API interface
2. Create web-based client
3. Add replay/recording system
4. Implement event sourcing for save/load

### Long Term
1. Multiplayer support through interfaces
2. Unity 3D client
3. Mobile app interface
4. AI training for optimal strategies

## Compatibility

- **Original Game**: Still works with `--old` flag
- **Save Files**: Compatible between all interfaces
- **Data Files**: No changes to JSON data format

## Technical Notes

### Event Flow

```
GameCore.EnterCombat()
  → InterfacedCombatSystem.StartBattle()
    → OnEvent(CombatStartedEvent)
      → ConsoleInterface displays enemy
      OR AutomatedInterface logs enemy
    → RequestCombatAction(state)
      ← ConsoleInterface asks user
      OR AutomatedInterface uses strategy
    → OnEvent(DamageDealtEvent)
      → Interface handles display
    → OnEvent(CombatEndedEvent)
      → Interface shows results
  → GameCore continues
```

### Adding a New Interface

1. Create class implementing `IGameInterface`
2. Implement `OnEvent<T>()` to handle game events
3. Implement `Request*()` methods to provide input
4. Add command-line flag in `Program.cs`

Example minimal interface:

```csharp
public class WebSocketInterface : IGameInterface
{
    private WebSocket socket;

    public void OnEvent<T>(T gameEvent)
    {
        // Send event to client via WebSocket
        socket.Send(JsonSerializer.Serialize(gameEvent));
    }

    public CombatAction RequestCombatAction(CombatState state)
    {
        // Wait for client response via WebSocket
        var json = socket.Receive();
        return JsonSerializer.Deserialize<CombatAction>(json);
    }

    // ... implement other Request* methods
}
```

## Conclusion

This refactor successfully decouples game logic from presentation, opening up endless possibilities for how the game can be played and tested. The architecture is clean, extensible, and maintains backwards compatibility with the original console experience.

**Key Achievement**: Claude AI can now play and analyze the game through the AutomatedInterface! 🎉
