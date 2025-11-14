# Dungeo

**A feature-rich console RPG built with AI-assisted development**

Dungeo is a turn-based RPG that showcases modern software architecture principles while delivering an engaging dungeon-crawling experience. Built entirely using Claude Code, this project demonstrates the power of AI-assisted development combined with clean architecture patterns.

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![Tests](https://img.shields.io/badge/tests-375%20passing-brightgreen)
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)
![License](https://img.shields.io/badge/license-MIT-blue)

---

## Features

### Core Gameplay
- **Three Unique Classes**: Warrior (tank/melee), Mage (ranged/magic), Rogue (agile/critical)
- **Dynamic Combat System**: Turn-based battles with abilities, status effects, and strategic AI
- **Procedural Equipment**: 9 equipment slots with 5 rarity tiers and special stat modifiers
- **Dungeon Progression**: 5 unique dungeons with increasing difficulty and boss encounters
- **Achievement System**: 27 achievements tracking various gameplay milestones
- **Save System**: 3 save slots with auto-save functionality

### Technical Highlights
- **Interface-Driven Architecture**: Swap UI implementations freely - console, automated AI, or future web/mobile interfaces
- **Event-Based Communication**: 30+ event types for loosely coupled systems
- **Data-Driven Design**: All content defined in JSON files - modify and expand the game without touching code
- **Comprehensive Testing**: 375 unit tests with 54% code coverage
- **AI Player Mode**: Fully automated gameplay for testing and simulation

---

## Easy Content Modification

**Want to add a new enemy?** Edit `Data/enemies.json` - no code changes needed.

**Want to rebalance combat?** Tweak values in `gameconfig.json` - instant results.

**Want new abilities?** Add them to `Data/abilities.json` - the game loads them automatically.

This data-driven design means you can:
- Add dozens of enemies in minutes
- Create entire new dungeons without programming
- Rebalance the entire game with JSON edits
- Design new abilities with complex effects
- Modify equipment generation rules

**The code is generic - the content is data.** This separation makes Dungeo incredibly easy to extend and modify.

---

## Technology Stack

- **Language**: C# (.NET 9.0)
- **Testing**: xUnit with Moq
- **Architecture**: SOLID principles, Dependency Injection, Event-Driven Design
- **Data Format**: JSON for all game content
- **Development**: Built with [Claude Code](https://claude.com/claude-code)

---

## Quick Start

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Installation

```bash
# Clone the repository
git clone https://github.com/rillyboss/Dungeo.git
cd Dungeo

# Build the project
dotnet build

# Run tests to verify installation
dotnet test
```

### Running the Game

```bash
# Play with console UI
dotnet run

# Watch AI play automatically
dotnet run -- --automated
# or
dotnet run -- -a
```

---

## How to Play

### Starting Out
1. Choose from three classes:
   - **Warrior**: High HP, strong defense, melee abilities
   - **Mage**: Powerful magic, elemental damage, mana management
   - **Rogue**: High critical chance, agile attacks, dodge abilities

2. Begin your adventure in the town hub:
   - **Dungeon**: Challenge procedurally generated encounters
   - **Shop**: Buy potions and equipment
   - **Rest**: Restore HP and mana
   - **Inventory**: Manage equipment and items

### Combat
- Use **abilities** for powerful attacks with cooldowns
- **Basic attacks** are always available
- Watch for **status effects** like poison, burn, and stun
- **Critical hits** can turn the tide of battle
- Strategic **ability timing** is key to victory

### Progression
- Defeat enemies to gain **experience** and **gold**
- **Level up** to increase stats and unlock new abilities
- Find and equip **better gear** from dungeon loot
- Complete **achievements** for bragging rights
- Progress through **dungeons** to face tougher challenges

---

## Game Systems

### Combat System
- **Turn-Based**: Tactical decision-making each turn
- **Ability System**: 6 abilities per class (3 starting + 3 unlockable)
- **Status Effects**: Burn, poison, stun, regen, shields, and more
- **Critical Hits**: Chance-based bonus damage
- **Enemy AI**: Context-aware decision making with ability priorities

### Equipment System
- **9 Equipment Slots**: Weapon, helmet, chest, legs, gloves, boots, ring, amulet, belt
- **5 Rarity Tiers**: Common → Uncommon → Rare → Epic → Legendary
- **Procedural Generation**: Randomized stats with prefix/suffix modifiers
- **Stat Scaling**: Higher rarity = better stat ranges

### Progression System
- **Experience & Leveling**: Gain XP from combat, level up for stat increases
- **Ability Unlocks**: Unlock 3 additional abilities as you level
- **Gold Economy**: Earn gold, spend on potions and equipment
- **Statistics Tracking**: 40+ statistics tracked across your adventure

### Dungeon System
- **5 Unique Dungeons**: Forest Ruins, Crystal Caves, Shadow Crypts, Volcanic Fortress, Void Citadel
- **Encounter Variety**: Regular enemies, elite encounters, and boss fights
- **Scaling Difficulty**: Each dungeon tier increases challenge and rewards
- **Boss Mechanics**: Unique boss abilities and multi-effect attacks

### Achievement System
- **27 Achievements**: Track milestones across combat, exploration, and progression
- **Categories**: Combat feats, exploration, wealth, legendary actions
- **Automatic Tracking**: Achievements unlock as you play

---

## Architecture Overview

Dungeo is built with professional software engineering practices:

### Design Patterns
- **Interface-Driven Design**: `IGameInterface` abstraction allows multiple UIs (Console, Automated, future Web/Unity)
- **Event-Driven Architecture**: Systems communicate through events, not direct calls
- **Repository Pattern**: Data access abstracted through `IDataRepository`
- **Manager Pattern**: Dedicated managers for Save, Dungeon, Progression, Statistics, Achievements
- **Factory Pattern**: Centralized entity and enemy creation

### SOLID Principles
- **Single Responsibility**: Each class has one clear purpose
- **Open/Closed**: Extend via JSON data, not code modification
- **Liskov Substitution**: Interface implementations are interchangeable
- **Interface Segregation**: Focused, minimal interfaces
- **Dependency Inversion**: Depend on abstractions, not concrete classes

### Key Architectural Benefits
- **Swap UIs Freely**: Game logic has zero UI dependencies - add web, mobile, or Unity interfaces
- **Fully Testable**: All dependencies injected, 375 tests validate behavior
- **Extend Without Code Changes**: Add content via JSON - new enemies, abilities, dungeons without recompiling
- **Maintainable**: Clear separation of concerns, predictable structure

```
Game Logic (GameCore)
        ↓
  IGameInterface
    ↙    ↓    ↘
Console  AI   Future
  UI   Player  UIs
```

---

## Project Structure

```
TestRPGGame/
├── GameCore.cs              # Main game engine
├── Interfaces/              # UI abstractions & events
├── Systems/                 # Managers (Save, Dungeon, Progression, etc.)
├── Combat/                  # Combat logic & status effects
├── Entities/                # Player, Enemy, Dungeon entities
├── Equipment/               # Equipment generation & stats
├── Abilities/               # Ability system
├── Data/                    # JSON content files
│   ├── classes.json
│   ├── abilities.json
│   ├── enemies.json
│   ├── bosses.json
│   ├── dungeons.json
│   ├── achievements.json
│   └── Items/              # Equipment generation data
├── UI/                     # ASCII art & console helpers
├── DataLoading/            # JSON data repository
└── Factories/              # Entity creation

TestRPGGame.Tests/          # 375 unit tests
AgenticContext/             # AI development documentation
```

---

## Current Status

### Completed Features
- ✅ Core game loop (explore → combat → loot → progress)
- ✅ Three balanced classes with 6 abilities each
- ✅ Turn-based combat with abilities and status effects
- ✅ Procedural equipment system (9 slots, 5 rarities)
- ✅ Dungeon system with 5 dungeons and boss fights
- ✅ Save/load system (3 slots, auto-save)
- ✅ Achievement system (27 achievements)
- ✅ Statistics tracking (40+ stats)
- ✅ Shop system with potions and equipment
- ✅ Interface-driven architecture (Console + Automated UIs)
- ✅ Comprehensive test suite (375 tests, 100% passing)

### In Active Development
See [CONTENT_EXPANSION_ROADMAP.md](AgenticContext/CONTENT_EXPANSION_ROADMAP.md) for detailed plans.

**Sprint 2: Equipment Enhancement**
- Equipment-granted abilities (make loot more interesting)
- 3-5x more equipment variety (more prefixes, suffixes, base types)
- Set bonuses (reward matching equipment pieces)

**Sprint 3: Combat Content Expansion**
- Triple enemy count (more variety per dungeon tier)
- 2-3x abilities per class (deeper combat options)
- Enhanced boss mechanics (more unique encounters)

**Sprint 4: Deep Systems**
- Skill tree system (branching progression choices)
- Legendary equipment mechanics (build-defining items)
- Challenge modes (hard mode, ironman, speedrun)
- Prestige system (new game+)

---

## Future Ambitions

### Short-Term (Next 3 Months)
- **Content Explosion**: 3-5x more enemies, abilities, and equipment
- **Equipment 2.0**: Set bonuses, equipment abilities, legendary mechanics
- **Enhanced Boss Fights**: Unique mechanics, phases, special abilities
- **Skill Trees**: Meaningful progression choices beyond leveling

### Mid-Term (6-12 Months)
- **New Classes**: Druid, Paladin, Necromancer
- **Advanced Systems**: Crafting, enchanting, gem socketing
- **Expanded World**: 10+ dungeons, branching paths, optional areas
- **Multiplayer Foundation**: Co-op dungeon runs (architecture already supports this)

### Long-Term Vision
- **Web UI**: Browser-based interface using existing game logic
- **Unity Integration**: 2D/3D visualization of the same core game
- **Mobile Port**: Touch-friendly interface implementation
- **Modding Support**: Community content creation tools
- **Live Services**: Daily challenges, seasonal events, leaderboards

The beauty of our interface-driven architecture is that **the core game logic never changes** - we just add new UI implementations!

---

## Modifying the Game

### Adding Content (No Code Required!)

The game is fully data-driven. Modify content by editing JSON files:

```bash
# Add new abilities
TestRPGGame/Data/abilities.json

# Add enemies
TestRPGGame/Data/enemies.json

# Add bosses with unique abilities
TestRPGGame/Data/bosses.json

# Create new dungeons
TestRPGGame/Data/dungeons.json

# Configure balance and economy
TestRPGGame/gameconfig.json
```

**Example: Adding a New Enemy**

Edit `Data/enemies.json`:
```json
{
  "id": "shadow_assassin",
  "name": "Shadow Assassin",
  "baseHealth": 80,
  "baseDamage": 18,
  "baseDefense": 5,
  "level": 7,
  "goldReward": 45,
  "experienceReward": 120,
  "abilities": ["backstab", "vanish"]
}
```

That's it! The enemy is now in the game.

### Development

```bash
# Run all tests
dotnet test

# Run specific test
dotnet test --filter "FullyQualifiedName~TestName"

# Detailed output
dotnet test --verbosity detailed
```

### Code Quality Standards
- All tests must pass before committing
- Follow SOLID principles
- All game data in JSON files
- Inject dependencies (don't `new` them)

### For AI Assistants
See [CLAUDE.md](CLAUDE.md) for AI coding assistant context and instructions.

---

## Testing

The project maintains high code quality through comprehensive testing:

- **375 Unit Tests**: All core systems validated
- **54% Code Coverage**: 3,269/6,042 lines covered
- **100% Pass Rate**: Tests run on every build
- **Mocked Dependencies**: Clean, fast test execution
- **AAA Pattern**: Arrange, Act, Assert structure

Test Categories:
- Data Loading & Validation
- Player Mechanics & Progression
- Combat System & Status Effects
- Equipment Generation & Stats
- Boss Abilities & Encounters
- Save/Load Functionality
- Dungeon Progression
- Achievement System
- Statistics Tracking

---

## Documentation

Comprehensive documentation in `AgenticContext/`:

- **[ARCHITECTURE.md](AgenticContext/ARCHITECTURE.md)**: Technical architecture deep-dive
- **[CONTENT_EXPANSION_ROADMAP.md](AgenticContext/CONTENT_EXPANSION_ROADMAP.md)**: Development roadmap
- **[SESSION_GUIDE.md](AgenticContext/SESSION_GUIDE.md)**: Quick reference for common tasks
- **[DEVELOPMENT_LOG.md](AgenticContext/DEVELOPMENT_LOG.md)**: Historical development decisions

---

## Contributing

This is a personal learning project, but feedback and suggestions are welcome!

### Want to Contribute?
1. Fork the repository
2. Create a feature branch
3. Follow existing code patterns
4. Write tests for new features
5. Ensure all tests pass
6. Submit a pull request

### Development Setup
```bash
# Clone your fork
git clone https://github.com/your-username/Dungeo.git
cd Dungeo

# Create a branch
git checkout -b feature/your-feature

# Make changes and test
dotnet test

# Commit using conventional format
git commit -m "feat: your feature description"
```

---

## License

MIT License - see LICENSE file for details

---

## Acknowledgments

- **Built with [Claude Code](https://claude.com/claude-code)**: Demonstrates AI-assisted development
- **Inspired by**: Classic console RPGs, roguelikes, and dungeon crawlers
- **Special Thanks**: To the .NET and xUnit communities for excellent tools

---

## Project Stats

- **C# Source Files**: 128
- **Unit Tests**: 375 (100% passing)
- **Code Coverage**: 54% (3,269/6,042 lines)
- **Game Content**: 100% data-driven JSON
- **Architecture**: Fully interface-driven for UI flexibility

---

## Contact

**Developer**: Billy
**Repository**: [https://github.com/rillyboss/Dungeo](https://github.com/rillyboss/Dungeo)
**Built With**: [Claude Code](https://claude.com/claude-code)

---

**Ready to explore the dungeons? Start your adventure today!**

```bash
dotnet run
```
