# Changelog

All notable changes to Dungeo will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **Test Infrastructure Improvements**:
  - Created comprehensive TestFixtures.cs (456 lines) - Central infrastructure for creating controlled test data
  - Added helper methods for all major entity types:
    - `CreateTestEnemy()`, `CreateWeakEnemy()`, `CreateTestBoss()` - Enemy creation with known stats
    - `CreateTestPlayer()`, `CreateWarriorPlayer()`, `CreateMagePlayer()` - Player creation
    - `CreateTestWeapon()`, `CreateTestArmor()` - Equipment creation
    - `CreateTestAbility()` - Ability creation
    - `CreateMockRepository()` - Mock IDataRepository with test data
    - `CreateTestEnemyData()`, `CreateTestPrefix()`, `CreateTestSuffix()` - Data model creation
  - All test fixtures provide predictable, controlled data eliminating JSON dependencies
- **UI/UX Enhancements**:
  - Equipment-granted abilities now display friendly names instead of IDs in character sheet
  - Unlock Abilities screen now sorts abilities by cost (cheapest first), then by level
  - Comprehensive help system with 8 categorized sections:
    - Getting Started, Combat, Progression, Equipment & Inventory
    - Dungeons, Shop, Statistics & Achievements, Saving & Loading, Tips & Tricks
  - Level-up rewards now display newly unlocked abilities with descriptions
  - Added `NewlyUnlockedAbilities` property to `PlayerLeveledUpEvent` for better feedback
- **Custom Save Path Support**:
  - Added command-line argument support for custom save directories (`--save-path` or `-sp`)
  - Allows automated runner to use separate saves from manual gameplay
  - Added `SaveSystem.SetSaveDirectory()`, `GetSaveDirectory()`, and `ResetSaveDirectory()` methods
  - Updated `Program.cs` to parse save path argument and configure SaveSystem
  - Added 5 comprehensive tests for custom save path functionality:
    - `SetSaveDirectory_ChangesActiveSaveDirectory` - Verifies directory changes
    - `SetSaveDirectory_WithCustomPath_SavesAndLoadsFromCustomLocation` - Verifies save/load isolation
    - `ResetSaveDirectory_RestoresDefaultDirectory` - Verifies reset functionality
    - `SetSaveDirectory_WithNullOrEmpty_UsesDefaultDirectory` - Verifies null handling
    - `CustomSaveDirectory_IsolatesFromDefaultSaves` - Verifies complete save isolation
  - Total tests now: 423 (increased from 418)
- **Character Sheet Enhancements**:
  - Filters out unlearned abilities from character sheet display
  - Separates equipment-granted abilities into dedicated section
  - Shows base stats vs equipment bonuses breakdown
  - Displays which equipment grants which abilities
- **Inventory Display Improvements**:
  - Shows granted abilities on equipped items
  - Displays granted abilities in item details view
  - Clear indication of ability sources in equipment list
- **5 New Comprehensive Tests** for character sheet functionality:
  - `GetCharacterSheetInfo_OnlyShowsUnlockedLearnedAbilities` - Verifies locked abilities hidden
  - `GetCharacterSheetInfo_SeparatesEquipmentAbilities` - Verifies equipment ability separation
  - `GetCharacterSheetInfo_ShowsBaseStatsVsBonusStats` - Verifies stat calculation accuracy
  - `GetCharacterSheetInfo_EquipmentAbilities_ShowCorrectSource` - Verifies source tracking
  - `GetCharacterSheetInfo_AfterLevelUp_ShowsCorrectBaseStats` - Verifies level progression
- **Data Model Enhancements**:
  - Added `Source` property to `AbilityInfo` for tracking ability origin
  - Added `EquipmentAbilities` list to `CharacterSheetInfo`
  - Added `BaseStats` and `BonusStats` to `CharacterSheetInfo`
  - Added `EquipmentStats` class for stat breakdown
- **Mandatory Testing Policy** enforced across all documentation:
  - Updated CLAUDE.md with explicit "ALL NEW FEATURES REQUIRE TESTS" policy
  - Updated SESSION_GUIDE.md with testing-first workflow
  - Updated context-enrichment.md hook with mandatory testing requirements
  - Added examples of what requires tests vs what doesn't
