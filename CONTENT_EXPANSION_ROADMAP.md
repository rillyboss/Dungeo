# Content Expansion Roadmap

## Overview
This roadmap builds upon the solid foundation established in the code improvement phases. All new features will follow existing design patterns: event-driven architecture, dependency injection, SOLID principles, data-driven design, and comprehensive testing.

---

## **Phase 1: Statistics Tracking System** ✅ COMPLETE
**Priority:** HIGH (Foundation for achievements)
**Estimated Complexity:** Medium
**Dependencies:** None
**Status:** ✅ Implemented and tested (375 tests passing)

### Goals
- Track detailed player statistics across game sessions
- Persist stats with save files
- Provide foundation for achievement system

### Implementation Plan
1. **Create StatisticsTracker class**
   - Tracks: kills by enemy type, deaths, damage dealt/taken, gold earned/spent, dungeons completed, abilities used, items purchased, etc.
   - Subscribe to existing GameEvents (CombatEndedEvent, DamageDealtEvent, ItemPurchasedEvent, etc.)
   - Pattern: Observer pattern (already used in event system)
   - Inject IGameInterface for event subscription

2. **Add Statistics to SaveSystem**
   - Extend SaveData to include PlayerStatistics
   - Serialize/deserialize with player save

3. **Create StatisticsManager**
   - Similar to ProgressionManager, DungeonManager pattern
   - Methods: GetStatistics(), ResetStatistics(), GetLifetimeStats()
   - Display stats through IGameInterface.DisplayStatistics()

4. **Add to Main Menu**
   - New MainMenuChoice.Statistics option
   - Display comprehensive stat sheet

### Testing
- StatisticsTrackerTests: Event handling, stat increments, persistence
- StatisticsManagerTests: Display, reset, lifetime tracking
- Integration tests: End-to-end stat tracking through combat/dungeon

### Files to Create
- `Systems/StatisticsTracker.cs`
- `Systems/StatisticsManager.cs`
- `Entities/Player/PlayerStatistics.cs`
- `Tests/StatisticsTrackerTests.cs`
- `Tests/StatisticsManagerTests.cs`

### ✅ Completion Summary
**Implemented:**
- ✅ PlayerStatistics class with 40+ tracked statistics across combat, economy, progression, and exploration
- ✅ StatisticsTracker with event-driven tracking via EventTrackingInterface decorator
- ✅ StatisticsManager following existing manager pattern (ProgressionManager, DungeonManager)
- ✅ Extended SaveSystem to persist statistics (backward compatible with old saves)
- ✅ Statistics menu option in GameCore with full ConsoleInterface display
- ✅ Comprehensive unit tests (StatisticsTrackerTests with 19 tests)
- ✅ All 375 tests passing (100% pass rate)

**Key Features:**
- Tracks kills by enemy type, damage dealt/taken, gold earned/spent, dungeon completions, ability usage, items bought/sold, potions used, rests taken, bosses defeated, legendary items found, and much more
- Calculates derived statistics (K/D ratio, average damage per combat, dungeon success rate, net gold)
- Beautiful formatted display with categories and colors
- Full persistence with save files

---

## **Phase 2: Achievement System** ✅ COMPLETE
**Priority:** HIGH (Highly visible feature)
**Estimated Complexity:** Medium
**Dependencies:** Phase 1 (Statistics)
**Status:** ✅ Implemented and tested (375 tests passing)

### Goals
- Reward players for accomplishments
- Provide long-term goals
- Add replay value

### Implementation Plan
1. **Create Achievement framework**
   - Achievement class: Name, Description, Condition, Reward, IsUnlocked
   - IAchievementCondition interface for checking unlock status
   - Built-in conditions: StatThresholdCondition, CombinedCondition, etc.
   - Pattern: Strategy pattern for conditions

2. **Define achievements in JSON**
   - `Data/achievements.json` - data-driven like enemies/abilities
   - Categories: Combat, Exploration, Collection, Progression, Special
   - Examples:
     - "First Blood" - Kill first enemy
     - "Giant Slayer" - Kill 100 enemies
     - "Treasure Hunter" - Earn 10,000 gold
     - "Dungeon Master" - Complete all dungeons
     - "Legendary Hero" - Reach level 50

3. **Create AchievementManager**
   - Check conditions after each game event
   - Publish AchievementUnlockedEvent
   - Persist unlocked achievements with save
   - Award rewards (gold, items, titles)

4. **UI Integration**
   - Display achievement progress
   - Show unlock notifications
   - Achievement gallery with completion percentage

### Testing
- AchievementTests: Condition evaluation, unlock logic
- AchievementManagerTests: Event handling, rewards, persistence
- Integration tests: Unlock achievements through gameplay

