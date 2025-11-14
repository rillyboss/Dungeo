using System;
using System.Linq;
using Xunit;
using Moq;
using TestRPGGame.Systems;
using TestRPGGame.Interfaces;
using TestRPGGame.Entities.Player;

namespace TestRPGGame.Tests
{
    public class ProgressionManagerTests : TestBase
    {
        private readonly Mock<IGameInterface> _mockInterface;

        public ProgressionManagerTests()
        {
            _mockInterface = new Mock<IGameInterface>();
        }

        [Fact]
        public void ProgressionManager_CanBeInstantiated()
        {
            // Act
            var progressionManager = new ProgressionManager(_mockInterface.Object);

            // Assert
            Assert.NotNull(progressionManager);
        }

        #region UnlockAbilities Tests

        [Fact]
        public void UnlockAbilities_AllAbilitiesUnlocked_ShowsSuccessMessage()
        {
            // Arrange
            var progressionManager = new ProgressionManager(_mockInterface.Object);
            var player = new Player("Hero", PlayerClass.Warrior);

            // Unlock all abilities
            foreach (var ability in player.Abilities)
            {
                if (!ability.IsUnlocked)
                {
                    ability.Unlock();
                }
            }

            // Act
            progressionManager.UnlockAbilities(player);

            // Assert
            _mockInterface.Verify(x => x.OnEvent(It.Is<GameEvents.InfoMessageEvent>(
                e => e.Message.Contains("All abilities unlocked") && e.Type == GameEvents.MessageType.Success
            )), Times.Once);

            // RequestAbilityUnlock should not be called
            _mockInterface.Verify(x => x.RequestAbilityUnlock(
                It.IsAny<System.Collections.Generic.List<AbilityInfo>>(),
                It.IsAny<int>(),
                It.IsAny<int>()
            ), Times.Never);
        }

