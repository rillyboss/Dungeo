using Xunit;
using TestRPGGame;
using TestRPGGame.Interfaces;
using TestRPGGame.Entities.Player;
using TestRPGGame.Systems;
using System.IO;
using System.Linq;

namespace TestRPGGame.Tests
{
    [Collection("SaveSystem")]
    public class GameCoreTests : TestBase
    {
        private readonly string saveDirectory;

        public GameCoreTests()
        {
            // SaveSystem uses AppData/TestRPGGame/Saves by default
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            saveDirectory = Path.Combine(appData, "TestRPGGame", "Saves");

            // Clean up any existing save files before test
            CleanupSaveFiles();
        }

        private void CleanupSaveFiles()
        {
            if (Directory.Exists(saveDirectory))
            {
                foreach (var file in Directory.GetFiles(saveDirectory, "save_slot_*.json"))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // Ignore errors during cleanup
                    }
                }
            }
        }

        [Fact]
        public void GameCore_CanBeInstantiated()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();

            // Act
            var gameCore = new GameCore(autoInterface);

            // Assert
            Assert.NotNull(gameCore);
        }

        [Fact]
        public void GameCore_Start_CreatesNewCharacter()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var gameCore = new GameCore(autoInterface);

            // Act
            gameCore.Start();
            var log = autoInterface.GetLog();

            // Assert
            Assert.Contains("Character Created", log);
            Assert.Contains("AIHero", log);
            Assert.Contains("Warrior", log);
        }

        [Fact]
        public void GameCore_Start_RunsCombat()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var gameCore = new GameCore(autoInterface);

            // Act
            gameCore.Start();
            var log = autoInterface.GetLog();

            // Assert
            // Should run 3 combats as per DefaultStrategy
            Assert.Contains("COMBAT 1:", log);
            Assert.Contains("COMBAT 2:", log);
            Assert.Contains("COMBAT 3:", log);
        }

        [Fact]
        public void GameCore_Start_AutoSavesAfterCombat()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var gameCore = new GameCore(autoInterface);

            // Act
            gameCore.Start();
            var log = autoInterface.GetLog();

            // Assert
            Assert.Contains("Game saved to slot", log);
        }

        [Fact]
        public void GameCore_Start_PlayerGainsExperience()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var gameCore = new GameCore(autoInterface);

            // Act
            gameCore.Start();
            var log = autoInterface.GetLog();

            // Assert
            // Check for XP gain (logs say "Gained: X gold, Y XP")
            Assert.Contains("XP", log);
            Assert.Contains("gold", log);
        }

        [Fact]
        public void GameCore_Start_CombatUsesEventDrivenPattern()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var gameCore = new GameCore(autoInterface);

            // Act
            gameCore.Start();
            var log = autoInterface.GetLog();

            // Assert
            // Verify event-driven combat is working
            Assert.Contains("COMBAT", log);
            Assert.Contains("Turn", log);
            Assert.Contains("damage", log);
        }

        [Fact]
        public void GameCore_LoadsExistingCharacter()
        {
            // Arrange - Fill all save slots so AutomatedInterface is forced to load
            var player1 = new Player("ExistingHero", PlayerClass.Mage);
            player1.Level = 5;
            player1.Gold = 1000;
            var progress = new Entities.Dungeon.DungeonProgress();
            SaveSystem.SaveGame(player1, 1, progress);

            // Fill slots 2 and 3 so no empty slots exist
            var player2 = new Player("Hero2", PlayerClass.Warrior);
            SaveSystem.SaveGame(player2, 2, progress);
            var player3 = new Player("Hero3", PlayerClass.Rogue);
            SaveSystem.SaveGame(player3, 3, progress);

            // Create interface that will load existing save
            var autoInterface = new AutomatedInterface();
            var gameCore = new GameCore(autoInterface);

            // Act
            gameCore.Start();
            var log = autoInterface.GetLog();

            // Assert
            Assert.Contains("Load existing save from slot 1", log);
            Assert.Contains("Game loaded successfully", log);
        }

        [Fact]
        public void GameCore_PlayerCanDefeatEnemy()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var gameCore = new GameCore(autoInterface);

            // Act
            gameCore.Start();
            var log = autoInterface.GetLog();

            // Assert
            // Should have at least one victory
            Assert.Contains("VICTORY", log);
        }

        [Fact]
        public void GameCore_UsesAbilitiesInCombat()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var gameCore = new GameCore(autoInterface);

            // Act
            gameCore.Start();
            var log = autoInterface.GetLog();

            // Assert
            // AutomatedInterface should use abilities when mana available
            Assert.Contains("used", log); // "Player used [Ability]"
        }

        [Fact]
        public void GameCore_DamageEventsAreLogged()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var gameCore = new GameCore(autoInterface);

            // Act
            gameCore.Start();
            var log = autoInterface.GetLog();

            // Assert
            Assert.Contains("damage", log);
        }

        [Fact]
        public void GameCore_ExitsAfterThreeCombats()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var gameCore = new GameCore(autoInterface);

            // Act
            gameCore.Start();
            var log = autoInterface.GetLog();

            // Assert
            // DefaultStrategy exits after 3 combats
            int combatCount = System.Text.RegularExpressions.Regex.Matches(log, "=== COMBAT").Count;
            Assert.True(combatCount <= 3, $"Expected at most 3 combats, got {combatCount}");
        }
    }
}
