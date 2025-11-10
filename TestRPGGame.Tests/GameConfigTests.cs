using Xunit;
using TestRPGGame;
using TestRPGGame.Systems;

namespace TestRPGGame.Tests
{
    public class GameConfigTests : TestBase
    {
        [Fact]
        public void GameConfig_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var config = GameConfig.Config;

            // Assert
            Assert.Equal(0.05, config.ManaRegenRate);
            Assert.Equal(0.0, config.HealthRegenRate);
            Assert.Equal(10, config.RestingCost);
            Assert.True(config.CombatAutosave);
        }

        [Fact]
        public void GameConfig_ManaRegen_CalculatesCorrectly()
        {
            // Arrange
            var config = GameConfig.Config;
            int maxMana = 100;

            // Act
            int regen = (int)(maxMana * config.ManaRegenRate);

            // Assert
            Assert.Equal(5, regen); // 5% of 100
        }

        [Fact]
        public void GameConfig_PotionHeal_CalculatesCorrectly()
        {
            // Arrange
            var config = GameConfig.Config;
            int maxHP = 100;

            // Act
            int healAmount = (int)(maxHP * config.PotionHealPercent);

            // Assert
            Assert.Equal(50, healAmount); // 50% of 100
        }
    }
}
