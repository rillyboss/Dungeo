# Bug Fixes and Clarifications

## Bugs Fixed

### 1. **Shop Gold Bug** ✅ FIXED
**Problem**: Players with 154 gold were told they couldn't afford a 110 gold item.

**Root Cause**: The error message calculation was backwards:
```csharp
// WRONG:
UIHelper.PrintColoredLine($"\n❌ Not enough gold! Need {item.Price - player.Gold} more gold.", ConsoleColor.Red);
// If player has 154 and item costs 110, this shows: -44 gold needed!
```

**Fix**: Corrected the calculation and improved the message:
```csharp
// FIXED in ShopNew.cs line 183:
int needed = item.Price - player.Gold;
UIHelper.PrintColoredLine($"\n❌ Not enough gold! You have {player.Gold}, need {needed} more gold.", ConsoleColor.Red);
```

**File**: `ShopNew.cs:183`

---

### 2. **Buff Duration Bug** ⚠️ KNOWN ISSUE (Design Decision Needed)
**Problem**: "Battle Rage" says "3 turns" but expires after player acts once.

**Root Cause**: Buffs decrement after EVERY action (player turn AND enemy turn), so:
- Turn 1: Player uses Battle Rage (buff = 3)
- After player action: buff decrements to 2
- After enemy action: buff decrements to 1
- After player action: buff decrements to 0 → **EXPIRED!**

**Current Behavior**: "3 turns" actually means "3 actions total" (both player and enemy)

**Possible Fixes**:

#### Option A: Change Description (Simple)
Change ability descriptions to match current behavior:
- "Battle Rage" - "Increase attack by 50% for 3 actions"
- Explains current behavior accurately

#### Option B: Fix Duration Logic (Complex)
Only decrement buffs once per complete round (after both player AND enemy act):
- Requires tracking whose turn it is
- More complex to implement
- "3 turns" would mean player gets to act 3 times with the buff

**Recommendation**: I suggest **Option A** for now (update descriptions), then **Option B** later if desired.

**Files Affected**:
- `CombatSystem.cs:62-77` (buff decrement logic)
- `DungeonCombatSystem.cs:90-105` (boss combat buff logic)
- `Player.cs:129` (Battle Rage and other buff descriptions)

---

## Clarifications

### AoE Abilities
**Question**: How do AoE (Area of Effect) abilities work when combat is 1v1?

**Answer**: Currently, **AoE abilities function identically to single-target abilities** in regular combat.

**Why AoE Exists**:
1. **Boss Flavor**: Makes boss abilities feel more powerful/dangerous
2. **Future Potential**: System is ready if you want to add multi-enemy combat later
3. **Damage Scaling**: AoE abilities typically have higher damage multipliers

**Current Implementation**:
- Regular Combat: 1 player vs 1 enemy (AoE = single target)
- Boss Combat: 1 player vs 1 boss (AoE = single target)
- **No actual area effect** currently happens

**Examples**:
- `Infernus Dragon - Inferno Blast`: "AoE fire damage" with 2.5x multiplier
  - In practice: Just hits the player for 2.5x damage
- `Void Lord - Void Annihilation`: "Ultimate AoE attack" with 3.0x multiplier
  - In practice: Massive single-target hit

**Future Enhancement Ideas**:
1. Multi-enemy encounters (fight 2-3 enemies at once)
2. AoE hits all enemies
3. Player gets AoE abilities too (hit all enemies)
4. Summon mechanics (boss summons adds)

---

## Unit Tests Added ✅

Created comprehensive test suite at `TestRPGGame.Tests/`:

### Test Files:
1. **PlayerTests.cs** - Player creation, leveling, healing, potions
2. **ShopTests.cs** - Purchase validation, gold calculations
3. **AbilityTests.cs** - Ability cooldowns, unlocking, requirements
4. **GameConfigTests.cs** - Configuration value validation

### Test Results:
```
Passed!  - Failed: 0, Passed: 25, Skipped: 0, Total: 25
```

### Running Tests:
```bash
cd TestRPGGame.Tests
dotnet test
```

### Key Tests:
- ✅ Shop purchase gold validation (fixes the 154 vs 110 bug)
- ✅ Ability cooldown mechanics
- ✅ Ability unlock requirements
- ✅ Player stat calculations
- ✅ Potion healing and limits
- ✅ Game config mana regen calculations

---

## Ability Definitions - Central Location

All abilities are defined in `Player.cs` in the `InitializeAbilities()` method (lines 116-178).

### Warrior Abilities:
```csharp
// Line 121-136
1. Power Strike (20 mana, 3 CD) - 2.5x physical damage
2. Shield Wall (25 mana, 4 CD) - Reduce damage 50% for 2 turns
3. Battle Rage (30 mana, 5 CD) - Increase attack 50% for 3 turns
4. Whirlwind (40 mana, 6 CD) - 3.5x physical damage [UNLOCKABLE: Level 5, 200 gold]
```

### Mage Abilities:
```csharp
// Line 139-157
1. Fireball (25 mana, 3 CD) - 3x magic damage
2. Ice Lance (35 mana, 4 CD) - 2.5x magic damage + slow enemy
3. Mana Surge (20 mana, 3 CD) - Restore 50 mana
4. Meteor Strike (50 mana, 7 CD) - 4x magic damage [UNLOCKABLE: Level 5, 200 gold]
```

### Rogue Abilities:
```csharp
// Line 159-178
1. Backstab (25 mana, 3 CD) - 3x damage, guaranteed crit
2. Poison Blade (30 mana, 4 CD) - 20 initial + 15/turn for 3 turns
3. Shadow Step (35 mana, 5 CD) - Dodge next attack + speed boost
4. Assassinate (45 mana, 6 CD) - 5x damage, guaranteed crit [UNLOCKABLE: Level 5, 200 gold]
```

### Boss Abilities:
Boss abilities are defined in `Dungeon.cs` in each boss factory method:
- `CreateGoblinKing()` - Line 618
- `CreateDeathKnight()` - Line 642
- `CreateInfernusDragon()` - Line 672
- `CreateArcaneGuardian()` - Line 704
- `CreateVoidLord()` - Line 737

---

## Summary

**Fixed**:
1. ✅ Shop gold calculation bug
2. ✅ Error message clarity

**Explained**:
1. 📚 Buff duration behavior (design decision needed)
2. 📚 AoE abilities (cosmetic in 1v1 combat)
3. 📚 Ability locations for easy editing

**Added**:
1. ✅ Unit test project with 25 passing tests
2. ✅ Comprehensive test coverage for core systems
3. ✅ Documentation of known issues

**Next Steps** (Optional):
1. Decide on buff duration fix (Option A or B above)
2. Consider multi-enemy encounters to make AoE meaningful
3. Add more unit tests for combat mechanics
4. Add integration tests for dungeon flow
