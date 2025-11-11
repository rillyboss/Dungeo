using Xunit;
using TestRPGGame.Systems;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Dungeon;
using System.IO;
using System.Linq;

namespace TestRPGGame.Tests
{
    public class SaveSystemTests : TestBase
    {
        private readonly string saveDirectory;

        public SaveSystemTests()
        {
            // SaveSystem uses AppData/TestRPGGame/Saves by default
            // For tests, we'll just clean up before and after each test
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
        public void SaveGame_CreatesValidSaveFile()
        {
            // Arrange
            var player = new Player("TestHero", PlayerClass.Warrior);
            player.Gold = 500;
            player.Level = 5;
            var progress = new DungeonProgress();

            // Act
            bool result = SaveSystem.SaveGame(player, 1, progress);

            // Assert
            Assert.True(result);
            var saveFile = Path.Combine(saveDirectory, "save_slot_1.json");
            Assert.True(File.Exists(saveFile));
        }

        [Fact]
        public void LoadGame_RestoresPlayerCorrectly()
        {
            // Arrange
            var originalPlayer = new Player("LoadTest", PlayerClass.Mage);
            originalPlayer.Gold = 750;
            originalPlayer.Level = 3;
            originalPlayer.CurrentHP = 50;
            var progress = new DungeonProgress();

            SaveSystem.SaveGame(originalPlayer, 2, progress);

            // Act
            var (loadedPlayer, loadedProgress) = SaveSystem.LoadGame(2);

            // Assert
            Assert.NotNull(loadedPlayer);
            Assert.Equal("LoadTest", loadedPlayer.Name);
            Assert.Equal(PlayerClass.Mage, loadedPlayer.Class);
            Assert.Equal(750, loadedPlayer.Gold);
            Assert.Equal(3, loadedPlayer.Level);
            Assert.Equal(50, loadedPlayer.CurrentHP);
        }

        [Fact]
        public void DeleteSave_RemovesSaveFile()
        {
            // Arrange
            var player = new Player("DeleteMe", PlayerClass.Rogue);
            var progress = new DungeonProgress();
            SaveSystem.SaveGame(player, 3, progress);

            // Verify save exists
            var saveFile = Path.Combine(saveDirectory, "save_slot_3.json");
            Assert.True(File.Exists(saveFile));

            // Act
            bool result = SaveSystem.DeleteSave(3);

            // Assert
            Assert.True(result);
            Assert.False(File.Exists(saveFile));
        }

        [Fact]
        public void DeleteSave_ReturnsFalseForEmptySlot()
        {
            // Act
            bool result = SaveSystem.DeleteSave(1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void DeleteSave_ReturnsFalseForInvalidSlot()
        {
            // Act
            bool resultNegative = SaveSystem.DeleteSave(-1);
            bool resultZero = SaveSystem.DeleteSave(0);
            bool resultTooHigh = SaveSystem.DeleteSave(99);

            // Assert
            Assert.False(resultNegative);
            Assert.False(resultZero);
            Assert.False(resultTooHigh);
        }

        [Fact]
        public void GetAllSaveSlots_ReturnsThreeSlots()
        {
            // Act
            var slots = SaveSystem.GetAllSaveSlots();

            // Assert
            Assert.Equal(3, slots.Count);
            Assert.Contains(slots, s => s.SlotNumber == 1);
            Assert.Contains(slots, s => s.SlotNumber == 2);
            Assert.Contains(slots, s => s.SlotNumber == 3);
        }

        [Fact]
        public void GetAllSaveSlots_CorrectlyIdentifiesEmptySlots()
        {
            // Arrange
            var player = new Player("Slot1Hero", PlayerClass.Warrior);
            var progress = new DungeonProgress();
            SaveSystem.SaveGame(player, 1, progress);

            // Act
            var slots = SaveSystem.GetAllSaveSlots();

            // Assert
            var slot1 = slots.First(s => s.SlotNumber == 1);
            var slot2 = slots.First(s => s.SlotNumber == 2);
            var slot3 = slots.First(s => s.SlotNumber == 3);

            Assert.False(slot1.IsEmpty);
            Assert.Equal("Slot1Hero", slot1.Name);
            Assert.True(slot2.IsEmpty);
            Assert.True(slot3.IsEmpty);
        }

        [Fact]
        public void SaveGame_OverwritesExistingSave()
        {
            // Arrange
            var player1 = new Player("First", PlayerClass.Warrior);
            player1.Gold = 100;
            var progress = new DungeonProgress();
            SaveSystem.SaveGame(player1, 1, progress);

            var player2 = new Player("Second", PlayerClass.Mage);
            player2.Gold = 200;

            // Act
            SaveSystem.SaveGame(player2, 1, progress);
            var (loaded, _) = SaveSystem.LoadGame(1);

            // Assert
            Assert.NotNull(loaded);
            Assert.Equal("Second", loaded.Name);
            Assert.Equal(PlayerClass.Mage, loaded.Class);
            Assert.Equal(200, loaded.Gold);
        }

        [Fact]
        public void LoadGame_ReturnsNullForEmptySlot()
        {
            // Act
            var (player, progress) = SaveSystem.LoadGame(1);

            // Assert
            Assert.Null(player);
            Assert.Null(progress);
        }

        [Fact]
        public void SaveGame_PreservesDungeonProgress()
        {
            // Arrange
            var player = new Player("ProgressTest", PlayerClass.Warrior);
            var progress = new DungeonProgress();
            progress.CompletedDungeons["The Dark Cave"] = true;
            progress.CompletedDungeons["Ancient Ruins"] = true;

            // Act
            SaveSystem.SaveGame(player, 1, progress);
            var (loadedPlayer, loadedProgress) = SaveSystem.LoadGame(1);

            // Assert
            Assert.NotNull(loadedProgress);
            Assert.True(loadedProgress.CompletedDungeons.GetValueOrDefault("The Dark Cave", false));
            Assert.True(loadedProgress.CompletedDungeons.GetValueOrDefault("Ancient Ruins", false));
            Assert.False(loadedProgress.CompletedDungeons.GetValueOrDefault("Unknown Dungeon", false));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void SaveGame_WorksForAllSlots(int slotNumber)
        {
            // Arrange
            var player = new Player($"Slot{slotNumber}Test", PlayerClass.Rogue);
            var progress = new DungeonProgress();

            // Act
            bool result = SaveSystem.SaveGame(player, slotNumber, progress);

            // Assert
            Assert.True(result);
            var slots = SaveSystem.GetAllSaveSlots();
            var slot = slots.First(s => s.SlotNumber == slotNumber);
            Assert.False(slot.IsEmpty);
            Assert.Equal($"Slot{slotNumber}Test", slot.Name);
        }

        [Fact]
        public void SaveGame_PreservesPlayerInventory()
        {
            // Arrange
            var player = new Player("InventoryTest", PlayerClass.Warrior);
            var originalWeapon = player.Inventory.Weapon;
            var originalArmor = player.Inventory.Armor;
            var progress = new DungeonProgress();

            // Act
            SaveSystem.SaveGame(player, 1, progress);
            var (loadedPlayer, _) = SaveSystem.LoadGame(1);

            // Assert
            Assert.NotNull(loadedPlayer);
            Assert.NotNull(loadedPlayer.Inventory.Weapon);
            Assert.NotNull(loadedPlayer.Inventory.Armor);
            Assert.Equal(originalWeapon?.Name, loadedPlayer.Inventory.Weapon?.Name);
            Assert.Equal(originalArmor?.Name, loadedPlayer.Inventory.Armor?.Name);
        }

        [Fact]
        public void SaveGame_PreservesPlayerAbilities()
        {
            // Arrange
            var player = new Player("AbilityTest", PlayerClass.Mage);
            int originalAbilityCount = player.Abilities.Count;
            var progress = new DungeonProgress();

            // Act
            SaveSystem.SaveGame(player, 1, progress);
            var (loadedPlayer, _) = SaveSystem.LoadGame(1);

            // Assert
            Assert.NotNull(loadedPlayer);
            Assert.Equal(originalAbilityCount, loadedPlayer.Abilities.Count);
        }
    }
}
