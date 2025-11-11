# Combat System Redesign: Min/Max Damage, Accuracy, and Agility

## 🎯 Design Goals

1. **Weapon/Ability Identity** - Min/Max damage creates distinct feels
2. **Skill-Based Combat** - Accuracy stat rewards precision
3. **Defensive Counterplay** - Agility provides dodge chance
4. **Strategic Depth** - Choice between reliable vs risky options

---

## 📊 Current vs Proposed System

### Current System
```csharp
Attack: 19              // Single flat value
Speed: 14               // Only determines turn order
Crit: 25%              // Only RNG element
```

### Proposed System
```csharp
Attack: 15-22          // Min-Max damage range
Accuracy: 85%          // Chance to hit
Agility: 14            // Turn order + dodge chance
Crit: 25%              // Still exists
```

---

## 1. 📦 Min/Max Damage System

### Weapon Properties
```csharp
public class EquipmentItem
{
    // OLD: Single attack bonus
    // public int AttackBonus { get; set; }

    // NEW: Damage range
    public int MinDamage { get; set; }
    public int MaxDamage { get; set; }
    public double Accuracy { get; set; } = 1.0;  // 1.0 = 100% accuracy

    // Calculate average for backwards compat
    public int AttackBonus => (MinDamage + MaxDamage) / 2;
}
```

### Ability Properties
```csharp
public class DamageEffect : IAbilityEffect
{
    // OLD: Single multiplier
    // public double Multiplier { get; set; }

    // NEW: Damage range multipliers
    public double MinMultiplier { get; set; }
    public double MaxMultiplier { get; set; }
    public double Accuracy { get; set; } = 1.0;

    // Calculate average for backwards compat
    public double Multiplier => (MinMultiplier + MaxMultiplier) / 2.0;
}
```

### Weapon Type Examples

```json
// weapons.json
{
  "Dagger": {
    "MinDamage": 8,
    "MaxDamage": 12,
    "Accuracy": 0.95,
    "Description": "Fast, precise, reliable"
  },
  "Sword": {
    "MinDamage": 10,
    "MaxDamage": 18,
    "Accuracy": 0.90,
    "Description": "Balanced, versatile"
  },
  "Axe": {
    "MinDamage": 5,
    "MaxDamage": 25,
    "Accuracy": 0.80,
    "Description": "High variance, devastating when it hits"
  },
  "Warhammer": {
    "MinDamage": 12,
    "MaxDamage": 20,
    "Accuracy": 0.85,
    "Description": "Heavy, consistent crushing damage"
  },
  "Staff": {
    "MinDamage": 15,
    "MaxDamage": 18,
    "Accuracy": 0.95,
    "Description": "Magical, very consistent"
  }
}
```

### Ability Examples

```json
// abilities.json
{
  "PowerStrike": {
    "Name": "Power Strike",
    "MinMultiplier": 1.8,
    "MaxMultiplier": 2.5,
    "Accuracy": 0.90,
    "Description": "Powerful but less accurate"
  },
  "PreciseStrike": {
    "Name": "Precise Strike",
    "MinMultiplier": 1.3,
    "MaxMultiplier": 1.6,
    "Accuracy": 0.98,
    "Description": "Reliable, accurate damage"
  },
  "Backstab": {
    "Name": "Backstab",
    "MinMultiplier": 2.5,
    "MaxMultiplier": 3.5,
    "Accuracy": 0.85,
    "Description": "High risk, huge reward"
  },
  "Fireball": {
    "Name": "Fireball",
    "MinMultiplier": 1.5,
    "MaxMultiplier": 2.0,
    "Accuracy": 0.80,
    "Description": "Explosive but can miss"
  }
}
```

---

## 2. 🎯 Accuracy & Miss System

### How It Works

```csharp
// Base accuracy from weapon/ability
double baseAccuracy = weapon.Accuracy * ability.Accuracy;

// Target's dodge chance (from agility)
double dodgeChance = CalculateDodgeChance(target.Agility, attacker.Level);

// Final hit chance
double hitChance = baseAccuracy * (1.0 - dodgeChance);

// Roll to hit
bool hit = random.NextDouble() < hitChance;
```

### Dodge Chance Calculation

```csharp
public static double CalculateDodgeChance(int agility, int attackerLevel)
{
    // Base dodge from agility
    // 10 agility = 5% dodge, 20 agility = 10% dodge, etc.
    double baseDodge = agility / 200.0;

    // Cap at 30% dodge (can't become unhittable)
    return Math.Min(0.30, baseDodge);
}
```

### Examples

| Attacker Accuracy | Target Dodge | Final Hit Chance |
|-------------------|--------------|------------------|
| 100% (precise)    | 5%           | 95%              |
| 90% (sword)       | 10%          | 81%              |
| 85% (backstab)    | 5%           | 80.75%           |
| 80% (fireball)    | 15%          | 68%              |

### Miss Messages

```csharp
if (!hit)
{
    string missReason = random.NextDouble() < dodgeChance / (1.0 - hitChance)
        ? "dodged"
        : "missed";

    Console.WriteLine($"{attacker.Name}'s attack {missReason}!");
    return 0; // No damage
}
```

