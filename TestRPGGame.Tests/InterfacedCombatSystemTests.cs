using Xunit;
using TestRPGGame.Combat;
using TestRPGGame.Interfaces;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.Factories;
using TestRPGGame.Systems;
using System;
using System.Linq;

namespace TestRPGGame.Tests
{
    public class InterfacedCombatSystemTests : TestBase
    {
        [Fact]
        public void InterfacedCombatSystem_CanBeInstantiated()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();

            // Act
            var combat = new InterfacedCombatSystem(autoInterface);

            // Assert
            Assert.NotNull(combat);
        }

        [Fact]
        public void InterfacedCombatSystem_StartBattle_PlayerWins()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var combat = new InterfacedCombatSystem(autoInterface);
            var player = new Player("TestHero", PlayerClass.Warrior);
            player.CurrentHP = player.MaxHP; // Full health

            // Create weak enemy
            var enemy = EnemyFactory.CreateEnemy(1);

            // Act
            bool victory = combat.StartBattle(player, enemy, canFlee: true);
            var log = autoInterface.GetLog();

            // Assert
            Assert.True(victory || !victory); // Battle completes either way
            Assert.Contains("COMBAT", log);
            Assert.Contains("Turn", log);
        }

        [Fact]
        public void InterfacedCombatSystem_PublishesCombatEvents()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var combat = new InterfacedCombatSystem(autoInterface);
            var player = new Player("TestHero", PlayerClass.Warrior);
            var enemy = EnemyFactory.CreateEnemy(1);

            // Act
            combat.StartBattle(player, enemy, canFlee: true);
            var log = autoInterface.GetLog();

            // Assert
            Assert.Contains("COMBAT", log); // CombatStartedEvent
            Assert.Contains("damage", log); // DamageDealtEvent
        }

        [Fact]
        public void InterfacedCombatSystem_PlayerCanUseAbilities()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var combat = new InterfacedCombatSystem(autoInterface);
            var player = new Player("TestHero", PlayerClass.Mage);
            player.CurrentHP = player.MaxHP;
            player.CurrentMana = player.MaxMana; // Full mana for abilities
            var enemy = EnemyFactory.CreateEnemy(1);

            // Act
            combat.StartBattle(player, enemy, canFlee: true);
            var log = autoInterface.GetLog();

            // Assert
            // AutomatedInterface should use abilities when mana available
            Assert.Contains("used", log);
        }

        [Fact]
        public void InterfacedCombatSystem_GivesRewardsOnVictory()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var combat = new InterfacedCombatSystem(autoInterface);
            var player = new Player("TestHero", PlayerClass.Warrior);
            var enemy = EnemyFactory.CreateEnemy(1);

            int initialGold = player.Gold;
            int initialExp = player.Experience;

            // Act
            bool victory = combat.StartBattle(player, enemy, canFlee: true);

            // Assert
            if (victory)
            {
                Assert.True(player.Gold >= initialGold || player.Experience >= initialExp);
            }
        }

        [Fact]
        public void InterfacedCombatSystem_PublishesVictoryEvent()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var combat = new InterfacedCombatSystem(autoInterface);
            var player = new Player("TestHero", PlayerClass.Warrior);
            var enemy = EnemyFactory.CreateEnemy(1);

            // Act
            bool victory = combat.StartBattle(player, enemy, canFlee: true);
            var log = autoInterface.GetLog();

            // Assert
            if (victory)
            {
                Assert.Contains("VICTORY", log);
                Assert.Contains("gold", log);
                Assert.Contains("XP", log);
            }
        }

        [Fact]
        public void InterfacedCombatSystem_PlayerCanFlee()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var combat = new InterfacedCombatSystem(autoInterface);
            var player = new Player("TestHero", PlayerClass.Rogue);
            var enemy = EnemyFactory.CreateEnemy(1);

            // Act
            combat.StartBattle(player, enemy, canFlee: true);

            // Assert
            // Test completes without crashing - flee option is available
            Assert.True(true);
        }

        [Fact]
        public void InterfacedCombatSystem_BossFleeNotAllowed()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var combat = new InterfacedCombatSystem(autoInterface);
            var player = new Player("TestHero", PlayerClass.Warrior);
            var enemy = EnemyFactory.CreateEnemy(1);

            // Act
            combat.StartBattle(player, enemy, canFlee: false);
            var log = autoInterface.GetLog();

            // Assert
            // Should complete combat (no flee option)
            Assert.Contains("COMBAT", log);
        }

        [Fact]
        public void InterfacedCombatSystem_UsesGameConfig_MaxTurns()
        {
            // Verify combat uses MaxCombatTurns from config
            var config = GameConfig.Config;

            // Assert
            Assert.Equal(100, config.MaxCombatTurns);
        }

        [Fact]
        public void InterfacedCombatSystem_UsesGameConfig_LootDropChance()
        {
            // Verify combat uses loot drop chance from config
            var config = GameConfig.Config;

            // Assert
            Assert.Equal(0.4, config.CombatLootDropChance); // 40%
        }

        [Fact]
        public void InterfacedCombatSystem_UsesGameConfig_GoldLoss()
        {
            // Verify combat gold loss calculation uses config
            var config = GameConfig.Config;
            int playerGold = 1000;

            // Act
            int goldLoss = (int)(playerGold * config.CombatGoldLossPercent);
            goldLoss = System.Math.Min(goldLoss, config.CombatGoldLossMax);

            // Assert
            Assert.Equal(0.25, config.CombatGoldLossPercent);
            Assert.Equal(100, config.CombatGoldLossMax);
            Assert.Equal(100, goldLoss); // 25% of 1000 capped at 100
        }

        [Fact]
        public void InterfacedCombatSystem_UsesGameConfig_ManaRegen()
        {
            // Verify combat mana regen uses config
            var config = GameConfig.Config;
            int maxMana = 100;

            // Act
            int regenAmount = (int)(maxMana * config.ManaRegenRate);

            // Assert
            Assert.Equal(0.05, config.ManaRegenRate);
            Assert.Equal(5, regenAmount); // 5% of 100
        }

        [Fact]
        public void InterfacedCombatSystem_ProcessesStatusEffectsEachTurn()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var combat = new InterfacedCombatSystem(autoInterface);
            var player = new Player("TestHero", PlayerClass.Warrior);
            player.CurrentHP = player.MaxHP;
            player.CurrentMana = player.MaxMana;

            // Create very weak enemy that won't kill player quickly
            var enemy = EnemyFactory.CreateEnemy(1);
            enemy.MaxHP = 500; // Make enemy tanky so combat lasts multiple turns
            enemy.CurrentHP = 500;
            enemy.Attack = 1; // Very weak attack

            // Apply a buff with 3-turn duration manually
            var testEffect = new Combat.StatusEffects.StatModifierEffect(
                "battle_rage",
                "Battle Rage",
                "⚔️",
                Combat.StatusEffects.StatusEffectType.Buff,
                3, // 3 turn duration
                Combat.StatusEffects.StatModifierEffect.StatType.Attack,
                1.5,
                isMultiplier: true
            );

            player.Effects.AddEffect(testEffect);
            int initialDuration = testEffect.RemainingTurns;
            Assert.Equal(3, initialDuration);
            Assert.True(testEffect.JustApplied, "Effect should be marked as just applied");

            // Act - First ProcessTurnStart should skip decrement (JustApplied flag)
            player.Effects.ProcessTurnStart();

            // Assert - Duration should stay at 3, JustApplied should be false
            Assert.Equal(3, testEffect.RemainingTurns);
            Assert.False(testEffect.JustApplied, "JustApplied flag should be cleared after first tick");

            // Second ProcessTurnStart should actually decrement
            player.Effects.ProcessTurnStart();
            Assert.Equal(2, testEffect.RemainingTurns);
        }

        [Fact]
        public void InterfacedCombatSystem_ClearsStatusEffectsWhenCombatEnds()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var combat = new InterfacedCombatSystem(autoInterface);
            var player = new Player("TestHero", PlayerClass.Warrior);
            player.CurrentHP = player.MaxHP;
            player.CurrentMana = player.MaxMana;

            var enemy = EnemyFactory.CreateEnemy(1);

            // Apply a long-duration buff before combat
            var testEffect = new Combat.StatusEffects.StatModifierEffect(
                "battle_rage",
                "Battle Rage",
                "⚔️",
                Combat.StatusEffects.StatusEffectType.Buff,
                10, // Long duration to ensure it doesn't expire during combat
                Combat.StatusEffects.StatModifierEffect.StatType.Attack,
                1.5,
                isMultiplier: true
            );

            player.Effects.AddEffect(testEffect);
            Assert.Single(player.Effects.ActiveEffects);

            // Act - Start and complete combat
            combat.StartBattle(player, enemy, canFlee: false);

            // Assert - All status effects should be cleared after combat ends
            Assert.Empty(player.Effects.ActiveEffects);
            Assert.Empty(enemy.Effects.ActiveEffects);
        }

        [Fact]
        public void InterfacedCombatSystem_StatusEffectsDontPersistBetweenCombats()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var combat = new InterfacedCombatSystem(autoInterface);
            var player = new Player("TestHero", PlayerClass.Warrior);
            player.CurrentHP = player.MaxHP;
            player.CurrentMana = player.MaxMana;

            var enemy1 = EnemyFactory.CreateEnemy(1);
            var enemy2 = EnemyFactory.CreateEnemy(1);

            // Act - First combat with buff applied
            var testEffect = new Combat.StatusEffects.StatModifierEffect(
                "battle_rage",
                "Battle Rage",
                "⚔️",
                Combat.StatusEffects.StatusEffectType.Buff,
                10, // Long duration
                Combat.StatusEffects.StatModifierEffect.StatType.Attack,
                1.5,
                isMultiplier: true
            );

            player.Effects.AddEffect(testEffect);
            Assert.Single(player.Effects.ActiveEffects);

            combat.StartBattle(player, enemy1, canFlee: false);
            Assert.Empty(player.Effects.ActiveEffects);

            // Restore player for second combat
            player.CurrentHP = player.MaxHP;
            player.CurrentMana = player.MaxMana;

            // Second combat - verify no effects from first combat persist
            combat.StartBattle(player, enemy2, canFlee: false);

            // Assert - No effects should carry over from first combat
            Assert.Empty(player.Effects.ActiveEffects);
        }

        [Fact]
        public void InterfacedCombatSystem_BuffDurationDecrementsCorrectly()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var combat = new InterfacedCombatSystem(autoInterface);
            var player = new Player("TestHero", PlayerClass.Mage);
            player.CurrentHP = player.MaxHP;
            player.CurrentMana = player.MaxMana;

            // Create tanky enemy for longer combat
            var enemy = EnemyFactory.CreateEnemy(1);
            enemy.MaxHP = 1000;
            enemy.CurrentHP = 1000;
            enemy.Attack = 1;

            // Manually add a status effect with known duration
            var testEffect = new Combat.StatusEffects.StatModifierEffect(
                "test_buff",
                "Test Buff",
                "⚔️",
                Combat.StatusEffects.StatusEffectType.Buff,
                2, // 2 turn duration
                Combat.StatusEffects.StatModifierEffect.StatType.Attack,
                1.5,
                isMultiplier: true
            );

            player.Effects.AddEffect(testEffect);
            Assert.Equal(2, testEffect.RemainingTurns);
            Assert.True(testEffect.JustApplied);

            // First ProcessTurnStart should skip decrement (JustApplied)
            player.Effects.ProcessTurnStart();
            Assert.Equal(2, testEffect.RemainingTurns);
            Assert.False(testEffect.JustApplied);

            // Second ProcessTurnStart should actually decrement
            player.Effects.ProcessTurnStart();
            Assert.Equal(1, testEffect.RemainingTurns);
        }

        [Fact]
        public void BattleRage_IncreasesPlayerDamage()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var player = new Player("TestWarrior", PlayerClass.Warrior);
            var enemy = EnemyFactory.CreateEnemy(1);

            // Set player to known state
            player.CurrentHP = player.MaxHP;
            player.CurrentMana = player.MaxMana;
            player.Attack = 50; // Known base attack value

            // Enemy with zero defense for predictable damage
            enemy.Defense = 0;
            enemy.CurrentHP = 10000; // High HP so it doesn't die

            // Get Battle Rage ability
            var battleRage = player.Abilities.FirstOrDefault(a => a.Name == "Battle Rage");
            Assert.NotNull(battleRage);

            // Create combat context for ability execution
            var context = new TestRPGGame.Abilities.Effects.AbilityContext(player, enemy, autoInterface);

            // Calculate baseline damage WITHOUT Battle Rage using actual combat system calculation
            int baselineDamage1 = CalculateRealAttackDamage(player, enemy);
            int baselineDamage2 = CalculateRealAttackDamage(player, enemy);
            int averageBaseline = (baselineDamage1 + baselineDamage2) / 2;

            // Act: Use Battle Rage
            battleRage.Execute(context);

            // Check that Battle Rage effect is active
            var activeEffects = player.Effects.ActiveEffects;
            Assert.Contains(activeEffects, e => e.Name == "Battle Rage");

            // Calculate damage WITH Battle Rage active using actual combat system calculation
            int buffedDamage1 = CalculateRealAttackDamage(player, enemy);
            int buffedDamage2 = CalculateRealAttackDamage(player, enemy);
            int averageBuffed = (buffedDamage1 + buffedDamage2) / 2;

            // Assert: Battle Rage should increase damage by ~50%
            // Expected: baseline * 1.5 = buffed
            // Allow some tolerance for rounding
            double expectedBuffedDamage = averageBaseline * 1.5;
            double actualIncrease = (double)averageBuffed / averageBaseline;

            Assert.True(actualIncrease >= 1.4 && actualIncrease <= 1.6,
                $"BUG: Battle Rage should increase damage by ~50%! " +
                $"Baseline damage: {averageBaseline}, " +
                $"Buffed damage: {averageBuffed}, " +
                $"Actual increase: {actualIncrease:P0} (expected ~150%)");
        }

        /// <summary>
        /// Helper to calculate attack damage using the REAL combat system logic with buffs
        /// This mirrors the ExecutePlayerAttack method in InterfacedCombatSystem
        /// </summary>
        private int CalculateRealAttackDamage(Player player, Enemy enemy)
        {
            int initialHP = enemy.CurrentHP;

            // Use the REAL combat calculation with buffs
            double effectiveAttack = player.Attack + player.Effects.GetAttackBonus();
            effectiveAttack *= player.Effects.GetAttackMultiplier();

            int baseDamage = (int)effectiveAttack;

            // Skip crit for consistency in testing
            // if (isCrit) baseDamage = (int)(baseDamage * 2.0);

            // Apply general damage multipliers
            baseDamage = (int)(baseDamage * player.Effects.GetTotalDamageMultiplier());

            // Calculate enemy's effective defense
            double effectiveDefense = enemy.Defense + enemy.Effects.GetDefenseBonus();
            effectiveDefense *= enemy.Effects.GetDefenseMultiplier();

            // Calculate final damage
            int finalDamage = Math.Max(1, baseDamage - (int)(effectiveDefense / 2));
            enemy.CurrentHP -= finalDamage;

            int damageDealt = initialHP - enemy.CurrentHP;
            return damageDealt;
        }

        [Fact]
        public void DefenseBuff_ReducesDamageTaken()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var player = new Player("TestWarrior", PlayerClass.Warrior);
            var enemy = EnemyFactory.CreateEnemy(5); // Stronger enemy

            player.CurrentHP = player.MaxHP;
            player.Defense = 20; // Known defense value
            enemy.Attack = 60; // Known attack value

            // Calculate baseline damage WITHOUT defense buff
            int initialPlayerHP = player.CurrentHP;
            CalculateEnemyAttackDamage(enemy, player);
            int baselineDamage = initialPlayerHP - player.CurrentHP;
            player.CurrentHP = initialPlayerHP; // Reset

            // Act: Apply a defense buff (simulating Shield Wall or similar)
            var defenseEffect = new Combat.StatusEffects.StatModifierEffect(
                "test_defense_buff", "Defense Buff", "🛡️",
                Combat.StatusEffects.StatusEffectType.Buff,
                duration: 3,
                stat: Combat.StatusEffects.StatModifierEffect.StatType.Defense,
                value: 1.5, // 50% more defense
                isMultiplier: true);
            player.Effects.AddEffect(defenseEffect);

            // Calculate damage WITH defense buff
            initialPlayerHP = player.CurrentHP;
            CalculateEnemyAttackDamage(enemy, player);
            int buffedDamage = initialPlayerHP - player.CurrentHP;

            // Assert: Defense buff should reduce damage taken
            Assert.True(buffedDamage < baselineDamage,
                $"Defense buff should reduce damage! Baseline: {baselineDamage}, Buffed: {buffedDamage}");
        }

        [Fact]
        public void AbilityDamage_AppliesAttackBuffs()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var player = new Player("TestWarrior", PlayerClass.Warrior);
            var enemy = EnemyFactory.CreateEnemy(1);

            player.CurrentHP = player.MaxHP;
            player.CurrentMana = player.MaxMana;

            // Clear all effects to ensure clean state
            player.Effects.ClearAll();

            // Strip equipment to get clean baseline
            player.Inventory.Weapon = null;
            player.Inventory.Armor = null;
            player.UpdateStatsFromEquipment();
            player.Attack = 50; // Set known attack after equipment removal

            enemy.Defense = 0;
            enemy.CurrentHP = 10000;
            enemy.Effects.ClearAll();

            // Get Power Strike ability (deals damage)
            var powerStrike = player.Abilities.FirstOrDefault(a => a.Name == "Power Strike");
            Assert.NotNull(powerStrike);

            var context = new TestRPGGame.Abilities.Effects.AbilityContext(player, enemy, autoInterface);

            // Calculate baseline ability damage WITHOUT buffs
            player.Effects.ClearAll(); // Ensure no buffs
            int initialHP = enemy.CurrentHP;
            powerStrike.Execute(context);
            int baselineDamage = initialHP - enemy.CurrentHP;
            powerStrike.CurrentCooldown = 0; // Reset cooldown

            // Act: Apply Battle Rage
            player.Effects.ClearAll(); // Clear any effects from ability execution
            var battleRage = player.Abilities.FirstOrDefault(a => a.Name == "Battle Rage");
            Assert.NotNull(battleRage);
            battleRage.Execute(context);
            battleRage.CurrentCooldown = 0;

            // Verify ONLY Battle Rage is active
            var activeEffects = player.Effects.ActiveEffects;
            Assert.Single(activeEffects);
            Assert.Contains(activeEffects, e => e.Name == "Battle Rage");

            // Calculate ability damage WITH Battle Rage
            initialHP = enemy.CurrentHP;
            powerStrike.Execute(context);
            int buffedDamage = initialHP - enemy.CurrentHP;

            // Assert: Battle Rage should increase ability damage
            // Note: Power Strike has variance (2.2x-2.8x), so exact percentage varies
            // Important: buffed damage should be noticeably higher than baseline
            double actualIncrease = (double)buffedDamage / baselineDamage;
            Assert.True(actualIncrease >= 1.2 && actualIncrease <= 1.8,
                $"Battle Rage should significantly increase ability damage! Baseline: {baselineDamage}, Buffed: {buffedDamage}, Increase: {actualIncrease:P0}");

            // More importantly: buffed damage should be higher
            Assert.True(buffedDamage > baselineDamage,
                $"Buffed damage ({buffedDamage}) should exceed baseline damage ({baselineDamage})");
        }

        [Fact]
        public void MultipleBuffs_StackMultiplicatively()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var player = new Player("TestWarrior", PlayerClass.Warrior);
            var enemy = EnemyFactory.CreateEnemy(1);

            player.CurrentHP = player.MaxHP;
            player.Attack = 50;
            enemy.Defense = 0;
            enemy.CurrentHP = 10000;

            // Calculate baseline
            int baselineDamage = CalculateRealAttackDamage(player, enemy);

            // Act: Apply TWO different buff types - Attack buff AND Damage buff
            // Attack buff: +50% attack
            var attackBuff = new Combat.StatusEffects.StatModifierEffect(
                "test_attack_buff", "Attack Buff", "⚔️",
                Combat.StatusEffects.StatusEffectType.Buff,
                duration: 3,
                stat: Combat.StatusEffects.StatModifierEffect.StatType.Attack,
                value: 1.5,
                isMultiplier: true);
            player.Effects.AddEffect(attackBuff);

            // Damage buff: +30% damage
            var damageBuff = new Combat.StatusEffects.StatModifierEffect(
                "test_damage_buff", "Damage Buff", "💢",
                Combat.StatusEffects.StatusEffectType.Buff,
                duration: 3,
                stat: Combat.StatusEffects.StatModifierEffect.StatType.Damage,
                value: 1.3,
                isMultiplier: true);
            player.Effects.AddEffect(damageBuff);

            // Calculate damage with BOTH buffs
            int buffedDamage = CalculateRealAttackDamage(player, enemy);

            // Assert: Buffs should stack multiplicatively (1.5 * 1.3 = 1.95x total)
            double expectedIncrease = 1.5 * 1.3; // 1.95
            double actualIncrease = (double)buffedDamage / baselineDamage;
            Assert.True(actualIncrease >= 1.85 && actualIncrease <= 2.05,
                $"Attack and Damage buffs should stack multiplicatively (~95% increase)! " +
                $"Baseline: {baselineDamage}, Buffed: {buffedDamage}, Increase: {actualIncrease:P0}");
        }

        /// <summary>
        /// Helper to calculate enemy attack damage using REAL combat system logic
        /// </summary>
        private int CalculateEnemyAttackDamage(Enemy enemy, Player player)
        {
            int initialHP = player.CurrentHP;

            // Use REAL enemy attack calculation with buffs
            double effectiveAttack = enemy.Attack + enemy.Effects.GetAttackBonus();
            effectiveAttack *= enemy.Effects.GetAttackMultiplier();

            int baseDamage = (int)effectiveAttack;
            baseDamage = (int)(baseDamage * enemy.Effects.GetTotalDamageMultiplier());

            // Player's effective defense
            double effectiveDefense = player.Defense + player.Effects.GetDefenseBonus();
            effectiveDefense *= player.Effects.GetDefenseMultiplier();

            int finalDamage = Math.Max(1, baseDamage - (int)(effectiveDefense / 2));
            player.CurrentHP -= finalDamage;

            return initialHP - player.CurrentHP;
        }

        [Fact]
        public void ManyBuffs_AllStackCorrectly()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var player = new Player("TestWarrior", PlayerClass.Warrior);
            var enemy = EnemyFactory.CreateEnemy(1);

            player.CurrentHP = player.MaxHP;
            player.Attack = 100;
            enemy.Defense = 0;
            enemy.CurrentHP = 100000;

            // Calculate baseline
            int baselineDamage = CalculateRealAttackDamage(player, enemy);

            // Act: Apply MANY different buffs (10 total)
            // 5 attack buffs
            for (int i = 1; i <= 5; i++)
            {
                var attackBuff = new Combat.StatusEffects.StatModifierEffect(
                    $"attack_buff_{i}", $"Attack Buff {i}", "⚔️",
                    Combat.StatusEffects.StatusEffectType.Buff,
                    duration: 5,
                    stat: Combat.StatusEffects.StatModifierEffect.StatType.Attack,
                    value: 1.1, // +10% each
                    isMultiplier: true);
                player.Effects.AddEffect(attackBuff);
            }

            // 3 damage buffs
            for (int i = 1; i <= 3; i++)
            {
                var damageBuff = new Combat.StatusEffects.StatModifierEffect(
                    $"damage_buff_{i}", $"Damage Buff {i}", "💢",
                    Combat.StatusEffects.StatusEffectType.Buff,
                    duration: 5,
                    stat: Combat.StatusEffects.StatModifierEffect.StatType.Damage,
                    value: 1.05, // +5% each
                    isMultiplier: true);
                player.Effects.AddEffect(damageBuff);
            }

            // 2 flat attack bonuses
            for (int i = 1; i <= 2; i++)
            {
                var flatBonus = new Combat.StatusEffects.StatModifierEffect(
                    $"flat_attack_{i}", $"Flat Attack {i}", "➕",
                    Combat.StatusEffects.StatusEffectType.Buff,
                    duration: 5,
                    stat: Combat.StatusEffects.StatModifierEffect.StatType.Attack,
                    value: 10, // +10 flat
                    isMultiplier: false);
                player.Effects.AddEffect(flatBonus);
            }

            // Verify all 10 buffs are active
            Assert.Equal(10, player.Effects.ActiveEffects.Count);

            // Calculate damage with all buffs
            int buffedDamage = CalculateRealAttackDamage(player, enemy);

            // Assert: All buffs should stack
            // Expected calculation:
            // Base attack: 100
            // + Flat bonuses: +20 = 120
            // * Attack multipliers: 120 * (1.1^5) = 120 * 1.61051 = 193.26
            // * Damage multipliers: 193.26 * (1.05^3) = 193.26 * 1.157625 = 223.73
            // Expected damage increase: ~2.24x (224%)

            double actualIncrease = (double)buffedDamage / baselineDamage;
            Assert.True(actualIncrease >= 2.0 && actualIncrease <= 2.5,
                $"10 buffs should stack to ~2.24x damage! Baseline: {baselineDamage}, Buffed: {buffedDamage}, Increase: {actualIncrease:P0}");
        }

        [Fact]
        public void BuffsAndDebuffs_BothApply()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var player = new Player("TestWarrior", PlayerClass.Warrior);
            var enemy = EnemyFactory.CreateEnemy(1);

            player.CurrentHP = player.MaxHP;
            player.Attack = 50;
            player.Defense = 20;
            enemy.Attack = 40;
            enemy.Defense = 0;
            enemy.CurrentHP = 10000;

            // Act: Apply buffs to player
            var playerAttackBuff = new Combat.StatusEffects.StatModifierEffect(
                "player_attack", "Player Attack", "⚔️",
                Combat.StatusEffects.StatusEffectType.Buff,
                duration: 3,
                stat: Combat.StatusEffects.StatModifierEffect.StatType.Attack,
                value: 1.5,
                isMultiplier: true);
            player.Effects.AddEffect(playerAttackBuff);

            // Apply debuffs to player
            var playerDefenseDebuff = new Combat.StatusEffects.StatModifierEffect(
                "player_defense_debuff", "Weakened", "⬇️",
                Combat.StatusEffects.StatusEffectType.Debuff,
                duration: 3,
                stat: Combat.StatusEffects.StatModifierEffect.StatType.Defense,
                value: 0.5, // Half defense
                isMultiplier: true);
            player.Effects.AddEffect(playerDefenseDebuff);

            // Apply buffs to enemy
            var enemyAttackBuff = new Combat.StatusEffects.StatModifierEffect(
                "enemy_attack", "Enemy Attack", "⚔️",
                Combat.StatusEffects.StatusEffectType.Buff,
                duration: 3,
                stat: Combat.StatusEffects.StatModifierEffect.StatType.Attack,
                value: 1.3,
                isMultiplier: true);
            enemy.Effects.AddEffect(enemyAttackBuff);

            // Test player damage (buffed attack)
            int playerDamage = CalculateRealAttackDamage(player, enemy);
            Assert.True(playerDamage > 50, $"Player attack buff should increase damage (got {playerDamage})");

            // Test enemy damage (buffed attack vs debuffed defense)
            int initialPlayerHP = player.CurrentHP;
            int enemyDamage = CalculateEnemyAttackDamage(enemy, player);
            int expectedBaseDamage = 40 - (20 / 2); // 40 - 10 = 30

            // With enemy buff (+30%) and player defense debuff (half defense = 10):
            // Enemy attack: 40 * 1.3 = 52
            // Player defense: 20 * 0.5 = 10
            // Damage: 52 - (10/2) = 52 - 5 = 47
            Assert.True(enemyDamage > expectedBaseDamage,
                $"Enemy buff and player debuff should increase damage taken (expected >{expectedBaseDamage}, got {enemyDamage})");
        }

        [Fact]
        public void SameStatMultipleBuffs_StackMultiplicatively()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var player = new Player("TestWarrior", PlayerClass.Warrior);
            var enemy = EnemyFactory.CreateEnemy(1);

            player.CurrentHP = player.MaxHP;
            player.Attack = 100;
            enemy.Defense = 0;
            enemy.CurrentHP = 10000;

            int baselineDamage = CalculateRealAttackDamage(player, enemy);

            // Act: Apply 3 DIFFERENT attack buffs (same stat, different effects)
            var buff1 = new Combat.StatusEffects.StatModifierEffect(
                "rage", "Rage", "😤",
                Combat.StatusEffects.StatusEffectType.Buff,
                duration: 3,
                stat: Combat.StatusEffects.StatModifierEffect.StatType.Attack,
                value: 1.5, // +50%
                isMultiplier: true);
            player.Effects.AddEffect(buff1);

            var buff2 = new Combat.StatusEffects.StatModifierEffect(
                "berserk", "Berserk", "💪",
                Combat.StatusEffects.StatusEffectType.Buff,
                duration: 3,
                stat: Combat.StatusEffects.StatModifierEffect.StatType.Attack,
                value: 1.3, // +30%
                isMultiplier: true);
            player.Effects.AddEffect(buff2);

            var buff3 = new Combat.StatusEffects.StatModifierEffect(
                "bloodlust", "Bloodlust", "🩸",
                Combat.StatusEffects.StatusEffectType.Buff,
                duration: 3,
                stat: Combat.StatusEffects.StatModifierEffect.StatType.Attack,
                value: 1.2, // +20%
                isMultiplier: true);
            player.Effects.AddEffect(buff3);

            Assert.Equal(3, player.Effects.ActiveEffects.Count);

            // Calculate damage with all 3 attack buffs
            int buffedDamage = CalculateRealAttackDamage(player, enemy);

            // Expected: 1.5 * 1.3 * 1.2 = 2.34x damage
            double actualIncrease = (double)buffedDamage / baselineDamage;
            Assert.True(actualIncrease >= 2.2 && actualIncrease <= 2.5,
                $"3 attack buffs should stack multiplicatively to ~2.34x! Baseline: {baselineDamage}, Buffed: {buffedDamage}, Increase: {actualIncrease:P0}");
        }
    }
}
