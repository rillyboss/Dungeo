# Active Context - Quick Reference for AI Assistants

**Last Updated:** 2025-01-12
**Project Status:** Phase 2 Complete (Statistics + Achievements), Sprint 2 Starting

---

## 📋 Priority Reading Order

### For New Sessions (Start Here)
1. **README.md** - Project overview and current state
2. **ARCHITECTURE.md** - Technical architecture and design
3. **CONTENT_EXPANSION_ROADMAP.md** - Current priorities (Sprint 2: Equipment Enhancement)

### For Specific Tasks
4. **SESSION_GUIDE.md** - Quick command reference
5. **MULTIPLE_EFFECTS_GUIDE.md** - How to add abilities/effects
6. **DATA_DRIVEN_SYSTEM.md** - How to modify game content
7. **DEVELOPMENT_LOG.md** - Historical decisions and balance changes

### For Major Changes
8. **CODE_IMPROVEMENT_ROADMAP.md** - Completed architectural improvements
9. **FUTURE_REFACTORS.md** - Potential future improvements

---

## 🎯 Current Development Focus

### Sprint 2: Equipment Enhancement (IN PROGRESS)
**Priority:** HIGH
**Status:** Starting

**Goals:**
1. Equipment-granted abilities - Gear can grant temporary abilities
2. Equipment content expansion - 3-5x more variety
3. Set bonuses - Reward for wearing matching gear

**Next Steps:**
- Implement equipment ability system
- Expand item generation data
- Create set bonus framework

---

## ✅ Recently Completed

### Phase 1: Statistics Tracking ✅ COMPLETE
- 40+ tracked statistics (combat, economy, progression, exploration)
- Full persistence with save files
- Beautiful formatted display
- **Commit:** 6ddb9fd

### Phase 2: Achievement System ✅ COMPLETE
- 27 achievements across 6 categories
- Condition framework (4 condition types)
- Automatic checking and rewards
- **Commit:** 6ddb9fd

### Phase 9: GameCore Refactor ✅ COMPLETE
- Extracted SaveManager, DungeonManager, ProgressionManager
- GameCore reduced from 571 → 240 lines (58% reduction)
- 42 new unit tests
- **Commit:** ba5a58a

---

## 📁 Active Documentation Files

| File | Purpose | When to Read |
|------|---------|--------------|
| **README.md** | Project overview | New sessions, onboarding |
| **ARCHITECTURE.md** | Technical design | Architecture questions, refactoring |
| **CONTENT_EXPANSION_ROADMAP.md** | Development priorities | Feature planning |
| **SESSION_GUIDE.md** | Quick reference | Common tasks, commands |
| **MULTIPLE_EFFECTS_GUIDE.md** | Effect system guide | Adding abilities, bosses |
| **DATA_DRIVEN_SYSTEM.md** | JSON data guide | Content modification |
| **DEVELOPMENT_LOG.md** | Historical log | Understanding decisions |
| **CODE_IMPROVEMENT_ROADMAP.md** | Completed work | Understanding architecture evolution |
| **FUTURE_REFACTORS.md** | Potential improvements | Long-term planning |

---

## 📦 Archived Documentation

Historical and completed work documentation moved to `ARCHIVED/`:
- **IMPLEMENTATION_STATUS.md** - Old equipment system status (now complete)
- **HYBRID_SYSTEMS_ANALYSIS.md** - Console cleanup phases (all complete)
- **CLEANUP_PLAN.md** - Legacy code removal (complete)
- **REFACTOR_SUMMARY.md** - Interface refactor summary (historical)
- **INTERFACE_REFACTOR.md** - Original refactor plan (historical)
- **GAME_FEEDBACK.md** - AI playthrough analysis (historical)

---

## 🔍 Quick Search Guide

### "I need to add a new ability"
→ Read `MULTIPLE_EFFECTS_GUIDE.md`
→ Edit `TestRPGGame/Data/abilities.json`

### "I need to balance the game"
→ Edit `TestRPGGame/gameconfig.json`
→ Or edit specific data files in `Data/`

### "I need to understand the architecture"
→ Read `ARCHITECTURE.md`
→ Check `CODE_IMPROVEMENT_ROADMAP.md` for completed improvements

### "I need to add a new feature"
→ Read `CONTENT_EXPANSION_ROADMAP.md` for priorities
→ Check `ARCHITECTURE.md` for patterns to follow

### "I need to understand a past decision"
→ Read `DEVELOPMENT_LOG.md`
→ Search git history: `git log --all --grep="keyword"`

---

## 🎮 Test Commands

```bash
# Quick test
dotnet test

# With details
dotnet test --verbosity detailed

# Specific test
dotnet test --filter "FullyQualifiedName~TestName"
```

**Expected:** 375 tests passing (100%)

---

## 🚀 Run Commands

```bash
# Console UI (human player)
dotnet run

# AI player (automated testing)
dotnet run -- --automated
```

---

## 📊 Project Metrics (Current)

- **C# Files:** 128
- **Tests:** 375 (100% passing)
- **Code Coverage:** 54% (3,269/6,042 lines)
- **Achievements:** 27
- **Statistics Tracked:** 40+
- **JSON Data Files:** 10+
- **Equipment Slots:** 9
- **Classes:** 3
- **Dungeons:** 5
- **Abilities:** 12+ (per class varies)

---

## ⚠️ Important Reminders

### Architecture Rules
- ✅ Zero Console I/O in game logic
- ✅ All data in JSON files
- ✅ Dependency injection everywhere
- ✅ Publish events, don't call methods
- ✅ Tests required for new features

### Before Committing
```bash
dotnet test                      # All tests must pass
git status                       # Review changes
```

### Commit Message Format
```
type: brief description

Detailed changes:
- Change 1
- Change 2

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
```

---

## 🎯 Next Session Checklist

1. [ ] Read README.md to understand current state
2. [ ] Check CONTENT_EXPANSION_ROADMAP.md for priorities
3. [ ] Run `dotnet test` to verify everything works
4. [ ] Review git status for any uncommitted work
5. [ ] Check DEVELOPMENT_LOG.md for recent decisions

---

**Remember:** The root `CLAUDE.md` file provides comprehensive onboarding for new AI assistants. This file is for quick navigation and current status.
