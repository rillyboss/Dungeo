# Composable Effects System - Content Creator Guide

**Last Updated:** 2025-01-12
**Status:** Production Ready

---

## Overview

The **Composable Effects System** allows content creators to build unique, creative abilities by combining **generic effect types** with **custom display names** and **parameters** — all in JSON, without touching code.

### Key Concepts

- **EffectKind**: Generic effect type (e.g., `AttackBoost`, `DefenseBoost`, `Stun`)
- **DisplayName**: Custom name shown to players (e.g., "Enraged", "Iron Skin", "Shadow Cloak")
- **Parameters**: Multiplier (strength), Duration (turns), FlatValue (for flat bonuses)
- **Composition**: Combine multiple effects in a single ability

### Design Philosophy

**Code provides mechanics. Data provides creativity.**

Instead of hardcoding specific buffs like "Battle Rage" or "Enraged" in code, the system provides generic mechanics (AttackBoost) that you compose with custom names and values.

**Example:**
```
AttackBoost + Multiplier 1.5 + DisplayName "Battle Rage" = +50% attack for 3 turns
AttackBoost + Multiplier 1.8 + DisplayName "Enraged" = +80% attack for 2 turns
```

---

## Available Effect Types

### Stat Boosts (Buffs)

| EffectKind | Description | Parameters |
|------------|-------------|------------|
| `AttackBoost` | Increase attack power | Multiplier (1.5 = +50%) |
| `DefenseBoost` | Increase damage reduction | Multiplier (2.0 = +100%) |
| `SpeedBoost` | Increase speed | FlatValue (5 = +5 speed) OR Multiplier |
| `DamageBoost` | Amplify all damage dealt | Multiplier (1.3 = +30%) |
| `EvasionBoost` | Increase dodge chance | Multiplier (1.5 = +50% defense) |

### Stat Reductions (Debuffs)

| EffectKind | Description | Parameters |
|------------|-------------|------------|
| `AttackReduction` | Reduce enemy attack | Multiplier (0.5 = -50%) |
| `DefenseReduction` | Reduce enemy defense | Multiplier (0.7 = -30%) |
| `SpeedReduction` | Slow enemy | FlatValue (-5) OR Multiplier |
| `AccuracyReduction` | Reduce hit chance | Multiplier (0.8 = -20%) |

### Special Mechanics

| EffectKind | Description | Parameters |
|------------|-------------|------------|
| `Regeneration` | Heal over time | FlatValue (HP per turn) |
| `Shield` | Absorb damage | FlatValue (shield HP) |
| `Stun` | Disable target | Duration (turns stunned) |
| `DamageOverTime` | Poison/Burn/Bleed | FlatValue (damage per turn) |
| `LifeSteal` | Heal on damage | Multiplier (0.3 = 30% of damage) |
| `Thorns` | Reflect damage | FlatValue (damage reflected) |
| `Dodge` | Complete evasion | Duration (turns) |

---

## Creating Simple Effects

### Single Effect Format

```json
{
  "Type": "Effect",
  "EffectKind": "AttackBoost",
  "DisplayName": "Enraged",
  "Multiplier": 1.8,
  "Duration": 2
}
```

**What this does:**
- Generic mechanic: `AttackBoost` (increases attack stat)
- Custom name: `"Enraged"` (shown to player)
- Strength: `1.8` multiplier (+80% attack)
- Duration: `2` turns

### Examples: Attack Buffs

```json
// Battle Rage: +50% attack for 3 turns
{
  "Type": "Effect",
  "EffectKind": "AttackBoost",
  "DisplayName": "Battle Rage",
  "Multiplier": 1.5,
  "Duration": 3
}

// Bloodlust: +100% attack for 1 turn
{
  "Type": "Effect",
  "EffectKind": "AttackBoost",
  "DisplayName": "Bloodlust",
  "Multiplier": 2.0,
  "Duration": 1
}

// Frenzy: +60% attack for 4 turns
{
  "Type": "Effect",
  "EffectKind": "AttackBoost",
  "DisplayName": "Frenzy",
  "Multiplier": 1.6,
  "Duration": 4
}
```

### Examples: Defense Buffs

