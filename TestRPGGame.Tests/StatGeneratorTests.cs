using Xunit;
using TestRPGGame.Equipment;
using TestRPGGame.Equipment.StatGenerators;
using TestRPGGame.DataLoading;
using TestRPGGame.Combat;

namespace TestRPGGame.Tests
{
    /// <summary>
    /// Unit tests for individual stat generator strategies.
    /// These tests verify each generator in isolation.
    /// </summary>
    public class StatGeneratorTests : TestBase
    {
        // Test helpers
        private EquipmentItem CreateTestItem(EquipmentSlot slot, ItemRarity rarity, int level)
        {
            return new EquipmentItem
            {
                Slot = slot,
                Rarity = rarity,
                Level = level,
                Name = "Test Item"
            };
        }

        #region WeaponStatGenerator Tests

        [Fact]
        public void WeaponStatGenerator_WithData_GeneratesCorrectStats()
        {
            // Arrange
            var generator = new WeaponStatGenerator();
            var item = CreateTestItem(EquipmentSlot.Weapon, ItemRarity.Common, 5);
            var weaponPrefix = new WeaponPrefixData
            {
                Name = "Sharp",
                AttackMultiplier = 1.2,
                MagicMultiplier = 0.5,
                MinRarity = 0
            };
            var weaponType = new WeaponTypeData
            {
                Name = "Sword",
                AttackType = "Physical",
                AttackWeight = 1.0,
                MagicWeight = 0.0,
                SpeedBonus = 2
            };

            // Act
            generator.GenerateStats(item, 10, 1.0, weaponPrefix, weaponType, null);

            // Assert
            Assert.True(item.AttackBonus > 0, "Weapon should have attack bonus");
            Assert.Equal(2, item.SpeedBonus);
            Assert.Equal(AttackType.Physical, item.WeaponAttackType);
        }

        [Fact]
        public void WeaponStatGenerator_WithoutData_UsesFallback()
        {
            // Arrange
            var generator = new WeaponStatGenerator();
            var item = CreateTestItem(EquipmentSlot.Weapon, ItemRarity.Common, 5);

            // Act
            generator.GenerateStats(item, 10, 1.0, null, null, null);

            // Assert
            Assert.True(item.AttackBonus > 0, "Weapon should have attack bonus from fallback");
            Assert.NotNull(item.WeaponAttackType);
        }

        [Fact]
        public void WeaponStatGenerator_RareWeapon_GetsCritBonus()
        {
            // Arrange
            var generator = new WeaponStatGenerator();
            var item = CreateTestItem(EquipmentSlot.Weapon, ItemRarity.Rare, 5);

            // Act
            generator.GenerateStats(item, 10, 1.5, null, null, null);

            // Assert
            Assert.True(item.CritBonus > 0, "Rare weapon should have crit bonus");
            Assert.True(item.CritBonus >= 0.05 && item.CritBonus <= 0.20, "Crit bonus should be in range");
        }

        [Fact]
        public void WeaponStatGenerator_CommonWeapon_NoCritBonus()
        {
            // Arrange
            var generator = new WeaponStatGenerator();
            var item = CreateTestItem(EquipmentSlot.Weapon, ItemRarity.Common, 5);

            // Act
            generator.GenerateStats(item, 10, 1.0, null, null, null);

            // Assert
            Assert.Equal(0, item.CritBonus);
        }

        #endregion

        #region ArmorStatGenerator Tests

        [Fact]
        public void ArmorStatGenerator_WithData_GeneratesCorrectStats()
        {
            // Arrange
            var generator = new ArmorStatGenerator();
            var item = CreateTestItem(EquipmentSlot.Armor, ItemRarity.Common, 5);
            var armorPrefix = new ArmorPrefixData
            {
                Name = "Sturdy",
                DefenseMultiplier = 1.3,
                HPMultiplier = 1.2,
                MinRarity = 0
            };

            // Act
            generator.GenerateStats(item, 10, 1.0, null, null, armorPrefix);

            // Assert
            Assert.True(item.DefenseBonus > 0, "Armor should have defense bonus");
            Assert.True(item.HPBonus > 0, "Armor should have HP bonus");
        }

        [Fact]
        public void ArmorStatGenerator_WithoutData_UsesFallback()
        {
            // Arrange
            var generator = new ArmorStatGenerator();
            var item = CreateTestItem(EquipmentSlot.Armor, ItemRarity.Common, 5);

            // Act
            generator.GenerateStats(item, 10, 1.0, null, null, null);

            // Assert
            Assert.True(item.DefenseBonus > 0, "Armor should have defense bonus from fallback");
            Assert.True(item.HPBonus > 0, "Armor should have HP bonus from fallback");
        }

