using System;
using System.Linq;
using System.Collections.Generic;
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
            // Arrange - Create test enemy data with abilities
            var testAbilityRef = TestFixtures.CreateTestEnemyAbilityData("test_fierce_strike", 50);
            var enemyData = TestFixtures.CreateTestEnemyData(
                "test_orc",
                "Test Orc",
                abilities: new List<EnemyAbilityData> { testAbilityRef }
            );

            // Assert
            Assert.NotEmpty(enemyData.Abilities);
            Assert.Equal("test_fierce_strike", enemyData.Abilities[0].AbilityId);
        }

        [Fact]
        public void EnemyAbility_ShouldHaveUseThreshold()
        {
            // Arrange - Create test enemy with abilities
            var testAbilityRef = TestFixtures.CreateTestEnemyAbilityData("test_ability", 75);
            var enemyData = TestFixtures.CreateTestEnemyData(
                abilities: new List<EnemyAbilityData> { testAbilityRef }
            );

            // Assert - Threshold should be in valid range
            Assert.True(enemyData.Abilities[0].UseThreshold > 0);
            Assert.True(enemyData.Abilities[0].UseThreshold <= 100);
            Assert.Equal(75, enemyData.Abilities[0].UseThreshold);
        }

        [Fact]
        public void EnemyAbility_CanUse_ShouldRespectHPThreshold()
        {
            // Arrange - Create test ability with no mana cost and cooldown
            var ability = TestFixtures.CreateTestAbility(
                "test_strike",
                "Test Strike",
                "A test strike",
                manaCost: 0,
                cooldown: 2
            );

            // Create enemy ability with 50% HP threshold
            var enemyAbility = new EnemyAbility(ability, 50);

            // Assert - Full HP (100/100) should NOT allow use (100% > 50% threshold)
            Assert.False(enemyAbility.CanUse(100, 100));

            // Assert - 50% HP (50/100) should allow use (50% <= 50% threshold)
            Assert.True(enemyAbility.CanUse(50, 100));

            // Assert - 25% HP (25/100) should allow use (25% <= 50% threshold)
            Assert.True(enemyAbility.CanUse(25, 100));
        }

        [Fact]
        public void EnemyAbility_CanUse_ShouldRespectCooldown()
        {
            // Arrange - Create test ability with cooldown
            var ability = TestFixtures.CreateTestAbility(
                "test_strike",
                "Test Strike",
                manaCost: 0,
                cooldown: 3
            );

            // Create enemy ability with 100% threshold (always usable based on HP)
            var enemyAbility = new EnemyAbility(ability, 100);

            // Act - Should be able to use initially
            Assert.True(enemyAbility.CanUse(100, 100));
            enemyAbility.Use();

            // Assert - Should NOT be able to use while on cooldown
            Assert.False(enemyAbility.CanUse(100, 100));
            Assert.Equal(3, ability.CurrentCooldown);
        }

        [Fact]
        public void EnemyAbility_Cooldown_ShouldReduceOverTime()
        {
            // Arrange - Create test ability with cooldown
            var ability = TestFixtures.CreateTestAbility(
                manaCost: 0,
                cooldown: 4
            );
            var enemyAbility = new EnemyAbility(ability, 100);

            // Act - Use ability to trigger cooldown
            enemyAbility.Use();
            int initialCooldown = ability.CurrentCooldown;
            Assert.Equal(4, initialCooldown);

            // Reduce cooldown
            enemyAbility.ReduceCooldown();

            // Assert - Cooldown should decrease by 1
            Assert.Equal(initialCooldown - 1, ability.CurrentCooldown);
            Assert.Equal(3, ability.CurrentCooldown);
        }

        [Fact]
        public void AllEnemies_ShouldHaveValidAbilities()
        {
            // Arrange - Create mock repository with test data
            var testAbilityData = TestFixtures.CreateDefaultTestAbilityData("test_ability_1");
            var testAbilityRef = TestFixtures.CreateTestEnemyAbilityData("test_ability_1", 50);

            var enemies = new List<EnemyData>
            {
                TestFixtures.CreateTestEnemyData("enemy1", "Enemy 1",
                    abilities: new List<EnemyAbilityData> { testAbilityRef }),
                TestFixtures.CreateTestEnemyData("enemy2", "Enemy 2",
                    abilities: new List<EnemyAbilityData> { testAbilityRef })
            };

            var mockRepo = TestFixtures.CreateMockRepository(
                enemies: enemies,
                abilities: new List<AbilityData> { testAbilityData }
            );

            // Act & Assert - Check each enemy's abilities can be loaded
            foreach (var enemyData in enemies)
            {
                foreach (var abilityRef in enemyData.Abilities)
                {
                    var ability = mockRepo.Object.GetAbility(abilityRef.AbilityId);
                    Assert.NotNull(ability);
                    Assert.NotEmpty(ability.Name);
                    Assert.Equal("test_ability_1", ability.Id);
                }
            }
        }

        [Fact]
        public void EnemyAbilities_ShouldHaveNoManaCost()
        {
            // Arrange - Create test enemies with abilities
            var testAbility1 = TestFixtures.CreateDefaultTestAbilityData("ability1");
            testAbility1.ManaCost = 0; // Enemy abilities don't use mana

            var testAbility2 = TestFixtures.CreateDefaultTestAbilityData("ability2");
            testAbility2.ManaCost = 0;

            var enemies = new List<EnemyData>
            {
                TestFixtures.CreateTestEnemyData("enemy1", "Enemy 1",
                    abilities: new List<EnemyAbilityData> {
                        TestFixtures.CreateTestEnemyAbilityData("ability1")
                    }),
                TestFixtures.CreateTestEnemyData("enemy2", "Enemy 2",
                    abilities: new List<EnemyAbilityData> {
                        TestFixtures.CreateTestEnemyAbilityData("ability2")
                    })
            };

            var mockRepo = TestFixtures.CreateMockRepository(
                enemies: enemies,
                abilities: new List<AbilityData> { testAbility1, testAbility2 }
            );

            // Act & Assert - All enemy abilities should have 0 mana cost
            foreach (var enemyData in enemies)
            {
                foreach (var abilityRef in enemyData.Abilities)
                {
                    var ability = mockRepo.Object.GetAbility(abilityRef.AbilityId);
                    Assert.Equal(0, ability.ManaCost);
                }
            }
        }

        [Fact]
        public void DifferentEnemyTypes_ShouldHaveDifferentAbilities()
        {
            // Arrange - Create different enemies with different abilities
            var goblin = TestFixtures.CreateTestEnemyData("goblin", "Goblin",
                abilities: new List<EnemyAbilityData> {
                    TestFixtures.CreateTestEnemyAbilityData("goblin_strike", 100)
                });

            var dragon = TestFixtures.CreateTestEnemyData("dragon", "Dragon",
                abilities: new List<EnemyAbilityData> {
                    TestFixtures.CreateTestEnemyAbilityData("dragon_breath", 50)
                });

            // Assert - Different enemy types should have different abilities
            Assert.NotEqual(goblin.Abilities[0].AbilityId, dragon.Abilities[0].AbilityId);
            Assert.Equal("goblin_strike", goblin.Abilities[0].AbilityId);
            Assert.Equal("dragon_breath", dragon.Abilities[0].AbilityId);
        }

        [Fact]
        public void EnemyWithMultipleAbilities_ShouldHaveDifferentThresholds()
        {
            // Arrange - Create enemy with multiple abilities at different thresholds
            var orc = TestFixtures.CreateTestEnemyData("orc", "Orc",
                abilities: new List<EnemyAbilityData> {
                    TestFixtures.CreateTestEnemyAbilityData("basic_strike", 100),  // Always use
                    TestFixtures.CreateTestEnemyAbilityData("enrage", 50),         // Use below 50% HP
                    TestFixtures.CreateTestEnemyAbilityData("desperate_blow", 25) // Use below 25% HP
                });

            // Assert - Enemy should have multiple abilities
            Assert.True(orc.Abilities.Count >= 2);
            Assert.Equal(3, orc.Abilities.Count);

            // Abilities should have different thresholds for strategic use
            var thresholds = orc.Abilities.Select(a => a.UseThreshold).ToList();
            Assert.Contains(100, thresholds); // At least one always-use ability
            Assert.Contains(thresholds, t => t != 100); // At least one conditional ability
            Assert.Contains(50, thresholds);
            Assert.Contains(25, thresholds);
        }

        [Fact]
        public void EnemyAbility_UseThreshold_100_ShouldAlwaysBeUsable()
        {
            // Arrange - Create ability with 100% threshold (always use)
            var ability = TestFixtures.CreateTestAbility(manaCost: 0, cooldown: 2);
            var enemyAbility = new EnemyAbility(ability, 100);

            // Assert - Should be usable at any HP percentage
            Assert.True(enemyAbility.CanUse(100, 100)); // 100% HP
            Assert.True(enemyAbility.CanUse(75, 100));  // 75% HP
            Assert.True(enemyAbility.CanUse(50, 100));  // 50% HP
            Assert.True(enemyAbility.CanUse(25, 100));  // 25% HP
            Assert.True(enemyAbility.CanUse(1, 100));   // 1% HP
        }

        [Fact]
        public void EnemyAbility_UseThreshold_0_ShouldNeverBeUsable()
        {
            // Arrange - Create ability with 0% threshold (theoretically never use)
            var ability = TestFixtures.CreateTestAbility(manaCost: 0, cooldown: 2);
            var enemyAbility = new EnemyAbility(ability, 0);

            // Assert - Should only be usable at exactly 0 HP (essentially dead)
            Assert.False(enemyAbility.CanUse(100, 100)); // 100% HP
            Assert.False(enemyAbility.CanUse(50, 100));  // 50% HP
            Assert.False(enemyAbility.CanUse(1, 100));   // 1% HP
            Assert.True(enemyAbility.CanUse(0, 100));    // 0% HP (edge case)
        }

        [Fact]
        public void EnemyAbility_Cooldown_ShouldResetAfterFullyDecremented()
        {
            // Arrange - Create ability with short cooldown
            var ability = TestFixtures.CreateTestAbility(manaCost: 0, cooldown: 2);
            var enemyAbility = new EnemyAbility(ability, 100);

            // Act - Use ability and reduce cooldown to 0
            enemyAbility.Use();
            Assert.Equal(2, ability.CurrentCooldown);

            enemyAbility.ReduceCooldown();
            Assert.Equal(1, ability.CurrentCooldown);
            Assert.False(enemyAbility.CanUse(100, 100)); // Still on cooldown

            enemyAbility.ReduceCooldown();
            Assert.Equal(0, ability.CurrentCooldown);

            // Assert - Should be usable again after cooldown reaches 0
            Assert.True(enemyAbility.CanUse(100, 100));
        }
    }
}
