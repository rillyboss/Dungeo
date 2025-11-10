using Xunit;
using TestRPGGame;
using TestRPGGame.Entities.Player;
using TestRPGGame.Abilities;

namespace TestRPGGame.Tests
{
    public class AbilityTests : TestBase
    {
        [Fact]
        public void Ability_CanUse_RequiresSufficientMana()
        {
            // Arrange
            var ability = new Ability("Test Strike", 30, 3, "Test", AbilityType.Physical);
            ability.IsUnlocked = true;

            // Act & Assert
            Assert.True(ability.CanUse(50));  // Has enough mana
            Assert.True(ability.CanUse(30));  // Has exact mana
            Assert.False(ability.CanUse(29)); // Not enough mana
        }

        [Fact]
        public void Ability_CanUse_RequiresNotOnCooldown()
        {
            // Arrange
            var ability = new Ability("Test Strike", 30, 3, "Test", AbilityType.Physical);
            ability.IsUnlocked = true;
            ability.CurrentCooldown = 2;

            // Act & Assert
            Assert.False(ability.CanUse(50)); // On cooldown
        }

        [Fact]
        public void Ability_CanUse_RequiresUnlocked()
        {
            // Arrange
            var ability = new Ability("Test Strike", 30, 3, "Test", AbilityType.Physical, unlockLevel: 5, purchaseCost: 200);

            // Act & Assert
            Assert.False(ability.CanUse(50)); // Not unlocked
        }

        [Fact]
        public void Ability_Use_SetsCooldown()
        {
            // Arrange
            var ability = new Ability("Test Strike", 30, 3, "Test", AbilityType.Physical);
            ability.IsUnlocked = true;

            // Act
            ability.Use();

            // Assert
            Assert.Equal(3, ability.CurrentCooldown);
        }

        [Fact]
        public void Ability_ReduceCooldown_DecreasesCorrectly()
        {
            // Arrange
            var ability = new Ability("Test Strike", 30, 3, "Test", AbilityType.Physical);
            ability.IsUnlocked = true;
            ability.CurrentCooldown = 3;

            // Act
            ability.ReduceCooldown();

            // Assert
            Assert.Equal(2, ability.CurrentCooldown);
        }

        [Fact]
        public void Ability_ReduceCooldown_DoesNotGoBelowZero()
        {
            // Arrange
            var ability = new Ability("Test Strike", 30, 3, "Test", AbilityType.Physical);
            ability.IsUnlocked = true;
            ability.CurrentCooldown = 0;

            // Act
            ability.ReduceCooldown();

            // Assert
            Assert.Equal(0, ability.CurrentCooldown);
        }

        [Fact]
        public void Ability_Unlock_WorksCorrectly()
        {
            // Arrange
            var ability = new Ability("Test Strike", 30, 3, "Test", AbilityType.Physical, unlockLevel: 5, purchaseCost: 200);

            // Act
            ability.Unlock();

            // Assert
            Assert.True(ability.IsUnlocked);
        }

        [Fact]
        public void Ability_CanUnlock_ValidatesRequirements()
        {
            // Arrange
            var ability = new Ability("Test Strike", 30, 3, "Test", AbilityType.Physical, unlockLevel: 5, purchaseCost: 200);

            // Act & Assert
            Assert.True(ability.CanUnlock(5, 200));   // Meets requirements
            Assert.True(ability.CanUnlock(6, 300));   // Exceeds requirements
            Assert.False(ability.CanUnlock(4, 200));  // Level too low
            Assert.False(ability.CanUnlock(5, 150));  // Not enough gold
            Assert.False(ability.CanUnlock(4, 150));  // Both insufficient
        }
    }
}
