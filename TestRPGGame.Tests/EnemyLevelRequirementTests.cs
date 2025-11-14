using System.Collections.Generic;
using System.Linq;
using Xunit;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.DataLoading;
using TestRPGGame.Interfaces;

namespace TestRPGGame.Tests
{
    public class EnemyLevelRequirementTests : TestBase
    {
        [Fact]
        public void Level1Player_OnlyEncountersTier1Enemies()
        {
            // Arrange - Create mock repository with enemies at different levels
            var enemies = new List<EnemyData>
            {
                // Tier 1 enemies (MinLevel 1)
                TestFixtures.CreateTestEnemyData("goblin", "Goblin", minLevel: 1),
                TestFixtures.CreateTestEnemyData("bandit", "Bandit", minLevel: 1),
                TestFixtures.CreateTestEnemyData("skeleton", "Skeleton", minLevel: 1),

                // Tier 2 enemies (MinLevel 3) - should not appear
                TestFixtures.CreateTestEnemyData("orc", "Orc", minLevel: 3),
                TestFixtures.CreateTestEnemyData("troll", "Troll", minLevel: 3),

                // Tier 3 enemies (MinLevel 5) - should not appear
                TestFixtures.CreateTestEnemyData("shadow_assassin", "Shadow Assassin", minLevel: 5),

                // Tier 4 enemies (MinLevel 7) - should not appear
                TestFixtures.CreateTestEnemyData("fire_elemental", "Fire Elemental", minLevel: 7)
            };

            var mockRepo = TestFixtures.CreateMockRepository(enemies: enemies);
            EnemyFactory.SetRepository(mockRepo.Object);
            EnemyFactory.SetLogger(new NullLogger());

            // Act - Create 20 enemies for level 1 player
            var createdEnemies = new List<Enemy>();
            for (int i = 0; i < 20; i++)
            {
                createdEnemies.Add(EnemyFactory.CreateEnemy(1));
            }

            // Assert - All enemies should be from the valid pool (MinLevel <= 1)
            var validNames = new[] { "Goblin", "Bandit", "Skeleton" };
            foreach (var enemy in createdEnemies)
            {
                Assert.Contains(enemy.BaseName, validNames);
            }

            // Get unique enemy types encountered
            var encounteredTypes = createdEnemies.Select(e => e.BaseName).Distinct().ToList();

            // Should only encounter Tier 1 enemies
            var tier1Names = new[] { "Goblin", "Bandit", "Skeleton" };
            foreach (var type in encounteredTypes)
            {
                Assert.Contains(type, tier1Names);
            }

            // Should NOT encounter higher tier enemies
            Assert.DoesNotContain("Orc", encounteredTypes);
            Assert.DoesNotContain("Shadow Assassin", encounteredTypes);
            Assert.DoesNotContain("Fire Elemental", encounteredTypes);
        }

        [Fact]
        public void Level3Player_EncountersTier1And2Enemies()
        {
            // Arrange - Create test enemies
            var enemies = new List<EnemyData>
            {
                // Tier 1 (MinLevel 1) - should appear
                TestFixtures.CreateTestEnemyData("goblin", "Goblin", minLevel: 1),
                TestFixtures.CreateTestEnemyData("bandit", "Bandit", minLevel: 1),

                // Tier 2 (MinLevel 3) - should appear
                TestFixtures.CreateTestEnemyData("orc", "Orc", minLevel: 3),
                TestFixtures.CreateTestEnemyData("dark_knight", "Dark Knight", minLevel: 3),

                // Tier 3 (MinLevel 5) - should NOT appear
                TestFixtures.CreateTestEnemyData("shadow_assassin", "Shadow Assassin", minLevel: 5),

                // Tier 4 (MinLevel 7) - should NOT appear
                TestFixtures.CreateTestEnemyData("fire_elemental", "Fire Elemental", minLevel: 7)
            };

            var mockRepo = TestFixtures.CreateMockRepository(enemies: enemies);
            EnemyFactory.SetRepository(mockRepo.Object);
            EnemyFactory.SetLogger(new NullLogger());

            // Act - Create 30 enemies for level 3 player
            var createdEnemies = new List<Enemy>();
            for (int i = 0; i < 30; i++)
            {
                createdEnemies.Add(EnemyFactory.CreateEnemy(3));
            }

            // Assert - All enemies should be from valid pool (MinLevel <= 3)
            var validNames = new[] { "Goblin", "Bandit", "Orc", "Dark Knight" };
            foreach (var enemy in createdEnemies)
            {
                Assert.Contains(enemy.BaseName, validNames);
            }

            // Get unique enemy types
            var encounteredTypes = createdEnemies.Select(e => e.BaseName).Distinct().ToList();

            // Should NOT encounter Tier 3+ enemies
            Assert.DoesNotContain("Shadow Assassin", encounteredTypes);
            Assert.DoesNotContain("Fire Elemental", encounteredTypes);
        }

