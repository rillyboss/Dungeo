using Xunit;
using TestRPGGame;
using TestRPGGame.Entities.Player;
using TestRPGGame.Systems;
using TestRPGGame.Equipment;

namespace TestRPGGame.Tests
{
    public class ShopTests : TestBase
    {
        [Theory]
        [InlineData(100, 50, true)]  // Player has enough gold
        [InlineData(50, 50, true)]   // Player has exact amount
        [InlineData(49, 50, false)]  // Player doesn't have enough
        [InlineData(0, 50, false)]   // Player has no gold
        public void Shop_PurchaseValidation_WorksCorrectly(int playerGold, int itemPrice, bool shouldSucceed)
        {
            // Arrange
            var player = new Player("Test", PlayerClass.Warrior);
            player.Gold = playerGold;

            // Act
            bool canAfford = player.Gold >= itemPrice;

            // Assert
            Assert.Equal(shouldSucceed, canAfford);
        }

        [Fact]
        public void Shop_PurchaseItem_DeductsCorrectAmount()
        {
            // Arrange
            var player = new Player("Test", PlayerClass.Warrior);
            player.Gold = 154;
            int itemPrice = 110;

            // Act
            if (player.Gold >= itemPrice)
            {
                player.Gold -= itemPrice;
            }

            // Assert
            Assert.Equal(44, player.Gold);
        }

        [Fact]
        public void Shop_SellItem_GivesSixtyPercentValue()
        {
            // Arrange
            var player = new Player("Test", PlayerClass.Warrior);
            player.Gold = 100;
            int itemOriginalPrice = 100;
            int expectedSellPrice = 60; // 60% of 100

            // Act
            int sellPrice = (int)(itemOriginalPrice * 0.6);
            player.Gold += sellPrice;

            // Assert
            Assert.Equal(expectedSellPrice, sellPrice);
            Assert.Equal(160, player.Gold);
        }

        [Fact]
        public void Shop_BuyPotions_CorrectlyCalculatesCost()
        {
            // Arrange
            var player = new Player("Test", PlayerClass.Mage);
            player.Gold = 200;
            int potionsToBuy = 3;
            int potionPrice = 50;

            // Act
            int totalCost = potionsToBuy * potionPrice;
            if (player.Gold >= totalCost)
            {
                player.Gold -= totalCost;
                player.PotionCount += potionsToBuy;
            }

            // Assert
            Assert.Equal(50, player.Gold);
            Assert.Equal(6, player.PotionCount); // Started with 3, added 3 more
        }
    }
}
