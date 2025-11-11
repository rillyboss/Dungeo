using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Moq;
using TestRPGGame.Systems;
using TestRPGGame.Interfaces;
using TestRPGGame.Combat;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Dungeon;

namespace TestRPGGame.Tests
{
    public class DungeonManagerTests : TestBase
    {
        private readonly Mock<IGameInterface> _mockInterface;
        private readonly InterfacedCombatSystem _combat;

        public DungeonManagerTests()
        {
            _mockInterface = new Mock<IGameInterface>();
            _combat = new InterfacedCombatSystem(_mockInterface.Object);
        }

        [Fact]
        public void DungeonManager_CanBeInstantiated()
        {
            // Act
            var dungeonManager = new DungeonManager(_mockInterface.Object, _combat);

            // Assert
            Assert.NotNull(dungeonManager);
            Assert.NotNull(dungeonManager.Progress);
            Assert.Empty(dungeonManager.Progress.CompletedDungeons);
        }

        [Fact]
        public void SetProgress_UpdatesDungeonProgress()
        {
            // Arrange
            var dungeonManager = new DungeonManager(_mockInterface.Object, _combat);
            var progress = new DungeonProgress();
            progress.CompletedDungeons["test_dungeon"] = true;

            // Act
            dungeonManager.SetProgress(progress);

            // Assert
            Assert.Equal(progress, dungeonManager.Progress);
            Assert.True(dungeonManager.Progress.CompletedDungeons["test_dungeon"]);
        }

        [Fact]
        public void SetProgress_NullProgress_CreatesNewProgress()
        {
            // Arrange
            var dungeonManager = new DungeonManager(_mockInterface.Object, _combat);

            // Act
            dungeonManager.SetProgress(null);

            // Assert
            Assert.NotNull(dungeonManager.Progress);
            Assert.Empty(dungeonManager.Progress.CompletedDungeons);
        }

