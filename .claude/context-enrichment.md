# Context Enrichment - TestRPGGame

> This context is automatically loaded on every command to provide essential working rules and quick references.

---

## 🎯 Core Working Principles

### Testing Rules (🚨 MANDATORY)

**🚨 ALL NEW FEATURES AND BUG FIXES MUST HAVE TESTS. NO EXCEPTIONS. 🚨**

If you add code without tests, **THE TASK IS NOT COMPLETE.**

**ALWAYS run tests before committing:**
```bash
dotnet test                    # Run all 423 tests (must pass 100%)
dotnet test --no-build         # Skip rebuild if just ran dotnet build
```

**When to test:**
- ✅ After modifying any C# code
- ✅ After changing JSON data files
- ✅ Before any git commit
- ✅ After fixing bugs
- ✅ **IMMEDIATELY after adding new features**
- ❌ NOT needed for pure documentation changes

**Test expectations:**
- All tests must pass (currently 423 tests)
- **🚨 WRITE TESTS FOR NEW FEATURES** - Not optional, not later, NOW
- **🚨 WRITE TESTS FOR BUG FIXES** - Prove the bug is fixed
- **🚨 WRITE TESTS FOR NEW PUBLIC METHODS** - Every method needs at least one test
- Update test counts in this file when adding/removing tests
- Update test counts in CLAUDE.md when they change
- Test edge cases: null values, empty lists, boundary conditions

**What MUST have tests:**
- ✅ New public methods or properties
- ✅ New game mechanics or systems
- ✅ Bug fixes (test proves bug is fixed)
- ✅ Data model changes (test serialization)
- ✅ UI data preparation logic
- ✅ Refactoring (tests ensure behavior unchanged)

**What does NOT need tests:**
- ❌ Pure display code (Console.WriteLine formatting)
- ❌ Comment/documentation changes

### Version Control Rules
**Commit format:**
```
type: brief description

Detailed changes:
- Change 1
- Change 2
```

**Types:** feat, fix, refactor, test, docs, chore

**When to commit:**
- ✅ After completing a feature and tests pass
- ✅ When user explicitly requests it
- ✅ After significant refactoring
- ❌ NOT without running tests first
- ❌ NOT without updating CHANGELOG.md first

**Git workflow:**
```bash
dotnet test                              # Verify tests pass
# Edit CHANGELOG.md                      # Document your changes under [Unreleased]
git status                               # Check what changed
git add -A                               # Stage changes
git commit -m "type: description..."     # Commit with proper format
```

**CHANGELOG.md Requirements:**
- **ALWAYS update CHANGELOG.md** before committing changes
- Add entries under `[Unreleased]` section
- Use appropriate categories:
  - `Added` - New features
  - `Changed` - Changes to existing functionality
  - `Fixed` - Bug fixes
  - `Removed` - Removed features
- Include specific details: file names, test counts, clear descriptions
- Update test counts when they change

---

## 📚 Quick Reference - Where to Find Things

### Architecture & Patterns
- **CLAUDE.md** - Start here! Project overview, tech stack, quick start
- **AgenticContext/ARCHITECTURE.md** - Complete technical architecture, patterns, managers
- **AgenticContext/SESSION_GUIDE.md** - Common operations, quick reference

### Adding Content
- **TestRPGGame/Data/abilities.json** - All player abilities (16 total now)
- **TestRPGGame/Data/enemies.json** - Regular enemies
- **TestRPGGame/Data/bosses.json** - Boss enemies with abilities
- **TestRPGGame/Data/dungeons.json** - Dungeon configurations
- **TestRPGGame/Data/achievements.json** - Achievement definitions
- **TestRPGGame/Data/classes.json** - Class stats and configurations
- **TestRPGGame/Data/gameconfig.json** - Balance tuning, combat parameters