- **JustApplied flag** to StatusEffect base class
  - Ensures buffs/debuffs last their full stated duration
  - Skips first duration tick when effect is applied
  - Matches standard RPG behavior (3-turn buff = 3 beneficial turns)
- Added 4 comprehensive unit tests for status effect system in combat
  - Test for status effects ticking down each turn
  - Test for effects clearing when combat ends
  - Test for effects not persisting between combats
  - Test for buff duration decrementing correctly
- Added CHANGELOG.md to track all project changes
- Created comprehensive README.md highlighting data-driven design
- **Equipment Content Expansion** (Phase 5A: Sprint 2):
  - **Weapon Prefixes**: Added 26 new prefixes (30 → 56 total)
    - Elemental themes: Blazing, Frozen, Toxic, Thunderous
    - Material types: Obsidian, Bone, Blood, Star Metal, Void Crystal
    - Power themes: Spectral, Wrathful, Merciless, Vampiric, Demonic, Draconic
    - Hybrid builds: Corrupted, Hallowed, Shadow, Radiant, Arcane, Savage, Warped
    - Legendary tier: Primal, Soulbound
  - **Weapon Suffixes**: Added 25 new suffixes (22 → 47 total)
    - Elemental damage: of the Inferno, of Glaciers, of the Tempest
    - Combat styles: of Conquest, of the Berserker, of the Duelist, of the Slayer
    - Dark themes: of the Reaper, of Spite, of the Plague, of Suffering, of the Abyss
    - Special effects: of Fury and Honor, of Shadows and Flame, of Momentum
    - Legendary tier: of Annihilation, of Domination, of Obliteration, of Decimation, of Eternity, of Savagery, of Apocalypse
  - **Armor Prefixes**: Added 25 new prefixes (25 → 50 total)
    - Material types: Shadow, Obsidian, Duskwood, Runed, Spiked
    - Crafting styles: Blessed, Warded, Sanctified, Warforged, Spectral, Masterwork
    - Racial variants: Elven, Dwarven, Orcish
    - Elemental themes: Infernal, Lunar, Solar, Living, Stormforged
    - Hybrid builds: Blood-forged, Voidtouched, Blessed Steel
    - Legendary tier: Celestial, Abyssal, Dragonhide
  - **Armor Suffixes**: Added 26 new suffixes (21 → 47 total)
    - Defensive themes: of the Juggernaut, of the Bulwark, of the Bastion, of the Stalwart
    - Regeneration: of the Immortal, of Lifebinding, of Recovery
    - Hybrid defense: of the Templar, of the Sentinel, of Balance
    - Mobility: of Swiftness, of Deflection, of Winds
    - Counter-attack: of Vengeance, of Retribution
    - Legendary tier: of the Titan, of Steadfastness, of the Ancients, of Sanctuary, of the Storm, of Iron Will, of Courage, of the Vanguard
  - **Total Equipment Variety**: ~340,000 possible unique combinations (up from ~15,000)
  - All 423 tests passing with new equipment data
- **Enemy Difficulty Scaling System** (Combat Balance Tuning):
  - **New Config Properties** in `gameconfig.json`:
    - `EnemyDamageMultiplier` - Multiplies enemy damage output (default: 1.0)
    - `EnemyAttackMultiplier` - Multiplies enemy attack stat (default: 1.0)
    - `EnemyDefenseMultiplier` - Multiplies enemy defense stat (default: 1.0)
    - `EnemyHPMultiplier` - Multiplies enemy HP (default: 1.0)
  - **Implementation**:
    - Added 4 new properties to `GameConfiguration` class in GameConfig.cs
    - Updated `EntityFactory.CreateEnemy()` to apply HP/Attack/Defense multipliers
    - Updated `InterfacedCombatSystem.ExecuteEnemyTurn()` to apply damage multiplier
  - **Balance Tuning**:
    - Set multipliers to 1.0 by default (neutral, current difficulty)
    - Increase to 1.5 for moderate challenge
    - Increase to 2.0+ for hard mode
    - All multipliers apply independently for fine-tuned control
  - **Added 6 New Tests**:
    - `GameConfig_EnemyDifficultyMultipliers_ExistInConfig` - Verifies all 4 multipliers load
    - `GameConfig_EnemyDamageMultiplier_IsApplied` - Tests damage scaling
    - `GameConfig_EnemyStatMultipliers_CanScaleStats` - Tests stat scaling calculation
    - `GameConfig_EnemyHPMultiplier_ScalesCorrectly` - Theory test for HP scaling (3 cases)
  - Total tests now: 429 (increased from 423)
  - **Note**: After test refactoring, final count is 447 tests (EnemyModifierTests gained 16 additional tests)

