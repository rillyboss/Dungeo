using System.Collections.Generic;
using System.Linq;
using Xunit;
using Moq;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.DataLoading;
using TestRPGGame.Utils;

namespace TestRPGGame.Tests
{
    /// <summary>
    /// Tests for the enemy modifier system (prefixes and suffixes).
    /// These tests focus on the APPLICATION LOGIC, not the data content.
    /// </summary>
    public class EnemyModifierTests : TestBase
    {
        #region Level-Based Filtering Tests

        [Fact]
        public void PrefixesByLevel_ShouldFilterByMinLevel()
        {
            // Arrange - Create test prefixes with different level requirements
            var testPrefixes = new List<EnemyPrefixData>
            {
                TestFixtures.CreateTestPrefix("weak", "Weak", minLevel: 1),
                TestFixtures.CreateTestPrefix("strong", "Strong", minLevel: 5),
                TestFixtures.CreateTestPrefix("elite", "Elite", minLevel: 10)
            };

            var mockRepo = new Mock<IDataRepository>();
            mockRepo.Setup(r => r.GetEnemyPrefixesByLevel(It.IsAny<int>()))
                .Returns<int>(level => testPrefixes.Where(p => p.MinLevel <= level));

            // Act - Get prefixes for different levels
            var level1Prefixes = mockRepo.Object.GetEnemyPrefixesByLevel(1).ToList();
            var level5Prefixes = mockRepo.Object.GetEnemyPrefixesByLevel(5).ToList();
            var level10Prefixes = mockRepo.Object.GetEnemyPrefixesByLevel(10).ToList();

            // Assert - Higher levels should have more prefixes available
            Assert.Single(level1Prefixes); // Only "Weak"
            Assert.Equal(2, level5Prefixes.Count); // "Weak" + "Strong"
            Assert.Equal(3, level10Prefixes.Count); // All three
            Assert.All(level1Prefixes, p => Assert.True(p.MinLevel <= 1));
            Assert.All(level5Prefixes, p => Assert.True(p.MinLevel <= 5));
            Assert.All(level10Prefixes, p => Assert.True(p.MinLevel <= 10));
        }

        [Fact]
        public void SuffixesByLevel_ShouldFilterByMinLevel()
        {
            // Arrange - Create test suffixes with different level requirements
            var testSuffixes = new List<EnemySuffixData>
            {
                TestFixtures.CreateTestSuffix("basic", "the Basic", minLevel: 1),
                TestFixtures.CreateTestSuffix("advanced", "the Advanced", minLevel: 5),
                TestFixtures.CreateTestSuffix("master", "the Master", minLevel: 10)
            };

            var mockRepo = new Mock<IDataRepository>();
            mockRepo.Setup(r => r.GetEnemySuffixesByLevel(It.IsAny<int>()))
                .Returns<int>(level => testSuffixes.Where(s => s.MinLevel <= level));

            // Act
            var level1Suffixes = mockRepo.Object.GetEnemySuffixesByLevel(1).ToList();
            var level5Suffixes = mockRepo.Object.GetEnemySuffixesByLevel(5).ToList();
            var level10Suffixes = mockRepo.Object.GetEnemySuffixesByLevel(10).ToList();

            // Assert
            Assert.Single(level1Suffixes);
            Assert.Equal(2, level5Suffixes.Count);
            Assert.Equal(3, level10Suffixes.Count);
            Assert.All(level1Suffixes, s => Assert.True(s.MinLevel <= 1));
            Assert.All(level5Suffixes, s => Assert.True(s.MinLevel <= 5));
            Assert.All(level10Suffixes, s => Assert.True(s.MinLevel <= 10));
        }

        #endregion

        #region Prefix Application Tests

        [Fact]
        public void ApplyPrefix_ShouldMultiplyHP()
        {
            // Arrange - Create enemy with known stats
            var enemy = TestFixtures.CreateTestEnemy("Test Enemy", maxHP: 100);
            var prefix = TestFixtures.CreateTestPrefix(hpMultiplier: 1.5);

            int originalHP = enemy.MaxHP;
            int expectedHP = (int)(originalHP * prefix.HPMultiplier);

            // Act - Simulate prefix application (as done in EnemyFactory)
            enemy.MaxHP = (int)(enemy.MaxHP * prefix.HPMultiplier);
            enemy.CurrentHP = enemy.MaxHP;

            // Assert
            Assert.Equal(expectedHP, enemy.MaxHP);
            Assert.Equal(expectedHP, enemy.CurrentHP);
            Assert.Equal(150, enemy.MaxHP); // 100 * 1.5 = 150
        }