        [Fact]
        public void ArmorStatGenerator_HigherLevel_HigherStats()
        {
            // Arrange
            var generator = new ArmorStatGenerator();
            var lowLevelItem = CreateTestItem(EquipmentSlot.Armor, ItemRarity.Common, 5);
            var highLevelItem = CreateTestItem(EquipmentSlot.Armor, ItemRarity.Common, 15);

            // Act
            generator.GenerateStats(lowLevelItem, 10, 1.0, null, null, null);
            generator.GenerateStats(highLevelItem, 30, 1.0, null, null, null);

            // Assert
            Assert.True(highLevelItem.DefenseBonus > lowLevelItem.DefenseBonus, "Higher level should have higher defense");
            Assert.True(highLevelItem.HPBonus > lowLevelItem.HPBonus, "Higher level should have higher HP");
        }

        #endregion

        #region HelmetStatGenerator Tests

        [Fact]
        public void HelmetStatGenerator_GeneratesDefenseAndHP()
        {
            // Arrange
            var generator = new HelmetStatGenerator();
            var item = CreateTestItem(EquipmentSlot.Helmet, ItemRarity.Common, 5);

            // Act
            generator.GenerateStats(item, 10, 1.0, null, null, null);

            // Assert
            Assert.True(item.DefenseBonus > 0, "Helmet should have defense bonus");
            Assert.True(item.HPBonus > 0, "Helmet should have HP bonus");
        }

        [Fact]
        public void HelmetStatGenerator_SometimesGeneratesMana()
        {
            // Arrange
            var generator = new HelmetStatGenerator();
            bool foundMana = false;

            // Act - Generate multiple helmets
            for (int i = 0; i < 50; i++)
            {
                var item = CreateTestItem(EquipmentSlot.Helmet, ItemRarity.Common, 5);
                generator.GenerateStats(item, 10, 1.0, null, null, null);
                if (item.ManaBonus > 0)
                {
                    foundMana = true;
                    break;
                }
            }

            // Assert
            Assert.True(foundMana, "Some helmets should have mana bonus (33% chance)");
        }

        [Fact]
        public void HelmetStatGenerator_WithArmorPrefix_UsesMultipliers()
        {
            // Arrange
            var generator = new HelmetStatGenerator();
            var item = CreateTestItem(EquipmentSlot.Helmet, ItemRarity.Rare, 5);
            var armorPrefix = new ArmorPrefixData
            {
                Name = "Reinforced",
                DefenseMultiplier = 2.0,
                HPMultiplier = 2.0,
                MinRarity = 0
            };

            // Act
            generator.GenerateStats(item, 10, 1.0, null, null, armorPrefix);

            // Assert
            Assert.True(item.DefenseBonus > 5, "Helmet with prefix should have boosted defense");
            Assert.True(item.HPBonus > 5, "Helmet with prefix should have boosted HP");
        }

        #endregion

        #region BootsStatGenerator Tests

        [Fact]
        public void BootsStatGenerator_GeneratesDefenseAndSpeed()
        {
            // Arrange
            var generator = new BootsStatGenerator();
            var item = CreateTestItem(EquipmentSlot.Boots, ItemRarity.Common, 5);

            // Act
            generator.GenerateStats(item, 10, 1.0, null, null, null);

            // Assert
            Assert.True(item.DefenseBonus > 0, "Boots should have defense bonus");
            Assert.True(item.SpeedBonus > 0, "Boots should have speed bonus");
        }

        [Fact]
        public void BootsStatGenerator_SpeedHigherThanDefense()
        {
            // Arrange
            var generator = new BootsStatGenerator();
            var item = CreateTestItem(EquipmentSlot.Boots, ItemRarity.Common, 10);

            // Act
            generator.GenerateStats(item, 20, 1.0, null, null, null);

            // Assert
            // Boots have defense multiplier 0.5 and speed multiplier 0.6
            Assert.True(item.SpeedBonus > item.DefenseBonus, "Boots should prioritize speed over defense");
        }

        #endregion

        #region GlovesStatGenerator Tests

        [Fact]
        public void GlovesStatGenerator_GeneratesDefenseAttackAndSpeed()
        {
            // Arrange
            var generator = new GlovesStatGenerator();
            var item = CreateTestItem(EquipmentSlot.Gloves, ItemRarity.Common, 5);

            // Act
            generator.GenerateStats(item, 10, 1.0, null, null, null);

            // Assert
            Assert.True(item.DefenseBonus > 0, "Gloves should have defense bonus");
            Assert.True(item.AttackBonus > 0, "Gloves should have attack bonus");
            Assert.True(item.SpeedBonus > 0, "Gloves should have speed bonus");
        }

