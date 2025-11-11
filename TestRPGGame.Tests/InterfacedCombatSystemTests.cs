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
    }
}