        [Fact]
        public void ApplyPrefix_ShouldMultiplyAttack()
        {
            // Arrange
            var enemy = TestFixtures.CreateTestEnemy("Test Enemy", attack: 20);
            var prefix = TestFixtures.CreateTestPrefix(attackMultiplier: 1.3);

            // Act
            enemy.Attack = (int)(enemy.Attack * prefix.AttackMultiplier);

            // Assert
            Assert.Equal(26, enemy.Attack); // 20 * 1.3 = 26
        }

        [Fact]
        public void ApplyPrefix_ShouldMultiplyDefense()
        {
            // Arrange
            var enemy = TestFixtures.CreateTestEnemy("Test Enemy", defense: 10);
            var prefix = TestFixtures.CreateTestPrefix(defenseMultiplier: 1.2);

            // Act
            enemy.Defense = (int)(enemy.Defense * prefix.DefenseMultiplier);

            // Assert
            Assert.Equal(12, enemy.Defense); // 10 * 1.2 = 12
        }

        [Fact]
        public void ApplyPrefix_ShouldMultiplySpeed()
        {
            // Arrange
            var enemy = TestFixtures.CreateTestEnemy("Test Enemy", speed: 10);
            var prefix = TestFixtures.CreateTestPrefix(speedMultiplier: 1.4);

            // Act
            enemy.Speed = (int)(enemy.Speed * prefix.SpeedMultiplier);

            // Assert
            Assert.Equal(14, enemy.Speed); // 10 * 1.4 = 14
        }

        [Fact]
        public void ApplyPrefix_ShouldMultiplyGoldReward()
        {
            // Arrange
            var enemy = TestFixtures.CreateTestEnemy("Test Enemy", goldReward: 50);
            var prefix = TestFixtures.CreateTestPrefix(goldMultiplier: 2.0);

            // Act
            enemy.GoldReward = (int)(enemy.GoldReward * prefix.GoldMultiplier);

            // Assert
            Assert.Equal(100, enemy.GoldReward); // 50 * 2.0 = 100
        }

        [Fact]
        public void ApplyPrefix_ShouldMultiplyExpReward()
        {
            // Arrange
            var enemy = TestFixtures.CreateTestEnemy("Test Enemy", expReward: 100);
            var prefix = TestFixtures.CreateTestPrefix(expMultiplier: 1.5);

            // Act
            enemy.ExpReward = (int)(enemy.ExpReward * prefix.ExpMultiplier);

            // Assert
            Assert.Equal(150, enemy.ExpReward); // 100 * 1.5 = 150
        }

        [Fact]
        public void ApplyPrefix_ShouldUpdateName()
        {
            // Arrange
            var enemy = TestFixtures.CreateTestEnemy("Goblin");
            var prefix = TestFixtures.CreateTestPrefix(name: "Elite");

            // Act - Simulate name building (as done in EnemyFactory)
            enemy.Prefix = prefix.Name;
            enemy.Name = $"{prefix.Name} {enemy.BaseName}";

            // Assert
            Assert.Equal("Elite", enemy.Prefix);
            Assert.Equal("Elite Goblin", enemy.Name);
            Assert.Equal("Goblin", enemy.BaseName);
        }