Output examples:
- `"Warrior's Power Strike missed!"`
- `"Mage's Fireball was dodged!"`
- `"Goblin's attack missed!"`

---

## 3. ⚡ Speed → Agility System

### Stat Changes

```csharp
public class Combatant
{
    // OLD
    // public int Speed { get; set; }

    // NEW
    public int Agility { get; set; }

    // Backwards compatibility
    public int Speed => Agility;
}
```

### Agility Benefits

```csharp
// 1. Turn Order (existing functionality)
int turnOrder = player.Agility + player.Effects.GetSpeedModifier();

// 2. NEW: Dodge Chance
double dodgeChance = player.Agility / 200.0;  // 10 agility = 5% dodge
dodgeChance = Math.Min(0.30, dodgeChance);    // Cap at 30%

// 3. NEW: Crit Chance Bonus (optional)
double critBonus = player.Agility / 500.0;    // 50 agility = +10% crit
```

### Display

```csharp
// Character sheet shows both
Console.WriteLine($"Agility: {player.Agility}");
Console.WriteLine($"  → Turn Priority: {player.Agility > enemy.Agility ? "First" : "Second"}");
Console.WriteLine($"  → Dodge Chance: {player.Agility / 200.0:P0}");
Console.WriteLine($"  → Crit Bonus: {player.Agility / 500.0:P0}");
```

Example output:
```
Agility: 20
  → Turn Priority: First
  → Dodge Chance: 10%
  → Crit Bonus: 4%
```

---

## 4. 🎲 Complete Damage Calculation Flow

### New Algorithm

```csharp
// 1. Check if attack hits
double baseAccuracy = weapon.Accuracy * ability.Accuracy;
double dodgeChance = target.Agility / 200.0;
double hitChance = baseAccuracy * (1.0 - Math.Min(0.30, dodgeChance));

if (random.NextDouble() >= hitChance)
{
    // MISS!
    PublishMissEvent();
    return 0;
}

// 2. Roll damage within min/max range
int weaponDamage = random.Next(weapon.MinDamage, weapon.MaxDamage + 1);
double abilityRoll = ability.MinMultiplier +
    (random.NextDouble() * (ability.MaxMultiplier - ability.MinMultiplier));

int baseDamage = (int)(weaponDamage * abilityRoll);

// 3. Apply status effect multipliers
baseDamage = (int)(baseDamage * statusEffectMultipliers);

// 4. Check for critical hit
bool isCrit = random.NextDouble() < (player.CritChance + player.Agility / 500.0);
if (isCrit)
{
    baseDamage = (int)(baseDamage * 2);
}

// 5. Apply defense reduction (existing formula)
double defenseReduction = target.Defense / (double)(target.Defense + 100);
int finalDamage = Math.Max(1, (int)(baseDamage * (1 - defenseReduction)));

return finalDamage;
```

---

## 5. 📈 Strategic Implications

### Weapon Choices

**Dagger (8-12, 95% accuracy):**
- ✅ Consistent, reliable
- ✅ Rarely misses
- ❌ Lower max damage
- **Best for:** Sustained DPS, beginners

**Sword (10-18, 90% accuracy):**
- ✅ Balanced variance
- ✅ Good accuracy
- ✅ Decent max damage
- **Best for:** All-around versatility

**Axe (5-25, 80% accuracy):**
- ✅ HUGE max damage potential
- ✅ High risk, high reward
- ❌ Can roll very low
- ❌ Misses more often
- **Best for:** Glass cannon builds, boss bursting

**Staff (15-18, 95% accuracy):**
- ✅ Most consistent magic damage
- ✅ High accuracy
- ❌ Low variance (boring?)
- **Best for:** Mages who hate RNG

### Ability Synergies

**High Agility Rogue:**
- Uses accurate abilities (Backstab 85%)
- High dodge chance (15-20%)
- Bonus crit from agility
- → Mobile, evasive, precise striker

**Low Agility Warrior:**
- Uses reliable weapons (sword/hammer)
- Takes hits but has high defense
- Consistent damage output
- → Tanky, dependable damage

**Mage with Staff:**
- Very consistent damage (95% accuracy)
- Low dodge but high burst
- Predictable DPS for mana planning
- → Glass cannon with reliable output

---

## 6. 🎮 Player Experience Examples

### Combat Flow (New System)

```
Turn 1: Warrior attacks with Greatsword (12-24 dmg, 85% accuracy)
  → Hit! Rolled 19 damage
  → After defense: 17 damage dealt

Turn 2: Goblin attacks (5% dodge chance)
  → Attack was dodged!
  → 0 damage

Turn 3: Warrior uses Power Strike (1.8x-2.5x, 90% accuracy)
  → Hit! Rolled 2.3x multiplier
  → Greatsword rolled 21 damage
  → 21 * 2.3 = 48 damage
  → After defense: 42 damage dealt

Turn 4: Goblin attacks
  → Hit! Rolled 8 damage
  → After defense: 3 damage dealt

Turn 5: Warrior attacks
  → Critical hit! (2x)
  → Greatsword rolled 18 damage * 2 = 36
  → After defense: 32 damage dealt

Turn 6: Goblin attacks
  → Attack missed!
  → 0 damage
```

