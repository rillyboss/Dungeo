# Multiple Effects System

## Overview

**ALL effect-bearing entities now support multiple effects!** This includes:
- ✅ **Player Abilities** - Can have multiple effects (already supported)
- ✅ **Boss Abilities** - Now support multiple effects
- 🔄 **Equipment** - Will support multiple special effects (future)

The code has **zero knowledge** of specific combinations - it only knows how to **apply effects** defined in JSON.

## Boss Abilities with Multiple Effects

### JSON Format

Boss abilities can now define an `Effects` array with multiple effects that all trigger when the ability is used:

```json
{
  "Name": "Void Drain",
  "Description": "Life steal + DoT combo",
  "Cooldown": 4,
  "Effects": [
    {
      "Type": "LifeSteal",
      "Value": 40,
      "Duration": 0,
      "Multiplier": 1.8
    },
    {
      "Type": "DamageOverTime",
      "Value": 15,
      "Duration": 3,
      "Multiplier": 1.0
    }
  ]
}
```

### Backward Compatibility

The system still supports the old single-effect format:

```json
{
  "Name": "Simple Attack",
  "Description": "Just damage",
  "Cooldown": 3,
  "Effect": {
    "Type": "HeavyStrike",
    "Value": 50,
    "Duration": 0,
    "Multiplier": 2.0
  }
}
```

Both formats work! Use `Effect` for single effects, `Effects` for multiple.

## Available Boss Effect Types

| Effect Type | Parameters | Description |
|-------------|-----------|-------------|
| `HealOverTime` | Value, Duration | Boss regenerates HP each turn |
| `DamageOverTime` | Value, Duration | Player burns each turn |
| `Stun` | Duration, Multiplier | Stuns player + deals damage |
| `Thorns` | Value, Duration | Reflects damage back to player |
| `Enrage` | Duration, Multiplier | Increases boss damage multiplier |
| `LifeSteal` | Value, Multiplier | Drains player HP, heals boss |
| `Shield` | Value, Duration | Absorbs incoming damage |
| `Bleed` | Value, Duration, Multiplier | Initial damage + DoT |
| `HeavyStrike` | Value, Multiplier | High burst damage |
| `StatBoost` | Value, Duration, Multiplier | Boosts boss stats |

## Example Multi-Effect Abilities

### 1. Drain + Weaken Combo
```json
{
  "Name": "Soul Siphon",
  "Description": "Drains life and weakens defenses",
  "Cooldown": 5,
  "Effects": [
    {
      "Type": "LifeSteal",
      "Value": 50,
      "Duration": 0,
      "Multiplier": 1.5
    },
    {
      "Type": "DamageOverTime",
      "Value": 20,
      "Duration": 4,
      "Multiplier": 1.0
    }
  ]
}
```

**Result:** Boss deals 1.5x damage, steals 50 HP, AND applies burning for 20 damage/turn for 4 turns!

### 2. Triple Threat
```json
{
  "Name": "Chaos Blast",
  "Description": "Massive damage, bleed, and thorns",
  "Cooldown": 7,
  "Effects": [
    {
      "Type": "HeavyStrike",
      "Value": 80,
      "Duration": 0,
      "Multiplier": 2.5
    },
    {
      "Type": "Bleed",
      "Value": 25,
      "Duration": 5,
      "Multiplier": 1.0
    },
    {
      "Type": "Thorns",
      "Value": 35,
      "Duration": 3,
      "Multiplier": 1.0
    }
  ]
}
```

**Result:**
1. Deals 2.5x boss attack + 80 burst damage
2. Applies bleed for 25 damage/turn for 5 turns
3. Boss gains thorns reflecting 35 damage for 3 turns

### 3. Self-Buff + Attack
```json
{
  "Name": "Berserker Rage",
  "Description": "Enrages and strikes with fury",
  "Cooldown": 6,
  "Effects": [
    {
      "Type": "Enrage",
      "Value": 0,
      "Duration": 4,
      "Multiplier": 2.0
    },
    {
      "Type": "HeavyStrike",
      "Value": 60,
      "Duration": 0,
      "Multiplier": 1.8
    },
    {
      "Type": "HealOverTime",
      "Value": 30,
      "Duration": 3,
      "Multiplier": 1.0
    }
  ]
}
```

**Result:**
1. Boss enrages (2x damage for 4 turns)
2. Immediately strikes for 1.8x + 60 damage (WITH enrage bonus!)
3. Boss regenerates 30 HP/turn for 3 turns

### 4. Defensive Combination
```json
{
  "Name": "Fortress Mode",
  "Description": "Maximum defense",
  "Cooldown": 8,
  "Effects": [
    {
      "Type": "Shield",
      "Value": 200,
      "Duration": 3,
      "Multiplier": 1.0
    },
    {
      "Type": "Thorns",
      "Value": 40,
      "Duration": 3,
      "Multiplier": 1.0
    },
    {
      "Type": "HealOverTime",
      "Value": 35,
      "Duration": 3,
      "Multiplier": 1.0
    }
  ]
}
```

**Result:** Boss creates 200 HP shield, reflects 40 damage, AND regenerates 35 HP/turn!

