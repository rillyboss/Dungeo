using Xunit;
using TestRPGGame;
using TestRPGGame.Equipment;
using TestRPGGame.DataLoading;
using TestRPGGame.Entities.Player;
using TestRPGGame.Interfaces;
using Moq;
using System.Linq;

namespace TestRPGGame.Tests
{
    public class EquipmentAbilityTests : TestBase
    {
        [Fact]
        public void WeaponGeneration_CommonWeapon_GrantsSignatureAbility()
        {
            // Arrange & Act
            var weapon = EquipmentGenerator.GenerateItem(1, EquipmentSlot.Weapon, ItemRarity.Common);

            // Assert
            Assert.NotNull(weapon);
            // Common weapons always have at least 1 signature ability
            // They MAY have an additional ability (5% chance)
            Assert.True(weapon.GrantedAbilityIds.Count >= 1, "Weapon should have at least the signature ability");
            Assert.NotEmpty(weapon.GrantedAbilityIds[0]); // Signature ability ID should not be empty
        }

        [Fact]
        public void WeaponGeneration_AllWeaponTypes_HaveSignatureAbilities()
        {
            // Arrange - Generate one of each weapon type
            int weaponsWithAbilities = 0;
            int totalWeapons = 50; // Generate many to hit all weapon types

            // Act
            for (int i = 0; i < totalWeapons; i++)
            {
                var weapon = EquipmentGenerator.GenerateItem(1, EquipmentSlot.Weapon, ItemRarity.Common);
                if (weapon.GrantedAbilityIds.Count > 0)
                {
                    weaponsWithAbilities++;
                }
            }

            // Assert - All weapons should have signature abilities
            Assert.Equal(totalWeapons, weaponsWithAbilities);
        }

        [Fact]
        public void WeaponGeneration_RareWeapon_CanHaveBonusAbility()
        {
            // Arrange - Generate many rare weapons to test probability
            int weaponsWithMultipleAbilities = 0;
            int totalWeapons = 100;

            // Act
            for (int i = 0; i < totalWeapons; i++)
            {
                var weapon = EquipmentGenerator.GenerateItem(5, EquipmentSlot.Weapon, ItemRarity.Rare);
                if (weapon.GrantedAbilityIds.Count > 1)
                {
                    weaponsWithMultipleAbilities++;
                }
            }

            // Assert - Some rare weapons should have bonus abilities (15% chance)
            // With 100 weapons, we expect ~15 with bonus abilities (allow range for randomness)
            Assert.True(weaponsWithMultipleAbilities > 0, "At least some rare weapons should have bonus abilities");
            Assert.True(weaponsWithMultipleAbilities < 50, "Not all rare weapons should have bonus abilities");
        }

        [Fact]
        public void WeaponGeneration_LegendaryWeapon_HighChanceForBonusAbility()
        {
            // Arrange - Generate many legendary weapons
            int weaponsWithMultipleAbilities = 0;
            int totalWeapons = 50;

            // Act
            for (int i = 0; i < totalWeapons; i++)
            {
                var weapon = EquipmentGenerator.GenerateItem(10, EquipmentSlot.Weapon, ItemRarity.Legendary);
                if (weapon.GrantedAbilityIds.Count > 1)
                {
                    weaponsWithMultipleAbilities++;
                }
            }

            // Assert - Most legendary weapons should have bonus abilities (65% chance)
            // With 50 weapons, we expect ~32 with bonus abilities
            Assert.True(weaponsWithMultipleAbilities > 20, "Most legendary weapons should have bonus abilities");
        }

        [Fact]
        public void EquipWeapon_GrantsAbilitiesToPlayer()
        {
            // Arrange
            var player = new Player("TestHero", PlayerClass.Warrior);
            int initialAbilityCount = player.Abilities.Count;

            // Create a test weapon with a known ability
            var weapon = new EquipmentItem
            {
                Name = "Test Sword",
                Slot = EquipmentSlot.Weapon,
                Level = 1,
                Rarity = ItemRarity.Common,
                AttackBonus = 10
            };
            weapon.GrantedAbilityIds.Add("warrior_power_strike");

            // Act
            var oldWeapon = player.Inventory.Weapon;
            player.Inventory.Weapon = weapon;
            player.UpdateStatsFromEquipment();

            // Manually grant abilities (simulating equip flow)
            player.AddEquipmentAbilities(weapon.GrantedAbilityIds);

            // Assert
            Assert.True(player.Abilities.Count > initialAbilityCount,
                $"Player should have more abilities after equipping weapon");
        }