```json
// Iron Skin: +80% defense for 3 turns
{
  "Type": "Effect",
  "EffectKind": "DefenseBoost",
  "DisplayName": "Iron Skin",
  "Multiplier": 1.8,
  "Duration": 3
}

// Shield Wall: +100% defense for 2 turns
{
  "Type": "Effect",
  "EffectKind": "DefenseBoost",
  "DisplayName": "Shield Wall",
  "Multiplier": 2.0,
  "Duration": 2
}

// Fortified: +50% defense for 5 turns
{
  "Type": "Effect",
  "EffectKind": "DefenseBoost",
  "DisplayName": "Fortified",
  "Multiplier": 1.5,
  "Duration": 5
}
```

### Examples: Special Effects

```json
// Regeneration: 15 HP per turn for 5 turns
{
  "Type": "Effect",
  "EffectKind": "Regeneration",
  "DisplayName": "Regeneration",
  "FlatValue": 15,
  "Duration": 5
}

// Arcane Barrier: 100 HP shield for 3 turns
{
  "Type": "Effect",
  "EffectKind": "Shield",
  "DisplayName": "Arcane Barrier",
  "FlatValue": 100,
  "Duration": 3
}

// Stunned: Disabled for 1 turn
{
  "Type": "Effect",
  "EffectKind": "Stun",
  "DisplayName": "Stunned",
  "Duration": 1
}

// Shadow Cloak: +30% evasion for 3 turns
{
  "Type": "Effect",
  "EffectKind": "EvasionBoost",
  "DisplayName": "Shadow Cloak",
  "Multiplier": 1.3,
  "Duration": 3
}
```

---

## Creating Composite Effects

Composite effects allow you to combine **multiple effect types** in a single ability.

### Composite Effect Format

```json
{
  "Type": "Effect",
  "DisplayName": "Gladiator's Resolve",
  "Duration": 3,
  "CompositeEffects": [
    {
      "EffectKind": "AttackBoost",
      "Multiplier": 1.3,
      "FlatValue": 0
    },
    {
      "EffectKind": "DefenseBoost",
      "Multiplier": 1.3,
      "FlatValue": 0
    },
    {
      "EffectKind": "SpeedBoost",
      "Multiplier": 1.0,
      "FlatValue": 5
    }
  ]
}
```

**What this does:**
- Single buff with ONE display name: `"Gladiator's Resolve"`
- Applies THREE effects simultaneously:
  1. +30% attack
  2. +30% defense
  3. +5 speed
- All effects last 3 turns

### Example: Warrior Banner

```json
{
  "Name": "Commanding Presence",
  "Type": "Buff",
  "ManaCost": 25,
  "Cooldown": 5,
  "Description": "Rally your strength, gaining massive combat bonuses",
  "Effects": [
    {
      "Type": "Effect",
      "DisplayName": "Banner",
      "Duration": 4,
      "CompositeEffects": [
        {
          "EffectKind": "AttackBoost",
          "Multiplier": 1.2,
          "FlatValue": 0
        },
        {
          "EffectKind": "DefenseBoost",
          "Multiplier": 1.2,
          "FlatValue": 0
        },
        {
          "EffectKind": "SpeedBoost",
          "Multiplier": 1.0,
          "FlatValue": 3
        }
      ]
    }
  ]
}
```

### Example: Mage Time Warp

```json
{
  "Type": "Effect",
  "DisplayName": "Time Warp",
  "Duration": 2,
  "CompositeEffects": [
    {
      "EffectKind": "SpeedBoost",
      "Multiplier": 1.0,
      "FlatValue": 15
    },
    {
      "EffectKind": "EvasionBoost",
      "Multiplier": 1.5,
      "FlatValue": 0
    }
  ]
}
```

### Example: Rogue Shadow Dance

```json
{
  "Type": "Effect",
  "DisplayName": "Shadow Dance",
  "Duration": 3,
  "CompositeEffects": [
    {
      "EffectKind": "AttackBoost",
      "Multiplier": 1.4,
      "FlatValue": 0
    },
    {
      "EffectKind": "SpeedBoost",
      "Multiplier": 1.0,
      "FlatValue": 10
    },
    {
      "EffectKind": "EvasionBoost",
      "Multiplier": 1.3,
      "FlatValue": 0
    }
  ]
}
```

