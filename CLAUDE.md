# CLAUDE.md - AI Coding Assistant Context

**Project:** TestRPGGame
**Purpose:** Console RPG built with Claude Code to demonstrate AI-assisted development capabilities
**Architecture:** Interface-driven, event-based, data-driven
**Status:** Production-ready foundation with active content expansion
**Last Updated:** 2025-01-12

---

## Quick Start for AI Assistants

### Project Overview
TestRPGGame is a feature-rich console RPG with:
- **3 playable classes** (Warrior, Mage, Rogue)
- **Turn-based combat** with abilities, status effects, and critical hits
- **Procedural equipment** with 5 rarity tiers and special effects
- **Dungeon system** with progression, bosses, and encounters
- **Multiple interfaces** (Console for humans, Automated for AI)
- **Data-driven design** (all content in JSON files)
- **Comprehensive testing** (450 tests, 100% passing)

### Technology Stack
- **Language:** C# (.NET 9.0)
- **Testing:** xUnit with Moq
- **Architecture:** SOLID principles, DI, event-driven
- **Tools:** Built entirely with Claude Code

---

## Essential Files to Read First

### Architecture & Design
1. **AgenticContext/ARCHITECTURE.md** - Complete technical architecture
2. **AgenticContext/README.md** - Feature overview and project structure
3. **AgenticContext/CONTENT_EXPANSION_ROADMAP.md** - Current development priorities

### For Quick Tasks
4. **AgenticContext/SESSION_GUIDE.md** - Quick reference for common operations
5. **AgenticContext/MULTIPLE_EFFECTS_GUIDE.md** - How to add abilities/effects

### For Major Refactoring
6. **AgenticContext/CODE_IMPROVEMENT_ROADMAP.md** - Completed architectural improvements

---

## Architecture Principles

### 1. Interface-Driven Design
**Core Pattern:** Game logic communicates through IGameInterface abstraction

```
┌─────────────────────────────────────────┐
│            GameCore                     │
│    (Pure game logic - no Console)      │
└─────────────┬───────────────────────────┘
              │ IGameInterface
              │
    ┌─────────┼─────────┬────────────┐
    │         │         │            │
┌───▼───┐ ┌──▼───┐ ┌───▼────┐ ┌────▼────┐
│Console│ │  AI  │ │  Web   │ │  Unity  │
│  UI   │ │Agent │ │  API   │ │(future) │
└───────┘ └──────┘ └────────┘ └─────────┘
```

**Key Files:**
- `TestRPGGame/GameCore.cs` - Main game engine
- `TestRPGGame/Interfaces/IGameInterface.cs` - Interface contract
- `TestRPGGame/Interfaces/ConsoleInterface.cs` - Default UI
- `TestRPGGame/Interfaces/AutomatedInterface.cs` - AI player

### 2. Event-Driven Communication
**Pattern:** GameCore publishes 30+ event types, interfaces handle display

**Common Events:**
- `CombatStartedEvent`, `CombatEndedEvent`, `DamageDealtEvent`
- `PlayerLeveledUpEvent`, `AbilityUnlockedEvent`
- `ItemReceivedEvent`, `ItemEquippedEvent`
- `AchievementUnlockedEvent`, `GameSavedEvent`

**Event Location:** `TestRPGGame/Interfaces/GameEvents.cs`

### 3. Data-Driven Design
**Pattern:** All game content defined in JSON, code is generic

**Data Files:**
- `TestRPGGame/Data/classes.json` - Class definitions
- `TestRPGGame/Data/abilities.json` - Player abilities
- `TestRPGGame/Data/enemies.json` - Regular enemies
- `TestRPGGame/Data/bosses.json` - Boss enemies with abilities
- `TestRPGGame/Data/dungeons.json` - Dungeon configurations
- `TestRPGGame/Data/achievements.json` - Achievement system
- `TestRPGGame/Data/Items/*.json` - Equipment generation data
- `TestRPGGame/gameconfig.json` - Balance tuning

### 4. Dependency Injection
**Pattern:** All dependencies injected, fully testable

**Key Abstractions:**
- `IDataRepository` - Data loading
- `ILogger` - Logging
- `IGameInterface` - UI abstraction

### 5. Manager Pattern
**Pattern:** Each major system has a dedicated manager

**Managers:**
- `SaveManager` - Save/load system (3 save slots)
- `DungeonManager` - Dungeon progression
- `ProgressionManager` - Leveling, gold, experience
- `StatisticsManager` - Player statistics tracking
- `AchievementManager` - Achievement system

---

## Project Structure