        [Fact]
        public void Level5Player_EncountersTier1Through3Enemies()
        {
            // Arrange
            var enemies = new List<EnemyData>
            {
                TestFixtures.CreateTestEnemyData("goblin", "Goblin", minLevel: 1),
                TestFixtures.CreateTestEnemyData("orc", "Orc", minLevel: 3),
                TestFixtures.CreateTestEnemyData("shadow_assassin", "Shadow Assassin", minLevel: 5),
                TestFixtures.CreateTestEnemyData("dark_knight", "Dark Knight", minLevel: 5),

                // Tier 4 (MinLevel 7) - should NOT appear
                TestFixtures.CreateTestEnemyData("fire_elemental", "Fire Elemental", minLevel: 7),
                TestFixtures.CreateTestEnemyData("dragon_whelp", "Dragon Whelp", minLevel: 7)
            };

            var mockRepo = TestFixtures.CreateMockRepository(enemies: enemies);
            EnemyFactory.SetRepository(mockRepo.Object);
            EnemyFactory.SetLogger(new NullLogger());

            // Act - Create 40 enemies for level 5 player
            var createdEnemies = new List<Enemy>();
            for (int i = 0; i < 40; i++)
            {
                createdEnemies.Add(EnemyFactory.CreateEnemy(5));
            }

            // Assert - All enemies should be from valid pool (MinLevel <= 5)
            var validNames = new[] { "Goblin", "Orc", "Shadow Assassin", "Dark Knight" };
            foreach (var enemy in createdEnemies)
            {
                Assert.Contains(enemy.BaseName, validNames);
            }

            // Get unique types
            var encounteredTypes = createdEnemies.Select(e => e.BaseName).Distinct().ToList();

            // Should NOT encounter Tier 4 enemies
            Assert.DoesNotContain("Fire Elemental", encounteredTypes);
            Assert.DoesNotContain("Dragon Whelp", encounteredTypes);
        }

        [Fact]
        public void Level7Player_CanEncounterAllEnemyTiers()
        {
            // Arrange - Create enemies spanning all tiers
            var enemies = new List<EnemyData>
            {
                TestFixtures.CreateTestEnemyData("goblin", "Goblin", minLevel: 1),
                TestFixtures.CreateTestEnemyData("orc", "Orc", minLevel: 3),
                TestFixtures.CreateTestEnemyData("shadow_assassin", "Shadow Assassin", minLevel: 5),
                TestFixtures.CreateTestEnemyData("fire_elemental", "Fire Elemental", minLevel: 7),
                TestFixtures.CreateTestEnemyData("dragon_whelp", "Dragon Whelp", minLevel: 7)
            };

            var mockRepo = TestFixtures.CreateMockRepository(enemies: enemies);
            EnemyFactory.SetRepository(mockRepo.Object);
            EnemyFactory.SetLogger(new NullLogger());

            // Act - Create 50 enemies for level 7 player
            var createdEnemies = new List<Enemy>();
            for (int i = 0; i < 50; i++)
            {
                createdEnemies.Add(EnemyFactory.CreateEnemy(7));
            }

            // Assert - All enemies should be from valid pool (MinLevel <= 7)
            var validNames = new[] { "Goblin", "Orc", "Shadow Assassin", "Fire Elemental", "Dragon Whelp" };
            foreach (var enemy in createdEnemies)
            {
                Assert.Contains(enemy.BaseName, validNames);
            }

            // Level 7 players should successfully create all requested enemies
            Assert.Equal(50, createdEnemies.Count);
        }

