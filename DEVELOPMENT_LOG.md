# Development Log

This file tracks development sessions, changes made, and pending tasks for the TestRPGGame project.

---

## Session: 2025-01-10 - Major Balance & Systems Overhaul

### Context
The game had several balance issues and missing features that needed addressing. Rogue class was significantly overpowered, and players lacked quality-of-life features like save management.

### Completed Tasks ✅

#### 1. Fixed Test Failure - DataLoader Dungeon Encounters
- **Issue**: Test expected old `Encounters` property, but dungeon system was updated to use `EncounterPool`
- **Fix**: Updated test to check `EncounterPool` and `EncounterConfig` instead
- **Files**: `TestRPGGame.Tests/DataLoaderTests.cs:154`

#### 2. Fixed Critical Hit Damage on Special Attacks
- **Issue**: Crits were detected but 2x damage multiplier wasn't applied to special attacks
- **Fix**: Updated `DamageEffect.cs` to apply crit multiplier before calculating final damage
- **Files**: `TestRPGGame/Abilities/Effects/DamageEffect.cs:33-38`
- **Result**: Special attacks now properly deal double damage on critical hits

#### 3. Implemented Flee Mechanic
- **Feature**: Players can now flee from regular combat (disabled for boss battles)
- **Mechanics**:
  - Flee chance: 50% base ± speed difference (clamped 20-90%)
  - Success penalties: -25% gold, -20% max HP
  - Failure penalty: Enemy gets free attack
- **Files**: `TestRPGGame/Combat/CombatSystem.cs:232, 289-318`

#### 4. Fixed Heal Over Time for Enemies
- **Issue**: Enemy regeneration abilities weren't actually healing
- **Root Cause**:
  - `HealOverTimeEffect` was executed but never applied to enemy's `StatusEffects`
  - `ApplyEnemyTurnEffects()` was never being called
- **Fix**:
  - Added call to `enemy.StatusEffects.ApplyEnemyTurnEffects()` at start of enemy turns
  - Added handler for `HealOverTimeEffect` in enemy ability execution
- **Files**: `TestRPGGame/Combat/CombatSystem.cs:551-555, 643-652`

#### 5. Fixed Invalid Combat Choices
- **Change**: Invalid choices now prompt retry instead of losing your turn
- **Files**: `TestRPGGame/Combat/CombatSystem.cs:272-274`

