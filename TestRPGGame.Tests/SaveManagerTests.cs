using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Moq;
using TestRPGGame.Systems;
using TestRPGGame.Interfaces;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Dungeon;

namespace TestRPGGame.Tests
{
    [Collection("SaveSystem")]
    public class SaveManagerTests : TestBase
    {
        private readonly Mock<IGameInterface> _mockInterface;
        private readonly string _saveDirectory;

        public SaveManagerTests()
        {
            _mockInterface = new Mock<IGameInterface>();

            // Clean up save files before each test
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _saveDirectory = Path.Combine(appData, "TestRPGGame", "Saves");
            CleanupSaveFiles();
        }

        private void CleanupSaveFiles()
        {
            if (Directory.Exists(_saveDirectory))
            {
                foreach (var file in Directory.GetFiles(_saveDirectory, "save_slot_*.json"))
                {
                    try { File.Delete(file); } catch { }
                }
            }
        }

        [Fact]
        public void SaveManager_CanBeInstantiated()
        {
            // Act
            var saveManager = new SaveManager(_mockInterface.Object);

            // Assert
            Assert.NotNull(saveManager);
            Assert.Null(saveManager.LastSaveSlot);
        }

        [Fact]
        public void CreateNewCharacter_CreatesPlayerAndPublishesEvent()
        {
            // Arrange
            var saveManager = new SaveManager(_mockInterface.Object);
            _mockInterface.Setup(x => x.RequestCharacterCreation())
                .Returns(("TestHero", PlayerClass.Warrior));

            // Act
            var player = saveManager.CreateNewCharacter(1);

            // Assert
            Assert.NotNull(player);
            Assert.Equal("TestHero", player.Name);
            Assert.Equal(PlayerClass.Warrior, player.Class);
            Assert.Equal(1, saveManager.LastSaveSlot);

            // Verify CharacterCreatedEvent was published
            _mockInterface.Verify(x => x.OnEvent(It.Is<GameEvents.CharacterCreatedEvent>(
                e => e.Name == "TestHero" && e.Class == PlayerClass.Warrior
            )), Times.Once);
        }

        [Fact]
        public void CreateNewCharacter_SetsLastSaveSlot()
        {
            // Arrange
            var saveManager = new SaveManager(_mockInterface.Object);
            _mockInterface.Setup(x => x.RequestCharacterCreation())
                .Returns(("Hero", PlayerClass.Mage));

            // Act
            saveManager.CreateNewCharacter(2);

            // Assert
            Assert.Equal(2, saveManager.LastSaveSlot);
        }

        [Fact]
        public void LoadCharacter_Success_ReturnsPlayerAndProgress()
        {
            // Arrange
            var saveManager = new SaveManager(_mockInterface.Object);

            // Create a save file
            var player = new Player("SavedHero", PlayerClass.Rogue);
            player.Level = 5;
            player.Gold = 500;
            var progress = new DungeonProgress();
            progress.CompletedDungeons["test_dungeon"] = true;
            SaveSystem.SaveGame(player, 1, progress);

            // Act
            var (loadedPlayer, loadedProgress, _, _) = saveManager.LoadCharacter(1);

            // Assert
            Assert.NotNull(loadedPlayer);
            Assert.NotNull(loadedProgress);
            Assert.Equal("SavedHero", loadedPlayer.Name);
            Assert.Equal(5, loadedPlayer.Level);
            Assert.Equal(500, loadedPlayer.Gold);
            Assert.True(loadedProgress.CompletedDungeons.GetValueOrDefault("test_dungeon", false));
            Assert.Equal(1, saveManager.LastSaveSlot);

            // Verify events
            _mockInterface.Verify(x => x.OnEvent(It.IsAny<GameEvents.GameLoadedEvent>()), Times.Once);
            _mockInterface.Verify(x => x.OnEvent(It.Is<GameEvents.InfoMessageEvent>(
                e => e.Message.Contains("loaded successfully")
            )), Times.Once);
        }

        [Fact]
        public void LoadCharacter_Failure_ReturnsNullsAndPublishesError()
        {
            // Arrange
            var saveManager = new SaveManager(_mockInterface.Object);

            // Act - Try to load from non-existent slot
            var (loadedPlayer, loadedProgress, _, _) = saveManager.LoadCharacter(3);

            // Assert
            Assert.Null(loadedPlayer);
            Assert.Null(loadedProgress);

            // Verify error event
            _mockInterface.Verify(x => x.OnEvent(It.Is<GameEvents.InfoMessageEvent>(
                e => e.Message.Contains("Failed to load") && e.Type == GameEvents.MessageType.Error
            )), Times.Once);
        }

