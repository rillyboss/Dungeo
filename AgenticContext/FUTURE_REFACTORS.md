# Future Refactoring Opportunities

**Status:** Informational - No immediate work required
**Last Updated:** 2025-01-12

---

## Overview

This document outlines **potential** future refactoring opportunities. These are **NOT** immediate priorities - they are ideas for long-term code improvement when the project matures further.

**Current Focus:** Content expansion (see CONTENT_EXPANSION_ROADMAP.md)

---

## ✅ COMPLETED REFACTORS

### Damage Calculation System (Completed 2025-01-11)
**Problem:** Linear defense subtraction caused 1-damage hits on bosses, making combat grindy and unrewarding.

**Solution Implemented:** Percentage-based defense reduction with diminishing returns
```csharp
defenseReduction = defense / (defense + 100)
damage = max(1, rawDamage * (1 - defenseReduction))
```

**Results:**
- Boss fights reduced from 350 hits → ~21 hits
- 2x damage abilities properly deal ~2x damage
- All 226 tests passing
- Combat feels impactful and rewarding

See `DAMAGE_FORMULA_UPDATE.md` for detailed analysis and examples.

---

## Damage and Balance System

### 4. Evaluate and Rebalance Damage Calculation System
**Goal:** Fix the damage calculation formula to make combat more intuitive and engaging
**Current Issues:**
- Defense scaling too strongly - a 2x damage "Fierce Strike" dealing only 1 damage feels unintuitive
- Most monsters dealing ~1 damage regardless of attack power
- Player dealing ~1 damage to bosses even with high attack stats
- Armor/Defense appears to be too powerful relative to Attack values
- Combat feels grindy and lacks impact when attacks consistently deal minimal damage

