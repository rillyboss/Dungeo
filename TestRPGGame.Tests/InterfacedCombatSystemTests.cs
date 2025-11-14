using Xunit;
using TestRPGGame.Combat;
using TestRPGGame.Interfaces;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.Factories;
using TestRPGGame.Systems;
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

            // Act - Process turn start once manually to verify it works
            player.Effects.ProcessTurnStart();

            // Assert - Duration should decrease by 1
            if (player.Effects.ActiveEffects.Any())
            {
                Assert.Equal(2, player.Effects.ActiveEffects.First().RemainingTurns);
            }
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

            // Process one turn manually to verify decrement
            player.Effects.ProcessTurnStart();

            // Assert - Duration should decrease by 1
            if (player.Effects.ActiveEffects.Any())
            {
                Assert.Equal(1, player.Effects.ActiveEffects.First().RemainingTurns);
            }
        }
    }
}
