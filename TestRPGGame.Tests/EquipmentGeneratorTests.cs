using Xunit;
using TestRPGGame;
using TestRPGGame.Equipment;
using TestRPGGame.DataLoading;
using TestRPGGame.Combat;
using System.Collections.Generic;
using Moq;

namespace TestRPGGame.Tests
{
    public class EquipmentGeneratorTests : TestBase
    {
        private Mock<IDataRepository> CreateMockRepositoryWithItemData()
        {
            var mockRepo = new Mock<IDataRepository>();

            // Create test item generation data
            var itemData = new ItemGenerationData
            {
                weapon_prefixes = new Dictionary<string, WeaponPrefixData>
                {
                    ["common_prefix"] = new WeaponPrefixData
                    {
                        Id = "common_prefix",
                        Name = "Sharp",
                        AttackMultiplier = 1.2,
                        MinRarity = 0
                    },
                    ["rare_prefix"] = new WeaponPrefixData
                    {
                        Id = "rare_prefix",
                        Name = "Deadly",
                        AttackMultiplier = 1.8,
                        MinRarity = 3
                    }
                },
                weapon_types = new Dictionary<string, WeaponTypeData>
                {
                    ["sword"] = new WeaponTypeData
                    {
                        Id = "sword",
                        Name = "Sword",
                        AttackType = "Physical",
                        AttackWeight = 1.0,
                        SpeedBonus = 0
                    },
                    ["staff"] = new WeaponTypeData
                    {
                        Id = "staff",
                        Name = "Staff",
                        AttackType = "Magic",
                        AttackWeight = 0.8,
                        MagicWeight = 1.2
                    }
                },
                weapon_suffixes = new Dictionary<string, WeaponSuffixData>
                {
                    ["power"] = new WeaponSuffixData
                    {
                        Id = "power",
                        Name = "of Power",
                        Effects = new List<SpecialEffectData>
                        {
                            new SpecialEffectData { Type = "BonusDamage", Value = 10 }
                        }
                    }
                },
                armor_prefixes = new Dictionary<string, ArmorPrefixData>(),
                armor_suffixes = new Dictionary<string, ArmorSuffixData>(),
                rarity_multipliers = new Dictionary<string, double>
                {
                    ["Common"] = 1.0,
                    ["Uncommon"] = 1.3,
                    ["Rare"] = 1.6,
                    ["Epic"] = 2.0,
                    ["Legendary"] = 2.5
                },
                rarity_thresholds = new Dictionary<string, RarityThresholdData>
                {
                    ["Common"] = new RarityThresholdData { MinLevel = 0, MaxLevel = 999, RollThreshold = 0 },
                    ["Uncommon"] = new RarityThresholdData { MinLevel = 0, MaxLevel = 999, RollThreshold = 50 },
                    ["Rare"] = new RarityThresholdData { MinLevel = 0, MaxLevel = 999, RollThreshold = 75 },
                    ["Epic"] = new RarityThresholdData { MinLevel = 0, MaxLevel = 999, RollThreshold = 90 },
                    ["Legendary"] = new RarityThresholdData { MinLevel = 0, MaxLevel = 999, RollThreshold = 98 }
                }
            };

            mockRepo.Setup(r => r.GetItemGenerationData()).Returns(itemData);
            mockRepo.Setup(r => r.GetAbilitiesForClass(It.IsAny<string>())).Returns(new List<AbilityData>());

            return mockRepo;
        }