        [Fact]
        public void SaveGame_NewSlot_SavesSuccessfully()
        {
            // Arrange
            var saveManager = new SaveManager(_mockInterface.Object);
            var player = new Player("TestHero", PlayerClass.Warrior);
            var progress = new DungeonProgress();

            var slots = new List<InterfaceSaveSlotInfo>
            {
                new InterfaceSaveSlotInfo { SlotNumber = 1, IsEmpty = true },
                new InterfaceSaveSlotInfo { SlotNumber = 2, IsEmpty = true },
                new InterfaceSaveSlotInfo { SlotNumber = 3, IsEmpty = true }
            };

            _mockInterface.Setup(x => x.RequestSaveSlot(It.IsAny<List<InterfaceSaveSlotInfo>>()))
                .Returns(1);

            // Act
            saveManager.SaveGame(player, progress);

            // Assert
            Assert.Equal(1, saveManager.LastSaveSlot);

            // Verify success events
            _mockInterface.Verify(x => x.OnEvent(It.Is<GameEvents.GameSavedEvent>(
                e => e.SlotNumber == 1 && e.Success
            )), Times.Once);
            _mockInterface.Verify(x => x.OnEvent(It.Is<GameEvents.InfoMessageEvent>(
                e => e.Message.Contains("saved to slot 1")
            )), Times.Once);
        }

        [Fact]
        public void SaveGame_OverwriteWithConfirmation_SavesSuccessfully()
        {
            // Arrange
            var saveManager = new SaveManager(_mockInterface.Object);
            var player = new Player("NewHero", PlayerClass.Mage);
            var progress = new DungeonProgress();

            // Create existing save
            var oldPlayer = new Player("OldHero", PlayerClass.Warrior);
            SaveSystem.SaveGame(oldPlayer, 1, progress);

            var slots = new List<InterfaceSaveSlotInfo>
            {
                new InterfaceSaveSlotInfo
                {
                    SlotNumber = 1,
                    IsEmpty = false,
                    Name = "OldHero",
                    Level = 1,
                    Class = PlayerClass.Warrior
                }
            };

            _mockInterface.Setup(x => x.RequestSaveSlot(It.IsAny<List<InterfaceSaveSlotInfo>>()))
                .Returns(1);
            _mockInterface.Setup(x => x.RequestConfirmation(It.IsAny<string>()))
                .Returns(true); // User confirms overwrite

            // Act
            saveManager.SaveGame(player, progress);

            // Assert
            Assert.Equal(1, saveManager.LastSaveSlot);

            // Verify confirmation was requested
            _mockInterface.Verify(x => x.RequestConfirmation(
                It.Is<string>(s => s.Contains("Overwrite") && s.Contains("slot 1"))
            ), Times.Once);

            // Verify save succeeded
            var (loaded, _, _, _) = SaveSystem.LoadGame(1);
            Assert.NotNull(loaded);
            Assert.Equal("NewHero", loaded.Name);
        }

        [Fact]
        public void SaveGame_OverwriteWithoutConfirmation_DoesNotSave()
        {
            // Arrange
            var saveManager = new SaveManager(_mockInterface.Object);
            var player = new Player("NewHero", PlayerClass.Mage);
            var progress = new DungeonProgress();

            // Create existing save
            var oldPlayer = new Player("OldHero", PlayerClass.Warrior);
            SaveSystem.SaveGame(oldPlayer, 1, progress);

            var slots = new List<InterfaceSaveSlotInfo>
            {
                new InterfaceSaveSlotInfo
                {
                    SlotNumber = 1,
                    IsEmpty = false,
                    Name = "OldHero"
                }
            };

            _mockInterface.Setup(x => x.RequestSaveSlot(It.IsAny<List<InterfaceSaveSlotInfo>>()))
                .Returns(1);
            _mockInterface.Setup(x => x.RequestConfirmation(It.IsAny<string>()))
                .Returns(false); // User cancels overwrite

            // Act
            saveManager.SaveGame(player, progress);

            // Assert - LastSaveSlot should not be updated
            Assert.Null(saveManager.LastSaveSlot);

            // Verify old save is still there
            var (loaded, _, _, _) = SaveSystem.LoadGame(1);
            Assert.Equal("OldHero", loaded?.Name);
        }