#### 6. Removed AoE References
- **Change**: Updated ability descriptions to remove references to Area-of-Effect (doesn't make sense in 1v1 combat)
- **Examples**:
  - Whirlwind: "Deal 3.5x AoE damage" → "Devastating spinning strike for 3.5x damage"
  - Earthshaker: "Stunning AoE attack" → "Powerful ground smash that stuns"
- **Files**: `TestRPGGame/Data/abilities.json`

#### 7. Fixed Damage Over Time Effects
- **Issue**: Enemy abilities with "DamageOverTime" type weren't working
- **Root Causes**:
  - "DamageOverTime" wasn't mapped in EntityFactory
  - PoisonEffect handler was setting wrong variables
- **Fix**:
  - Added mapping: "damageovertime" → PoisonEffect
  - Added mappings for: healovertime, stun, thorns, shield, lifesteal
  - Updated enemy ability handler to apply burning to player via enemy.StatusEffects.DamageOverTime
- **Files**:
  - `TestRPGGame/Factories/EntityFactory.cs:64-69`
  - `TestRPGGame/Combat/CombatSystem.cs:676-682`

#### 8. Added Comprehensive Status Effect Display
- **Feature**: Battle status screen now shows ALL active effects for both player and enemy
- **Player Effects Shown**: Burning, Poisoned, Bleeding, Battle Rage, Dodge Ready
- **Enemy Effects Shown**: Regenerating, Burning, Bleeding, Shield, Thorns, Enraged, Stunned
- **Display Format**: Shows icon, name, amount per turn, and remaining turns
- **Files**: `TestRPGGame/Combat/CombatSystem.cs:187-235`

#### 9. Fixed Backstab Guaranteed Crit
- **Issue**: Backstab description said "guaranteed crit" but only crit on normal crit chance
- **Fix**:
  - Added `GuaranteedCrit` field to `AbilityEffectData` class
  - Updated factory to pass `guaranteedCrit` parameter
  - Updated backstab ability JSON to include `"GuaranteedCrit": true`
- **Result**: Backstab now ALWAYS crits (3x base × 2 crit = 6x total damage)
- **Files**:
  - `TestRPGGame/DataLoading/AbilityEffectData.cs:11`
  - `TestRPGGame/Factories/EntityFactory.cs:58`
  - `TestRPGGame/Data/abilities.json:195`

#### 10. Made Classes Data-Driven
- **Feature**: All class stats and progression now loaded from JSON configuration
- **Created**:
  - `Data/classes.json` - Complete class configurations
  - `DataLoading/ClassData.cs` - Class data model
- **Updated**:
  - `DataLoader` to load class data
  - `Player.cs` to use data-driven initialization
  - Level-up system to use data-driven stat growth
- **Benefits**: Easy balance tuning without code changes
- **Files**:
  - `TestRPGGame/Data/classes.json` (NEW)
  - `TestRPGGame/DataLoading/ClassData.cs` (NEW)
  - `TestRPGGame/DataLoading/DataLoader.cs:25, 78-83, 257-271`
  - `TestRPGGame/Entities/Player/Player.cs:60-95, 136-158`

#### 11. Balanced All Three Classes
**Previous Issues**:
- Rogue was overpowered: 35% base crit + guaranteed crit backstab + 18 attack
- Mage was too fragile: only 80 HP
- Warrior lagged behind in damage output

**New Balanced Stats** (classes.json):
```
| Class   | HP  | Mana | Atk | Def | Magic | Spd | Crit | HP/Lvl | Atk/Lvl |
|---------|-----|------|-----|-----|-------|-----|------|--------|---------|
| Warrior | 140 | 80   | 14  | 12  | 5     | 9   | 15%  | 12     | 2       |
| Mage    | 90  | 140  | 7   | 6   | 18    | 10  | 20%  | 7      | 1       |
| Rogue   | 110 | 90   | 12  | 8   | 6     | 14  | 25%  | 9      | 2       |
```

**Key Changes**:
- Rogue crit: 35% → 25% (still highest, but fair)
- Rogue attack: 18 → 12 (balanced with guaranteed crit backstab)
- Mage HP: 80 → 90 (better survivability)
- Warrior HP: 150 → 140 (more balanced)
- All classes have proportional scaling

#### 12. Implemented Starting Equipment System
- **Feature**: Each new character starts with randomized low-tier equipment
- **Equipment Given**:
  - **Weapon**: Class-appropriate type (level 1, mostly Common rarity)
    - Warriors/Rogues: Physical weapons (swords, daggers, axes, etc.)
    - Mages: Elemental weapons (fire/ice/lightning/arcane staffs, wands, etc.)
  - **Armor**: Random common armor with defensive stats
- **Stats Provided**: Typical bonuses: +10-20 HP, +3-7 attack/magic, +2-5 defense
- **Implementation**:
  - Uses existing `EquipmentGenerator.GenerateItem()` system
  - Filters weapon types to match class playstyle
  - Auto-equips to Weapon and Armor slots
- **Files**: `TestRPGGame/Entities/Player/Player.cs:97-141`

#### 13. Added Save Slot Deletion
- **Feature**: Players can delete save slots from character selection screen
- **Usage**: Type `D1`, `D2`, or `D3` to delete the corresponding slot
- **Safety**: Confirmation prompt before permanent deletion
- **UI**: Clear instructions and feedback messages
- **Files**: `TestRPGGame/Game.cs:84-117`

### Technical Improvements
- All 86 tests passing
- Updated tests to account for equipment bonuses
- Improved code organization with data-driven approach
- Better separation of concerns (data vs logic)

### Balance Impact
- Rogue no longer dominates early and late game
- Mage is more viable and less one-shot prone
- Warrior is competitive without being only a tank
- All classes start with meaningful equipment bonuses
- Class power curves are more balanced across levels

---

## Pending Tasks 📋

### High Priority
- None currently

### Medium Priority
- Consider adding more detailed equipment filtering (e.g., daggers for rogues, swords for warriors)
- Evaluate if starting equipment should always be Common or allow occasional Uncommon drops

### Low Priority / Future Ideas
- Add class-specific passive abilities to further differentiate playstyles
- Consider adding a "respec" system to allow players to rebalance their character
- Potential: Add more starting equipment slots (boots, gloves, etc.) for new characters

### Known Issues
- None currently

---

## Development Notes

### Class Balance Philosophy
The goal is to make all three classes viable with different playstyles:
- **Warrior**: Tanky bruiser with sustained damage and survivability
- **Mage**: Glass cannon with high burst damage and crowd control
- **Rogue**: High mobility DPS with critical strike focus

### Equipment System
The random starting equipment system ensures:
1. No two characters are exactly identical (adds replayability)
2. Players start with meaningful power boost
3. Class identity is preserved (mages don't get physical weapons)
4. Early game is less punishing

### Data-Driven Design
Moving to JSON configuration files allows:
- Easy balance patches without recompiling
- Community modding potential
- Rapid iteration on game balance
- Clear documentation of game mechanics

---

## Session Summary
This was a major overhaul focusing on game balance and quality-of-life improvements. The Rogue class has been brought in line with other classes, the equipment system now provides meaningful starting gear, and players have better control over their save files. The codebase is now more maintainable with the data-driven class system.

**Files Created**: 2
**Files Modified**: 15+
**Tests Passing**: 86/86 ✅
**Net Lines Changed**: ~500+

---

## Historical Bug Fixes Archive

### Shop Gold Calculation Bug (Fixed)
**Date**: Early development
**Problem**: Players with 154 gold told they couldn't afford 110 gold item
**Cause**: Error message calculation was backwards (item.Price - player.Gold showed negative)
**Fix**: Corrected calculation in ShopNew.cs:183
**Result**: ✅ Fixed

### Buff Duration Behavior (Design Decision)
**Problem**: "Battle Rage (3 turns)" expires after player acts once
**Cause**: Buffs decrement after EVERY action (player AND enemy)
**Current**: "3 turns" = 3 total actions
**Note**: This is working as designed (buffs count actions, not rounds)

### Ability Selection Not Showing (Fixed)
**Date**: 2025-11-10 (Commit 388d43c)
**Problem**: Rogue "Use Ability" option didn't show ability list
**Cause**: RequestCombatAction() returned UseAbility without prompting
**Fix**: Added RequestAbilitySelection() call in ConsoleInterface
**Result**: ✅ Fixed - abilities now display with mana costs and cooldowns

### Enemy Art Not Displaying (Fixed + Enhanced)
**Date**: 2025-11-10 (Commit 388d43c)
**Problem**: Enemy ASCII art wasn't displaying in combat
**Fix**: Added AsciiArt.DrawEnemy() call to CombatStartedEvent handler
**Enhancement**: Implemented data-driven enemy art system
  - Created Data/Enemies/enemy-art.json
  - Pattern-based matching (e.g., "Goblin" matches "Armored Goblin")
  - 8 enemy art templates included
  - Customizable colors per enemy type
**Result**: ✅ Fixed + modding support added

### AoE Abilities in 1v1 Combat (Clarification)
**Question**: How do AoE abilities work when combat is 1v1?
**Answer**: AoE abilities function identically to single-target in current system
**Purpose**: Boss flavor, future multi-enemy support, damage scaling
**Current**: No actual area effect (hits single target with higher multiplier)
**Future**: Could add multi-enemy encounters where AoE hits all targets

---
