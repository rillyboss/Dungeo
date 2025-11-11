using Xunit;
using TestRPGGame.Entities.Player;
using TestRPGGame.Equipment;
using TestRPGGame.Interfaces;

namespace TestRPGGame.Tests
{
    public class PlayerInventoryTests : TestBase
    {
        [Fact]
        public void PlayerInventory_StartsWithEquipment()
        {
            // Arrange & Act
            var player = new Player("Test", PlayerClass.Warrior);

            // Assert
            Assert.NotNull(player.Inventory.Weapon);
            Assert.NotNull(player.Inventory.Armor);
        }

        [Fact]
        public void PlayerInventory_HasBackpack()
        {
            // Arrange & Act
            var player = new Player("Test", PlayerClass.Warrior);

            // Assert
            Assert.NotNull(player.Inventory.BackpackItems);
            Assert.IsType<List<EquipmentItem>>(player.Inventory.BackpackItems);
        }

        [Fact]
        public void PlayerInventory_CanAddItemsToBackpack()
        {
            // Arrange
            var player = new Player("Test", PlayerClass.Warrior);
            var newItem = new EquipmentItem
            {
                Name = "Test Sword",
                Slot = EquipmentSlot.Weapon,
                Level = 1,
                Rarity = ItemRarity.Common,
                AttackBonus = 10
            };

            // Act
            player.Inventory.BackpackItems.Add(newItem);

            // Assert
            Assert.Contains(newItem, player.Inventory.BackpackItems);
        }

        [Fact]
        public void PlayerInventory_ManageInventory_WithInterface()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var player = new Player("Test", PlayerClass.Warrior);
            player.Inventory.BackpackItems.Add(new EquipmentItem
            {
                Name = "Extra Sword",
                Slot = EquipmentSlot.Weapon,
                Level = 1,
                Rarity = ItemRarity.Common
            });

            // Act
            player.Inventory.ManageInventory(player, autoInterface);
            var log = autoInterface.GetLog();

            // Assert
            Assert.Contains("Inventory:", log); // AutomatedInterface logs its decisions
        }

        [Fact]
        public void PlayerInventory_CanSetWeaponDirectly()
        {
            // Arrange
            var player = new Player("Test", PlayerClass.Warrior);
            var newWeapon = new EquipmentItem
            {
                Name = "New Sword",
                Slot = EquipmentSlot.Weapon,
                Level = 1,
                Rarity = ItemRarity.Common,
                AttackBonus = 15
            };

            // Act
            player.Inventory.Weapon = newWeapon;

            // Assert
            Assert.Equal(newWeapon, player.Inventory.Weapon);
        }

        [Fact]
        public void PlayerInventory_CanSetArmorDirectly()
        {
            // Arrange
            var player = new Player("Test", PlayerClass.Warrior);
            var newArmor = new EquipmentItem
            {
                Name = "New Armor",
                Slot = EquipmentSlot.Armor,
                Level = 1,
                Rarity = ItemRarity.Common,
                DefenseBonus = 20
            };

            // Act
            player.Inventory.Armor = newArmor;

            // Assert
            Assert.Equal(newArmor, player.Inventory.Armor);
        }

        [Fact]
        public void PlayerInventory_HasAllEquipmentSlots()
        {
            // Arrange
            var player = new Player("Test", PlayerClass.Warrior);

            // Act - Access all slot properties
            var weapon = player.Inventory.Weapon;
            var armor = player.Inventory.Armor;
            var helmet = player.Inventory.Helmet;
            var boots = player.Inventory.Boots;
            var gloves = player.Inventory.Gloves;
            var ring1 = player.Inventory.Ring1;
            var ring2 = player.Inventory.Ring2;
            var amulet = player.Inventory.Amulet;
            var relic = player.Inventory.Relic;

            // Assert - Properties exist and are accessible
            Assert.NotNull(weapon);  // Warriors start with weapon
            Assert.NotNull(armor);   // Warriors start with armor
            // Other slots may be null initially
        }

        [Fact]
        public void PlayerInventory_BackpackStartsEmpty()
        {
            // Arrange & Act
            var inventory = new PlayerInventory();

            // Assert
            Assert.Empty(inventory.BackpackItems);
        }

        [Fact]
        public void PlayerInventory_CanHaveMultipleRings()
        {
            // Arrange
            var player = new Player("Test", PlayerClass.Warrior);
            var ring1 = new EquipmentItem { Name = "Ring 1", Slot = EquipmentSlot.Ring1, Level = 1, Rarity = ItemRarity.Common };
            var ring2 = new EquipmentItem { Name = "Ring 2", Slot = EquipmentSlot.Ring2, Level = 1, Rarity = ItemRarity.Common };

            // Act
            player.Inventory.Ring1 = ring1;
            player.Inventory.Ring2 = ring2;

            // Assert
            Assert.Equal(ring1, player.Inventory.Ring1);
            Assert.Equal(ring2, player.Inventory.Ring2);
            Assert.NotEqual(ring1, ring2);
        }
    }
}