        [Fact]
        public void GlovesStatGenerator_BalancesOffensiveAndDefensive()
        {
            // Arrange
            var generator = new GlovesStatGenerator();
            var item = CreateTestItem(EquipmentSlot.Gloves, ItemRarity.Common, 10);

            // Act
            generator.GenerateStats(item, 20, 1.0, null, null, null);

            // Assert
            // Gloves: defense 0.4, attack 0.6, speed 0.3
            Assert.True(item.AttackBonus > item.DefenseBonus, "Gloves should favor attack over defense");
            Assert.True(item.AttackBonus > item.SpeedBonus, "Gloves should have highest attack");
        }

        #endregion

        #region RingStatGenerator Tests

        [Fact]
        public void RingStatGenerator_GeneratesVariedStats()
        {
            // Arrange
            var generator = new RingStatGenerator();
            var statsFound = new HashSet<string>();

            // Act - Generate many rings to find variety
            for (int i = 0; i < 100; i++)
            {
                var item = CreateTestItem(EquipmentSlot.Ring1, ItemRarity.Common, 5);
                generator.GenerateStats(item, 10, 1.0, null, null, null);

                if (item.AttackBonus > 0) statsFound.Add("Attack");
                if (item.DefenseBonus > 0) statsFound.Add("Defense");
                if (item.MagicBonus > 0) statsFound.Add("Magic");
                if (item.HPBonus > 0) statsFound.Add("HP");
            }

            // Assert - Rings should generate all 4 stat types across multiple generations
            Assert.True(statsFound.Count >= 3, $"Rings should generate varied stats (found {statsFound.Count}/4 types)");
        }

        [Fact]
        public void RingStatGenerator_OnlyOneStatAtATime()
        {
            // Arrange
            var generator = new RingStatGenerator();
            var item = CreateTestItem(EquipmentSlot.Ring1, ItemRarity.Common, 5);

            // Act
            generator.GenerateStats(item, 10, 1.0, null, null, null);

            // Assert - Rings should only have exactly ONE non-zero stat
            int nonZeroStats = 0;
            if (item.AttackBonus > 0) nonZeroStats++;
            if (item.DefenseBonus > 0) nonZeroStats++;
            if (item.MagicBonus > 0) nonZeroStats++;
            if (item.HPBonus > 0) nonZeroStats++;

            Assert.Equal(1, nonZeroStats);
        }

        #endregion

        #region AmuletStatGenerator Tests

        [Fact]
        public void AmuletStatGenerator_GeneratesHPAndMana()
        {
            // Arrange
            var generator = new AmuletStatGenerator();
            var item = CreateTestItem(EquipmentSlot.Amulet, ItemRarity.Common, 5);

            // Act
            generator.GenerateStats(item, 10, 1.0, null, null, null);

            // Assert
            Assert.True(item.HPBonus > 0, "Amulet should have HP bonus");
            Assert.True(item.ManaBonus > 0, "Amulet should have mana bonus");
        }

        [Fact]
        public void AmuletStatGenerator_RareAmulet_GetsCritBonus()
        {
            // Arrange
            var generator = new AmuletStatGenerator();
            var item = CreateTestItem(EquipmentSlot.Amulet, ItemRarity.Rare, 5);

            // Act
            generator.GenerateStats(item, 10, 1.5, null, null, null);

            // Assert
            Assert.True(item.CritBonus > 0, "Rare amulet should have crit bonus");
            Assert.True(item.CritBonus >= 0.03 && item.CritBonus <= 0.13, "Crit bonus should be in range");
        }

        [Fact]
        public void AmuletStatGenerator_CommonAmulet_NoCritBonus()
        {
            // Arrange
            var generator = new AmuletStatGenerator();
            var item = CreateTestItem(EquipmentSlot.Amulet, ItemRarity.Common, 5);

            // Act
            generator.GenerateStats(item, 10, 1.0, null, null, null);

            // Assert
            Assert.Equal(0, item.CritBonus);
        }

        #endregion

        #region RelicStatGenerator Tests

        [Fact]
        public void RelicStatGenerator_GeneratesMagicalStats()
        {
            // Arrange
            var generator = new RelicStatGenerator();
            var item = CreateTestItem(EquipmentSlot.Relic, ItemRarity.Common, 5);

            // Act
            generator.GenerateStats(item, 10, 1.0, null, null, null);

            // Assert
            Assert.True(item.MagicBonus > 0, "Relic should have magic bonus");
            Assert.True(item.ManaBonus > 0, "Relic should have mana bonus");
            Assert.True(item.AttackBonus > 0, "Relic should have attack bonus");
        }