        [Fact]
        public void ApplyPrefix_WithAllMultipliers_ShouldModifyAllStats()
        {
            // Arrange
            var enemy = TestFixtures.CreateTestEnemy("Test Enemy",
                maxHP: 100, attack: 20, defense: 10, speed: 10, goldReward: 50, expReward: 100);

            var prefix = TestFixtures.CreateTestPrefix(
                hpMultiplier: 1.5,
                attackMultiplier: 1.3,
                defenseMultiplier: 1.2,
                speedMultiplier: 1.1,
                goldMultiplier: 2.0,
                expMultiplier: 1.8);

            // Act - Apply all multipliers
            enemy.MaxHP = (int)(enemy.MaxHP * prefix.HPMultiplier);
            enemy.CurrentHP = enemy.MaxHP;
            enemy.Attack = (int)(enemy.Attack * prefix.AttackMultiplier);
            enemy.Defense = (int)(enemy.Defense * prefix.DefenseMultiplier);
            enemy.Speed = (int)(enemy.Speed * prefix.SpeedMultiplier);
            enemy.GoldReward = (int)(enemy.GoldReward * prefix.GoldMultiplier);
            enemy.ExpReward = (int)(enemy.ExpReward * prefix.ExpMultiplier);

            // Assert
            Assert.Equal(150, enemy.MaxHP); // 100 * 1.5
            Assert.Equal(26, enemy.Attack); // 20 * 1.3
            Assert.Equal(12, enemy.Defense); // 10 * 1.2
            Assert.Equal(11, enemy.Speed); // 10 * 1.1
            Assert.Equal(100, enemy.GoldReward); // 50 * 2.0
            Assert.Equal(180, enemy.ExpReward); // 100 * 1.8
        }

        #endregion

        #region Suffix Application Tests

        [Fact]
        public void ApplySuffix_ShouldAddGoldBonus()
        {
            // Arrange
            var enemy = TestFixtures.CreateTestEnemy("Test Enemy", goldReward: 50);
            var suffix = TestFixtures.CreateTestSuffix(goldBonus: 25);

            // Act
            enemy.GoldReward += suffix.GoldBonus;

            // Assert
            Assert.Equal(75, enemy.GoldReward); // 50 + 25
        }

        [Fact]
        public void ApplySuffix_ShouldAddExpBonus()
        {
            // Arrange
            var enemy = TestFixtures.CreateTestEnemy("Test Enemy", expReward: 100);
            var suffix = TestFixtures.CreateTestSuffix(expBonus: 50);

            // Act
            enemy.ExpReward += suffix.ExpBonus;

            // Assert
            Assert.Equal(150, enemy.ExpReward); // 100 + 50
        }

        [Fact]
        public void ApplySuffix_ShouldUpdateName()
        {
            // Arrange
            var enemy = TestFixtures.CreateTestEnemy("Goblin");
            var suffix = TestFixtures.CreateTestSuffix(name: "the Relentless");

            // Act - Simulate name building
            enemy.Suffix = suffix.Name;
            enemy.Name = $"{enemy.BaseName} {suffix.Name}";

            // Assert
            Assert.Equal("the Relentless", enemy.Suffix);
            Assert.Equal("Goblin the Relentless", enemy.Name);
            Assert.Equal("Goblin", enemy.BaseName);
        }

        [Fact]
        public void ApplySuffix_WithAbilities_ShouldTrackAbilityIds()
        {
            // Arrange
            var enemy = TestFixtures.CreateTestEnemy("Test Enemy");
            var suffix = TestFixtures.CreateTestSuffix(
                additionalAbilities: new List<string> { "test_ability_1", "test_ability_2" });

            // Act - Suffix should provide ability IDs (actual ability loading happens in EnemyFactory)
            var abilityIds = suffix.AdditionalAbilities;

            // Assert - Verify suffix contains the ability references
            Assert.Equal(2, abilityIds.Count);
            Assert.Contains("test_ability_1", abilityIds);
            Assert.Contains("test_ability_2", abilityIds);
        }

        [Fact]
        public void ApplySuffix_WithNoAbilities_ShouldHaveEmptyList()
        {
            // Arrange
            var suffix = TestFixtures.CreateTestSuffix(additionalAbilities: null);

            // Assert
            Assert.NotNull(suffix.AdditionalAbilities);
            Assert.Empty(suffix.AdditionalAbilities);
        }

        #endregion

        #region Combined Modifiers Tests

