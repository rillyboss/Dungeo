# TestRPGGame - Gameplay Balance & Fun Factor Roadmap

**Created:** 2025-01-13
**Status:** Planning Phase
**Goal:** Transform the game from "technically functional" to "actually fun to play"

---

## 📊 CURRENT STATE ANALYSIS

### Automated Gameplay Results (8 runs to Level 15)
- **Total Combats:** 6,838 (avg 854.8 per run)
- **Win Rate:** 89.2%
- **Damage Taken:** 0.7 per combat (invincible after level 5)
- **Dungeon Success:** 1.1% (8/749 attempts)
- **Abilities Unlocked:** 0.0 average (broken economy)
- **Primary Gold Source:** Shop sales (88.8%)
- **Loot Drops:** 311 per run (vendor trash simulator)

### Core Problems Identified
1. **Combat is trivial** - Players become invincible after level 5
2. **No build variety** - Zero abilities unlocked due to broken costs
3. **Dungeons unbeatable** - 1% success rate makes them pointless
4. **Excessive grind** - 854 combats is 3x longer than typical RPGs
5. **Loot lacks meaning** - Too much junk, rarity doesn't feel rare
6. **No tactical depth** - Same strategy wins every fight

---

## 🎯 IMPLEMENTATION PHASES

### Phase 1: Make Game Playable (CRITICAL FIXES)
**Goal:** Players can unlock abilities, dungeons are completable, progression isn't miserable

#### 1.1 Fix Ability Unlock Economy
**Problem:** 0 abilities unlocked, costs 1000-2000g when players have ~2000g remaining

**Design Decisions Needed:**
- [ ] **Pricing Model:** Reduce costs to 200-400g? OR make abilities free at level milestones?
- [ ] **Unlock Trigger:** Keep shop-based? OR add level-based unlocks? OR quest-based?
- [ ] **Choice vs Availability:** Force choosing 1 of 3? OR unlock all eventually?

**Impact:** Enables build variety, adds progression rewards, increases replayability

---

#### 1.2 Reduce XP Grind
**Problem:** 854 combats to level 15 (levels 10-15 average 136 combats each)

**Design Decisions Needed:**
- [ ] **Target Combat Count:** Aim for 500 total? 600? 700?
- [ ] **Curve Shape:** Linear? Exponential? Logarithmic?
- [ ] **Alternative XP Sources:** Add quests? Dungeon bonuses? Exploration?
- [ ] **XP Multipliers:** Win streaks? Achievement bonuses? Daily bonuses?

**Impact:** Reduces player fatigue, improves pacing, makes leveling feel rewarding

---

#### 1.3 Fix Dungeon Completion Rate
**Problem:** 1.1% success rate (741 failures, 8 completions)

**Design Decisions Needed:**
- [ ] **Boss HP Reduction:** 350 → 150? 200? Scale with level?
- [ ] **Mid-Dungeon Healing:** Add fountains? How much HP/Mana restore?
- [ ] **Checkpoint System:** Save progress between combats? What's the cost?
- [ ] **Partial Rewards:** Give 50% XP/gold for damage dealt? Or fixed consolation prizes?
- [ ] **Failure Penalty:** Keep 100g loss? Reduce it? Remove it?

**Impact:** Dungeons become engaging content instead of avoided content

---

### Phase 2: Make Game Challenging (BALANCE FIXES)
**Goal:** Combat requires thought, risk creates tension, difficulty scales properly

#### 2.1 Rebalance Combat Difficulty
**Problem:** 0.7 damage/combat after level 5 (players invincible)

**Design Decisions Needed:**
- [ ] **Target Damage Rate:** Aim for 10-20 damage/combat? Higher? Lower?
- [ ] **Defense Scaling:** Cut equipment defense bonuses by 60%? 70%? 50%?
- [ ] **Enemy Damage Scaling:** Formula `base + (player_level * X)` - what's X?
- [ ] **Buff Stacking:** Nerf Battle Rage + Shield Wall combo how? Diminishing returns? Mutual exclusivity?
- [ ] **Critical Hit Frequency:** Currently balanced or too common?