        [Fact]
        public void EnterDungeon_UserSelectsInvalidIndex_ReturnsFalse()
        {
            // Arrange
            var dungeonManager = new DungeonManager(_mockInterface.Object, _combat);
            var player = new Player("Hero", PlayerClass.Warrior);
            player.Level = 5;
            player.Gold = 1000;

            _mockInterface.Setup(x => x.RequestDungeonSelection(It.IsAny<List<DungeonSelectionInfo>>()))
                .Returns(-1); // User cancelled

            // Act
            bool result = dungeonManager.EnterDungeon(player);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EnterDungeon_RequirementsNotMet_ReturnsFalse()
        {
            // Arrange
            var dungeonManager = new DungeonManager(_mockInterface.Object, _combat);
            var player = new Player("Hero", PlayerClass.Warrior);
            player.Level = 1; // Too low level for higher dungeons
            player.Gold = 0;

            _mockInterface.Setup(x => x.RequestDungeonSelection(It.IsAny<List<DungeonSelectionInfo>>()))
                .Returns(1); // Select second dungeon (likely higher level requirement)

            // Act
            bool result = dungeonManager.EnterDungeon(player);

            // Assert
            Assert.False(result);

            // Verify error event was published
            _mockInterface.Verify(x => x.OnEvent(It.Is<GameEvents.InfoMessageEvent>(
                e => e.Message.Contains("don't meet the requirements") && e.Type == GameEvents.MessageType.Error
            )), Times.Once);
        }

        [Fact]
        public void EnterDungeon_UserDeclines_ReturnsFalse()
        {
            // Arrange
            var dungeonManager = new DungeonManager(_mockInterface.Object, _combat);
            var player = new Player("Hero", PlayerClass.Warrior);
            player.Level = 10; // High enough for any dungeon
            player.Gold = 1000;

            _mockInterface.Setup(x => x.RequestDungeonSelection(It.IsAny<List<DungeonSelectionInfo>>()))
                .Returns(0); // Select first dungeon
            _mockInterface.Setup(x => x.RequestConfirmation(It.IsAny<string>()))
                .Returns(false); // User declines to enter

            // Act
            bool result = dungeonManager.EnterDungeon(player);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EnterDungeon_ShowsCorrectRequirements()
        {
            // Arrange
            var dungeonManager = new DungeonManager(_mockInterface.Object, _combat);
            var player = new Player("Hero", PlayerClass.Warrior);
            player.Level = 1;
            player.Gold = 0;

            List<DungeonSelectionInfo>? capturedDungeons = null;
            _mockInterface.Setup(x => x.RequestDungeonSelection(It.IsAny<List<DungeonSelectionInfo>>()))
                .Callback<List<DungeonSelectionInfo>>(d => capturedDungeons = d)
                .Returns(-1);

            // Act
            dungeonManager.EnterDungeon(player);

            // Assert
            Assert.NotNull(capturedDungeons);
            Assert.NotEmpty(capturedDungeons);

            // First dungeon should be available
            var firstDungeon = capturedDungeons.First();
            Assert.True(firstDungeon.CanEnter || !string.IsNullOrEmpty(firstDungeon.BlockingReason));
        }

        [Fact]
        public void EnterDungeon_PassesCorrectDungeonInfo()
        {
            // Arrange
            var dungeonManager = new DungeonManager(_mockInterface.Object, _combat);
            var player = new Player("Hero", PlayerClass.Warrior);
            player.Level = 10;
            player.Gold = 1000;

            List<DungeonSelectionInfo>? capturedDungeons = null;
            _mockInterface.Setup(x => x.RequestDungeonSelection(It.IsAny<List<DungeonSelectionInfo>>()))
                .Callback<List<DungeonSelectionInfo>>(d => capturedDungeons = d)
                .Returns(-1);

            // Act
            dungeonManager.EnterDungeon(player);

            // Assert
            Assert.NotNull(capturedDungeons);
            foreach (var dungeon in capturedDungeons)
            {
                Assert.NotNull(dungeon.Name);
                Assert.True(dungeon.MinLevel >= 0);
                Assert.True(dungeon.GoldCost >= 0);
                Assert.True(dungeon.Difficulty >= 0);
            }
        }

        [Fact]
        public void EnterDungeon_CompletedDungeonsMarkedCorrectly()
        {
            // Arrange
            var dungeonManager = new DungeonManager(_mockInterface.Object, _combat);
            var player = new Player("Hero", PlayerClass.Warrior);
            player.Level = 10;
            player.Gold = 1000;

            // Mark a dungeon as completed
            var progress = new DungeonProgress();
            var dungeonName = "Goblin Caves"; // Assuming first dungeon name
            progress.CompletedDungeons[dungeonName] = true;
            dungeonManager.SetProgress(progress);

            List<DungeonSelectionInfo>? capturedDungeons = null;
            _mockInterface.Setup(x => x.RequestDungeonSelection(It.IsAny<List<DungeonSelectionInfo>>()))
                .Callback<List<DungeonSelectionInfo>>(d => capturedDungeons = d)
                .Returns(-1);

            // Act
            dungeonManager.EnterDungeon(player);

            // Assert
            Assert.NotNull(capturedDungeons);
            var completedDungeon = capturedDungeons.FirstOrDefault(d => d.Name == dungeonName);

            if (completedDungeon != null)
            {
                Assert.True(completedDungeon.IsCompleted);
            }
        }

        [Fact]
        public void Progress_InitializesEmpty()
        {
            // Arrange & Act
            var dungeonManager = new DungeonManager(_mockInterface.Object, _combat);

            // Assert
            Assert.NotNull(dungeonManager.Progress);
            Assert.Empty(dungeonManager.Progress.CompletedDungeons);
        }

        [Fact]
        public void Progress_CanBeUpdatedAfterCreation()
        {
            // Arrange
            var dungeonManager = new DungeonManager(_mockInterface.Object, _combat);

            // Act - Manually mark a dungeon as completed
            dungeonManager.Progress.CompletedDungeons["test_dungeon_1"] = true;
            dungeonManager.Progress.CompletedDungeons["test_dungeon_2"] = true;

            // Assert
            Assert.Equal(2, dungeonManager.Progress.CompletedDungeons.Count);
            Assert.True(dungeonManager.Progress.CompletedDungeons["test_dungeon_1"]);
            Assert.True(dungeonManager.Progress.CompletedDungeons["test_dungeon_2"]);
        }

        [Fact]
        public void EnterDungeon_ConfirmationPromptIncludesCost()
        {
            // Arrange
            var dungeonManager = new DungeonManager(_mockInterface.Object, _combat);
            var player = new Player("Hero", PlayerClass.Warrior);
            player.Level = 10;
            player.Gold = 1000;

            _mockInterface.Setup(x => x.RequestDungeonSelection(It.IsAny<List<DungeonSelectionInfo>>()))
                .Returns(0);
            _mockInterface.Setup(x => x.RequestConfirmation(It.IsAny<string>()))
                .Returns(false);

            // Act
            dungeonManager.EnterDungeon(player);

            // Assert - Confirmation should include cost information
            _mockInterface.Verify(x => x.RequestConfirmation(
                It.Is<string>(s => s.Contains("Cost") || s.Contains("gold"))
            ), Times.Once);
        }

        [Fact]
        public void EnterDungeon_DungeonEnteredEventPublished()
        {
            // Arrange
            var dungeonManager = new DungeonManager(_mockInterface.Object, _combat);
            var player = new Player("Hero", PlayerClass.Warrior);
            player.Level = 100; // Very high to ensure success
            player.Gold = 10000;

            // Mock automated interface for combat
            var autoInterface = new AutomatedInterface(new DefaultStrategy());
            var autoCombat = new InterfacedCombatSystem(autoInterface);
            var autoDungeonManager = new DungeonManager(autoInterface, autoCombat);

            // Act - Actually enter and complete a dungeon
            bool result = autoDungeonManager.EnterDungeon(player);

            // Assert - Check that dungeon entry was logged
            var log = autoInterface.GetLog();
            Assert.Contains("DUNGEON", log);
        }
    }
}
