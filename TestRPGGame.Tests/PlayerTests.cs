using Xunit;
using TestRPGGame;
using TestRPGGame.Entities.Player;
using TestRPGGame.Abilities;
using TestRPGGame.DataLoading;
using System.Linq;
using Moq;

namespace TestRPGGame.Tests
{
    public class PlayerTests : TestBase
    {
        [Fact]
        public void Player_CreatesWithCorrectStats()
        {
            // Arrange & Act - Use TestFixtures with known stats
            var player = TestFixtures.CreateWarriorPlayer("Test Warrior", level: 1);

            // Assert - Verify exact stats from test fixture
            Assert.Equal("Test Warrior", player.Name);
            Assert.Equal(1, player.Level);
            Assert.Equal(140, player.MaxHP); // Known from TestFixtures
            Assert.Equal(140, player.CurrentHP);
            Assert.Equal(80, player.MaxMana); // Known from TestFixtures
            Assert.Equal(80, player.CurrentMana);
            Assert.Equal(100, player.Gold);
        }

        [Fact]
        public void Player_GainExperience_LevelsUp()
        {
            // Arrange - Use TestFixtures with known stats
            var player = TestFixtures.CreateWarriorPlayer("Test", level: 1);
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
            // Arrange - Use TestFixtures
            var player = TestFixtures.CreateMagePlayer("Test", level: 1);
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
            // Arrange - Use TestFixtures
            var player = TestFixtures.CreateTestPlayer("Test", level: 1);
            player.PotionCount = 0;

            // Act
            bool used = player.UsePotion();

            // Assert
            Assert.False(used);
        }

        [Fact]
        public void Player_Heal_DoesNotExceedMaxHP()
        {
            // Arrange - Use TestFixtures with known stats
            var player = TestFixtures.CreateWarriorPlayer("Test", level: 1);
            player.CurrentHP = player.MaxHP - 10;
            int maxHP = player.MaxHP;

            // Act
            player.Heal(50);

            // Assert
            Assert.Equal(maxHP, player.CurrentHP);
        }

        [Fact]
        public void Player_RestoreMana_DoesNotExceedMaxMana()
        {
            // Arrange - Use TestFixtures with known stats
            var player = TestFixtures.CreateMagePlayer("Test", level: 1);
            player.CurrentMana = player.MaxMana - 10;
            int maxMana = player.MaxMana;

            // Act
            player.RestoreMana(50);

            // Assert
            Assert.Equal(maxMana, player.CurrentMana);
        }

        [Fact]
        public void Player_EquipWeapon_GrantsAbility()
        {
            // Arrange - Create player with controlled test data
            var player = TestFixtures.CreateWarriorPlayer("Test", level: 1);
            var ability = TestFixtures.CreateTestAbility(id: "weapon_slash", name: "Weapon Slash");

            // Create weapon that grants an ability
            var weapon = TestFixtures.CreateTestWeapon(
                "Test Sword",
                attack: 20,
                grantedAbilityIds: new System.Collections.Generic.List<string> { "weapon_slash" }
            );

            // Act - Equip the weapon by assigning it to the inventory
            player.Inventory.Weapon = weapon;

            // Assert - Verify weapon is equipped
            Assert.NotNull(player.Inventory.Weapon);
            Assert.Equal("Test Sword", player.Inventory.Weapon.Name);
            Assert.Equal(20, player.Inventory.Weapon.AttackBonus);
        }

        [Fact]
        public void GetCharacterSheetInfo_SeparatesLearnedAndEquipmentAbilities()
        {
            // Arrange - Create player with controlled abilities
            var mockRepo = TestFixtures.CreateMockRepository();
            var player = new Player("Test", PlayerClass.Warrior, mockRepo.Object);

            // Add a learned ability
            var learnedAbility = TestFixtures.CreateTestAbility("learned_ability", "Learned Skill");
            player.Abilities.Add(learnedAbility);

            // Add an equipment-granted ability
            var equipAbility = TestFixtures.CreateTestAbility("equip_ability", "Equipment Skill");
            equipAbility.IsEquipmentGranted = true;
            player.Abilities.Add(equipAbility);

            // Act
            var sheetInfo = player.GetCharacterSheetInfo();

            // Assert - Abilities should be separated correctly
            Assert.Contains(sheetInfo.Abilities, a => a.Name == "Learned Skill");
            Assert.DoesNotContain(sheetInfo.Abilities, a => a.Name == "Equipment Skill");
            Assert.Contains(sheetInfo.EquipmentAbilities, a => a.Name == "Equipment Skill");
        }

        [Fact]
        public void GetCharacterSheetInfo_OnlyShowsUnlockedLearnedAbilities()
        {
            // Arrange - Create player with controlled abilities
            var mockRepo = TestFixtures.CreateMockRepository();
            var player = new Player("Test", PlayerClass.Warrior, mockRepo.Object);

            // Add unlocked ability
            var unlockedAbility = TestFixtures.CreateTestAbility("unlocked", "Unlocked Skill");
            unlockedAbility.IsUnlocked = true;
            player.Abilities.Add(unlockedAbility);

            // Add locked ability
            var lockedAbility = TestFixtures.CreateTestAbility("locked", "Locked Skill");
            lockedAbility.IsUnlocked = false;
            player.Abilities.Add(lockedAbility);

            // Act
            var sheetInfo = player.GetCharacterSheetInfo();

            // Assert - Only unlocked abilities should appear
            Assert.Contains(sheetInfo.Abilities, a => a.Name == "Unlocked Skill");
            Assert.DoesNotContain(sheetInfo.Abilities, a => a.Name == "Locked Skill");
        }

        [Fact]
        public void GetCharacterSheetInfo_ShowsBaseStatsVsBonusStats()
        {
            // Arrange - Create player with known stats
            var player = TestFixtures.CreateWarriorPlayer("Test", level: 1);

            // Equip an item with known bonuses
            var weapon = TestFixtures.CreateTestWeapon("Bonus Sword", attack: 10);
            player.Inventory.Weapon = weapon;

            // Act
            var sheetInfo = player.GetCharacterSheetInfo();

            // Assert - Bonus stats should reflect equipment
            var equipStats = player.Inventory.GetTotalStats();
            Assert.Equal(equipStats.HP, sheetInfo.BonusStats.HP);
            Assert.Equal(equipStats.Attack, sheetInfo.BonusStats.Attack);
            Assert.Equal(equipStats.Defense, sheetInfo.BonusStats.Defense);

            // Character sheet should show bonus attack from weapon
            Assert.True(sheetInfo.BonusStats.Attack >= 10, $"Bonus attack should include weapon (+10), got {sheetInfo.BonusStats.Attack}");
        }

        [Fact]
        public void GetCharacterSheetInfo_AfterLevelUp_ShowsCorrectBaseStats()
        {
            // Arrange - Create player with known stats
            var player = TestFixtures.CreateWarriorPlayer("Test", level: 1);
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