---

## Complete Ability Examples

### Example 1: Simple Self-Buff

```json
{
  "Name": "Enrage",
  "Type": "Buff",
  "ManaCost": 15,
  "Cooldown": 3,
  "Description": "Enter a berserk rage, massively boosting attack",
  "Effects": [
    {
      "Type": "Effect",
      "EffectKind": "AttackBoost",
      "DisplayName": "Enraged",
      "Multiplier": 1.8,
      "Duration": 2
    }
  ]
}
```

### Example 2: Damage + Debuff

```json
{
  "Name": "Armor Break",
  "Type": "Attack",
  "ManaCost": 20,
  "Cooldown": 4,
  "Description": "Strike with crushing force, reducing enemy defense",
  "Effects": [
    {
      "Type": "Damage",
      "Multiplier": 1.5
    },
    {
      "Type": "Effect",
      "EffectKind": "DefenseReduction",
      "DisplayName": "Exposed",
      "Multiplier": 0.6,
      "Duration": 3
    }
  ]
}
```

### Example 3: Multi-Effect Ultimate

```json
{
  "Name": "Avatar of War",
  "Type": "Buff",
  "ManaCost": 50,
  "Cooldown": 8,
  "UnlockLevel": 10,
  "Description": "Become an unstoppable force of destruction",
  "Effects": [
    {
      "Type": "Effect",
      "DisplayName": "Avatar of War",
      "Duration": 4,
      "CompositeEffects": [
        {
          "EffectKind": "AttackBoost",
          "Multiplier": 1.5,
          "FlatValue": 0
        },
        {
          "EffectKind": "DefenseBoost",
          "Multiplier": 1.5,
          "FlatValue": 0
        },
        {
          "EffectKind": "DamageBoost",
          "Multiplier": 1.3,
          "FlatValue": 0
        },
        {
          "EffectKind": "SpeedBoost",
          "Multiplier": 1.0,
          "FlatValue": 10
        }
      ]
    },
    {
      "Type": "Effect",
      "EffectKind": "Regeneration",
      "DisplayName": "Battle Regeneration",
      "FlatValue": 20,
      "Duration": 4
    }
  ]
}
```

---

## Backward Compatibility

The system still supports the OLD format for existing abilities:

### Old Format (Still Works)

```json
{
  "Type": "Buff",
  "BuffName": "Battle Rage",
  "Duration": 3
}
```

### New Format (Preferred)

```json
{
  "Type": "Effect",
  "EffectKind": "AttackBoost",
  "DisplayName": "Battle Rage",
  "Multiplier": 1.5,
  "Duration": 3
}
```

**Why use the new format?**
- More flexible (any multiplier, not hardcoded)
- More creative (custom display names)
- Composable (combine multiple effects)
- Data-driven (no code changes needed)

---

## Parameter Reference

### Type (Required)
- **Old System**: `"Damage"`, `"Buff"`, `"Restore"`, `"Poison"`, etc.
- **New System**: `"Effect"`

### EffectKind (Required for new format)
- See "Available Effect Types" section above
- Examples: `"AttackBoost"`, `"DefenseBoost"`, `"Stun"`, `"Regeneration"`

### DisplayName (Optional, recommended)
- Custom name shown to players
- Examples: `"Enraged"`, `"Iron Skin"`, `"Shadow Dance"`
- Defaults to EffectKind if not provided

### Multiplier (Optional, default 1.0)
- For percentage-based effects
- `1.5` = +50%, `2.0` = +100%, `0.7` = -30%
- Used for: Attack/Defense/Speed boosts, damage multipliers

### FlatValue (Optional, default 0)
- For flat numeric effects
- Used for: Regeneration HP, Shield HP, Speed bonuses, DoT damage

### Duration (Required for effects)
- How many turns the effect lasts
- Minimum: 1 turn
- Typical: 2-4 turns for buffs/debuffs

### CompositeEffects (Optional)
- Array of sub-effects to apply simultaneously
- Each sub-effect has: `EffectKind`, `Multiplier`, `FlatValue`
- All share the same Duration and DisplayName from parent effect

---

## Design Tips