        [Fact]
        public void RelicStatGenerator_PrioritizesMagic()
        {
            // Arrange
            var generator = new RelicStatGenerator();
            var item = CreateTestItem(EquipmentSlot.Relic, ItemRarity.Common, 10);

            // Act
            generator.GenerateStats(item, 20, 1.0, null, null, null);

            // Assert
            // Relic: magic 1.0, mana 2.5, attack 0.4
            Assert.True(item.ManaBonus > item.MagicBonus, "Relic should prioritize mana");
            Assert.True(item.MagicBonus > item.AttackBonus, "Relic should have higher magic than attack");
        }

        #endregion

        #region Strategy Pattern Dispatch Tests

        [Fact]
        public void EquipmentGenerator_UsesCorrectStrategyForWeapon()
        {
            // Arrange & Act
            var weapon = EquipmentGenerator.GenerateItem(5, EquipmentSlot.Weapon);

            // Assert
            Assert.True(weapon.AttackBonus > 0, "Weapon should use WeaponStatGenerator");
            Assert.NotNull(weapon.WeaponAttackType);
        }

        [Fact]
        public void EquipmentGenerator_UsesCorrectStrategyForArmor()
        {
            // Arrange & Act
            var armor = EquipmentGenerator.GenerateItem(5, EquipmentSlot.Armor);

            // Assert
            Assert.True(armor.DefenseBonus > 0, "Armor should use ArmorStatGenerator");
            Assert.True(armor.HPBonus > 0, "Armor should use ArmorStatGenerator");
        }

        [Fact]
        public void EquipmentGenerator_UsesCorrectStrategyForHelmet()
        {
            // Arrange & Act
            var helmet = EquipmentGenerator.GenerateItem(5, EquipmentSlot.Helmet);

            // Assert
            Assert.True(helmet.DefenseBonus > 0, "Helmet should use HelmetStatGenerator");
            Assert.True(helmet.HPBonus > 0, "Helmet should use HelmetStatGenerator");
        }

        [Fact]
        public void EquipmentGenerator_UsesCorrectStrategyForBoots()
        {
            // Arrange & Act
            var boots = EquipmentGenerator.GenerateItem(5, EquipmentSlot.Boots);

            // Assert
            Assert.True(boots.DefenseBonus > 0, "Boots should use BootsStatGenerator");
            Assert.True(boots.SpeedBonus > 0, "Boots should use BootsStatGenerator");
        }

        [Fact]
        public void EquipmentGenerator_UsesCorrectStrategyForGloves()
        {
            // Arrange & Act
            var gloves = EquipmentGenerator.GenerateItem(5, EquipmentSlot.Gloves);

            // Assert
            Assert.True(gloves.DefenseBonus > 0, "Gloves should use GlovesStatGenerator");
            Assert.True(gloves.AttackBonus > 0, "Gloves should use GlovesStatGenerator");
            Assert.True(gloves.SpeedBonus > 0, "Gloves should use GlovesStatGenerator");
        }

        [Fact]
        public void EquipmentGenerator_UsesCorrectStrategyForRing()
        {
            // Arrange & Act - Generate ring
            var ring = EquipmentGenerator.GenerateItem(5, EquipmentSlot.Ring1);

            // Assert - Ring should have exactly one non-zero stat
            int nonZeroStats = 0;
            if (ring.AttackBonus > 0) nonZeroStats++;
            if (ring.DefenseBonus > 0) nonZeroStats++;
            if (ring.MagicBonus > 0) nonZeroStats++;
            if (ring.HPBonus > 0) nonZeroStats++;

            Assert.Equal(1, nonZeroStats);
        }

        [Fact]
        public void EquipmentGenerator_UsesCorrectStrategyForAmulet()
        {
            // Arrange & Act
            var amulet = EquipmentGenerator.GenerateItem(5, EquipmentSlot.Amulet);

            // Assert
            Assert.True(amulet.HPBonus > 0, "Amulet should use AmuletStatGenerator");
            Assert.True(amulet.ManaBonus > 0, "Amulet should use AmuletStatGenerator");
        }

        [Fact]
        public void EquipmentGenerator_UsesCorrectStrategyForRelic()
        {
            // Arrange & Act
            var relic = EquipmentGenerator.GenerateItem(5, EquipmentSlot.Relic);

            // Assert
            Assert.True(relic.MagicBonus > 0, "Relic should use RelicStatGenerator");
            Assert.True(relic.ManaBonus > 0, "Relic should use RelicStatGenerator");
            Assert.True(relic.AttackBonus > 0, "Relic should use RelicStatGenerator");
        }

        #endregion
    }
}
