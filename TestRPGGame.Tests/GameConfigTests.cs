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

            // Assert - Old properties
            Assert.Equal(0.05, config.ManaRegenRate);
            Assert.Equal(0.0, config.HealthRegenRate);
            Assert.Equal(10, config.RestingCost);
            Assert.True(config.CombatAutosave);
            Assert.Equal(0.5, config.PotionHealPercent);
            Assert.Equal(5, config.AbilityUnlockLevel);

            // Assert - Phase 2: Shop properties
            Assert.Equal(50, config.ShopRefreshCost);
            Assert.Equal(50, config.PotionPrice);
            Assert.Equal(0.6, config.ItemSellPriceMultiplier);
            Assert.Equal(8, config.ShopInventoryMinItems);
            Assert.Equal(12, config.ShopInventoryMaxItems);

            // Assert - Phase 2: Combat properties
            Assert.Equal(100, config.MaxCombatTurns);
            Assert.Equal(0.4, config.CombatLootDropChance); // 40% chance for loot drop
            Assert.Equal(0.25, config.CombatGoldLossPercent);
            Assert.Equal(100, config.CombatGoldLossMax);

            // Assert - Phase 2: Dungeon properties
            Assert.Equal(0.25, config.DungeonGoldLossPercent);
            Assert.Equal(200, config.DungeonGoldLossMax);
            Assert.Equal(2, config.DungeonLootLevelBonus);
            Assert.Equal(0.5, config.DefeatHPRestorePercent);
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

        [Fact]
        public void GameConfig_ShopRefreshCost_IsUsedCorrectly()
        {
            // Verify that shop refresh cost comes from config
            var config = GameConfig.Config;
            int playerGold = 100;

            // Act
            int costToRefresh = config.ShopRefreshCost;
            bool canAfford = playerGold >= costToRefresh;

            // Assert
            Assert.Equal(50, costToRefresh);
            Assert.True(canAfford);
        }

        [Fact]
        public void GameConfig_ItemSellPrice_CalculatesCorrectly()
        {
            // Verify sell price multiplier from config
            var config = GameConfig.Config;
            int itemBuyPrice = 100;

            // Act
            int sellPrice = (int)(itemBuyPrice * config.ItemSellPriceMultiplier);

            // Assert
            Assert.Equal(60, sellPrice); // 60% of 100
        }

        [Fact]
        public void GameConfig_CombatGoldLoss_CalculatesCorrectly()
        {
            // Verify combat gold loss calculation
            var config = GameConfig.Config;
            int playerGold = 1000;

            // Act
            int goldLoss = (int)(playerGold * config.CombatGoldLossPercent);
            goldLoss = Math.Min(goldLoss, config.CombatGoldLossMax);

            // Assert
            Assert.Equal(100, goldLoss); // 25% of 1000 = 250, capped at 100
        }

        [Fact]
        public void GameConfig_DungeonGoldLoss_CalculatesCorrectly()
        {
            // Verify dungeon gold loss calculation
            var config = GameConfig.Config;
            int playerGold = 500;

            // Act
            int goldLoss = (int)(playerGold * config.DungeonGoldLossPercent);
            goldLoss = Math.Min(goldLoss, config.DungeonGoldLossMax);

            // Assert
            Assert.Equal(125, goldLoss); // 25% of 500, under cap of 200
        }

        [Fact]
        public void GameConfig_DefeatHPRestore_CalculatesCorrectly()
        {
            // Verify HP restore on defeat
            var config = GameConfig.Config;
            int maxHP = 200;

            // Act
            int restoredHP = (int)(maxHP * config.DefeatHPRestorePercent);

            // Assert
            Assert.Equal(100, restoredHP); // 50% of 200
        }

        [Theory]
        [InlineData(8, 12, true)]  // Min 8, Max 12
        [InlineData(12, 8, false)] // Invalid: min > max
        public void GameConfig_ShopInventoryRange_IsValid(int min, int max, bool shouldBeValid)
        {
            // Act
            bool isValid = min <= max && min > 0;

            // Assert
            Assert.Equal(shouldBeValid, isValid);
        }

        [Fact]
        public void GameConfig_AdditionalAbilityChances_AreCorrect()
        {
            // Arrange & Act
            var config = GameConfig.Config;

            // Assert - Equipment additional ability chances by rarity
            Assert.Equal(0.05, config.AdditionalAbilityChanceCommon);
            Assert.Equal(0.10, config.AdditionalAbilityChanceUncommon);
            Assert.Equal(0.25, config.AdditionalAbilityChanceRare);
            Assert.Equal(0.50, config.AdditionalAbilityChanceEpic);
            Assert.Equal(0.75, config.AdditionalAbilityChanceLegendary);
        }

        [Fact]
        public void GameConfig_RarityThresholds_AreCorrect()
        {
            // Arrange & Act
            var config = GameConfig.Config;

            // Assert - Equipment rarity drop thresholds
            Assert.Equal(98, config.RarityLegendaryThreshold);
            Assert.Equal(92, config.RarityEpicThreshold);
            Assert.Equal(80, config.RarityRareThreshold);
            Assert.Equal(30, config.RarityUncommonThreshold);
            Assert.Equal(0, config.RarityCommonThreshold);
        }

        [Fact]
        public void GameConfig_RarityThresholds_AreInDescendingOrder()
        {
            // Arrange & Act
            var config = GameConfig.Config;

            // Assert - Thresholds should be in descending order for proper rarity distribution
            Assert.True(config.RarityLegendaryThreshold > config.RarityEpicThreshold);
            Assert.True(config.RarityEpicThreshold > config.RarityRareThreshold);
            Assert.True(config.RarityRareThreshold > config.RarityUncommonThreshold);
            Assert.True(config.RarityUncommonThreshold >= config.RarityCommonThreshold);
        }

        [Fact]
        public void GameConfig_AbilityPurchasePriceMultiplier_IsReduced()
        {
            // Arrange & Act
            var config = GameConfig.Config;

            // Assert - Adjusted to 7.5x for balanced ability costs based on impact
            Assert.Equal(7.5, config.AbilityPurchasePriceMultiplier);
        }

        [Fact]
        public void GameConfig_EconomyMultipliers_HaveValidDefaults()
        {
            // Arrange & Act
            var config = GameConfig.Config;

            // Assert - Economy multipliers should default to 1.0 (neutral)
            Assert.Equal(1.0, config.CombatGoldMultiplier);
            Assert.Equal(1.0, config.DungeonGoldMultiplier);
            Assert.Equal(1.0, config.EquipmentPurchasePriceMultiplier);
            Assert.Equal(1.0, config.EquipmentSellPriceMultiplier);
            Assert.Equal(1.0, config.EnemyDamageMultiplier);
        }
    }
}