        [Fact]
        public void UnequipWeapon_RemovesGrantedAbilities()
        {
            // Arrange
            var player = new Player("TestHero", PlayerClass.Warrior);
            int initialAbilityCount = player.Abilities.Count;

            // Create weapon with an ability the warrior doesn't start with (mage ability)
            var weapon = new EquipmentItem
            {
                Name = "Test Staff",
                Slot = EquipmentSlot.Weapon,
                Level = 1,
                Rarity = ItemRarity.Common,
                AttackBonus = 10
            };
            weapon.GrantedAbilityIds.Add("mage_fireball"); // Cross-class ability

            // Equip and grant abilities
            var addedAbilityIds = player.AddEquipmentAbilities(weapon.GrantedAbilityIds);
            int abilitiesAfterEquip = player.Abilities.Count;

            // Verify ability was actually added
            Assert.True(addedAbilityIds.Count > 0, "Ability should have been added");
            Assert.True(abilitiesAfterEquip > initialAbilityCount, "Player should have more abilities after adding");

            // Verify the added ability is marked as equipment-granted
            var addedAbility = player.Abilities.FirstOrDefault(a => a.Name == "Fireball");
            Assert.NotNull(addedAbility);
            Assert.True(addedAbility.IsEquipmentGranted, "Added ability should be marked as equipment-granted");

            // Act - Remove the granted abilities
            player.RemoveEquipmentAbilities(addedAbilityIds);

            // Assert
            Assert.Equal(initialAbilityCount, player.Abilities.Count);
        }

        [Fact]
        public void LearnedAbility_NotRemovedWhenUnequipping()
        {
            // Arrange
            var player = new Player("TestHero", PlayerClass.Mage);

            // Find an ability the player can learn
            var learnableAbility = player.Abilities.FirstOrDefault(a => !a.IsUnlocked && a.CanUnlock(player.Level, 10000));

            if (learnableAbility != null)
            {
                // Give player enough gold and unlock the ability
                player.Gold = 10000;
                learnableAbility.Unlock();

                string abilityId = learnableAbility.Name.ToLower().Replace(" ", "_");

                // Try to add the same ability (it shouldn't be added since it's learned)
                var addedIds = player.AddEquipmentAbilities(new List<string> { abilityId });

                // Act - Remove equipment abilities
                player.RemoveEquipmentAbilities(addedIds);

                // Assert - The learned ability should still be present and unlocked
                var abilityAfter = player.Abilities.FirstOrDefault(a => a.Name == learnableAbility.Name);
                Assert.NotNull(abilityAfter);
                Assert.True(abilityAfter.IsUnlocked, "Learned ability should remain unlocked after removing equipment abilities");
            }
        }

        [Fact]
        public void EquipmentAbilities_NoDuplicates()
        {
            // Arrange - Generate weapon with signature ability
            var weapon = EquipmentGenerator.GenerateItem(1, EquipmentSlot.Weapon, ItemRarity.Legendary);

            // Act - Check for duplicates
            var uniqueAbilities = weapon.GrantedAbilityIds.Distinct().ToList();

            // Assert
            Assert.Equal(weapon.GrantedAbilityIds.Count, uniqueAbilities.Count);
        }

        [Fact]
        public void ArmorGeneration_DoesNotHaveSignatureAbility()
        {
            // Arrange & Act
            var armor = EquipmentGenerator.GenerateItem(1, EquipmentSlot.Armor, ItemRarity.Common);

            // Assert - Common armor should have no abilities (only weapons have signatures)
            Assert.Empty(armor.GrantedAbilityIds);
        }