### Code Patterns
- **TestRPGGame/GameCore.cs** - Main game engine (240 lines)
- **TestRPGGame/Interfaces/IGameInterface.cs** - Core UI abstraction
- **TestRPGGame/Interfaces/GameEvents.cs** - All 30+ event types
- **TestRPGGame/Combat/StatusEffects/** - Status effect implementations
- **TestRPGGame/Abilities/Applicators/** - Ability effect applicators
- **TestRPGGame/Systems/** - Managers (Save, Dungeon, Progression, etc.)

### Testing & Documentation
- **TestRPGGame.Tests/** - All 423 unit tests
- Run: `dotnet test` (must pass 100%)
- **🚨 ALL NEW CODE MUST HAVE TESTS** - No exceptions
- **CHANGELOG.md** - Project changelog (update with every change)

---

## ⚠️ Critical Rules - What NOT to Do

### ❌ Don't Assume
1. **Don't assume tests pass** - Always run `dotnet test` after changes
2. **Don't assume JSON is valid** - Build project to verify syntax
3. **Don't assume buff/effect exists** - Check Constants/BuffType.cs and StatusEffectId.cs first
4. **Don't assume file paths** - Use Read tool to verify files exist

### ❌ Don't Create Unnecessary Files
1. **Don't create markdown docs** unless explicitly requested
2. **Don't create new managers** without discussing architecture first
3. **Don't create duplicate files** - edit existing files instead
4. **Don't create comments as todos** - use TodoWrite tool for tracking

### ❌ Don't Use Console in Game Logic
1. **All game logic must use IGameInterface** - Never Console.WriteLine()
2. **Publish events, don't call methods** - Use event-driven architecture
3. **Check existing events** in GameEvents.cs before creating new ones

### ❌ Don't Modify Without Testing
1. **🚨 WRITE TESTS FOR ALL NEW CODE** - Task is not complete without tests
2. **Run tests after every code change**
3. **Rebuild if tests fail mysteriously** - `dotnet build && dotnet test`
4. **Update test expectations** when changing ability counts or data
5. **Never skip tests because "it's just a small change"** - Small bugs still break production

### ❌ Don't Leave a Mess
1. **Cleanup old and unused code during refactors and changes.**
2. **Dont litter the codebase with to-dos, we can leverage changelogs and roadmap markdown files for this**

---

## 🔧 Common Workflows

### Adding New Ability
1. Edit `TestRPGGame/Data/abilities.json`
2. If using new buff: Add to BuffType.cs, StatusEffectId.cs, StatusEffectFactory.cs
3. Run: `dotnet test`
4. Update test expectations if ability counts changed

### Adding New Enemy/Boss
1. Edit `TestRPGGame/Data/enemies.json` or `bosses.json`
2. Reference existing ability IDs
3. Run: `dotnet test`

### Fixing a Bug
1. Identify issue
2. **Write failing test** (REQUIRED for all bug fixes)
3. Fix the bug
4. Run: `dotnet test` (all must pass)
5. Update CHANGELOG.md under `### Fixed`
6. Commit if requested

### Refactoring
1. Run tests before: `dotnet test`
2. Make changes
3. Run tests after: `dotnet test`
4. All existing tests must still pass

---

## 💡 Success Checklist

Before considering work "done":
- [ ] **🚨 NEW TESTS WRITTEN** for all new features/bug fixes (MANDATORY)
- [ ] All tests pass: `dotnet test` (423 tests, 100%)
- [ ] **CHANGELOG.md updated** with changes
- [ ] Code follows SOLID principles
- [ ] No Console calls in game logic (use IGameInterface)
- [ ] JSON files are valid (project builds)
- [ ] Changes are focused and atomic
- [ ] Test count updated in context-enrichment.md and CLAUDE.md (if changed)
- [ ] Commit when request is completed and tests are passing

**🚨 CRITICAL: If you didn't write tests, the work is NOT done. Go back and write them.**

---

## 🎮 Current Game State (Quick Facts)

- **3 Classes:** Warrior, Mage, Rogue
- **6 Abilities per class:** 3 starting + 3 unlockable
- **Total Tests:** 423 (must all pass - 100% required)
- **Testing Policy:** 🚨 ALL new code MUST have tests - NO EXCEPTIONS
- **Architecture:** Interface-driven, event-based, data-driven
- **Tech Stack:** C# .NET 8.0, xUnit, Moq
- **Documentation:** CHANGELOG.md (update with every change)
- **Repository:** https://github.com/rillyboss/Dungeo

---

*This context enrichment keeps you aligned with project rules and expectations.*
