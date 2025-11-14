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

        [Fact]
        public void GetCharacterSheetInfo_OnlyShowsUnlockedLearnedAbilities()
        {
            // Arrange
            var player = new Player("Test", PlayerClass.Warrior);

            // Act
            var sheetInfo = player.GetCharacterSheetInfo();

            // Assert - Learned abilities should only contain unlocked non-equipment abilities
            foreach (var displayedAbility in sheetInfo.Abilities)
            {
                Assert.False(displayedAbility.Source != "",
                    $"Learned abilities list contains equipment ability: {displayedAbility.Name}");
            }

            // Verify the source data - player should have some locked abilities
            var lockedAbilities = player.Abilities.Where(a => !a.IsUnlocked && !a.IsEquipmentGranted).ToList();

            // None of the locked abilities should appear in the character sheet
            foreach (var lockedAbility in lockedAbilities)
            {
                bool foundInSheet = sheetInfo.Abilities.Any(a => a.Name == lockedAbility.Name);
                Assert.False(foundInSheet,
                    $"Locked ability '{lockedAbility.Name}' should not appear in character sheet");
            }
        }

        [Fact]
        public void GetCharacterSheetInfo_SeparatesEquipmentAbilities()
        {
            // Arrange
            var player = new Player("Test", PlayerClass.Warrior);

            // Act
            var sheetInfo = player.GetCharacterSheetInfo();

            // Assert - Equipment abilities should be in separate list
            var playerEquipmentAbilities = player.Abilities.Where(a => a.IsEquipmentGranted).ToList();

            Assert.Equal(playerEquipmentAbilities.Count, sheetInfo.EquipmentAbilities.Count);

            // All equipment abilities should have a source
            foreach (var equipAbility in sheetInfo.EquipmentAbilities)
            {
                Assert.NotNull(equipAbility.Source);
                Assert.NotEmpty(equipAbility.Source);
            }

            // Learned abilities should NOT contain any equipment abilities
            foreach (var learnedAbility in sheetInfo.Abilities)
            {
                bool isEquipmentAbility = playerEquipmentAbilities.Any(a => a.Name == learnedAbility.Name);
                Assert.False(isEquipmentAbility,
                    $"Equipment ability '{learnedAbility.Name}' should not be in learned abilities list");
            }
        }

        [Fact]
        public void GetCharacterSheetInfo_ShowsBaseStatsVsBonusStats()
        {
            // Arrange
            var player = new Player("Test", PlayerClass.Warrior);

            // Act
            var sheetInfo = player.GetCharacterSheetInfo();

            // Assert - Base stats should match class base stats (no equipment)
            var classData = new JsonDataRepository().GetClass("Warrior");
            Assert.Equal(classData.BaseMaxHP, sheetInfo.BaseStats.HP);
            Assert.Equal(classData.BaseMaxMana, sheetInfo.BaseStats.Mana);
            Assert.Equal(classData.BaseAttack, sheetInfo.BaseStats.Attack);
            Assert.Equal(classData.BaseDefense, sheetInfo.BaseStats.Defense);
            Assert.Equal(classData.BaseMagicPower, sheetInfo.BaseStats.MagicPower);
            Assert.Equal(classData.BaseSpeed, sheetInfo.BaseStats.Speed);
            Assert.Equal(classData.BaseCritChance, sheetInfo.BaseStats.CritChance);

            // Bonus stats should come from equipment
            var equipStats = player.Inventory.GetTotalStats();
            Assert.Equal(equipStats.HP, sheetInfo.BonusStats.HP);
            Assert.Equal(equipStats.Mana, sheetInfo.BonusStats.Mana);
            Assert.Equal(equipStats.Attack, sheetInfo.BonusStats.Attack);
            Assert.Equal(equipStats.Defense, sheetInfo.BonusStats.Defense);
            Assert.Equal(equipStats.Magic, sheetInfo.BonusStats.MagicPower);
            Assert.Equal(equipStats.Speed, sheetInfo.BonusStats.Speed);
            Assert.Equal(equipStats.Crit, sheetInfo.BonusStats.CritChance);

            // Total stats should equal base + bonus
            Assert.Equal(sheetInfo.BaseStats.HP + sheetInfo.BonusStats.HP, sheetInfo.MaxHP);
            Assert.Equal(sheetInfo.BaseStats.Mana + sheetInfo.BonusStats.Mana, sheetInfo.MaxMana);
            Assert.Equal(sheetInfo.BaseStats.Attack + sheetInfo.BonusStats.Attack, sheetInfo.Attack);
            Assert.Equal(sheetInfo.BaseStats.Defense + sheetInfo.BonusStats.Defense, sheetInfo.Defense);
            Assert.Equal(sheetInfo.BaseStats.MagicPower + sheetInfo.BonusStats.MagicPower, sheetInfo.MagicPower);
            Assert.Equal(sheetInfo.BaseStats.Speed + sheetInfo.BonusStats.Speed, sheetInfo.Speed);
            Assert.Equal(sheetInfo.BaseStats.CritChance + sheetInfo.BonusStats.CritChance, sheetInfo.CritChance);
        }

        [Fact]
        public void GetCharacterSheetInfo_EquipmentAbilities_ShowCorrectSource()
        {
            // Arrange
            var player = new Player("Test", PlayerClass.Warrior);

            // Manually verify that if the player has equipment abilities, they show the right source
            var weapon = player.Inventory.Weapon;
            var armor = player.Inventory.Armor;

            // Act
            var sheetInfo = player.GetCharacterSheetInfo();

            // Assert - Check each equipment ability's source
            foreach (var equipAbility in sheetInfo.EquipmentAbilities)
            {
                bool sourceIsWeapon = weapon != null && equipAbility.Source == weapon.Name;
                bool sourceIsArmor = armor != null && equipAbility.Source == armor.Name;
                bool sourceIsGeneric = equipAbility.Source == "Equipment";

                Assert.True(sourceIsWeapon || sourceIsArmor || sourceIsGeneric,
                    $"Equipment ability '{equipAbility.Name}' has invalid source: '{equipAbility.Source}'");

                // If weapon has this ability, source should be weapon name
                if (weapon != null && weapon.GrantedAbilityIds.Count > 0)
                {
                    bool weaponHasAbility = weapon.GrantedAbilityIds.Any(id =>
                        equipAbility.Name.Contains(id, System.StringComparison.OrdinalIgnoreCase) ||
                        id.Contains(equipAbility.Name, System.StringComparison.OrdinalIgnoreCase));

                    if (weaponHasAbility)
                    {
                        Assert.Equal(weapon.Name, equipAbility.Source);
                    }
                }
            }
        }

        [Fact]
        public void GetCharacterSheetInfo_AfterLevelUp_ShowsCorrectBaseStats()
        {
            // Arrange
            var player = new Player("Test", PlayerClass.Warrior);
            var initialSheetInfo = player.GetCharacterSheetInfo();
            int initialBaseHP = initialSheetInfo.BaseStats.HP;
            int initialBaseAttack = initialSheetInfo.BaseStats.Attack;

            // Act - Level up
            player.GainExperience(100);
            var newSheetInfo = player.GetCharacterSheetInfo();

            // Assert - Base stats should have increased (from level up)
            Assert.True(newSheetInfo.BaseStats.HP > initialBaseHP,
                "Base HP should increase after level up");
            Assert.True(newSheetInfo.BaseStats.Attack > initialBaseAttack,
                "Base Attack should increase after level up");

            // Bonus stats (equipment) should remain the same
            Assert.Equal(initialSheetInfo.BonusStats.HP, newSheetInfo.BonusStats.HP);
            Assert.Equal(initialSheetInfo.BonusStats.Attack, newSheetInfo.BonusStats.Attack);
        }
    }
}