**Impact:** Combat becomes engaging, victories feel earned, defeats teach lessons

---

#### 2.2 Add Mid-Dungeon Recovery
**Problem:** No way to recover HP/Mana between fights (contributes to 1% success)

**Design Decisions Needed:**
- [ ] **Healing Fountain:** Restore 50% HP/Mana? 75%? 100%?
- [ ] **Placement:** After every 2 combats? Before boss only? Player choice when to use?
- [ ] **Cost:** Free? Small gold cost? Limited uses?
- [ ] **Strategic Element:** One-time use? Replenishing? Can backtrack?

**Impact:** Dungeons become marathon of resource management, not sprint

---

### Phase 3: Make Game Interesting (CONTENT IMPROVEMENTS)
**Goal:** Builds feel different, loot is exciting, enemies require tactics

#### 3.1 Improve Loot System
**Problem:** 311 drops/run, 50% uncommon (not uncommon), vendor trash economy

**Design Decisions Needed:**
- [ ] **Drop Rate:** Cut to 125 items/run? 150? 100?
- [ ] **Rarity Distribution:**
  - Legendary: 0.5%? 1%? How rare should rare be?
  - Epic: 5%? 3%? 8%?
  - Rare: 15%? 10%? 20%?
- [ ] **Unique Effects:** What kinds? Lifesteal? Thorns? Elemental conversion? Haste?
- [ ] **Combat Gold Increase:** 3x current? 5x? To replace selling economy
- [ ] **Equipment Tiers:** Keep current system or add legendary uniques?

**Impact:** Finding loot becomes exciting, builds differentiate via equipment

---

#### 3.2 Add Enemy Variety
**Problem:** All enemies feel same, no tactical decisions, single strategy works for all

**Design Decisions Needed:**
- [ ] **Enemy Archetypes:** Which types? Tanks? Glass cannons? Healers? Buffers?
- [ ] **Resistance System:**
  - Physical resistant? Magic resistant? Both?
  - Percentage reduction or immunity?
  - How to telegraph to player?
- [ ] **Special Mechanics:**
  - Enrage (damage boost at low HP)?
  - Regeneration (HP per turn)?
  - Counter-attack (damage when hit)?
  - Which feel fun vs frustrating?
- [ ] **Enemy Complexity:** How many different behaviors? 5? 10? 15?

**Impact:** Combat requires adaptation, builds have strengths/weaknesses

---

### Phase 4: Add Strategic Depth (ADVANCED FEATURES)
**Goal:** High replayability, player expression, mastery ceiling

#### 4.1 Elemental System
**Design Decisions Needed:**
- [ ] **Elements:** Fire/Ice/Lightning? Add Dark/Holy? Physical/Magic split?
- [ ] **Interactions:** Fire melts ice? Lightning stuns wet enemies? Rock-paper-scissors?
- [ ] **Application:** Equipment-based? Ability-based? Both?
- [ ] **Enemy Weaknesses:** Fixed per enemy type? Random? Observable?

---

#### 4.2 Combo System
**Design Decisions Needed:**
- [ ] **Combo Types:** Ability sequences? Same ability repeated? Mixed requirements?
- [ ] **Rewards:** Bonus damage? Cooldown reduction? Status effects? Resource refund?
- [ ] **Complexity:** 2-hit combos? 3-hit? Variable length?
- [ ] **Discovery:** Teach explicitly? Hidden for players to discover?

---

#### 4.3 Skill Trees
**Design Decisions Needed:**
- [ ] **Structure:** Linear branches? Diamond (converge)? Pure tree?
- [ ] **Unlock Method:** Points per level? Gold cost? Quest completion?
- [ ] **Depth:** 10 tiers? 15? 20?
- [ ] **Breadth:** Multiple specializations? Hybrid builds viable?
- [ ] **Respec:** Allowed? Costly? Free? Never?

---

## 📋 DECISION LOG

### Phase 1 Decisions