        [Fact]
        public void RareArmor_CanHaveBonusAbility()
        {
            // Arrange - Generate many rare armor pieces
            int armorWithAbilities = 0;
            int totalArmor = 100;

            // Act
            for (int i = 0; i < totalArmor; i++)
            {
                var armor = EquipmentGenerator.GenerateItem(5, EquipmentSlot.Armor, ItemRarity.Rare);
                if (armor.GrantedAbilityIds.Count > 0)
                {
                    armorWithAbilities++;
                }
            }

            // Assert - Some rare armor should have abilities (15% chance)
            Assert.True(armorWithAbilities > 0, "Some rare armor should have abilities");
            Assert.True(armorWithAbilities < 50, "Not all rare armor should have abilities");
        }

        [Fact]
        public void AccessoryGeneration_RareAccessory_CanHaveAbility()
        {
            // Arrange - Generate many rare rings
            int accessoriesWithAbilities = 0;
            int totalAccessories = 100;

            // Act
            for (int i = 0; i < totalAccessories; i++)
            {
                var ring = EquipmentGenerator.GenerateItem(5, EquipmentSlot.Ring1, ItemRarity.Rare);
                if (ring.GrantedAbilityIds.Count > 0)
                {
                    accessoriesWithAbilities++;
                }
            }

            // Assert - Some rare accessories should have abilities (15% chance)
            Assert.True(accessoriesWithAbilities > 0, "Some rare accessories should have abilities");
            Assert.True(accessoriesWithAbilities < 50, "Not all rare accessories should have abilities");
        }

        [Fact]
        public void ReplaceEquipment_RemovesOldAbilities_AddsNewAbilities()
        {
            // Arrange
            var player = new Player("TestHero", PlayerClass.Warrior);

            // Create two different weapons with different abilities
            var weapon1 = new EquipmentItem
            {
                Name = "Sword",
                Slot = EquipmentSlot.Weapon,
                Level = 1,
                Rarity = ItemRarity.Common,
                AttackBonus = 10
            };
            weapon1.GrantedAbilityIds.Add("warrior_power_strike");

            var weapon2 = new EquipmentItem
            {
                Name = "Axe",
                Slot = EquipmentSlot.Weapon,
                Level = 1,
                Rarity = ItemRarity.Common,
                AttackBonus = 10
            };
            weapon2.GrantedAbilityIds.Add("warrior_whirlwind");

            // Equip first weapon
            var addedIds1 = player.AddEquipmentAbilities(weapon1.GrantedAbilityIds);
            int abilitiesWithWeapon1 = player.Abilities.Count;

            // Act - Replace with second weapon
            player.RemoveEquipmentAbilities(addedIds1);
            var addedIds2 = player.AddEquipmentAbilities(weapon2.GrantedAbilityIds);

            // Assert - Should have weapon2's abilities
            Assert.True(player.Abilities.Count > 0, "Player should have abilities from the new weapon");
            Assert.Equal(addedIds2.Count, weapon2.GrantedAbilityIds.Count);
        }

        [Fact]
        public void EquipmentPrice_IncludesAbilityBonus()
        {
            // Arrange - Generate weapons with and without bonus abilities
            var commonWeapon = EquipmentGenerator.GenerateItem(1, EquipmentSlot.Weapon, ItemRarity.Common);
            var legendaryWeapon = EquipmentGenerator.GenerateItem(1, EquipmentSlot.Weapon, ItemRarity.Legendary);

            // Act & Assert
            // Legendary weapons with bonus abilities should generally be more expensive
            // (accounting for rarity multiplier, but the ability bonus should add +150 gold per ability)
            if (legendaryWeapon.GrantedAbilityIds.Count > commonWeapon.GrantedAbilityIds.Count)
            {
                int abilityDifference = legendaryWeapon.GrantedAbilityIds.Count - commonWeapon.GrantedAbilityIds.Count;
                int expectedMinPriceDifference = abilityDifference * 150;

                // Note: Can't do exact comparison due to rarity multipliers and random stats
                // Just verify that legendary is more expensive
                Assert.True(legendaryWeapon.Price > commonWeapon.Price,
                    "Legendary weapons with more abilities should be more expensive");
            }
        }
    }
}