### Changed
- **MAJOR: Comprehensive Test Suite Refactoring** (447 tests, 100% passing):
  - **Problem Solved**: Tests were brittle and dependent on JSON data changes
  - **Solution**: Separated data validation tests from system behavior tests
  - **Refactored 7 Test Files** to eliminate JSON data dependencies:
    1. **InterfacedCombatSystemTests.cs** - Replaced all EnemyFactory.CreateEnemy() calls with TestFixtures
       - Removed hardcoded config assertions (e.g., `Assert.Equal(0.4, config.CombatLootDropChance)`)
       - Changed to range validation (e.g., `Assert.True(config.CombatLootDropChance >= 0 && <= 1)`)
    2. **StatusEffectSystemTests.cs** - Updated CreateTestEnemy() helper to use TestFixtures
    3. **EnemyModifierTests.cs** - Complete rewrite: 11 tests → 27 tests
       - Added 6 test regions: Level Filtering, Prefix Application, Suffix Application, Combined Modifiers, Data Validation, Probability
       - Tests now verify modifier application logic with controlled data
    4. **EnemyLevelRequirementTests.cs** - Replaced DataLoader with mock repository
    5. **EnemyAbilityTests.cs** - Replaced DataLoader calls with TestFixtures
    6. **EquipmentGeneratorTests.cs** - Mocked ItemGenerationData instead of loading from JSON
    7. **PlayerTests.cs** - Used TestFixtures for player creation instead of loading classes.json
  - **Documented Data Validation Tests**:
    - Added headers to DataLoaderTests.cs and GameConfigTests.cs
    - Clarified these ARE intentionally data-dependent (validate JSON structure)
    - Separated concerns: validation tests vs system tests
  - **Removed Hardcoded Config Assertions**:
    - ShopTests.cs now tests config usage patterns, not specific values
    - Tests validate ranges and relationships instead of exact numbers
  - **Result**: Test suite now resilient to JSON data changes while maintaining full system coverage
  - **Test Count**: 447 tests (up from 423) - gained 24 tests from EnemyModifierTests expansion
- **Modified Files for UI/UX Enhancements**:
  - `Player.cs` - Fixed `GetEquipmentSourceForAbility()` to use ability ID instead of name
  - `ProgressionManager.cs` - Abilities now sorted by cost, then level in unlock menu
  - `ProgressionManagerTests.cs` - Updated 2 tests to account for ability sorting
  - `GameEvents.cs` - Added `NewlyUnlockedAbilities` list to `PlayerLeveledUpEvent`
  - `InterfacedCombatSystem.cs` - Populates newly unlocked abilities on level up
  - `ConsoleInterface.cs` - Added comprehensive `DisplayHelp()` method (160 lines)
  - `ConsoleInterface.cs` - Display newly unlocked abilities in level-up event
  - `GameCore.cs` - Updated `ShowHelp()` to use new display method

### Fixed
- **CRITICAL**: Fixed status effects never expiring during combat
  - Added `ProcessTurnStart()` calls at the beginning of each combat turn
  - Status effects now properly decrement duration and expire as designed
  - Fixes Battle Rage and all other buffs/debuffs lasting forever
- **CRITICAL**: Fixed status effects persisting between combats
  - Added `ClearAll()` calls when combat ends
  - Prevents buffs from Combat #1 carrying into Combat #2
  - Ensures clean state for each new combat encounter
- **Fixed buff duration issue**: Buffs now last their full stated duration
  - Previously: 3-turn buff only gave 2 beneficial attacks (cast turn consumed one)
  - Now: 3-turn buff gives 3 beneficial attacks (JustApplied flag skips first tick)
  - Matches player expectations and industry standard RPG behavior