```
TestRPGGame/
├── GameCore.cs                  # Main game engine (240 lines)
├── Program.cs                   # Entry point, interface selection
│
├── Interfaces/
│   ├── IGameInterface.cs        # Core abstraction
│   ├── GameEvents.cs            # All event definitions
│   ├── ConsoleInterface.cs      # Console UI
│   └── AutomatedInterface.cs    # AI player
│
├── Systems/
│   ├── SaveManager.cs           # Save/load
│   ├── DungeonManager.cs        # Dungeon progression
│   ├── ProgressionManager.cs    # XP/leveling
│   ├── StatisticsManager.cs     # Statistics tracking
│   ├── AchievementManager.cs    # Achievements
│   ├── Shop.cs                  # Shop system
│   └── GameConfig.cs            # Configuration
│
├── Combat/
│   ├── InterfacedCombatSystem.cs  # Event-driven combat
│   ├── AttackTypeSystem.cs        # Damage types
│   ├── EnemyAI.cs                 # Enemy decision making
│   └── StatusEffects/             # Status effect implementations
│
├── Entities/
│   ├── Player/
│   │   ├── Player.cs              # Player entity
│   │   ├── PlayerInventory.cs     # Inventory management
│   │   └── PlayerStatistics.cs    # Statistics tracking
│   ├── Enemy/
│   │   ├── Enemy.cs               # Enemy entity
│   │   └── Boss.cs                # Boss mechanics
│   └── Dungeon/
│       ├── Dungeon.cs             # Dungeon definition
│       └── DungeonRunner.cs       # Dungeon execution
│
├── Equipment/
│   ├── EquipmentItem.cs           # Equipment entity
│   ├── EquipmentGenerator.cs      # Procedural generation
│   └── StatGenerators/            # Slot-specific generators
│
├── Abilities/
│   ├── Ability.cs                 # Ability definition
│   └── Effects/                   # Effect implementations
│
├── DataLoading/
│   ├── IDataRepository.cs         # Data access abstraction
│   ├── JsonDataRepository.cs      # JSON implementation
│   └── [Data models]
│
├── Factories/
│   ├── EntityFactory.cs           # Create game entities
│   └── EnemyFactory.cs            # Enemy creation
│
├── UI/
│   ├── AsciiArt.cs               # ASCII art rendering
│   └── UIHelper.cs               # Console helpers
│
└── Data/
    ├── classes.json
    ├── abilities.json
    ├── enemies.json
    ├── bosses.json
    ├── dungeons.json
    ├── achievements.json
    ├── gameconfig.json
    ├── Enemies/
    │   └── enemy-art.json
    └── Items/
        ├── weapon-prefixes.json
        ├── weapon-suffixes.json
        ├── armor-prefixes.json
        ├── armor-suffixes.json
        └── rarity-multipliers.json

TestRPGGame.Tests/              # 423 unit tests, 100% passing
```

---

## Common Tasks

### Running the Game
```bash
# Console UI (default)
dotnet run

# AI player (automated)
dotnet run -- --automated
dotnet run -- -a

# Run tests
dotnet test
```

### Adding New Content

**New Ability:**
1. Edit `Data/abilities.json`
2. Add new ability definition with effects
3. No code changes needed!

**New Enemy:**
1. Edit `Data/enemies.json`
2. Add enemy stats and abilities
3. Automatically available in random encounters

**New Boss:**
1. Edit `Data/bosses.json`
2. Define boss abilities with multiple effects
3. Reference in dungeon configuration

**New Achievement:**
1. Edit `Data/achievements.json`
2. Define conditions and rewards
3. Automatically tracked

### Balance Tuning
Edit `gameconfig.json` for:
- Combat parameters (max turns, flee chance, etc.)
- Economy (shop prices, potion costs, etc.)
- Regen rates (HP, mana per turn, per rest)
- Dungeon rewards

### Code Modifications

**Adding New Event Type:**
1. Define event class in `Interfaces/GameEvents.cs`
2. Publish from appropriate system
3. Handle in interface implementations (optional)

**Adding New Manager:**
Follow pattern in `Systems/`:
1. Inject `IGameInterface` in constructor
2. Subscribe to relevant events
3. Provide public API methods
4. Integrate into `GameCore`

**Adding New Status Effect:**
1. Create effect class in `Combat/StatusEffects/`
2. Implement `IStatusEffect` interface
3. Register in factory
4. Add to JSON data files

---

## Development Status