        [Fact]
        public void EnemyFactory_RespectsMinLevelFiltering()
        {
            // Arrange - Create enemies with specific MinLevel values
            var enemies = new List<EnemyData>
            {
                TestFixtures.CreateTestEnemyData("tier1_enemy", "Tier 1 Enemy", minLevel: 1),
                TestFixtures.CreateTestEnemyData("tier2_enemy", "Tier 2 Enemy", minLevel: 3),
                TestFixtures.CreateTestEnemyData("tier3_enemy", "Tier 3 Enemy", minLevel: 5),
                TestFixtures.CreateTestEnemyData("tier4_enemy", "Tier 4 Enemy", minLevel: 7)
            };

            var mockRepo = TestFixtures.CreateMockRepository(enemies: enemies);
            EnemyFactory.SetRepository(mockRepo.Object);
            EnemyFactory.SetLogger(new NullLogger());

            // Act - Create enemies at various player levels
            var level1Enemy = EnemyFactory.CreateEnemy(1);
            var level3Enemy = EnemyFactory.CreateEnemy(3);
            var level5Enemy = EnemyFactory.CreateEnemy(5);
            var level7Enemy = EnemyFactory.CreateEnemy(7);

            // Assert - All enemies should be created successfully
            Assert.NotNull(level1Enemy);
            Assert.NotNull(level3Enemy);
            Assert.NotNull(level5Enemy);
            Assert.NotNull(level7Enemy);

            // Assert - Verify enemies come from appropriate tier for their player level
            Assert.Equal("Tier 1 Enemy", level1Enemy.BaseName);
            Assert.Contains(level3Enemy.BaseName, new[] { "Tier 1 Enemy", "Tier 2 Enemy" });
            Assert.Contains(level5Enemy.BaseName, new[] { "Tier 1 Enemy", "Tier 2 Enemy", "Tier 3 Enemy" });
            Assert.Contains(level7Enemy.BaseName, new[] { "Tier 1 Enemy", "Tier 2 Enemy", "Tier 3 Enemy", "Tier 4 Enemy" });
        }

        [Fact]
        public void AllEnemies_HaveValidMinLevel()
        {
            // Arrange - Create test enemies with various MinLevel values
            var enemies = new List<EnemyData>
            {
                TestFixtures.CreateTestEnemyData("enemy1", "Enemy 1", minLevel: 1),
                TestFixtures.CreateTestEnemyData("enemy2", "Enemy 2", minLevel: 3),
                TestFixtures.CreateTestEnemyData("enemy3", "Enemy 3", minLevel: 5),
                TestFixtures.CreateTestEnemyData("enemy4", "Enemy 4", minLevel: 7),
                TestFixtures.CreateTestEnemyData("enemy5", "Enemy 5", minLevel: 10)
            };

            var mockRepo = TestFixtures.CreateMockRepository(enemies: enemies);

            // Act - Get all enemies from repository
            var allEnemies = mockRepo.Object.GetEnemiesByLevel(0, 100).ToList();

            // Assert - All enemies should have MinLevel in valid range
            foreach (var enemyData in allEnemies)
            {
                Assert.True(enemyData.MinLevel >= 1,
                    $"Enemy '{enemyData.Name}' has invalid MinLevel {enemyData.MinLevel}. Should be >= 1.");
                Assert.True(enemyData.MinLevel <= 10,
                    $"Enemy '{enemyData.Name}' has MinLevel {enemyData.MinLevel} which seems too high.");
            }
        }