### Files to Create
- `Systems/Achievement.cs`
- `Systems/AchievementManager.cs`
- `Systems/Conditions/IAchievementCondition.cs`
- `Systems/Conditions/StatThresholdCondition.cs`
- `Data/achievements.json`
- `Tests/AchievementTests.cs`
- `Tests/AchievementManagerTests.cs`

### ✅ Completion Summary
**Implemented:**
- ✅ Complete condition framework (4 condition types: StatThreshold, KillEnemyType, DungeonCompletion, CombinedCondition)
- ✅ Achievement class with full reward system (gold, XP, titles, points)
- ✅ 27 diverse achievements across 6 categories (Combat, Exploration, Economy, Collection, Progression, Special)
- ✅ AchievementManager with JSON loading, condition checking, and unlock tracking
- ✅ Full persistence through SaveSystem (4-tuple architecture with backward compatibility)
- ✅ Achievements UI in ConsoleInterface with progress display, categories, and beautiful formatting
- ✅ Integrated into GameCore with automatic checking after combat, dungeons, shop visits, rests, and saves
- ✅ AchievementUnlockedEvent with rewards automatically granted
- ✅ All 375 tests passing (100% pass rate)

**Key Features:**
- Automatic achievement checking after all major game events
- Real-time unlock notifications with reward details
- Progress tracking for locked achievements
- Hidden achievements that only show after unlock
- Points system for achievement completion percentage
- Category-based organization for easy browsing
- Full integration with statistics tracking from Phase 1

---

## **Phase 3: Equipment-Granted Abilities** ⚔️✨
**Priority:** HIGH (Enhances equipment meaningfully)
**Estimated Complexity:** Medium
**Dependencies:** None (uses existing ability system)

### Goals
- Make equipment more interesting and build-defining
- Equipment can grant temporary abilities while equipped
- Encourage experimentation with different gear combinations

### Implementation Plan
1. **Extend Equipment class**
   - Add `List<Ability> GrantedAbilities { get; set; }`
   - Pattern: Composition (equipment "has" abilities)

2. **Extend EquipmentGenerator**
   - Rare/Legendary items can roll with granted abilities
   - Define ability pools in JSON (`Data/Items/equipment-abilities.json`)
   - Examples:
     - Flaming Sword → grants "Flame Strike" ability
     - Ring of Shields → grants "Shield Barrier" ability
     - Boots of Speed → grants "Dash" ability

3. **Update PlayerInventory**
   - When equipping item: Add granted abilities to player.Abilities
   - When unequipping: Remove granted abilities
   - Mark abilities as "Equipment-Granted" vs "Learned"
   - Don't allow unlocking equipment-granted abilities (they're temporary)

4. **Balance considerations**
   - Equipment abilities don't cost gold to unlock
   - Balance by making equipment rare
   - Powerful abilities only on legendary items

### Testing
- EquipmentAbilityTests: Grant/remove abilities on equip/unequip
- PlayerInventoryTests: Ability management with equipment changes
- Integration tests: Use equipment abilities in combat

### Files to Modify
- `Equipment/Equipment.cs`
- `Equipment/EquipmentGenerator.cs`
- `Entities/Player/PlayerInventory.cs`

### Files to Create
- `Data/Items/equipment-abilities.json`
- `Tests/EquipmentAbilityTests.cs`

---

## **Phase 4: Skill Tree System** 🌳
**Priority:** MEDIUM (Complex but impactful)
**Estimated Complexity:** HIGH
**Dependencies:** Phase 2 (for skill tree achievements)

### Goals
- Deep character customization
- Branching progression paths
- Class-specific specializations

### Implementation Plan
1. **Create SkillTree framework**
   - SkillNode class: Name, Description, Effect, Prerequisites, Position (for UI)
   - SkillTree class: Root nodes, branches, current skill points
   - Pattern: Composite pattern (tree structure), Strategy pattern (effects)

2. **Define skill trees in JSON**
   - `Data/SkillTrees/warrior-tree.json`
   - `Data/SkillTrees/mage-tree.json`
   - `Data/SkillTrees/rogue-tree.json`
   - Multiple branches per class (e.g., Warrior: Berserker/Tank/Duelist)

3. **Skill node types**
   - Stat boosts: +10% HP, +5 Attack, etc.
   - New abilities: Unlock powerful signature moves
   - Passive effects: Life steal, thorns, dodge chance
   - Keystone nodes: Major build-defining effects

4. **Progression system**
   - Earn skill points on level up
   - Spend points to unlock nodes
   - Can only unlock if prerequisites met
   - Optional: Respec system (cost gold)

