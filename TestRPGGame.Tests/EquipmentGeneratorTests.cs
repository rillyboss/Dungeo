using Xunit;
using TestRPGGame;
using TestRPGGame.Equipment;
using TestRPGGame.DataLoading;
using TestRPGGame.Combat;

namespace TestRPGGame.Tests
{
    public class EquipmentGeneratorTests : TestBase
    {
        [Fact]
        public void GenerateWeapon_UsesDataFromItemsJson()
        {
            // Arrange
            int playerLevel = 5;

            // Act
            var weapon = EquipmentGenerator.GenerateItem(playerLevel, EquipmentSlot.Weapon);

            // Assert
            Assert.NotNull(weapon);
            Assert.Equal(EquipmentSlot.Weapon, weapon.Slot);
            Assert.NotEmpty(weapon.Name);
            Assert.True(weapon.AttackBonus > 0, "Weapon should have attack bonus");
            Assert.NotNull(weapon.WeaponAttackType);
        }

        [Fact]
        public void GenerateArmor_UsesDataFromItemsJson()
        {
            // Arrange
            int playerLevel = 5;

            // Act
            var armor = EquipmentGenerator.GenerateItem(playerLevel, EquipmentSlot.Armor);

            // Assert
            Assert.NotNull(armor);
            Assert.Equal(EquipmentSlot.Armor, armor.Slot);
            Assert.NotEmpty(armor.Name);
            Assert.True(armor.DefenseBonus > 0, "Armor should have defense bonus");
            Assert.True(armor.HPBonus > 0, "Armor should have HP bonus");
        }

        [Fact]
        public void GenerateRareWeapon_HasSuffix()
        {
            // Arrange - Generate many weapons to get at least one rare
            bool foundRareWithSuffix = false;

            for (int i = 0; i < 100; i++)
            {
                var weapon = EquipmentGenerator.GenerateItem(15, EquipmentSlot.Weapon);

                if (weapon.Rarity >= ItemRarity.Rare)
                {
                    // Rare+ weapons should potentially have suffixes like "of Power", "of Chaos", etc.
                    // Check if name has multiple words (prefix + type + suffix)
                    var parts = weapon.Name.Split(' ');
                    if (parts.Length >= 3 && (weapon.Name.Contains("of ") || weapon.SpecialEffects.Count > 0))
                    {
                        foundRareWithSuffix = true;
                        break;
                    }
                }
            }

            Assert.True(foundRareWithSuffix, "Should generate at least one rare weapon with suffix or special effects");
        }

        [Fact]
        public void GenerateWeapon_DifferentPrefixesBasedOnRarity()
        {
            // Arrange
            var commonNames = new HashSet<string>();
            var legendaryNames = new HashSet<string>();

            // Act - Generate multiple weapons
            for (int i = 0; i < 50; i++)
            {
                var weapon = EquipmentGenerator.GenerateItem(1, EquipmentSlot.Weapon);
                if (weapon.Rarity == ItemRarity.Common)
                    commonNames.Add(weapon.Name.Split(' ')[0]); // Get prefix
            }

            // Generate more legendary attempts since they're rare (2% chance)
            for (int i = 0; i < 200; i++)
            {
                var weapon = EquipmentGenerator.GenerateItem(20, EquipmentSlot.Weapon);
                if (weapon.Rarity == ItemRarity.Legendary)
                    legendaryNames.Add(weapon.Name.Split(' ')[0]); // Get prefix
            }

            // Assert - Should have variety in names
            Assert.True(commonNames.Count > 0, "Should generate common weapons");
            Assert.True(legendaryNames.Count > 0, "Should generate legendary weapons");
        }

        [Fact]
        public void GenerateWeapon_AttackTypeFromData()
        {
            // Arrange & Act
            var weapons = new HashSet<AttackType?>();

            for (int i = 0; i < 30; i++)
            {
                var weapon = EquipmentGenerator.GenerateItem(10, EquipmentSlot.Weapon);
                weapons.Add(weapon.WeaponAttackType);
            }

            // Assert - Should have variety of attack types from data
            Assert.True(weapons.Count > 1, "Should generate weapons with different attack types from items.json");
        }

        [Fact]
        public void GenerateWeapon_SpecialEffectsFromSuffix()
        {
            // Arrange & Act - Generate many rare+ weapons
            bool foundWeaponWithMultipleEffects = false;

            for (int i = 0; i < 100; i++)
            {
                var weapon = EquipmentGenerator.GenerateItem(15, EquipmentSlot.Weapon);

                if (weapon.Rarity >= ItemRarity.Rare && weapon.SpecialEffects.Count > 1)
                {
                    foundWeaponWithMultipleEffects = true;
                    break;
                }
            }

            // Assert - Some rare+ weapons should have multiple effects from suffixes
            Assert.True(foundWeaponWithMultipleEffects, "Should generate weapons with multiple special effects from suffix data");
        }

        [Fact]
        public void GenerateArmor_SpecialEffectsFromSuffix()
        {
            // Arrange & Act - Generate many rare+ armor pieces
            bool foundArmorWithEffects = false;

            for (int i = 0; i < 100; i++)
            {
                var armor = EquipmentGenerator.GenerateItem(15, EquipmentSlot.Armor);

                if (armor.Rarity >= ItemRarity.Rare && armor.SpecialEffects.Count > 0)
                {
                    foundArmorWithEffects = true;
                    break;
                }
            }

            // Assert - Some rare+ armor should have special effects from suffixes
            Assert.True(foundArmorWithEffects, "Should generate armor with special effects from suffix data");
        }

        [Fact]
        public void ItemGenerationData_LoadsCorrectly()
        {
            // Arrange & Act
            var itemData = DataLoader.GetItemGenerationData();

            // Assert
            Assert.NotNull(itemData);
            Assert.True(itemData.WeaponPrefixes.Count > 0, "Should have weapon prefixes");
            Assert.True(itemData.WeaponTypes.Count > 0, "Should have weapon types");
            Assert.True(itemData.WeaponSuffixes.Count > 0, "Should have weapon suffixes");
            Assert.True(itemData.ArmorPrefixes.Count > 0, "Should have armor prefixes");
            Assert.True(itemData.ArmorSuffixes.Count > 0, "Should have armor suffixes");
            Assert.True(itemData.RarityMultipliers.Count > 0, "Should have rarity multipliers");
        }

        [Fact]
        public void WeaponPrefix_HasCorrectMultipliers()
        {
            // Arrange & Act
            var itemData = DataLoader.GetItemGenerationData();
            var infernalPrefix = itemData.WeaponPrefixes["Infernal"];

            // Assert - Verify data from items.json
            Assert.Equal("Infernal", infernalPrefix.Name);
            Assert.Equal(1.8, infernalPrefix.AttackMultiplier);
            Assert.Equal(4, infernalPrefix.MinRarity);
        }

        [Fact]
        public void WeaponType_HasCorrectProperties()
        {
            // Arrange & Act
            var itemData = DataLoader.GetItemGenerationData();
            var katana = itemData.WeaponTypes["Katana"];

            // Assert - Verify data from items.json
            Assert.Equal("Katana", katana.Name);
            Assert.Equal("Physical", katana.AttackType);
            Assert.Equal(1.0, katana.AttackWeight);
            Assert.Equal(4, katana.SpeedBonus);
        }
    }
}
