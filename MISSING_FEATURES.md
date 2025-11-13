# Missing Features & Known Issues

This document tracks features that are partially implemented or causing issues in automated testing.

**Last Updated:** 2025-01-12

---

## 🚨 CRITICAL: Ability Unlock/Purchase System

**Status:** Not yet implemented
**Priority:** HIGH - Blocking automated testing
**Menu Option:** MainMenuChoice.UnlockAbilities exists but not functional

### The Problem

The game has a "UnlockAbilities" menu option in the main menu, and the `IGameInterface` has a `RequestAbilityUnlock()` method, but the underlying system to unlock/purchase additional abilities beyond the 3 starting abilities is not implemented.

**Impact:**
- Causes infinite loops in automated testing when AI tries to unlock abilities at level 3+
- Players cannot access the 3 additional unlockable abilities per class
- Automated strategies must skip this menu option entirely

### What Exists (Partial Implementation)

1. ✅ Menu option: `MainMenuChoice.UnlockAbilities`
2. ✅ Interface method: `RequestAbilityUnlock(List<AbilityInfo>, int playerGold, int playerLevel)`
3. ✅ 6 abilities per class defined in abilities.json (3 starting + 3 unlockable)
4. ❌ **Missing:** Actual unlock/purchase mechanism
5. ❌ **Missing:** Gold cost and level requirement enforcement
6. ❌ **Missing:** Persistence of unlocked abilities in save system
7. ❌ **Missing:** UI flow for browsing and purchasing locked abilities

### What Needs To Be Done

1. **Implement UnlockAbilities Menu Handler in GameCore**
   - Create method to handle `MainMenuChoice.UnlockAbilities`
   - Filter abilities to show only those that are:
     - Not already unlocked
     - Meet level requirements
     - Player can afford
   - Call `gameInterface.RequestAbilityUnlock()` with filtered list
   - Deduct gold and add ability to player's available abilities

2. **Update Abilities Data**
   - Add `unlockCost` field to abilities.json (e.g., 200-500 gold)
   - Add `unlockLevel` field to abilities.json (e.g., level 3, 6, 9)
   - Ensure abilities have proper `isStartingAbility` flag

3. **Update Save System**
   - Add `unlockedAbilityIds` list to SaveData
   - Persist which abilities have been unlocked
   - Restore unlocked abilities on load

4. **Add Tests**
   - Test ability unlock with sufficient gold
   - Test ability unlock denial (insufficient gold, too low level)
   - Test save/load of unlocked abilities
   - Test automated interface behavior

5. **Update Automated Strategies**
   - Re-enable the ability unlock logic once system is working
   - Test that it doesn't cause infinite loops

### Current Workaround

The automated testing strategies (Level15Strategy and UltraThinkStrategy) have the ability unlock logic commented out:

```csharp
// TODO: Ability unlocking system not yet implemented - skip for now
// if (level >= 3 && (level % 3 == 0 || gold > 500))
// {
//     return MainMenuChoice.UnlockAbilities;
// }
```

This allows automated playthroughs to proceed without hanging.

### Related Files

- `GameCore.cs` - Needs ability unlock handler
- `Data/abilities.json` - Needs unlock cost/level fields
- `Systems/SaveManager.cs` - Needs to persist unlocked abilities
- `Interfaces/AutomatedInterface.cs` - Automated strategies with workaround
- `Entities/Player/Player.cs` - May need UnlockAbility() method

---

## Other Known Issues

_(None currently tracked)_

---

## How to Use This Document

When implementing missing features:
1. Review the "What Needs To Be Done" section
2. Follow existing patterns (event-driven, SOLID, DI)
3. Write tests first
4. Update this document when complete
5. Remove workarounds from automated strategies