5. **Create SkillTreeManager**
   - Similar to ProgressionManager pattern
   - Validate unlocks, apply effects, handle respec
   - Persist with player save

6. **UI Integration**
   - Visual tree display
   - Show available/locked/unlocked nodes
   - Preview node effects

### Testing
- SkillTreeTests: Node unlocking, prerequisites, validation
- SkillTreeManagerTests: Point allocation, respec, persistence
- Integration tests: Skill effects in combat

### Files to Create
- `Systems/SkillTree/SkillNode.cs`
- `Systems/SkillTree/SkillTree.cs`
- `Systems/SkillTree/SkillTreeManager.cs`
- `Systems/SkillTree/Effects/ISkillEffect.cs`
- `Data/SkillTrees/warrior-tree.json`
- `Data/SkillTrees/mage-tree.json`
- `Data/SkillTrees/rogue-tree.json`
- `Tests/SkillTreeTests.cs`
- `Tests/SkillTreeManagerTests.cs`

---

## **Phase 5A: Content Expansion - Equipment** 🛡️
**Priority:** MEDIUM (Adds variety after systems are solid)
**Estimated Complexity:** LOW (Data-driven)
**Dependencies:** Phase 3 (Equipment abilities)

### Goals
- 3-5x more equipment variety
- More interesting stat combinations
- More prefix/suffix options

### Implementation Plan
1. **Expand item generation data**
   - Add 20+ new weapon prefixes/suffixes
   - Add 20+ new armor prefixes/suffixes
   - New stat combinations: Life steal, thorns, mana regen, cooldown reduction

2. **New equipment slots (optional)**
   - Already have: Weapon, Armor, Helmet, Boots, Gloves, Ring1, Ring2, Amulet, Relic
   - Could add: Belt, Trinket, Off-hand

3. **Set bonuses**
   - Wearing multiple items from same set grants bonus
   - Define sets in `Data/Items/equipment-sets.json`
   - Examples: "Dragon Knight Set" (2pc: +50 HP, 4pc: Fire immunity)

### Testing
- EquipmentGeneratorTests: New prefixes/suffixes, set bonuses
- Integration tests: Set bonuses apply correctly

### Files to Modify
- `Data/Items/weapon-prefixes.json` (expand)
- `Data/Items/weapon-suffixes.json` (expand)
- `Data/Items/armor-prefixes.json` (expand)
- `Data/Items/armor-suffixes.json` (expand)

### Files to Create (if sets implemented)
- `Equipment/EquipmentSet.cs`
- `Data/Items/equipment-sets.json`

---

## **Phase 5B: Content Expansion - Enemies & Bosses** 👹
**Priority:** MEDIUM
**Estimated Complexity:** LOW-MEDIUM
**Dependencies:** None

### Goals
- Triple enemy variety (currently ~10, expand to 30+)
- More boss encounters with unique mechanics
- Themed enemy families

### Implementation Plan
1. **Add new enemy types**
   - Expand `Data/enemies.json`
   - New families: Demons, Undead, Beasts, Elementals, Constructs
   - Varied AI behaviors using existing AI system

2. **Add new bosses**
   - Expand `Data/bosses.json`
   - Boss-specific mechanics:
     - Multi-phase fights (change behavior at 50% HP)
     - Summon minions
     - Area denial mechanics
     - Enrage timers

3. **Boss mechanics system**
   - Create IBossMechanic interface
   - Examples: SummonMinions, Enrage, PhaseTransition, SpecialAttack
   - Pattern: Strategy pattern

### Testing
- EnemyFactoryTests: New enemy creation
- BossMechanicTests: Mechanic execution
- Integration tests: Boss fights with mechanics

### Files to Modify
- `Data/enemies.json` (expand)
- `Data/bosses.json` (expand)
- `Data/enemy-abilities.json` (expand)

### Files to Create (if boss mechanics implemented)
- `Entities/Enemy/BossMechanics/IBossMechanic.cs`
- `Entities/Enemy/BossMechanics/SummonMinions.cs`
- `Entities/Enemy/BossMechanics/Enrage.cs`
- `Tests/BossMechanicTests.cs`

---

## **Phase 5C: Content Expansion - Abilities** 💫
**Priority:** MEDIUM
**Estimated Complexity:** LOW-MEDIUM
**Dependencies:** None

### Goals
- 2-3x more abilities per class
- More interesting ability combinations
- Ultimate abilities (high cost, high impact)

### Implementation Plan
1. **Add new abilities to JSON**
   - Expand `Data/abilities.json`
   - 10+ new abilities per class (currently ~4-5 each)
   - New ability types:
     - Ultimates: High mana, long cooldown, game-changing
     - Combos: Abilities that synergize
     - Transformations: Temporary stance changes