**Problem Analysis:**
Current formula appears to be: `damage = max(1, attack * multiplier - defense)`
- Linear defense subtraction means high defense completely negates attack scaling
- Base stats may be too close (e.g., if Attack=50 and Defense=48, multipliers don't matter much)
- No diminishing returns on defense, making armor too powerful at higher levels
- Minimum damage of 1 prevents total negation but makes all attacks feel the same

**Proposed Solutions to Investigate:**

**Option 1: Percentage-based Defense**
```csharp
// Defense reduces damage by a percentage instead of flat reduction
double defenseReduction = defense / (defense + 100); // Gives ~50% reduction at 100 defense
int actualDamage = (int)(baseDamage * (1 - defenseReduction));
```

**Option 2: Armor Penetration Formula**
```csharp
// Defense has diminishing returns
double damageReduction = defense / (defense + attack);
int actualDamage = (int)(baseDamage * (1 - damageReduction));
```

**Option 3: Tiered Reduction System**
```csharp
// Defense reduces a percentage of damage, capped
double reductionPercent = Math.Min(0.75, defense / 200.0); // Max 75% reduction
int actualDamage = (int)(baseDamage * (1 - reductionPercent));
```

**Option 4: Stat Scaling Adjustment**
- Increase base attack values relative to defense values
- Add attack scaling per level that outpaces defense scaling
- Reduce enemy defense values or increase player attack progression

**Areas to Evaluate:**
1. Current attack/defense stat ranges across all levels
2. Attack and defense scaling per level for each class
3. Enemy stat scaling vs player stat scaling
4. Boss stat values vs player capabilities at expected levels
5. Weapon/armor contribution to total stats
6. Whether critical hits and multipliers provide meaningful damage increases

**Testing Checklist:**
- [ ] Document current attack/defense values for player levels 1, 5, 10, 15, 20
- [ ] Document enemy attack/defense values across difficulty tiers
- [ ] Test damage formulas with sample values to find sweet spot
- [ ] Ensure 2x damage multipliers result in ~2x visible damage (not 1 → 2)
- [ ] Verify boss fights feel challenging but not impossible
- [ ] Check that equipment upgrades provide noticeable power increases
- [ ] Ensure different enemy types feel distinct in combat

**Success Criteria:**
- A 2x damage ability should deal significantly more than a basic attack (not 1 vs 1)
- Base attacks should deal meaningful damage (5-20% of enemy HP, not 1-2%)
- Defense should matter but not completely negate offense
- Combat progression should feel rewarding as stats increase
- Boss fights should be challenging but winnable with proper strategy

## Combat System Refactoring

### 1. Unify Enemy and Player Combat Systems
**Goal:** Reduce code duplication by using the same systems for both player and enemy
**Current Issues:**
- Player and Enemy have different ways of handling status effects (activeBuffs dictionary vs StatusEffects)
- Duplicate logic for combat actions, damage calculation, etc.
- Inconsistent patterns make maintenance harder

**Proposed Solution:**
- Create a base `Combatant` class or interface that both Player and Enemy implement
- Standardize all combat-related functionality (damage, healing, status effects, abilities)
- Use composition over inheritance where appropriate

### 2. Refactor CombatStatusEffects to Use Effect List Pattern
**Goal:** Replace individual properties with a flexible list-based system similar to MMO implementations
**Current Issues:**
- CombatStatusEffects has individual properties for each effect type (HealOverTimeTurns, DamageOverTimeTurns, etc.)
- Adding new effect types requires modifying the class
- Cannot have multiple instances of similar effects with different sources

**Proposed Solution:**
```csharp
public class CombatStatusEffect
{
    public string EffectId { get; set; }         // Unique identifier
    public string Name { get; set; }             // Display name (e.g., "Burning", "Cursed Flames")
    public string Icon { get; set; }             // Display icon/emoji
    public EffectCategory Category { get; set; } // HealOverTime, DamageOverTime, StatBuff, etc.
    public int Value { get; set; }               // Effect strength
    public int RemainingTurns { get; set; }      // Duration
    public Dictionary<string, object> Metadata { get; set; } // Additional data
}

public class CombatStatusEffects
{
    public List<CombatStatusEffect> ActiveEffects { get; set; }

    public void AddEffect(CombatStatusEffect effect) { }
    public void RemoveEffect(string effectId) { }
    public void TickEffects(Combatant owner, Combatant opponent) { }
    public List<CombatStatusEffect> GetEffectsByCategory(EffectCategory category) { }
}
```

**Benefits:**
- Easy to add new effect types without modifying core classes
- Support multiple effects of the same type (e.g., two different DoTs)
- Better display customization (different names/icons for thematically similar effects)
- Easier to implement stacking, refresh, and replacement logic
- More data-driven approach (effects could be defined in JSON)

### 3. Allow Thematically Different Names for Similar Effects
**Goal:** Enable effects with similar mechanics but different themes to have unique identities
**Examples:**
- "Burning" (fire damage DoT) vs "Bleeding" (physical damage DoT) vs "Cursed Flames" (dark fire DoT)
- "Regeneration" (nature healing HoT) vs "Dark Regeneration" (shadow healing HoT)
- "Speed Boost" vs "Haste" vs "Quicken"

**Implementation:**
- Each effect instance has its own Name and Icon properties
- Effect behavior is defined by Category + Value, not by name
- Display layer shows the unique name/icon while combat logic uses category

**Additional Benefits:**
- Better storytelling and immersion
- Clearer feedback to players about what's affecting them
- Enables boss-specific or class-specific themed abilities

## Implementation Priority
1. ~~Add Priority flags to abilities~~ ✅ **COMPLETED**
2. ~~Test current speed-based turn order system~~ ✅ **COMPLETED**
3. ~~Evaluate and rebalance damage calculation system~~ ✅ **COMPLETED** (See DAMAGE_FORMULA_UPDATE.md)
4. Plan Effect List Pattern refactor (design phase)
5. Implement base Combatant pattern
6. Migrate to Effect List Pattern
7. Update ability system to work with new effect pattern

## Notes
- These refactors should be done incrementally with testing between each phase
- Maintain backward compatibility where possible during transition
- Consider adding unit tests for combat system before major refactoring
