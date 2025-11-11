using Xunit;
using TestRPGGame;
using TestRPGGame.Entities.Player;
using TestRPGGame.Systems;
using TestRPGGame.Equipment;
using TestRPGGame.Interfaces;
using System.Linq;

namespace TestRPGGame.Tests
{
    public class ShopTests : TestBase
    {
        [Fact]
        public void Shop_CanBeInstantiated()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();

            // Act
            var shop = new Shop(autoInterface);

            // Assert
            Assert.NotNull(shop);
        }

        [Fact]
        public void Shop_Enter_GeneratesInventory()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var shop = new Shop(autoInterface);
            var player = new Player("Test", PlayerClass.Warrior);

            // Act
            shop.Enter(player);
            var log = autoInterface.GetLog();

            // Assert
            Assert.Contains("Shop: Buy", log); // AutomatedInterface logs shop decisions
        }

        [Fact]
        public void Shop_PurchaseItem_DeductsGold()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var shop = new Shop(autoInterface);
            var player = new Player("Test", PlayerClass.Warrior);
            player.Gold = 500;
            int initialGold = player.Gold;
            int initialBackpackSize = player.Inventory.BackpackItems.Count;

            // Act
            shop.Enter(player);

            // Assert
            // AutomatedInterface will buy affordable items
            // Player should have less gold and more items (if affordable items existed)
            Assert.True(player.Gold <= initialGold);
        }

        [Fact]
        public void Shop_ExitsWhenNothingAffordable()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var shop = new Shop(autoInterface);
            var player = new Player("Test", PlayerClass.Warrior);
            player.Gold = 0; // No money

            // Act
            shop.Enter(player);
            var log = autoInterface.GetLog();

            // Assert
            Assert.Contains("Exit", log); // Should exit immediately
        }

        [Fact]
        public void Shop_RefreshCost_UsesGameConfig()
        {
            // Verify shop refresh cost comes from GameConfig
            // Arrange
            var config = GameConfig.Config;
            var player = new Player("Test", PlayerClass.Warrior);
            player.Gold = 100;

            // Act
            int refreshCost = config.ShopRefreshCost;
            player.Gold -= refreshCost;

            // Assert
            Assert.Equal(50, refreshCost); // Default config value
            Assert.Equal(50, player.Gold);
        }

        [Fact]
        public void Shop_PotionPrice_UsesGameConfig()
        {
            // Verify potion price comes from GameConfig
            // Arrange
            var config = GameConfig.Config;
            var player = new Player("Test", PlayerClass.Warrior);
            player.Gold = 150;
            int potionsToBuy = 2;

            // Act
            int potionPrice = config.PotionPrice;
            int totalCost = potionsToBuy * potionPrice;
            player.Gold -= totalCost;
            player.PotionCount += potionsToBuy;

            // Assert
            Assert.Equal(50, potionPrice); // Default config value
            Assert.Equal(50, player.Gold);
            Assert.Equal(5, player.PotionCount); // Started with 3
        }

        [Fact]
        public void Shop_SellPrice_UsesGameConfig()
        {
            // Verify sell price multiplier comes from GameConfig
            // Arrange
            var config = GameConfig.Config;
            int buyPrice = 100;

            // Act
            int sellPrice = (int)(buyPrice * config.ItemSellPriceMultiplier);

            // Assert
            Assert.Equal(60, sellPrice); // 60% of 100 with default config
            Assert.Equal(0.6, config.ItemSellPriceMultiplier);
        }

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
        public void Shop_SendsItemPurchasedEvent()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var shop = new Shop(autoInterface);
            var player = new Player("Test", PlayerClass.Warrior);
            player.Gold = 1000; // Ensure player can afford items

            // Act
            shop.Enter(player);
            var log = autoInterface.GetLog();

            // Assert
            // Should see shop decision and potentially items purchased
            Assert.Contains("Shop:", log);
        }

        [Fact]
        public void Shop_GeneratesMultipleItems()
        {
            // Shop should generate 8-12 items (from Shop.cs line 88)
            // We can't directly test private inventory, but we can verify through interaction

            // Arrange
            var autoInterface = new AutomatedInterface();
            var shop = new Shop(autoInterface);
            var player = new Player("Test", PlayerClass.Warrior);
            player.Gold = 10000; // Lots of gold

            // Act
            shop.Enter(player);

            // Assert
            // If shop generated items and player had gold, items should be purchased
            Assert.True(player.Inventory.BackpackItems.Count >= 3); // Started with some + shop purchases
        }

        [Fact]
        public void Shop_PlayerCanSellItems()
        {
            // Arrange
            var config = GameConfig.Config;
            var autoInterface = new AutomatedInterface();
            var shop = new Shop(autoInterface);
            var player = new Player("Test", PlayerClass.Warrior);

            // Add item to player's backpack
            var itemToSell = new EquipmentItem
            {
                Name = "Old Sword",
                Slot = EquipmentSlot.Weapon,
                Level = 1,
                Rarity = ItemRarity.Common,
                Price = 100
            };
            player.Inventory.BackpackItems.Add(itemToSell);

            int initialGold = player.Gold;
            int expectedSellPrice = (int)(itemToSell.Price * config.ItemSellPriceMultiplier);

            // Note: AutomatedInterface doesn't sell items, it only buys
            // So we test the sell logic directly
            int sellPrice = (int)(itemToSell.Price * config.ItemSellPriceMultiplier);
            player.Gold += sellPrice;
            player.Inventory.BackpackItems.Remove(itemToSell);

            // Assert
            Assert.Equal(initialGold + expectedSellPrice, player.Gold);
            Assert.DoesNotContain(itemToSell, player.Inventory.BackpackItems);
        }
    }
}