2. **New ability effects**
   - Create new IAbilityEffect implementations
   - Examples:
     - MarkEffect (mark enemy for bonus damage)
     - TransformEffect (change player state)
     - SummonEffect (summon temporary ally)
     - ChainEffect (bounce to multiple targets)

3. **Ability combos**
   - Certain abilities deal bonus damage if used in sequence
   - Example: "Mark Target" → "Execute" deals 2x damage to marked enemies

### Testing
- AbilityTests: New effects, combo mechanics
- Integration tests: Abilities in combat

### Files to Modify
- `Data/abilities.json` (expand)

### Files to Create
- `Abilities/Effects/MarkEffect.cs`
- `Abilities/Effects/TransformEffect.cs`
- `Abilities/Effects/ChainEffect.cs`
- `Tests/NewAbilityEffectTests.cs`

---

## **Phase 6: Advanced Content Systems** 🎨
**Priority:** LOW (Polish after core content)
**Estimated Complexity:** VARIED
**Dependencies:** All previous phases

### Optional Features
1. **Legendary/Unique Items**
   - Hand-crafted unique items with special effects
   - Can't be randomly generated
   - Each has unique lore and ability

2. **Challenge Modes**
   - Dungeon modifiers: "Enemies deal 2x damage", "No healing", "Boss rush"
   - Bonus rewards for completing challenges
   - Leaderboards for challenge completion times

3. **Prestige System**
   - Reset character at max level for permanent bonuses
   - Prestige-only content

4. **Guild/Faction System**
   - Join factions for unique quests and rewards
   - Faction-specific skill trees

---

## Recommended Priority Order

### **Sprint 1** (High Impact, Lower Risk)
1. **Phase 1: Statistics Tracking** - Foundation for everything
2. **Phase 2: Achievement System** - Visible player value

### **Sprint 2** (Core Gameplay Enhancement)
3. **Phase 3: Equipment Abilities** - Makes loot more exciting
4. **Phase 5A: Equipment Content** - More variety to enjoy

### **Sprint 3** (Content Depth)
5. **Phase 5B: Enemies & Bosses** - More challenges
6. **Phase 5C: Abilities** - More build options

### **Sprint 4** (Deep Systems)
7. **Phase 4: Skill Trees** - Long-term progression
8. **Phase 6: Advanced Systems** - Polish and unique features

---

## Design Principles for All Phases

### 1. **Event-Driven Architecture**
- All new systems subscribe to existing GameEvents
- Publish new events for new systems (AchievementUnlockedEvent, SkillNodeUnlockedEvent, etc.)
- Use IGameInterface for all event publishing

### 2. **Dependency Injection**
- All managers accept IGameInterface in constructor
- Use IDataRepository for data loading
- Use ILogger for logging

### 3. **Data-Driven Design**
- All content in JSON files
- Easy to add/modify without code changes
- Validate JSON on load

### 4. **SOLID Principles**
- Single Responsibility: Each manager does one thing
- Open/Closed: Use strategy pattern for extensibility
- Liskov Substitution: Interface-based design
- Interface Segregation: Focused interfaces
- Dependency Inversion: Depend on abstractions

### 5. **Testing**
- Unit tests for all new classes (target: 70%+ coverage)
- Integration tests for end-to-end scenarios
- Use Moq for interface mocking
- Follow existing test patterns (TestBase, AutomatedInterface)

### 6. **Persistence**
- Extend SaveSystem for new data
- Backward compatibility with old saves
- Validate loaded data

---

## Success Metrics

### Phase 1-2 (Statistics & Achievements)
- 20+ achievements defined
- 15+ stats tracked
- 100% save/load success rate

### Phase 3 (Equipment Abilities)
- 10+ equipment abilities
- 5+ legendary items with granted abilities
- Zero ability persistence bugs

### Phase 4 (Skill Trees)
- 3 complete skill trees (1 per class)
- 15+ nodes per tree
- Respec system works correctly

### Phase 5 (Content)
- 30+ new enemies
- 10+ new bosses
- 30+ new abilities
- 50+ new equipment modifiers

### Overall
- All tests passing (target: 500+ tests)
- Code coverage: 70%+
- Zero console output in game logic
- All new features event-driven

---

## Questions for User

Before starting, let's align on:
1. **Which phase to start with?** (Recommendation: Phase 1 - Statistics)
2. **Should we implement everything or pick specific features?**
3. **Any specific achievements/stats you want to track?**
4. **Preferred skill tree depth?** (Linear chains vs branching paths)
5. **Equipment ability philosophy?** (Many weak abilities vs few powerful ones)

---

**Ready to begin when you are!** 🚀