### ✅ Completed (Production Ready)
- Interface-driven architecture (100% console-free game logic)
- Event system (30+ event types)
- Data-driven design (all content in JSON)
- Dependency injection (IDataRepository, ILogger, IGameInterface)
- SOLID principles throughout
- Statistics tracking system (40+ statistics)
- Achievement system (27 achievements)
- Comprehensive testing (375 tests, 100% passing)
- Save system (3 slots, auto-save)
- Equipment system (9 slots, 5 rarities, procedural generation)
- Combat system (abilities, status effects, AI)
- Dungeon system (5 dungeons with progression)
- Three balanced classes

### 🚀 In Progress (See CONTENT_EXPANSION_ROADMAP.md)
**Sprint 2: Equipment Enhancement**
- Equipment-granted abilities
- Equipment content expansion (3-5x variety)
- Set bonuses

**Sprint 3: Combat Content**
- Enemies & bosses expansion (triple count)
- Abilities expansion (2-3x per class)
- Ultimate abilities and combos

**Sprint 4: Deep Systems**
- Skill tree system (branching progression)
- Advanced systems (legendaries, challenge modes, prestige)

---

## Code Quality Standards

### Principles We Follow
1. **One class per file** - Clean organization
2. **Namespace matches folder** - Predictable structure
3. **Zero Console I/O in game logic** - Interface-driven
4. **All data in JSON** - Data-driven design
5. **Dependency injection** - Testable code
6. **Event-driven** - Loosely coupled systems
7. **SOLID principles** - Maintainable architecture
8. **Comprehensive tests** - 70%+ coverage target
9. **🚨 ALL NEW FEATURES REQUIRE TESTS** - No exceptions

### When Adding New Code
- ✅ Inject dependencies, don't instantiate
- ✅ Publish events, don't call Console
- ✅ Define data in JSON, keep code generic
- ✅ **🚨 WRITE TESTS FIRST OR IMMEDIATELY AFTER** - This is mandatory
- ✅ Follow existing patterns (see similar files)
- ✅ Keep methods focused and small
- ✅ Document complex logic

### Testing Requirements (MANDATORY)
**Every code change MUST include tests. No exceptions.**

When you add/modify functionality, you MUST:
1. **Write tests that prove it works** - Not optional, not later, NOW
2. **Test all new public methods** - Every method needs at least one test
3. **Test edge cases** - Empty lists, null values, boundary conditions
4. **Test integration** - How does it work with other systems?
5. **Verify tests PASS** - Run `dotnet test` before considering work complete

**Examples of what needs tests:**
- New properties on data classes → Test they serialize/populate correctly
- New UI display logic → Test the data preparation methods
- New game mechanics → Test the calculation/behavior
- Bug fixes → Test that proves the bug is fixed
- Refactoring → Tests ensure behavior unchanged

**If you skip tests, you're not done with the task.**

### Anti-Patterns to Avoid
- ❌ Console.WriteLine() in game logic
- ❌ `new Random()` (use RandomProvider)
- ❌ Hardcoded game data (use JSON)
- ❌ God objects (use Manager pattern)
- ❌ Static dependencies (use DI)
- ❌ Switch statements for types (use polymorphism)
- ❌ **SKIPPING TESTS FOR NEW CODE** - This is the worst anti-pattern

---

## Testing

### Running Tests
```bash
# All tests
dotnet test

# Specific test
dotnet test --filter "FullyQualifiedName~TestName"

# With details
dotnet test --verbosity detailed
```

### Test Organization
```
TestRPGGame.Tests/
├── DataLoaderTests.cs           # Data loading validation
├── PlayerTests.cs               # Player mechanics
├── EquipmentGeneratorTests.cs   # Equipment generation
├── CombatTests.cs               # Combat mechanics
├── BossAbilityTests.cs          # Boss abilities
├── StatisticsTrackerTests.cs    # Statistics tracking
├── AchievementManagerTests.cs   # Achievement system
├── SaveManagerTests.cs          # Save/load system
├── DungeonManagerTests.cs       # Dungeon progression
└── ProgressionManagerTests.cs   # Leveling system
```

### Test Patterns
- Use `Moq` for interface mocking
- Follow AAA pattern (Arrange, Act, Assert)
- One assertion per test (generally)
- Descriptive test names

---

## Git Workflow

### Commit Format
```bash
git commit -m "type: brief description

Detailed changes:
- Change 1
- Change 2

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

**Types:** feat, fix, refactor, test, docs, chore

### Before Committing
```bash
# Run tests
dotnet test

# Check status
git status