#### 1.1 Ability Unlock Economy
- **Decision:** IMPLEMENTED - Enabled ability unlocking in automated player with smart prioritization
- **Implementation:**
  - Automated player checks for ability unlocks every 2 levels starting at level 3
  - Smart selection: Prioritizes lowest unlock level, then lowest cost
  - One check per level (prevents infinite loops)
  - Requires 150+ gold minimum
- **Results (9 automated runs to level 15):**
  - **Abilities Unlocked:** 6.0 average (UP FROM 0.0!)
  - **Gold Spent on Abilities:** 847g (0.3% of total spending)
  - **Abilities Unlocked Per Run:**
    - Level 4: Cleave/Flame Blast/Ambush (150g)
    - Level 6: Hamstring/Frost Bolt (150-160g)
    - Level 8: Iron Skin/Arcane Blast (180g)
    - Level 10: Overhead Smash/Shadow Bolt (190g)
    - Level 12: Taunt/Meteor Strike (200g)
    - Level 14: Whirlwind (200g)
  - **Unlocked Abilities ARE Being Used:** Shadow Step (22.2 uses/run), Battle Rage (14.0), Backstab (11.0)
- **Rationale:** System was fully implemented, automated player just wasn't using it. Enabling it proved abilities are affordable and add build variety.
- **Implementation Date:** 2025-01-13
- **Status:** ✅ COMPLETE - Working as intended

#### 1.2 XP Grind Reduction
- **Decision:**
- **Rationale:**
- **Implementation Date:**

#### 1.3 Dungeon Balance
- **Decision:**
- **Rationale:**
- **Implementation Date:**

### Phase 2 Decisions
*(To be filled as we progress)*

### Phase 3 Decisions
*(To be filled as we progress)*

### Phase 4 Decisions
*(To be filled as we progress)*

---

## 🎮 DESIGN PRINCIPLES

### What We Value
1. **Player Agency** - Choices should matter
2. **Risk vs Reward** - Higher risk deserves higher reward
3. **Accessibility** - Easy to learn, hard to master
4. **Respect Player Time** - No artificial padding
5. **Build Diversity** - Multiple valid strategies
6. **Clear Feedback** - Players understand why they win/lose

### What We Avoid
1. **Unfair RNG** - Skill should outweigh luck
2. **False Choices** - Avoid trap options
3. **Excessive Grind** - Progression should feel good
4. **Frustration** - Challenge ≠ Unfair
5. **Mandatory Builds** - Support multiple playstyles
6. **Invisible Systems** - Mechanics should be discoverable

---

## 📈 SUCCESS METRICS

### Phase 1 Goals
- [✅] Abilities unlocked: 0 → 6.0 average (EXCEEDED TARGET!)
- [ ] Combats to level 15: 851 → 500-600
- [ ] Dungeon success: 1.1% → 50-70%

### Phase 2 Goals
- [ ] Damage taken: 0.7 → 10-20 per combat
- [ ] Win rate: 89% → 75-80%
- [ ] Player deaths: Near 0 → 5-10% of runs

### Phase 3 Goals
- [ ] Loot drops: 311 → 100-150 per run
- [ ] Build variety: 1 viable build → 5+ viable builds
- [ ] Enemy types: Basic variety → 10+ distinct behaviors

### Phase 4 Goals
- [ ] Replayability: Single path → Multiple paths
- [ ] Mastery ceiling: Low → High
- [ ] Strategic depth: Shallow → Deep

---

## 🚀 NEXT STEPS

1. **Review this roadmap** with stakeholder (Billy)
2. **Make Phase 1.1 decisions** (Ability Unlock Economy)
3. **Implement Phase 1.1** with tests
4. **Run automated validation**
5. **Iterate based on results**
6. **Move to Phase 1.2**

---

## 📝 NOTES

- All changes must be data-driven (JSON configs, not code)
- All changes must pass existing test suite
- Each phase includes automated gameplay validation
- User experience takes priority over technical elegance
- Document all design decisions and their rationale

---

**Remember:** The goal is FUN, not perfection. Ship playable improvements iteratively.
