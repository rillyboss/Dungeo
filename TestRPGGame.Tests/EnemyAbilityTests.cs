using System;
using System.Linq;
using Xunit;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.DataLoading;
using TestRPGGame.Abilities;

namespace TestRPGGame.Tests
{
    public class EnemyAbilityTests : TestBase
    {
        [Fact]
        public void EnemyAbility_ShouldLoadFromData()
        {
            // Arrange - Get an enemy with abilities
            var enemyData = DataLoader.GetEnemy("orc");

            // Assert
            Assert.NotEmpty(enemyData.Abilities);
            Assert.Equal("enemy_fierce_strike", enemyData.Abilities[0].AbilityId);
        }

        [Fact]
        public void EnemyAbility_ShouldHaveUseThreshold()
        {
            // Arrange
            var enemyData = DataLoader.GetEnemy("orc");

            // Assert
            Assert.True(enemyData.Abilities[0].UseThreshold > 0);
            Assert.True(enemyData.Abilities[0].UseThreshold <= 100);
        }

        [Fact]
        public void EnemyAbility_CanUse_ShouldRespectHPThreshold()
        {
            // Arrange
            var abilityData = DataLoader.GetAbility("enemy_fierce_strike");
            var ability = new Ability(
                abilityData.Name,
                abilityData.ManaCost,
                abilityData.Cooldown,
                abilityData.Description,
                Enum.Parse<AbilityType>(abilityData.Type)
            );
            var enemyAbility = new EnemyAbility(ability, 50); // Only use below 50% HP

            // Act & Assert - Full HP should not allow use
            Assert.False(enemyAbility.CanUse(100, 100));

            // Act & Assert - 50% HP should allow use
            Assert.True(enemyAbility.CanUse(50, 100));

            // Act & Assert - 25% HP should allow use
            Assert.True(enemyAbility.CanUse(25, 100));
        }

        [Fact]
        public void EnemyAbility_CanUse_ShouldRespectCooldown()
        {
            // Arrange
            var abilityData = DataLoader.GetAbility("enemy_fierce_strike");
            var ability = new Ability(
                abilityData.Name,
                abilityData.ManaCost,
                abilityData.Cooldown,
                abilityData.Description,
                Enum.Parse<AbilityType>(abilityData.Type)
            );
            var enemyAbility = new EnemyAbility(ability, 100); // Always use

            // Act - Use the ability
            Assert.True(enemyAbility.CanUse(100, 100));
            enemyAbility.Use();

            // Assert - Should not be able to use while on cooldown
            Assert.False(enemyAbility.CanUse(100, 100));
        }

        [Fact]
        public void EnemyAbility_Cooldown_ShouldReduceOverTime()
        {
            // Arrange
            var abilityData = DataLoader.GetAbility("enemy_fierce_strike");
            var ability = new Ability(
                abilityData.Name,
                abilityData.ManaCost,
                abilityData.Cooldown,
                abilityData.Description,
                Enum.Parse<AbilityType>(abilityData.Type)
            );
            var enemyAbility = new EnemyAbility(ability, 100);

            // Act - Use ability and reduce cooldown
            enemyAbility.Use();
            int initialCooldown = ability.CurrentCooldown;

            enemyAbility.ReduceCooldown();

            // Assert
            Assert.Equal(initialCooldown - 1, ability.CurrentCooldown);
        }

        [Fact]
        public void AllEnemies_ShouldHaveValidAbilities()
        {
            // Arrange - Get all enemies
            var enemies = DataLoader.GetEnemiesByLevel(0, 100);

            // Act & Assert - Check each enemy's abilities are valid
            foreach (var enemyData in enemies)
            {
                foreach (var abilityRef in enemyData.Abilities)
                {
                    // Should not throw exception
                    var ability = DataLoader.GetAbility(abilityRef.AbilityId);
                    Assert.NotNull(ability);
                    Assert.NotEmpty(ability.Name);
                }
            }
        }

        [Fact]
        public void EnemyAbilities_ShouldHaveNoManaCost()
        {
            // Arrange - Get all enemy abilities
            var enemies = DataLoader.GetEnemiesByLevel(0, 100);

            // Act & Assert
            foreach (var enemyData in enemies)
            {
                foreach (var abilityRef in enemyData.Abilities)
                {
                    var ability = DataLoader.GetAbility(abilityRef.AbilityId);
                    Assert.Equal(0, ability.ManaCost); // Enemies don't use mana
                }
            }
        }

        [Fact]
        public void DifferentEnemyTypes_ShouldHaveDifferentAbilities()
        {
            // Arrange
            var goblin = DataLoader.GetEnemy("goblin");
            var dragon = DataLoader.GetEnemy("dragon_whelp");

            // Assert - Different enemy types should have different abilities
            Assert.NotEqual(goblin.Abilities[0].AbilityId, dragon.Abilities[0].AbilityId);
        }

        [Fact]
        public void EnemyWithMultipleAbilities_ShouldHaveDifferentThresholds()
        {
            // Arrange - Find an enemy with multiple abilities
            var orc = DataLoader.GetEnemy("orc");

            // Assert
            Assert.True(orc.Abilities.Count >= 2);

            // Abilities should have different thresholds for strategic use
            var thresholds = orc.Abilities.Select(a => a.UseThreshold).ToList();
            Assert.Contains(thresholds, t => t != 100); // At least one is conditional
        }
    }
}