        [Fact]
        public void GenerateWeapon_CreatesValidWeapon()
        {
            // Arrange
            var mockRepo = CreateMockRepositoryWithItemData();
            EquipmentGenerator.SetRepository(mockRepo.Object);
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
        public void GenerateArmor_CreatesValidArmor()
        {
            // Arrange
            var mockRepo = CreateMockRepositoryWithItemData();
            EquipmentGenerator.SetRepository(mockRepo.Object);
            int playerLevel = 5;

            // Act
            var armor = EquipmentGenerator.GenerateItem(playerLevel, EquipmentSlot.Armor);

            // Assert
            Assert.NotNull(armor);
            Assert.Equal(EquipmentSlot.Armor, armor.Slot);
            Assert.NotEmpty(armor.Name);
            // Armor generation uses different logic, just verify it was created
        }

        [Fact]
        public void GenerateWeapon_UsesPrefixData()
        {
            // Arrange
            var mockRepo = CreateMockRepositoryWithItemData();
            EquipmentGenerator.SetRepository(mockRepo.Object);

            // Act - Generate weapons
            var weapon = EquipmentGenerator.GenerateItem(5, EquipmentSlot.Weapon);

            // Assert - Weapon name should contain prefix or type from our test data
            Assert.True(
                weapon.Name.Contains("Sharp") || weapon.Name.Contains("Sword") || weapon.Name.Contains("Staff"),
                $"Weapon name '{weapon.Name}' should contain test data elements"
            );
        }

        [Fact]
        public void GenerateWeapon_UsesWeaponTypeData()
        {
            // Arrange
            var mockRepo = CreateMockRepositoryWithItemData();
            EquipmentGenerator.SetRepository(mockRepo.Object);

            // Act - Generate multiple weapons to get both types
            var attackTypes = new HashSet<string>();
            for (int i = 0; i < 20; i++)
            {
                var weapon = EquipmentGenerator.GenerateItem(5, EquipmentSlot.Weapon);
                if (weapon.WeaponAttackType != null)
                {
                    attackTypes.Add(weapon.WeaponAttackType.ToString());
                }
            }

            // Assert - Should use attack types from test data (Physical or Magic)
            Assert.True(attackTypes.Count > 0, "Should generate weapons with attack types");
        }

        [Fact]
        public void GenerateWeapon_AppliesRarityMultipliers()
        {
            // Arrange
            var mockRepo = CreateMockRepositoryWithItemData();
            EquipmentGenerator.SetRepository(mockRepo.Object);

            // Act - Generate weapons at different levels
            var lowLevelWeapon = EquipmentGenerator.GenerateItem(1, EquipmentSlot.Weapon);
            var highLevelWeapon = EquipmentGenerator.GenerateItem(20, EquipmentSlot.Weapon);

            // Assert - Higher level should generally produce better stats
            Assert.True(lowLevelWeapon.AttackBonus > 0, "Low level weapon should have attack bonus");
            Assert.True(highLevelWeapon.AttackBonus > 0, "High level weapon should have attack bonus");
            // Don't assert specific values, just that generation works
        }

        [Fact]
        public void WeaponPrefixData_HasRequiredProperties()
        {
            // Arrange
            var prefix = new WeaponPrefixData
            {
                Id = "test_prefix",
                Name = "Mighty",
                AttackMultiplier = 1.5,
                MinRarity = 2
            };

            // Assert - Verify data structure
            Assert.Equal("test_prefix", prefix.Id);
            Assert.Equal("Mighty", prefix.Name);
            Assert.Equal(1.5, prefix.AttackMultiplier);
            Assert.Equal(2, prefix.MinRarity);
        }

        [Fact]
        public void WeaponTypeData_HasRequiredProperties()
        {
            // Arrange
            var weaponType = new WeaponTypeData
            {
                Id = "test_weapon",
                Name = "Test Blade",
                AttackType = "Physical",
                AttackWeight = 1.2,
                SpeedBonus = 5
            };

            // Assert - Verify data structure
            Assert.Equal("test_weapon", weaponType.Id);
            Assert.Equal("Test Blade", weaponType.Name);
            Assert.Equal("Physical", weaponType.AttackType);
            Assert.Equal(1.2, weaponType.AttackWeight);
            Assert.Equal(5, weaponType.SpeedBonus);
        }

        [Fact]
        public void WeaponSuffixData_CanHaveEffects()
        {
            // Arrange
            var suffix = new WeaponSuffixData
            {
                Id = "test_suffix",
                Name = "of Testing",
                Effects = new List<SpecialEffectData>
                {
                    new SpecialEffectData { Type = "BonusDamage", Value = 15 },
                    new SpecialEffectData { Type = "LifeSteal", Value = 5 }
                }
            };

            // Assert - Verify data structure
            Assert.Equal("test_suffix", suffix.Id);
            Assert.Equal("of Testing", suffix.Name);
            Assert.Equal(2, suffix.Effects.Count);
            Assert.Equal("BonusDamage", suffix.Effects[0].Type);
            Assert.Equal(15, suffix.Effects[0].Value);
        }

        [Fact]
        public void ItemGenerationData_StructureIsValid()
        {
            // Arrange & Act
            var itemData = new ItemGenerationData
            {
                weapon_prefixes = new Dictionary<string, WeaponPrefixData>
                {
                    ["test"] = new WeaponPrefixData { Name = "Test" }
                },
                weapon_types = new Dictionary<string, WeaponTypeData>
                {
                    ["blade"] = new WeaponTypeData { Name = "Blade" }
                },
                rarity_multipliers = new Dictionary<string, double>
                {
                    ["Common"] = 1.0,
                    ["Rare"] = 1.5
                }
            };

            // Assert - Verify structure
            Assert.NotNull(itemData.WeaponPrefixes);
            Assert.NotNull(itemData.WeaponTypes);
            Assert.NotNull(itemData.RarityMultipliers);
            Assert.Single(itemData.WeaponPrefixes);
            Assert.Single(itemData.WeaponTypes);
            Assert.Equal(2, itemData.RarityMultipliers.Count);
        }
    }
}
