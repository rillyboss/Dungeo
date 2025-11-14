# Changelog

All notable changes to Dungeo will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
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
- ✅ Comprehensive test suite (401 tests, 100% passing)

### Architecture
- Interface-driven design (IGameInterface abstraction)
- Event-based communication (30+ event types)
- Data-driven content (100% JSON configuration)
- SOLID principles throughout
- Manager pattern for major systems
- Factory pattern for entity creation

### Technical Stats
- **C# Source Files**: 128
- **Unit Tests**: 401 (100% passing)
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
