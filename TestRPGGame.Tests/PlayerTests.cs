using Xunit;
using TestRPGGame;
using TestRPGGame.Entities.Player;
using TestRPGGame.Abilities;
using TestRPGGame.DataLoading;

namespace TestRPGGame.Tests
{
    public class PlayerTests : TestBase
    {
        [Fact]
        public void Player_CreatesWithCorrectStats_Warrior()
        {
            // Arrange & Act
            var player = new Player("Test Warrior", PlayerClass.Warrior);

            // Assert - Stats now come from classes.json + equipment bonuses
            Assert.Equal("Test Warrior", player.Name);
            Assert.Equal(PlayerClass.Warrior, player.Class);
            Assert.Equal(1, player.Level);
            Assert.True(player.MaxHP >= 140); // Base 140 + equipment bonuses
            Assert.True(player.MaxMana >= 80); // Base 80 + possible equipment bonuses
            Assert.Equal(100, player.Gold);
            Assert.Equal(30, player.Abilities.Count); // 3 starting + 27 unlockable (Phase 5C: 5x expansion)

            // Verify starting equipment was given
            Assert.NotNull(player.Inventory.Weapon);
            Assert.NotNull(player.Inventory.Armor);
        }

        [Fact]
        public void Player_GainExperience_LevelsUp()
        {
            // Arrange
            var player = new Player("Test", PlayerClass.Warrior);
            int initialLevel = player.Level;
            int initialMaxHP = player.MaxHP;

            // Act
            bool leveledUp = player.GainExperience(100);

            // Assert
            Assert.True(leveledUp);
            Assert.Equal(initialLevel + 1, player.Level);
            Assert.True(player.MaxHP > initialMaxHP);
        }

        [Fact]
        public void Player_UsePotion_RestoresHealth()
        {
            // Arrange
            var player = new Player("Test", PlayerClass.Mage);
            player.CurrentHP = 10;
            int initialPotionCount = player.PotionCount;

            // Act
            bool used = player.UsePotion();

            // Assert
            Assert.True(used);
            Assert.Equal(initialPotionCount - 1, player.PotionCount);
            Assert.True(player.CurrentHP > 10);
        }

        [Fact]
        public void Player_UsePotion_FailsWhenNoPotions()
        {
            // Arrange
            var player = new Player("Test", PlayerClass.Rogue);
            player.PotionCount = 0;

            // Act
            bool used = player.UsePotion();

            // Assert
            Assert.False(used);
        }

        [Fact]
        public void Player_Heal_DoesNotExceedMaxHP()
        {
            // Arrange
            var player = new Player("Test", PlayerClass.Warrior);
            player.CurrentHP = player.MaxHP - 10;

            // Act
            player.Heal(50);

            // Assert
            Assert.Equal(player.MaxHP, player.CurrentHP);
        }

        [Fact]
        public void Player_RestoreMana_DoesNotExceedMaxMana()
        {
            // Arrange
            var player = new Player("Test", PlayerClass.Mage);
            player.CurrentMana = player.MaxMana - 10;

            // Act
            player.RestoreMana(50);

            // Assert
            Assert.Equal(player.MaxMana, player.CurrentMana);
        }
    }
}