        [Fact]
        public void EnemyFactory_FiltersByPlayerLevel_LowerBound()
        {
            // Arrange - Create enemies with specific thresholds
            var enemies = new List<EnemyData>
            {
                TestFixtures.CreateTestEnemyData("weak", "Weak Enemy", minLevel: 1),
                TestFixtures.CreateTestEnemyData("medium", "Medium Enemy", minLevel: 5),
                TestFixtures.CreateTestEnemyData("strong", "Strong Enemy", minLevel: 10)
            };

            var mockRepo = TestFixtures.CreateMockRepository(enemies: enemies);
            EnemyFactory.SetRepository(mockRepo.Object);
            EnemyFactory.SetLogger(new NullLogger());

            // Act - Create enemy for level 4 player (below medium threshold)
            var level4Enemies = new List<Enemy>();
            for (int i = 0; i < 20; i++)
            {
                level4Enemies.Add(EnemyFactory.CreateEnemy(4));
            }

            // Assert - Should only get weak enemies (from tier with MinLevel 1)
            foreach (var enemy in level4Enemies)
            {
                Assert.Equal("Weak Enemy", enemy.BaseName);
            }
        }

        [Fact]
        public void EnemyFactory_FiltersByPlayerLevel_ExactMatch()
        {
            // Arrange - Enemy with MinLevel exactly matching player level
            var enemies = new List<EnemyData>
            {
                TestFixtures.CreateTestEnemyData("exact_match", "Exact Match", minLevel: 5)
            };

            var mockRepo = TestFixtures.CreateMockRepository(enemies: enemies);
            EnemyFactory.SetRepository(mockRepo.Object);
            EnemyFactory.SetLogger(new NullLogger());

            // Act - Create enemy for level 5 player
            var enemy = EnemyFactory.CreateEnemy(5);

            // Assert - Should successfully create enemy with exact level match
            Assert.NotNull(enemy);
            Assert.Equal("Exact Match", enemy.BaseName);
        }

        [Fact]
        public void EnemyFactory_ThrowsWhenNoEnemiesAvailable()
        {
            // Arrange - Create enemies that are all too high level
            var enemies = new List<EnemyData>
            {
                TestFixtures.CreateTestEnemyData("high_level_1", "High Level 1", minLevel: 10),
                TestFixtures.CreateTestEnemyData("high_level_2", "High Level 2", minLevel: 15)
            };

            var mockRepo = TestFixtures.CreateMockRepository(enemies: enemies);
            EnemyFactory.SetRepository(mockRepo.Object);
            EnemyFactory.SetLogger(new NullLogger());

            // Act & Assert - Should throw when no valid enemies for level 1 player
            var exception = Assert.Throws<System.InvalidOperationException>(() =>
                EnemyFactory.CreateEnemy(1)
            );

            Assert.Contains("No enemies available for player level 1", exception.Message);
        }

        [Fact]
        public void MinLevel_Filtering_IsInclusive()
        {
            // Arrange - Test that filtering logic is inclusive (>=, not >)
            var enemies = new List<EnemyData>
            {
                TestFixtures.CreateTestEnemyData("level_3_enemy", "Level 3 Enemy", minLevel: 3)
            };

            var mockRepo = TestFixtures.CreateMockRepository(enemies: enemies);
            EnemyFactory.SetRepository(mockRepo.Object);
            EnemyFactory.SetLogger(new NullLogger());

            // Act - Player at exactly MinLevel should be able to encounter the enemy
            var enemy = EnemyFactory.CreateEnemy(3);

            // Assert - Should successfully create enemy at exact level match
            Assert.NotNull(enemy);
            Assert.Equal("Level 3 Enemy", enemy.BaseName);
        }
    }
}
