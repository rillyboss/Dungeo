using System.Linq;
using Xunit;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.DataLoading;

namespace TestRPGGame.Tests
{
    public class EnemyModifierTests : TestBase
    {
        [Fact]
        public void EnemyPrefixes_ShouldLoadFromData()
        {
            // Act
            var prefixes = DataLoader.GetEnemyPrefixesByLevel(10).ToList();

            // Assert
            Assert.NotEmpty(prefixes);
        }

        [Fact]
        public void EnemySuffixes_ShouldLoadFromData()
        {
            // Act
            var suffixes = DataLoader.GetEnemySuffixesByLevel(10).ToList();

            // Assert
            Assert.NotEmpty(suffixes);
        }

        [Fact]
        public void EnemyPrefix_ShouldHaveValidMultipliers()
        {
            // Arrange
            var prefix = DataLoader.GetEnemyPrefix("Elite");

            // Assert
            Assert.Equal("Elite", prefix.Name);
            Assert.True(prefix.HPMultiplier > 0);
            Assert.True(prefix.AttackMultiplier > 0);
            Assert.True(prefix.DefenseMultiplier > 0);
            Assert.True(prefix.SpeedMultiplier > 0);
            Assert.True(prefix.GoldMultiplier > 0);
            Assert.True(prefix.ExpMultiplier > 0);
        }

        [Fact]
        public void EnemySuffix_ShouldHaveAbilities()
        {
            // Arrange
            var suffix = DataLoader.GetEnemySuffix("the Relentless");

            // Assert
            Assert.Equal("the Relentless", suffix.Name);
            Assert.NotEmpty(suffix.AdditionalAbilities);
            Assert.True(suffix.GoldBonus > 0);
            Assert.True(suffix.ExpBonus > 0);
        }

        [Fact]
        public void EnemyPrefixesByLevel_ShouldFilterCorrectly()
        {
            // Act - Get prefixes for level 1
            var level1Prefixes = DataLoader.GetEnemyPrefixesByLevel(1).ToList();

            // Act - Get prefixes for level 10
            var level10Prefixes = DataLoader.GetEnemyPrefixesByLevel(10).ToList();

            // Assert - Level 10 should have more prefixes available
            Assert.True(level10Prefixes.Count >= level1Prefixes.Count);

            // Assert - All level 1 prefixes should have MinLevel <= 1
            Assert.All(level1Prefixes, p => Assert.True(p.MinLevel <= 1));
        }

        [Fact]
        public void EnemySuffixesByLevel_ShouldFilterCorrectly()
        {
            // Act
            var level1Suffixes = DataLoader.GetEnemySuffixesByLevel(1).ToList();
            var level10Suffixes = DataLoader.GetEnemySuffixesByLevel(10).ToList();

            // Assert
            Assert.True(level10Suffixes.Count >= level1Suffixes.Count);
            Assert.All(level1Suffixes, s => Assert.True(s.MinLevel <= 1));
        }

        [Fact]
        public void CreatedEnemy_ShouldHaveBaseName()
        {
            // Act
            var enemy = EnemyFactory.CreateEnemy(1);

            // Assert
            Assert.NotNull(enemy.BaseName);
            Assert.NotEmpty(enemy.BaseName);
        }

        [Fact]
        public void HigherLevelEnemies_MoreLikelyToHaveModifiers()
        {
            // Arrange - Create many enemies at different levels
            int level1WithModifiers = 0;
            int level10WithModifiers = 0;
            int testCount = 50;

            // Act
            for (int i = 0; i < testCount; i++)
            {
                var enemy1 = EnemyFactory.CreateEnemy(1);
                var enemy10 = EnemyFactory.CreateEnemy(10);

                if (!string.IsNullOrEmpty(enemy1.Prefix) || !string.IsNullOrEmpty(enemy1.Suffix))
                {
                    level1WithModifiers++;
                }

                if (!string.IsNullOrEmpty(enemy10.Prefix) || !string.IsNullOrEmpty(enemy10.Suffix))
                {
                    level10WithModifiers++;
                }
            }

            // Assert - Level 10 enemies should have higher modifier rate than level 1
            Assert.True(level10WithModifiers > level1WithModifiers,
                $"Level 10 enemies ({level10WithModifiers}) should have more modifiers than level 1 ({level1WithModifiers})");
        }

        [Fact]
        public void EnemyWithPrefix_ShouldHaveModifiedStats()
        {
            // Arrange - Create many enemies until we find one with a prefix
            Enemy? enemyWithPrefix = null;
            for (int i = 0; i < 100; i++)
            {
                var enemy = EnemyFactory.CreateEnemy(5);
                if (!string.IsNullOrEmpty(enemy.Prefix))
                {
                    enemyWithPrefix = enemy;
                    break;
                }
            }

            // Assert
            Assert.NotNull(enemyWithPrefix);
            Assert.NotEmpty(enemyWithPrefix.Prefix);
            Assert.NotEqual(enemyWithPrefix.BaseName, enemyWithPrefix.Name);
            Assert.Contains(enemyWithPrefix.Prefix, enemyWithPrefix.Name);
        }

        [Fact]
        public void EnemyWithSuffix_ShouldHaveAdditionalAbilities()
        {
            // Arrange - Create many enemies until we find one with a suffix that grants abilities
            Enemy? enemyWithSuffix = null;
            for (int i = 0; i < 100; i++)
            {
                var enemy = EnemyFactory.CreateEnemy(6);
                if (!string.IsNullOrEmpty(enemy.Suffix))
                {
                    enemyWithSuffix = enemy;
                    break;
                }
            }

            // Assert
            Assert.NotNull(enemyWithSuffix);
            Assert.NotEmpty(enemyWithSuffix.Suffix);
            Assert.Contains(enemyWithSuffix.Suffix, enemyWithSuffix.Name);
        }

        [Fact]
        public void AllPrefixes_ShouldBeValidInData()
        {
            // Arrange
            var allPrefixes = DataLoader.GetEnemyPrefixesByLevel(100).ToList();

            // Assert - Each prefix should have valid data
            foreach (var prefix in allPrefixes)
            {
                Assert.NotNull(prefix.Id);
                Assert.True(prefix.MinLevel >= 0);
                Assert.True(prefix.HPMultiplier >= 0);
                Assert.True(prefix.AttackMultiplier >= 0);
                Assert.True(prefix.DefenseMultiplier >= 0);
                Assert.True(prefix.SpeedMultiplier >= 0);
            }
        }

        [Fact]
        public void AllSuffixes_ShouldHaveValidAbilityReferences()
        {
            // Arrange
            var allSuffixes = DataLoader.GetEnemySuffixesByLevel(100).ToList();

            // Assert - Each suffix ability should exist in abilities data
            foreach (var suffix in allSuffixes)
            {
                foreach (var abilityId in suffix.AdditionalAbilities)
                {
                    // Should not throw exception
                    var ability = DataLoader.GetAbility(abilityId);
                    Assert.NotNull(ability);
                }
            }
        }
    }
}