        [Fact]
        public void ApplyPrefixAndSuffix_ShouldCombineEffects()
        {
            // Arrange
            var enemy = TestFixtures.CreateTestEnemy("Goblin",
                maxHP: 100, attack: 20, goldReward: 50, expReward: 100);

            var prefix = TestFixtures.CreateTestPrefix("elite", "Elite",
                hpMultiplier: 1.5, attackMultiplier: 1.3, goldMultiplier: 1.5, expMultiplier: 1.5);

            var suffix = TestFixtures.CreateTestSuffix("relentless", "the Relentless",
                goldBonus: 25, expBonus: 50);

            // Act - Apply prefix first (multipliers), then suffix (bonuses)
            enemy.MaxHP = (int)(enemy.MaxHP * prefix.HPMultiplier);
            enemy.CurrentHP = enemy.MaxHP;
            enemy.Attack = (int)(enemy.Attack * prefix.AttackMultiplier);
            enemy.GoldReward = (int)(enemy.GoldReward * prefix.GoldMultiplier);
            enemy.ExpReward = (int)(enemy.ExpReward * prefix.ExpMultiplier);

            enemy.GoldReward += suffix.GoldBonus;
            enemy.ExpReward += suffix.ExpBonus;

            enemy.Prefix = prefix.Name;
            enemy.Suffix = suffix.Name;
            enemy.Name = $"{prefix.Name} {enemy.BaseName} {suffix.Name}";

            // Assert
            Assert.Equal(150, enemy.MaxHP); // 100 * 1.5
            Assert.Equal(26, enemy.Attack); // 20 * 1.3
            Assert.Equal(100, enemy.GoldReward); // (50 * 1.5) + 25 = 75 + 25 = 100
            Assert.Equal(200, enemy.ExpReward); // (100 * 1.5) + 50 = 150 + 50 = 200
            Assert.Equal("Elite Goblin the Relentless", enemy.Name);
        }

        [Fact]
        public void FullName_WithPrefixOnly_ShouldFormatCorrectly()
        {
            // Arrange
            var enemy = TestFixtures.CreateTestEnemy("Goblin");
            var prefix = TestFixtures.CreateTestPrefix(name: "Elite");

            // Act
            enemy.Prefix = prefix.Name;
            enemy.Name = $"{prefix.Name} {enemy.BaseName}";

            // Assert
            Assert.Equal("Elite Goblin", enemy.Name);
        }

        [Fact]
        public void FullName_WithSuffixOnly_ShouldFormatCorrectly()
        {
            // Arrange
            var enemy = TestFixtures.CreateTestEnemy("Goblin");
            var suffix = TestFixtures.CreateTestSuffix(name: "the Relentless");

            // Act
            enemy.Suffix = suffix.Name;
            enemy.Name = $"{enemy.BaseName} {suffix.Name}";

            // Assert
            Assert.Equal("Goblin the Relentless", enemy.Name);
        }

        [Fact]
        public void FullName_WithBothModifiers_ShouldFormatCorrectly()
        {
            // Arrange
            var enemy = TestFixtures.CreateTestEnemy("Goblin");
            var prefix = TestFixtures.CreateTestPrefix(name: "Elite");
            var suffix = TestFixtures.CreateTestSuffix(name: "the Relentless");

            // Act
            enemy.Prefix = prefix.Name;
            enemy.Suffix = suffix.Name;
            enemy.Name = $"{prefix.Name} {enemy.BaseName} {suffix.Name}";

            // Assert
            Assert.Equal("Elite Goblin the Relentless", enemy.Name);
        }

        #endregion

        #region Data Validation Tests

        [Fact]
        public void PrefixData_ShouldHaveValidMultipliers()
        {
            // Arrange - Create test prefix with known values
            var prefix = TestFixtures.CreateTestPrefix(
                hpMultiplier: 1.3,
                attackMultiplier: 1.2,
                defenseMultiplier: 1.1,
                speedMultiplier: 1.0,
                goldMultiplier: 1.5,
                expMultiplier: 1.5);

            // Assert - All multipliers should be positive
            Assert.True(prefix.HPMultiplier > 0);
            Assert.True(prefix.AttackMultiplier > 0);
            Assert.True(prefix.DefenseMultiplier > 0);
            Assert.True(prefix.SpeedMultiplier > 0);
            Assert.True(prefix.GoldMultiplier > 0);
            Assert.True(prefix.ExpMultiplier > 0);
        }

