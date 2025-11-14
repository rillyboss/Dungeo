using Xunit;
using TestRPGGame;
using TestRPGGame.Entities.Player;
using TestRPGGame.Abilities;
using TestRPGGame.DataLoading;
using System.Linq;

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

            // 30 class abilities + 1 from starting weapon signature ability
            Assert.True(player.Abilities.Count >= 30,
                $"Expected at least 30 abilities, got {player.Abilities.Count}");

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

        [Fact]
        public void Player_StartingEquipment_GrantsSignatureAbilities()
        {
            // Arrange & Act - Create warriors multiple times to ensure we get a weapon
            Player? player = null;
            for (int i = 0; i < 5; i++)
            {
                player = new Player("Test Warrior", PlayerClass.Warrior);

                // All warriors should get a weapon
                if (player.Inventory.Weapon != null)
                    break;
            }

            // Assert - Verify we got a weapon
            Assert.NotNull(player);
            Assert.NotNull(player.Inventory.Weapon);

            // Check if weapon has granted abilities defined
            var weapon = player.Inventory.Weapon;
            if (weapon.GrantedAbilityIds.Count > 0)
            {
                // BUG TEST: Weapon has abilities defined, but are they granted to the player?
                var equipmentAbilities = player.Abilities.Where(a => a.IsEquipmentGranted).ToList();

                Assert.True(equipmentAbilities.Count > 0,
                    $"BUG FOUND: Weapon '{weapon.Name}' has {weapon.GrantedAbilityIds.Count} granted abilities " +
                    $"({string.Join(", ", weapon.GrantedAbilityIds)}), but player has 0 equipment-granted abilities!");

                // Verify at least one of the weapon's abilities is in the player's ability list
                bool hasWeaponAbility = weapon.GrantedAbilityIds.Any(id =>
                    equipmentAbilities.Any(a =>
                        a.Name.Equals(id, System.StringComparison.OrdinalIgnoreCase) ||
                        id.Contains(a.Name.Replace(" ", "_").ToLower())));

                Assert.True(hasWeaponAbility,
                    $"Weapon grants abilities {string.Join(", ", weapon.GrantedAbilityIds)} " +
                    $"but none found in player's equipment abilities: {string.Join(", ", equipmentAbilities.Select(a => a.Name))}");
            }
        }
    }
}
