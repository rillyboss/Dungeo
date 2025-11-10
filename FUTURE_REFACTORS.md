# Future Refactoring Tasks

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
1. Add Priority flags to abilities (current task)
2. Test current speed-based turn order system
3. Plan Effect List Pattern refactor (design phase)
4. Implement base Combatant pattern
5. Migrate to Effect List Pattern
6. Update ability system to work with new effect pattern

## Notes
- These refactors should be done incrementally with testing between each phase
- Maintain backward compatibility where possible during transition
- Consider adding unit tests for combat system before major refactoring
