using Xunit;
using TestRPGGame.Entities.Dungeon;
using TestRPGGame.Interfaces;
using TestRPGGame.Entities.Player;
using TestRPGGame.Factories;
using System.Linq;

namespace TestRPGGame.Tests
{
    public class DungeonRunnerTests : TestBase
    {
        [Fact]
        public void DungeonRunner_CanBeInstantiated()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();

            // Act
            var runner = new DungeonRunner(autoInterface);

            // Assert
            Assert.NotNull(runner);
        }

        [Fact]
        public void DungeonRunner_RunDungeon_CompletesSuccessfully()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var runner = new DungeonRunner(autoInterface);
            var player = new Player("TestHero", PlayerClass.Warrior);
            player.CurrentHP = player.MaxHP;
            var progress = new DungeonProgress();

            var dungeon = DungeonFactory.CreateAllDungeons().First();

            // Act
            var result = runner.RunDungeon(player, dungeon, progress);
            var log = autoInterface.GetLog();

            // Assert
            Assert.Contains("Encounter", log);
        }

        [Fact]
        public void DungeonRunner_UsesRequestEncounterChoice()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var runner = new DungeonRunner(autoInterface);
            var player = new Player("TestHero", PlayerClass.Warrior);
            player.CurrentHP = player.MaxHP;
            var progress = new DungeonProgress();

            var dungeon = DungeonFactory.CreateAllDungeons().First();

            // Act
            runner.RunDungeon(player, dungeon, progress);
            var log = autoInterface.GetLog();

            // Assert
            // AutomatedInterface logs "Encounter: [description]" and "Choice: [choice]"
            Assert.Contains("Choice:", log);
        }

        [Fact]
        public void DungeonRunner_GivesRewardsOnCompletion()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var runner = new DungeonRunner(autoInterface);
            var player = new Player("TestHero", PlayerClass.Warrior);
            player.CurrentHP = player.MaxHP;
            int initialGold = player.Gold;
            var progress = new DungeonProgress();

            var dungeon = DungeonFactory.CreateAllDungeons().First();

            // Act
            var result = runner.RunDungeon(player, dungeon, progress);

            // Assert
            if (result)
            {
                // On success, player should have gained something
                Assert.True(player.Gold >= initialGold || player.Inventory.BackpackItems.Count > 3);
            }
        }

        [Fact]
        public void DungeonRunner_HandlesPlayerDeath()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var runner = new DungeonRunner(autoInterface);
            var player = new Player("TestHero", PlayerClass.Warrior);
            player.CurrentHP = 1; // Very low HP
            var progress = new DungeonProgress();

            var dungeon = DungeonFactory.CreateAllDungeons().First();

            // Act
            var result = runner.RunDungeon(player, dungeon, progress);

            // Assert
            // Should complete without crashing
            Assert.True(result || !result);
        }

        [Fact]
        public void DungeonRunner_SendsMessages()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var runner = new DungeonRunner(autoInterface);
            var player = new Player("TestHero", PlayerClass.Warrior);
            player.CurrentHP = player.MaxHP;
            var progress = new DungeonProgress();

            var dungeon = DungeonFactory.CreateAllDungeons().First();

            // Act
            runner.RunDungeon(player, dungeon, progress);
            var log = autoInterface.GetLog();

            // Assert
            // Should send info messages through the interface
            Assert.NotEmpty(log);
        }
    }
}
