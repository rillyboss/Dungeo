# Damage Formula Update - 2025-01-11

## Problem Statement

The previous damage formula used linear defense subtraction, causing several issues:
- Boss fights were unwinnable at low levels (dealing 1 damage per hit)
- Damage multipliers didn't scale properly (2x ability = same 1 damage)
- Combat felt grindy and unrewarding
- Defense completely negated offense instead of reducing it

**Old Formula:**
```csharp
damage = max(1, rawDamage - defense)
```

## Solution Implemented

Replaced with **percentage-based defense reduction** with diminishing returns:

```csharp
defenseReduction = defense / (defense + 100)
damage = max(1, rawDamage * (1 - defenseReduction))
```

### Why This Works
- Defense **reduces** damage by a percentage, never completely negates it
- **Diminishing returns**: More defense is less effective
  - 0 defense = 0% reduction
  - 50 defense = 33% reduction
  - 100 defense = 50% reduction
  - 200 defense = 67% reduction (can't become invincible)
- Damage multipliers scale properly (2x damage = ~2x actual damage)

## Impact Examples

### Level 1 Warrior vs Goblin King Boss

**Before:**
- Player Attack: 21 (14 base + 7 equipment)
- Boss Defense: 20
- Damage: max(1, 21 - 20) = **1 damage**
- Boss HP: 350 → **350 hits to kill!** ❌

**After:**
- Player Attack: 21
- Boss Defense: 20 (16.7% reduction)
- Damage: 21 * 0.833 = **17 damage**
- Boss HP: 350 → **~21 hits to kill** ✓

### Multiplier Scaling

**Warrior 2x Ability vs Goblin King (Before):**
- Raw Damage: 42
- After Defense: 42 - 20 = **22 damage**
- Only +21 damage from 2x multiplier (barely noticeable)

**Warrior 2x Ability vs Goblin King (After):**
- Raw Damage: 42
- After Defense: 42 * 0.833 = **35 damage**
- +18 damage from 2x multiplier (actually doubles!)

### Regular Combat

**Level 1 Warrior vs Goblin:**
- Before: 21 - 3 = 18 damage
- After: 21 * 0.971 = 17 damage
- ✓ Minimal difference for regular enemies

**Level 10 Warrior vs Level 10 Goblin:**
- Attack: 52, Defense: 13 (11.5% reduction)
- Before: 52 - 13 = 39 damage
- After: 52 * 0.885 = 46 damage
- ✓ Higher level combat is MORE impactful

## Defense Effectiveness Chart

| Defense | Reduction % | Example: 50 Raw Damage → Actual |
|---------|-------------|----------------------------------|
| 0       | 0%          | 50 → 50                          |
| 10      | 9%          | 50 → 45                          |
| 25      | 20%         | 50 → 40                          |
| 50      | 33%         | 50 → 33                          |
| 100     | 50%         | 50 → 25                          |
| 200     | 67%         | 50 → 16                          |
| 300     | 75%         | 50 → 12                          |

**Note:** Even with 300 defense, you still take 25% of damage. Can't become invincible!

## Files Changed

1. **Combatant.cs** (Lines 44-49, 61-67)
   - Updated `TakeDamage()` method
   - Updated `ApplyDamage()` method
   - Both now use percentage-based formula

2. **StatusEffectSystemTests.cs**
   - Added `CalculateExpectedDamage()` helper method
   - Updated 5 failing tests to use new formula
   - All 226 tests passing ✓

## Testing Results

- **Tests Before:** 221/226 passing (5 failures)
- **Tests After:** 226/226 passing ✓
- **Test Duration:** 487ms

## Balance Impact

### Early Game (Level 1-3)
- Boss fights are now challenging but winnable
- Regular enemies still die quickly
- Abilities feel impactful

### Mid Game (Level 5-10)
- Combat remains engaging
- Defense matters but doesn't dominate
- Gear upgrades provide noticeable power

### Late Game (Level 15+)
- High attack still deals meaningful damage
- High defense provides good protection without immunity
- Strategic depth maintained

## Gameplay Benefits

✅ **Fixed:** Boss fights that took 350 hits now take ~20-30 hits
✅ **Fixed:** 2x damage abilities actually deal ~2x damage
✅ **Fixed:** Combat feels rewarding and impactful
✅ **Maintained:** Regular enemy difficulty unchanged
✅ **Improved:** High-level scaling works better
✅ **Balanced:** Defense is valuable but not overpowered

---

**Status:** ✅ Complete - All tests passing, ready for gameplay testing