**Player Feelings:**
- ✅ Every attack feels different
- ✅ Dodges feel awesome
- ✅ High rolls are exciting
- ✅ Misses create tension (but not frustration if accuracy is fair)

---

## 7. 🔧 Implementation Plan

### Phase 1: Data Model Updates
1. Add `MinDamage`, `MaxDamage`, `Accuracy` to EquipmentItem
2. Add `MinMultiplier`, `MaxMultiplier`, `Accuracy` to DamageEffect
3. Rename `Speed` → `Agility` everywhere
4. Update JSON data files with new properties

### Phase 2: Core Combat Logic
1. Implement accuracy/dodge calculation
2. Implement min/max damage rolling
3. Update damage calculation flow
4. Add miss/dodge event system

### Phase 3: UI/Display
1. Show weapon damage ranges (e.g., "Sword: 10-18 damage")
2. Show accuracy on abilities
3. Display dodge chance on character sheet
4. Add miss/dodge combat messages

### Phase 4: Balance & Data
1. Set min/max for all weapon types
2. Set accuracy for all abilities
3. Tune dodge formula
4. Playtest and iterate

### Phase 5: Testing
1. Unit tests for accuracy/dodge calculations
2. Integration tests for full combat flow
3. Automated combat simulations
4. Player feedback testing

---

## 8. 📝 JSON Data Examples

### Weapon Type Data
```json
{
  "WeaponTypes": {
    "Dagger": {
      "MinDamageBase": 8,
      "MaxDamageBase": 12,
      "Accuracy": 0.95,
      "Variance": "Low",
      "Description": "Quick, precise strikes"
    },
    "Greatsword": {
      "MinDamageBase": 15,
      "MaxDamageBase": 30,
      "Accuracy": 0.80,
      "Variance": "High",
      "Description": "Devastating but unwieldy"
    }
  }
}
```

### Ability Data
```json
{
  "PowerStrike": {
    "Name": "Power Strike",
    "ManaCost": 20,
    "Cooldown": 2,
    "Effects": [
      {
        "Type": "Damage",
        "MinMultiplier": 1.8,
        "MaxMultiplier": 2.5,
        "Accuracy": 0.90,
        "UsesMagic": false
      }
    ],
    "Description": "A powerful but less accurate strike (1.8x-2.5x damage, 90% accuracy)"
  }
}
```

### Class Starting Stats
```json
{
  "Rogue": {
    "BaseAgility": 14,  // Was Speed
    "BaseAccuracy": 0.95,
    "BaseDodge": "7%",  // From agility
    "Description": "High agility provides dodge and accuracy"
  },
  "Warrior": {
    "BaseAgility": 9,
    "BaseAccuracy": 0.90,
    "BaseDodge": "4.5%",
    "Description": "Lower agility, relies on armor"
  }
}
```

---

## 9. ⚖️ Balance Considerations

### Accuracy Sweet Spots
- **95%+** - Very reliable (daggers, precise abilities)
- **90%** - Standard accuracy (most weapons)
- **85%** - Moderate risk (power abilities)
- **80%** - High risk (devastating abilities)
- **<75%** - Probably too frustrating

### Dodge Caps
```csharp
// Dodge formula with diminishing returns
double baseDodge = agility / 200.0;
double finalDodge = Math.Min(0.30, baseDodge); // Hard cap at 30%
```

**Why cap at 30%?**
- Prevents "unkillable" builds
- Keeps combat engaging
- Maintains attacker agency
- 30% still feels impactful

### Damage Variance Guidelines
- **Tight** (±20%): 10-12, 20-24 → Reliable, boring
- **Medium** (±30%): 10-14, 20-28 → Balanced
- **Wide** (±50%): 10-18, 20-36 → Exciting, risky
- **Extreme** (±80%+): 5-25 → Slot machine, frustrating

---

## 10. 🎯 Success Metrics

### Goals
- ✅ No two attacks feel exactly the same
- ✅ Weapon choice matters strategically
- ✅ Agility is valuable for all classes
- ✅ Misses are rare enough to not frustrate
- ✅ High variance weapons feel exciting, not punishing
- ✅ Combat has more "WOW!" moments

### Testing Checklist
- [ ] Can land 100 hits, track damage distribution
- [ ] Verify dodge chance scales correctly
- [ ] Confirm accuracy formula feels fair
- [ ] Check that misses aren't frustrating
- [ ] Ensure high variance weapons are fun
- [ ] Test that agility builds are viable

---

## Summary

**Current System:**
- Flat attack value → boring, predictable
- Speed only for turn order → wasted stat potential
- No misses → no tension

**Proposed System:**
- Min/Max damage → exciting variance, weapon identity
- Accuracy stat → skill-based gameplay, strategic choices
- Agility → dodge chance + turn order + crit bonus

**Result:** Combat becomes WAY more engaging with strategic depth!

---

## Next Steps

1. Want me to start implementing this?
2. Any balance concerns to address first?
3. Should we do this incrementally or all at once?

This is a MUCH better system than simple variance! 🎲⚔️✨
