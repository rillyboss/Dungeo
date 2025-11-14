# Session Guide

Quick reference for continuing development on this project.

## How to Start a New Session

1. **Review the Development Log**: Check `DEVELOPMENT_LOG.md` for:
   - What was done in previous sessions
   - Current pending tasks
   - Known issues

2. **Check Project Status**:
   ```bash
   dotnet test  # Verify all tests pass
   git status   # Check for uncommitted changes
   ```

3. **Update the Log**: When starting work, add a new session entry to `DEVELOPMENT_LOG.md`

## How to End a Session

1. **🚨 VERIFY ALL TESTS PASS** (MANDATORY):
   ```bash
   dotnet test
   ```
   - **DO NOT COMMIT if tests fail**
   - **DO NOT END SESSION without adding tests for new features**
   - All new code MUST have corresponding tests

2. **Update DEVELOPMENT_LOG.md**:
   - Add completed tasks to the current session
   - Update pending tasks
   - Add any new issues discovered
   - Write session summary
   - **List any tests that were added**

3. **Commit Changes**:
   ```bash
   git add -A
   git commit -m "Your commit message

   🤖 Generated with [Claude Code](https://claude.com/claude-code)

   Co-Authored-By: Claude <noreply@anthropic.com>"
   ```

## Project Structure

### Key Directories
- `TestRPGGame/Data/` - JSON configuration files (classes, abilities, enemies, dungeons, items)
- `TestRPGGame/DataLoading/` - Data models and loaders
- `TestRPGGame/Entities/` - Game entities (Player, Enemy, Dungeon)
- `TestRPGGame/Combat/` - Combat system and AI
- `TestRPGGame/Equipment/` - Item generation and inventory
- `TestRPGGame.Tests/` - Unit tests

### Important Files
- `DEVELOPMENT_LOG.md` - Session history and task tracking
- `Data/classes.json` - Class balance configuration
- `Data/abilities.json` - All player and enemy abilities
- `Player.cs` - Player entity and initialization

## Current System Architecture

### Data-Driven Design
Most game systems are configured via JSON:
- **Classes**: `Data/classes.json`
- **Abilities**: `Data/abilities.json` + `Data/enemy-abilities.json`
- **Enemies**: `Data/enemies.json` + `Data/bosses.json`
- **Dungeons**: `Data/Dungeons/*.json`
- **Items**: `Data/Items/*.json`

### Key Systems
1. **Combat System** (`Combat/CombatSystem.cs`)
   - Turn-based combat
   - Status effects (DOT, HOT, buffs, debuffs)
   - Flee mechanic
   - Critical hits

2. **Equipment System** (`Equipment/EquipmentGenerator.cs`)
   - Procedural generation
   - Rarity system
   - Stat bonuses
   - Special effects

3. **Save System** (`Systems/SaveSystem.cs`)
   - 3 save slots
   - Auto-save after dungeons
   - Manual save
   - Slot deletion

## Testing

### 🚨 MANDATORY TESTING POLICY

**ALL NEW FEATURES MUST HAVE TESTS. NO EXCEPTIONS.**

Every time you add or modify functionality:
1. Write tests that prove it works
2. Run `dotnet test` to verify all tests pass
3. Do not consider the task complete until tests are written and passing

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test
dotnet test --filter "FullyQualifiedName~TestName"

# Run tests for a specific class
dotnet test --filter "FullyQualifiedName~PlayerTests"

# Build without tests
dotnet build
```

### What Requires Tests

✅ **MUST have tests:**
- New public methods or properties
- New game mechanics or systems
- Bug fixes (test proves the bug is fixed)
- Data model changes (test serialization works)
- UI data preparation (test the data layer, not the display)
- Refactoring (tests ensure behavior unchanged)

❌ **Does NOT need tests:**
- Console display code (pure UI rendering)
- Comment changes
- Documentation updates

### Test Organization

Tests are in `TestRPGGame.Tests/`:
- `PlayerTests.cs` - Player mechanics and character sheet
- `CombatTests.cs` - Combat mechanics
- `EquipmentTests.cs` - Equipment generation
- `AbilityTests.cs` - Ability system
- `StatusEffectTests.cs` - Status effects
- And more...

**Current test count: 418 tests, 100% passing**

## Balance Guidelines

### Class Balance Philosophy
- **Warrior**: Tanky bruiser (high HP, defense, sustained damage)
- **Mage**: Glass cannon (low HP, high magic power, burst damage)
- **Rogue**: Mobile DPS (medium HP, high crit, guaranteed crit backstab)

### When Adjusting Balance
1. Update `Data/classes.json`
2. Run tests to verify
3. Document changes in `DEVELOPMENT_LOG.md`
4. Test in actual gameplay if possible

## Common Tasks

### Adding a New Ability
1. Add to `Data/abilities.json` (player) or `Data/enemy-abilities.json` (enemy)
2. If new effect type needed, create in `Abilities/Effects/`
3. Add factory mapping in `EntityFactory.cs`
4. Test with unit tests

### Balancing Classes
1. Edit `Data/classes.json`
2. Adjust base stats and/or per-level growth
3. Run tests (some may need updating)
4. Document in `DEVELOPMENT_LOG.md`

### Adding New Equipment
1. Edit item data in `Data/Items/` directory
2. Equipment generator will automatically use new data
3. No code changes needed (data-driven)

## Git Workflow

### Standard Commit
```bash
git add -A
git commit -m "Brief description

Detailed changes:
- Change 1
- Change 2

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"
```

### Before Pushing
Always run tests first:
```bash
dotnet test && git push
```

## Current State (Last Updated: 2025-01-10)

- ✅ All classes balanced
- ✅ Starting equipment implemented
- ✅ Save deletion working
- ✅ All status effects functional
- ✅ 86/86 tests passing

## Next Session Ideas

Check `DEVELOPMENT_LOG.md` "Pending Tasks" section for prioritized work items.