### Changed
- **Updated test count from 401 to 418 tests** (all passing, 100% required)
- Updated all documentation files with new test counts:
  - CLAUDE.md
  - SESSION_GUIDE.md
  - context-enrichment.md
- Updated test count from 397 to 401 tests (all passing)
- Updated 6 existing tests to account for JustApplied flag behavior:
  - `DamageOverTimeEffect_DecrementsRemainingTurns`
  - `DamageOverTimeEffect_ExpiresAfterDuration`
  - `ShieldEffect_ExpiresAfterDuration`
  - `ThornsEffect_DecrementsAfterTurns`
  - `StunEffect_ExpiresAfterDuration`
  - `StatModifierEffect_ExpiresAfterDuration`
- Removed 11 outdated markdown documentation files:
  - Removed all HOOKS_*.md files (6 files)
  - Removed SESSION_SUMMARY.md
  - Removed CONTEXT_ENRICHMENT_SETUP.md
  - Removed AUTONOMOUS_WORKFLOW_GUIDE.md
  - Removed OPTIMIZATION_SUMMARY.md
  - Removed MISSING_FEATURES.md

### Repository
- Set up GitHub remote: https://github.com/rillyboss/Dungeo
- Cleaned git history to remove large log files (2.4GB)
- Successfully pushed all 106 commits to GitHub

---

## [1.0.0] - 2025-01-12

### Completed Features
- ✅ Core game loop (explore → combat → loot → progress)
- ✅ Three balanced classes with 6 abilities each (Warrior, Mage, Rogue)
- ✅ Turn-based combat with abilities and status effects
- ✅ Procedural equipment system (9 slots, 5 rarities)
- ✅ Dungeon system with 5 dungeons and boss fights
- ✅ Save/load system (3 slots, auto-save)
- ✅ Achievement system (27 achievements)
- ✅ Statistics tracking (40+ stats)
- ✅ Shop system with potions and equipment
- ✅ Interface-driven architecture (Console + Automated UIs)
- ✅ Comprehensive test suite (447 tests, 100% passing)

### Architecture
- Interface-driven design (IGameInterface abstraction)
- Event-based communication (30+ event types)
- Data-driven content (100% JSON configuration)
- SOLID principles throughout
- Manager pattern for major systems
- Factory pattern for entity creation

### Technical Stats
- **C# Source Files**: 128
- **Unit Tests**: 447 (100% passing)
- **Code Coverage**: 54% (3,269/6,042 lines)
- **Game Content**: 100% data-driven JSON
- **Architecture**: Fully interface-driven

---

## Development Roadmap

### Sprint 2: Equipment Enhancement (In Progress)
- Equipment-granted abilities
- 3-5x more equipment variety
- Set bonuses

### Sprint 3: Combat Content Expansion (Planned)
- Triple enemy count
- 2-3x abilities per class
- Enhanced boss mechanics

### Sprint 4: Deep Systems (Planned)
- Skill tree system
- Legendary equipment mechanics
- Challenge modes
- Prestige system

---

## Maintenance Notes

### For Contributors
When making changes, update this changelog following these guidelines:

1. **Add entries under [Unreleased]** for all changes
2. **Categorize changes**:
   - `Added` for new features
   - `Changed` for changes in existing functionality
   - `Deprecated` for soon-to-be removed features
   - `Removed` for removed features
   - `Fixed` for bug fixes
   - `Security` for vulnerability fixes
3. **Be specific**: Include file names, test counts, and clear descriptions
4. **Update test counts**: Always note changes to total test count
5. **Link related commits**: Reference commit hashes for major changes

### Version Release Process
When releasing a new version:
1. Move entries from [Unreleased] to a new version section
2. Add release date in format: `## [X.Y.Z] - YYYY-MM-DD`
3. Update version in project files
4. Tag the release in git: `git tag -a vX.Y.Z -m "Version X.Y.Z"`

---

**Legend**:
- 🐛 Bug Fix
- ✨ New Feature
- 🔧 Improvement
- 📚 Documentation
- ⚠️ Breaking Change
- 🧪 Tests