        [Fact]
        public void UnlockAbilities_UserCancels_NoAbilityUnlocked()
        {
            // Arrange
            var progressionManager = new ProgressionManager(_mockInterface.Object);
            var player = new Player("Hero", PlayerClass.Warrior);
            int initialGold = player.Gold;

            _mockInterface.Setup(x => x.RequestAbilityUnlock(
                It.IsAny<System.Collections.Generic.List<AbilityInfo>>(),
                It.IsAny<int>(),
                It.IsAny<int>()
            )).Returns(-1); // User cancelled

            // Act
            progressionManager.UnlockAbilities(player);

            // Assert
            Assert.Equal(initialGold, player.Gold); // Gold unchanged
            _mockInterface.Verify(x => x.RequestConfirmation(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void UnlockAbilities_SuccessfulUnlock_DeductsGoldAndUnlocksAbility()
        {
            // Arrange
            var progressionManager = new ProgressionManager(_mockInterface.Object);
            var player = new Player("Hero", PlayerClass.Warrior);
            player.Gold = 10000; // Need enough to afford abilities with 10x multiplier
            player.Level = 10;

            var lockedAbility = player.Abilities.FirstOrDefault(a => !a.IsUnlocked);
            Assert.NotNull(lockedAbility); // Ensure there is a locked ability

            int initialGold = player.Gold;
            int unlockCost = lockedAbility.PurchaseCost;

            _mockInterface.Setup(x => x.RequestAbilityUnlock(
                It.IsAny<System.Collections.Generic.List<AbilityInfo>>(),
                It.IsAny<int>(),
                It.IsAny<int>()
            )).Returns(0); // Select first locked ability

            _mockInterface.Setup(x => x.RequestConfirmation(It.IsAny<string>()))
                .Returns(true); // Confirm unlock

            // Act
            progressionManager.UnlockAbilities(player);

            // Assert
            Assert.Equal(initialGold - unlockCost, player.Gold);
            Assert.True(lockedAbility.IsUnlocked);

            // Verify events
            _mockInterface.Verify(x => x.OnEvent(It.IsAny<GameEvents.AbilityUnlockedEvent>()), Times.Once);
            _mockInterface.Verify(x => x.OnEvent(It.Is<GameEvents.InfoMessageEvent>(
                e => e.Message.Contains("unlocked") && e.Type == GameEvents.MessageType.Success
            )), Times.Once);
        }

        [Fact]
        public void UnlockAbilities_UserDeclinesConfirmation_NoAbilityUnlocked()
        {
            // Arrange
            var progressionManager = new ProgressionManager(_mockInterface.Object);
            var player = new Player("Hero", PlayerClass.Warrior);
            player.Gold = 1000;
            player.Level = 10;

            var lockedAbility = player.Abilities.FirstOrDefault(a => !a.IsUnlocked);
            int initialGold = player.Gold;

            _mockInterface.Setup(x => x.RequestAbilityUnlock(
                It.IsAny<System.Collections.Generic.List<AbilityInfo>>(),
                It.IsAny<int>(),
                It.IsAny<int>()
            )).Returns(0);

            _mockInterface.Setup(x => x.RequestConfirmation(It.IsAny<string>()))
                .Returns(false); // User declines

            // Act
            progressionManager.UnlockAbilities(player);

            // Assert
            Assert.Equal(initialGold, player.Gold);
            Assert.False(lockedAbility.IsUnlocked);
            _mockInterface.Verify(x => x.OnEvent(It.IsAny<GameEvents.AbilityUnlockedEvent>()), Times.Never);
        }

        [Fact]
        public void UnlockAbilities_NotEnoughGold_ShowsErrorMessage()
        {
            // Arrange
            var progressionManager = new ProgressionManager(_mockInterface.Object);
            var player = new Player("Hero", PlayerClass.Warrior);
            player.Gold = 0; // No gold
            player.Level = 10;

            _mockInterface.Setup(x => x.RequestAbilityUnlock(
                It.IsAny<System.Collections.Generic.List<AbilityInfo>>(),
                It.IsAny<int>(),
                It.IsAny<int>()
            )).Returns(0);

            _mockInterface.Setup(x => x.RequestConfirmation(It.IsAny<string>()))
                .Returns(true);

            // Act
            progressionManager.UnlockAbilities(player);

            // Assert
            _mockInterface.Verify(x => x.OnEvent(It.Is<GameEvents.InfoMessageEvent>(
                e => e.Message.Contains("Cannot unlock") && e.Type == GameEvents.MessageType.Error
            )), Times.Once);
        }

        [Fact]
        public void UnlockAbilities_LowLevel_CanStillPurchaseWithGold()
        {
            // Arrange
            var progressionManager = new ProgressionManager(_mockInterface.Object);
            var player = new Player("Hero", PlayerClass.Warrior);
            player.Gold = 10000; // Plenty of gold
            player.Level = 1; // Low level

            _mockInterface.Setup(x => x.RequestAbilityUnlock(
                It.IsAny<System.Collections.Generic.List<AbilityInfo>>(),
                It.IsAny<int>(),
                It.IsAny<int>()
            )).Returns(0);

            _mockInterface.Setup(x => x.RequestConfirmation(It.IsAny<string>()))
                .Returns(true);

            // Act
            progressionManager.UnlockAbilities(player);

            // Assert
            // Shop purchases only check gold (no level requirement)
            // Ability should be unlocked successfully
            _mockInterface.Verify(x => x.OnEvent(It.Is<GameEvents.AbilityUnlockedEvent>(
                e => true
            )), Times.Once);
        }

        [Fact]
        public void UnlockAbilities_ConfirmationMessageIncludesCost()
        {
            // Arrange
            var progressionManager = new ProgressionManager(_mockInterface.Object);
            var player = new Player("Hero", PlayerClass.Warrior);
            player.Gold = 10000;
            player.Level = 10;

            var lockedAbility = player.Abilities.FirstOrDefault(a => !a.IsUnlocked);
            Assert.NotNull(lockedAbility);

            _mockInterface.Setup(x => x.RequestAbilityUnlock(
                It.IsAny<System.Collections.Generic.List<AbilityInfo>>(),
                It.IsAny<int>(),
                It.IsAny<int>()
            )).Returns(0);

            _mockInterface.Setup(x => x.RequestConfirmation(It.IsAny<string>()))
                .Returns(false);

            // Act
            progressionManager.UnlockAbilities(player);

            // Assert - Confirmation should include ability name and cost
            _mockInterface.Verify(x => x.RequestConfirmation(
                It.Is<string>(s => s.Contains(lockedAbility.Name) && s.Contains(lockedAbility.PurchaseCost.ToString()))
            ), Times.Once);
        }

        #endregion

        #region Rest Tests

        [Fact]
        public void Rest_WithEnoughGold_RestoresHPAndMana()
        {
            // Arrange
            var progressionManager = new ProgressionManager(_mockInterface.Object);
            var player = new Player("Hero", PlayerClass.Warrior);
            player.Gold = 1000;

            // Damage player
            player.CurrentHP = player.MaxHP / 2;
            player.CurrentMana = player.MaxMana / 2;
            int initialGold = player.Gold;

            // Act
            bool result = progressionManager.Rest(player);

            // Assert
            Assert.True(result);
            Assert.Equal(player.MaxHP, player.CurrentHP);
            Assert.Equal(player.MaxMana, player.CurrentMana);
            Assert.True(player.Gold < initialGold); // Gold was spent

            // Verify success message
            _mockInterface.Verify(x => x.OnEvent(It.Is<GameEvents.InfoMessageEvent>(
                e => e.Message.Contains("Rested") && e.Type == GameEvents.MessageType.Success
            )), Times.Once);
        }

        [Fact]
        public void Rest_NotEnoughGold_ReturnsFalseAndShowsError()
        {
            // Arrange
            var progressionManager = new ProgressionManager(_mockInterface.Object);
            var player = new Player("Hero", PlayerClass.Warrior);
            player.Gold = 0; // No gold

            player.CurrentHP = player.MaxHP / 2;
            player.CurrentMana = player.MaxMana / 2;
            int hpBefore = player.CurrentHP;
            int manaBefore = player.CurrentMana;

            // Act
            bool result = progressionManager.Rest(player);

            // Assert
            Assert.False(result);
            Assert.Equal(hpBefore, player.CurrentHP); // HP unchanged
            Assert.Equal(manaBefore, player.CurrentMana); // Mana unchanged

            // Verify error message
            _mockInterface.Verify(x => x.OnEvent(It.Is<GameEvents.InfoMessageEvent>(
                e => e.Message.Contains("Not enough gold") && e.Type == GameEvents.MessageType.Error
            )), Times.Once);
        }

        [Fact]
        public void Rest_DeductsCorrectGoldAmount()
        {
            // Arrange
            var progressionManager = new ProgressionManager(_mockInterface.Object);
            var player = new Player("Hero", PlayerClass.Warrior);
            player.Gold = 1000;
            player.CurrentHP = player.MaxHP / 2;

            int initialGold = player.Gold;
            int expectedCost = GameConfig.Config.RestingCost;

            // Act
            progressionManager.Rest(player);

            // Assert
            Assert.Equal(initialGold - expectedCost, player.Gold);
        }

        [Fact]
        public void Rest_AlreadyFullHealth_StillWorks()
        {
            // Arrange
            var progressionManager = new ProgressionManager(_mockInterface.Object);
            var player = new Player("Hero", PlayerClass.Warrior);
            player.Gold = 1000;

            // Player already at full health
            player.CurrentHP = player.MaxHP;
            player.CurrentMana = player.MaxMana;
            int initialGold = player.Gold;

            // Act
            bool result = progressionManager.Rest(player);

            // Assert
            Assert.True(result);
            Assert.True(player.Gold < initialGold); // Still charged
            Assert.Equal(player.MaxHP, player.CurrentHP);
            Assert.Equal(player.MaxMana, player.CurrentMana);
        }

        [Fact]
        public void Rest_MessageIncludesRestoredAmounts()
        {
            // Arrange
            var progressionManager = new ProgressionManager(_mockInterface.Object);
            var player = new Player("Hero", PlayerClass.Warrior);
            player.Gold = 1000;

            player.CurrentHP = player.MaxHP / 2;
            player.CurrentMana = player.MaxMana / 2;
            int expectedHPRestore = player.MaxHP - player.CurrentHP;
            int expectedManaRestore = player.MaxMana - player.CurrentMana;

            // Act
            progressionManager.Rest(player);

            // Assert - Message should include restored amounts
            _mockInterface.Verify(x => x.OnEvent(It.Is<GameEvents.InfoMessageEvent>(
                e => e.Message.Contains($"HP +{expectedHPRestore}") && e.Message.Contains($"Mana +{expectedManaRestore}")
            )), Times.Once);
        }

        [Fact]
        public void Rest_ExactlyEnoughGold_Succeeds()
        {
            // Arrange
            var progressionManager = new ProgressionManager(_mockInterface.Object);
            var player = new Player("Hero", PlayerClass.Warrior);
            player.Gold = GameConfig.Config.RestingCost; // Exactly enough

            player.CurrentHP = player.MaxHP / 2;

            // Act
            bool result = progressionManager.Rest(player);

            // Assert
            Assert.True(result);
            Assert.Equal(0, player.Gold);
            Assert.Equal(player.MaxHP, player.CurrentHP);
        }

        [Fact]
        public void Rest_OneGoldShort_Fails()
        {
            // Arrange
            var progressionManager = new ProgressionManager(_mockInterface.Object);
            var player = new Player("Hero", PlayerClass.Warrior);
            player.Gold = GameConfig.Config.RestingCost - 1; // One short

            player.CurrentHP = player.MaxHP / 2;
            int hpBefore = player.CurrentHP;

            // Act
            bool result = progressionManager.Rest(player);

            // Assert
            Assert.False(result);
            Assert.Equal(hpBefore, player.CurrentHP); // HP unchanged
        }

        #endregion
    }
}