### 5. DoT Stack
```json
{
  "Name": "Plague Touch",
  "Description": "Multiple damage over time effects",
  "Cooldown": 5,
  "Effects": [
    {
      "Type": "DamageOverTime",
      "Value": 18,
      "Duration": 5,
      "Multiplier": 1.0
    },
    {
      "Type": "Bleed",
      "Value": 22,
      "Duration": 4,
      "Multiplier": 1.2
    }
  ]
}
```

**Result:** Player takes initial bleed damage (1.2x + 22), PLUS 18 burning/turn + 22 bleed/turn!

## Player Abilities (Already Support Multiple Effects)

Player abilities have always supported multiple effects:

```json
{
  "Id": "mage_ice_lance",
  "Name": "Ice Lance",
  "Description": "Deal 2.5x magic damage + slow enemy",
  "PlayerClass": "Mage",
  "ManaCost": 35,
  "Cooldown": 4,
  "Type": "Magic",
  "IsStarting": true,
  "IsUnlockable": false,
  "UnlockLevel": 0,
  "PurchaseCost": 0,
  "Effects": [
    {
      "Type": "Damage",
      "Value": 0,
      "Duration": 0,
      "Multiplier": 2.5
    },
    {
      "Type": "StatMod",
      "Value": -3,
      "Duration": 0
    }
  ]
}
```

### Player Effect Types

| Effect Type | Parameters | Description |
|-------------|-----------|-------------|
| `Damage` | Multiplier | Direct damage attack |
| `Buff` | BuffName, Duration | Applies named buff |
| `Restore` | Value | Restores HP or mana |
| `StatMod` | Value | Modifies enemy stats |
| `Dodge` | - | Dodge next attack |
| `Poison` | Value, Duration, DamagePerTurn | Poison damage over time |

## Equipment (Future Enhancement)

Equipment will support multiple special effects:

```json
{
  "Name": "Blade of Chaos",
  "SpecialEffects": [
    {
      "Type": "DoubleDamage",
      "ProcChance": 25
    },
    {
      "Type": "LifeSteal",
      "Value": 20,
      "ProcChance": 30
    },
    {
      "Type": "ChainLightning",
      "Value": 50,
      "ProcChance": 15
    }
  ]
}
```

## Implementation Details

### How It Works

1. **JSON Definition** - Effects defined as array in data files
2. **Data Loading** - `DataLoader` reads Effects array (or single Effect for compatibility)
3. **Entity Creation** - `EntityFactory` converts each effect data to effect object
4. **Execution** - Combat system loops through all effects and applies each one

### Code Flow (Boss Abilities)

```
JSON Data → BossAbilityData.Effects[] →
  EntityFactory.CreateBossAbility() →
  BossAbility.Effects (List<BossAbilityEffect>) →
  DungeonCombatSystem.ExecuteBossAbility() →
  foreach (effect in ability.Effects) →
  ExecuteSingleBossEffect() →
  Apply effect to game state
```

### Execution Order

Effects execute in the order they're defined:

```json
"Effects": [
  { "Type": "Enrage", ... },      // 1. Boss enrages first
  { "Type": "HeavyStrike", ... }  // 2. Then strikes (with enrage bonus!)
]
```

This matters! Put buffs BEFORE attacks to apply bonuses.

## Design Benefits

### For Game Designers
- ✅ Mix and match effects without code changes
- ✅ Create complex boss mechanics via JSON
- ✅ Balance abilities by tweaking numbers
- ✅ Test combinations instantly (restart game)

### For Modders
- ✅ Add custom ability combinations
- ✅ Create themed boss fights
- ✅ Balance total conversions
- ✅ Share ability definitions as JSON

### For Code
- ✅ Zero knowledge of specific combos
- ✅ Generic effect application system
- ✅ Easy to add new effect types
- ✅ Fully testable

## Creating New Boss Fights

Want a boss that heals while attacking and applies thorns? Just JSON:

```json
{
  "Name": "Regenerating Fortress",
  "BossAbilities": [
    {
      "Name": "Vampiric Spikes",
      "Description": "The ultimate defense",
      "Cooldown": 5,
      "Effects": [
        {
          "Type": "LifeSteal",
          "Value": 60,
          "Duration": 0,
          "Multiplier": 1.6
        },
        {
          "Type": "Thorns",
          "Value": 45,
          "Duration": 4,
          "Multiplier": 1.0
        },
        {
          "Type": "HealOverTime",
          "Value": 25,
          "Duration": 3,
          "Multiplier": 1.0
        }
      ]
    }
  ]
}
```

✅ Done! Boss now drains life, reflects damage, AND regenerates HP!

## Summary

**The multiple effects system makes the game infinitely more flexible:**

- ❌ Before: One ability = one effect (limited combinations)
- ✅ Now: One ability = unlimited effects (infinite possibilities)

**Code remains simple:**
```csharp
foreach (var effect in ability.Effects)
{
    ExecuteSingleBossEffect(player, boss, effect, ...);
}
```

**Data becomes powerful:**
```json
"Effects": [
  { ... },  // Effect 1
  { ... },  // Effect 2
  { ... },  // Effect 3
  // Add as many as you want!
]
```

**Data is king. Code is just the interpreter.** 👑