### Creating Balanced Effects

**Short Duration, High Power**
```json
{
  "DisplayName": "Bloodlust",
  "EffectKind": "AttackBoost",
  "Multiplier": 2.0,
  "Duration": 1
}
```

**Long Duration, Moderate Power**
```json
{
  "DisplayName": "Battle Focus",
  "EffectKind": "AttackBoost",
  "Multiplier": 1.3,
  "Duration": 5
}
```

### Creating Unique Identities

Same mechanic, different flavor:

```json
// Warrior: Raw strength
{"DisplayName": "Enraged", "EffectKind": "AttackBoost", "Multiplier": 1.8}

// Mage: Arcane power
{"DisplayName": "Spell Power", "EffectKind": "DamageBoost", "Multiplier": 1.5}

// Rogue: Precision strikes
{"DisplayName": "Deadly Precision", "EffectKind": "AttackBoost", "Multiplier": 1.6}
```

### Creating Theme Synergies

**Tank Theme** (Warrior)
- High Defense Boosts
- Regeneration effects
- Shield effects
- Lower attack, longer duration

**Burst Theme** (Rogue)
- High Attack Boosts
- Speed increases
- Short duration, high power
- Evasion for survival

**Control Theme** (Mage)
- Stuns and debuffs
- Speed reductions
- Defense reductions on enemies
- Damage over time

---

## Common Patterns

### Self-Buff Ability
```json
{
  "Name": "Power Up",
  "Type": "Buff",
  "Effects": [
    {
      "Type": "Effect",
      "EffectKind": "AttackBoost",
      "DisplayName": "Powered Up",
      "Multiplier": 1.5,
      "Duration": 3
    }
  ]
}
```

### Attack + Debuff Combo
```json
{
  "Name": "Crippling Strike",
  "Type": "Attack",
  "Effects": [
    {"Type": "Damage", "Multiplier": 1.2},
    {
      "Type": "Effect",
      "EffectKind": "SpeedReduction",
      "DisplayName": "Crippled",
      "FlatValue": -5,
      "Duration": 2
    }
  ]
}
```

### Ultimate Multi-Buff
```json
{
  "Name": "Ultimate Form",
  "Type": "Buff",
  "ManaCost": 50,
  "Cooldown": 10,
  "Effects": [
    {
      "Type": "Effect",
      "DisplayName": "Ultimate Power",
      "Duration": 3,
      "CompositeEffects": [
        {"EffectKind": "AttackBoost", "Multiplier": 1.5},
        {"EffectKind": "DefenseBoost", "Multiplier": 1.5},
        {"EffectKind": "SpeedBoost", "FlatValue": 10}
      ]
    }
  ]
}
```

---

## Testing Your Effects

### 1. Build the project
```bash
dotnet build
```

### 2. Run tests
```bash
dotnet test
```

### 3. Test in-game
```bash
dotnet run
```

### 4. Check effect application
- Use the ability in combat
- Verify the DisplayName appears
- Check that stats change correctly
- Confirm duration works as expected

---

## Troubleshooting

### Effect Not Applying
- Check `EffectKind` spelling (case-sensitive)
- Ensure `Type: "Effect"` is set
- Verify Duration is > 0

### Wrong Effect Strength
- Check Multiplier value (1.5 = +50%, not +150%)
- For flat bonuses, use FlatValue not Multiplier
- Speed uses FlatValue for flat bonuses

### Composite Effect Issues
- All sub-effects must have valid EffectKind
- Use same Duration for all (set on parent)
- Use same DisplayName for all (set on parent)

### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet build
```

---

## Next Steps

1. **Read abilities.json** to see existing abilities
2. **Modify an existing ability** to test the system
3. **Create a new ability** with custom effects
4. **Test your changes** in-game
5. **Share cool combinations** you discover!

---

## Summary

**The composable effect system gives you:**
- ✅ Generic effect types (AttackBoost, DefenseBoost, etc.)
- ✅ Custom display names for unique flavor
- ✅ Flexible parameters (multipliers, flat values)
- ✅ Unlimited effect composition
- ✅ No code changes needed
- ✅ Full backward compatibility

**Create unique, balanced, thematic abilities entirely in JSON!**