        [Fact]
        public void AutoSave_WithLastSaveSlot_SavesSuccessfully()
        {
            // Arrange
            var saveManager = new SaveManager(_mockInterface.Object);
            var player = new Player("Hero", PlayerClass.Warrior);
            var progress = new DungeonProgress();

            // Set up last save slot by creating a character
            _mockInterface.Setup(x => x.RequestCharacterCreation())
                .Returns(("Hero", PlayerClass.Warrior));
            saveManager.CreateNewCharacter(2);

            // Act
            saveManager.AutoSave(player, progress);

            // Assert
            _mockInterface.Verify(x => x.OnEvent(It.Is<GameEvents.GameSavedEvent>(
                e => e.SlotNumber == 2 && e.Success
            )), Times.Once);

            // Verify save file exists
            var (loaded, _, _, _) = SaveSystem.LoadGame(2);
            Assert.NotNull(loaded);
        }

        [Fact]
        public void AutoSave_WithoutLastSaveSlot_DoesNothing()
        {
            // Arrange
            var saveManager = new SaveManager(_mockInterface.Object);
            var player = new Player("Hero", PlayerClass.Warrior);
            var progress = new DungeonProgress();

            // Act - AutoSave without having set a save slot
            saveManager.AutoSave(player, progress);

            // Assert - No save event should be published
            _mockInterface.Verify(x => x.OnEvent(It.IsAny<GameEvents.GameSavedEvent>()), Times.Never);
        }

        [Fact]
        public void RequestSaveSlotSelection_EmptySlot_ReturnsNewCharacter()
        {
            // Arrange
            var saveManager = new SaveManager(_mockInterface.Object);
            var slots = new List<InterfaceSaveSlotInfo>
            {
                new InterfaceSaveSlotInfo { SlotNumber = 1, IsEmpty = true },
                new InterfaceSaveSlotInfo { SlotNumber = 2, IsEmpty = false, Name = "Existing" },
                new InterfaceSaveSlotInfo { SlotNumber = 3, IsEmpty = true }
            };

            _mockInterface.Setup(x => x.RequestSaveSlotSelection(It.IsAny<List<InterfaceSaveSlotInfo>>()))
                .Returns((1, true));

            // Act
            var (slotNumber, isNewCharacter) = saveManager.RequestSaveSlotSelection();

            // Assert
            Assert.Equal(1, slotNumber);
            Assert.True(isNewCharacter);
        }

        [Fact]
        public void RequestSaveSlotSelection_LoadExisting_ReturnsExistingCharacter()
        {
            // Arrange
            var saveManager = new SaveManager(_mockInterface.Object);
            _mockInterface.Setup(x => x.RequestSaveSlotSelection(It.IsAny<List<InterfaceSaveSlotInfo>>()))
                .Returns((2, false));

            // Act
            var (slotNumber, isNewCharacter) = saveManager.RequestSaveSlotSelection();

            // Assert
            Assert.Equal(2, slotNumber);
            Assert.False(isNewCharacter);
        }

        [Fact]
        public void SaveGame_InvalidSlot_DoesNotSave()
        {
            // Arrange
            var saveManager = new SaveManager(_mockInterface.Object);
            var player = new Player("Hero", PlayerClass.Warrior);
            var progress = new DungeonProgress();

            _mockInterface.Setup(x => x.RequestSaveSlot(It.IsAny<List<InterfaceSaveSlotInfo>>()))
                .Returns(0); // Invalid slot (should be 1-3)

            // Act
            saveManager.SaveGame(player, progress);

            // Assert - No save event should be published
            _mockInterface.Verify(x => x.OnEvent(It.IsAny<GameEvents.GameSavedEvent>()), Times.Never);
            Assert.Null(saveManager.LastSaveSlot);
        }

        [Fact]
        public void MultipleOperations_LastSaveSlotTracksCorrectly()
        {
            // Arrange
            var saveManager = new SaveManager(_mockInterface.Object);
            var player = new Player("Hero", PlayerClass.Warrior);
            var progress = new DungeonProgress();

            // Create character in slot 1
            _mockInterface.Setup(x => x.RequestCharacterCreation())
                .Returns(("Hero", PlayerClass.Warrior));
            saveManager.CreateNewCharacter(1);
            Assert.Equal(1, saveManager.LastSaveSlot);

            // Load from slot 2
            SaveSystem.SaveGame(player, 2, progress);
            saveManager.LoadCharacter(2);
            Assert.Equal(2, saveManager.LastSaveSlot);

            // AutoSave should now save to slot 2
            saveManager.AutoSave(player, progress);
            _mockInterface.Verify(x => x.OnEvent(It.Is<GameEvents.GameSavedEvent>(
                e => e.SlotNumber == 2
            )), Times.AtLeastOnce);
        }
    }
}