# 🚨 CRITICAL: Stage ONLY files you modified, NOT all files
# NEVER use "git add -A" or "git add ." - stages unrelated work!
git add TestRPGGame/Systems/GameConfig.cs TestRPGGame.Tests/GameConfigTests.cs CHANGELOG.md

# Verify ONLY your files are staged
git status

# If unrelated files are staged, unstage them:
git restore --staged TestRPGGame.Blazor/
git restore --staged TestRPGGame.sln
git restore --staged .claude/settings.local.json
```

### Critical Git Rules
**🚨 NEVER use `git add -A` or `git add .`**
- These commands stage ALL modified files, including work from other agents/sessions
- This causes unintended commits of unrelated work

**✅ ALWAYS stage specific files:**
```bash
# Good - explicit file paths
git add path/to/file1.cs path/to/file2.cs CHANGELOG.md

# Good - interactive staging (review each change)
git add -p

# Bad - stages everything including unrelated work
git add -A   # ❌ NEVER USE
git add .    # ❌ NEVER USE
```

**✅ ALWAYS verify staged files:**
```bash
git status  # Check "Changes to be committed" section
```

**✅ Common files to EXCLUDE:**
- `.sln` files (unless you created new project)
- `.csproj` files (unless you added new dependencies)
- `.claude/settings.local.json` (local settings)
- `TestRPGGame.Blazor/` (if working on core game)
- Test fixture files from other agents
- Any files you didn't modify in this session

---

## Current Development Focus

### Immediate Priorities (Sprint 2)
1. **Equipment-Granted Abilities** - Make equipment more interesting
2. **Equipment Content Expansion** - 3-5x more variety in loot
3. **Set Bonuses** - Reward matching equipment

### Next Up (Sprint 3)
4. **Enemy Variety** - Triple enemy count, more abilities
5. **Boss Mechanics** - More interesting boss fights
6. **Ability Expansion** - 2-3x abilities per class

See `AgenticContext/CONTENT_EXPANSION_ROADMAP.md` for full details.

---

## Troubleshooting

### Tests Failing
```bash
# Clean and rebuild
dotnet clean
dotnet build
dotnet test
```

### Game Won't Start
- Check `Data/` JSON files for syntax errors
- Verify `gameconfig.json` exists
- Look for exceptions in console output

### Balance Issues
- Edit `gameconfig.json` for quick tweaks
- Edit JSON data files for content changes
- Check AgenticContext/DEVELOPMENT_LOG.md for historical balance decisions

---

## Resources

### Documentation
- **Architecture:** AgenticContext/ARCHITECTURE.md
- **Roadmap:** AgenticContext/CONTENT_EXPANSION_ROADMAP.md
- **Session Guide:** AgenticContext/SESSION_GUIDE.md
- **Effects Guide:** AgenticContext/MULTIPLE_EFFECTS_GUIDE.md

### Key Concepts
- **Interface-Driven:** All UI through IGameInterface
- **Event-Driven:** Publish events, don't call methods
- **Data-Driven:** JSON content, generic code
- **Dependency Injection:** Inject dependencies, don't new()

### Getting Help
- Read AgenticContext/*.md files for detailed documentation
- Check similar existing code for patterns
- Review test files for usage examples
- Search git history for context: `git log --all --grep="keyword"`

---

## Success Metrics

**Current State:**
- ✅ 128 C# source files
- ✅ 450 tests passing (100%)
- ✅ 54% code coverage (3,269/6,042 lines)
- ✅ Zero Console calls in game logic
- ✅ 100% interface-driven architecture
- ✅ SOLID principles throughout
- ✅ Comprehensive JSON data system

**Quality Gates:**
- All tests must pass before commit
- New features require tests
- No Console I/O in game logic
- All game data in JSON files
- Follow SOLID principles

---

## Quick Command Reference

```bash
# Development
dotnet build                     # Build project
dotnet run                       # Run console UI
dotnet run -- --automated        # Run AI player
dotnet test                      # Run all tests
dotnet test --filter "Name"      # Run specific tests

# Git
git status                       # Check changes
git add -A                       # Stage all
git commit -m "msg"              # Commit with message
git log --oneline -10            # Recent commits

# Project Info
find TestRPGGame -name "*.cs" | wc -l    # Count C# files
git log --all --grep="keyword"            # Search commits
```

---

## Contact & Contribution

**Project Owner:** Billy
**Development Tool:** Claude Code (Anthropic)
**Project Type:** Educational / Portfolio

**Want to contribute?**
- Follow existing patterns
- Write tests for new features
- Update relevant documentation
- Keep commits atomic and well-described

---

**Built with Claude Code** - Demonstrating the future of AI-assisted software development