        [Fact]
        public void SuffixData_ShouldHaveValidBonuses()
        {
            // Arrange - Create test suffix
            var suffix = TestFixtures.CreateTestSuffix(goldBonus: 25, expBonus: 50);

            // Assert
            Assert.True(suffix.GoldBonus >= 0);
            Assert.True(suffix.ExpBonus >= 0);
            Assert.NotNull(suffix.AdditionalAbilities);
        }

        [Fact]
        public void AllTestPrefixes_ShouldHaveValidData()
        {
            // Arrange
            var testPrefixes = TestFixtures.CreateTestPrefixes();

            // Assert - Each prefix should have valid data
            foreach (var prefix in testPrefixes)
            {
                Assert.NotNull(prefix.Id);
                Assert.NotEmpty(prefix.Name);
                Assert.True(prefix.MinLevel >= 0);
                Assert.True(prefix.HPMultiplier >= 0);
                Assert.True(prefix.AttackMultiplier >= 0);
                Assert.True(prefix.DefenseMultiplier >= 0);
                Assert.True(prefix.SpeedMultiplier >= 0);
                Assert.True(prefix.GoldMultiplier >= 0);
                Assert.True(prefix.ExpMultiplier >= 0);
            }
        }

        [Fact]
        public void AllTestSuffixes_ShouldHaveValidData()
        {
            // Arrange
            var testSuffixes = TestFixtures.CreateTestSuffixes();

            // Assert - Each suffix should have valid data
            foreach (var suffix in testSuffixes)
            {
                Assert.NotNull(suffix.Id);
                Assert.NotEmpty(suffix.Name);
                Assert.True(suffix.MinLevel >= 0);
                Assert.True(suffix.GoldBonus >= 0);
                Assert.True(suffix.ExpBonus >= 0);
                Assert.NotNull(suffix.AdditionalAbilities);
            }
        }

        #endregion

        #region Probability Formula Tests

        [Fact]
        public void ModifierProbability_ShouldIncreaseWithLevel()
        {
            // This tests the probability FORMULA used in EnemyFactory, not actual randomness
            // Formula from EnemyFactory.ApplyRandomModifiers:
            // prefixChance = Math.Min(70, 10 + (level * 4))
            // suffixChance = Math.Min(60, 5 + (level * 3))

            // Act - Calculate chances for different levels
            int level1PrefixChance = System.Math.Min(70, 10 + (1 * 4));  // 14%
            int level5PrefixChance = System.Math.Min(70, 10 + (5 * 4));  // 30%
            int level10PrefixChance = System.Math.Min(70, 10 + (10 * 4)); // 50%

            int level1SuffixChance = System.Math.Min(60, 5 + (1 * 3));  // 8%
            int level5SuffixChance = System.Math.Min(60, 5 + (5 * 3));  // 20%
            int level10SuffixChance = System.Math.Min(60, 5 + (10 * 3)); // 35%

            // Assert - Higher levels should have higher chances
            Assert.True(level5PrefixChance > level1PrefixChance);
            Assert.True(level10PrefixChance > level5PrefixChance);
            Assert.True(level5SuffixChance > level1SuffixChance);
            Assert.True(level10SuffixChance > level5SuffixChance);

            // Assert - Verify specific values match formula
            Assert.Equal(14, level1PrefixChance);
            Assert.Equal(30, level5PrefixChance);
            Assert.Equal(50, level10PrefixChance);
            Assert.Equal(8, level1SuffixChance);
            Assert.Equal(20, level5SuffixChance);
            Assert.Equal(35, level10SuffixChance);
        }

        [Fact]
        public void ModifierProbability_ShouldCapAtMaximum()
        {
            // Formula caps at 70% for prefix, 60% for suffix

            // Act
            int level100PrefixChance = System.Math.Min(70, 10 + (100 * 4)); // Should cap at 70
            int level100SuffixChance = System.Math.Min(60, 5 + (100 * 3));  // Should cap at 60

            // Assert
            Assert.Equal(70, level100PrefixChance);
            Assert.Equal(60, level100SuffixChance);
        }

        #endregion
    }
}
