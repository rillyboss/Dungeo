using Xunit;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.DataLoading;

namespace TestRPGGame.Tests
{
    public class EnemyLevelRequirementTests : TestBase
    {
        [Fact]
        public void Level1Player_OnlyEncountersTier1Enemies()
        {
            // Arrange - Create 20 enemies for a level 1 player
            var enemies = new List<Enemy>();
            for (int i = 0; i < 20; i++)
            {
                enemies.Add(EnemyFactory.CreateEnemy(1));
            }

            // Act - Get unique base enemy types encountered
            var enemyTypes = enemies.Select(e => e.BaseName).Distinct().ToList();

            // Assert - Only Tier 1 enemies (MinLevel 1)
            // Tier 1: Goblin, Bandit, Skeleton Warrior, Wild Beast
            var tier1Enemies = new[] { "Goblin", "Bandit", "Skeleton Warrior", "Wild Beast" };

            foreach (var enemyType in enemyTypes)
            {
                Assert.Contains(enemyType, tier1Enemies);
            }

            // Should not encounter higher tier enemies
            Assert.DoesNotContain("Orc", enemyTypes); // Tier 2 (MinLevel 3)
            Assert.DoesNotContain("Shadow Assassin", enemyTypes); // Tier 3 (MinLevel 5)
            Assert.DoesNotContain("Fire Elemental", enemyTypes); // Tier 4 (MinLevel 7)
        }

        [Fact]
        public void Level3Player_EncountersTier1And2Enemies()
        {
            // Arrange - Create 30 enemies for a level 3 player
            var enemies = new List<Enemy>();
            for (int i = 0; i < 30; i++)
            {
                enemies.Add(EnemyFactory.CreateEnemy(3));
            }

            // Act - Get unique base enemy types
            var enemyTypes = enemies.Select(e => e.BaseName).Distinct().ToList();

            // Assert - Can encounter Tier 1 and Tier 2 enemies
            // All encountered enemies should have MinLevel <= 3
            foreach (var enemy in enemies)
            {
                var enemyData = DataLoader.GetEnemy(enemy.BaseName.ToLower().Replace(" ", "_"));
                Assert.True(enemyData.MinLevel <= 3,
                    $"{enemy.BaseName} has MinLevel {enemyData.MinLevel} but player is level 3");
            }

            // Should not encounter Tier 3+ enemies
            Assert.DoesNotContain("Shadow Assassin", enemyTypes); // MinLevel 5
            Assert.DoesNotContain("Dark Knight", enemyTypes); // MinLevel 5
            Assert.DoesNotContain("Fire Elemental", enemyTypes); // MinLevel 7
        }

        [Fact]
        public void Level5Player_EncountersTier1Through3Enemies()
        {
            // Arrange - Create 40 enemies for a level 5 player
            var enemies = new List<Enemy>();
            for (int i = 0; i < 40; i++)
            {
                enemies.Add(EnemyFactory.CreateEnemy(5));
            }

            // Act - Verify all encountered enemies have MinLevel <= 5
            foreach (var enemy in enemies)
            {
                var enemyData = DataLoader.GetEnemy(enemy.BaseName.ToLower().Replace(" ", "_"));
                Assert.True(enemyData.MinLevel <= 5,
                    $"{enemy.BaseName} has MinLevel {enemyData.MinLevel} but player is level 5");
            }

            // Assert - Get unique types
            var enemyTypes = enemies.Select(e => e.BaseName).Distinct().ToList();

            // Should not encounter Tier 4 enemies
            Assert.DoesNotContain("Fire Elemental", enemyTypes); // MinLevel 7
            Assert.DoesNotContain("Dragon Whelp", enemyTypes); // MinLevel 7
        }

        [Fact]
        public void Level7Player_CanEncounterAllEnemyTiers()
        {
            // Arrange - Create 50 enemies for a level 7 player
            var enemies = new List<Enemy>();
            for (int i = 0; i < 50; i++)
            {
                enemies.Add(EnemyFactory.CreateEnemy(7));
            }

            // Act - Verify all encountered enemies have MinLevel <= 7
            foreach (var enemy in enemies)
            {
                var enemyData = DataLoader.GetEnemy(enemy.BaseName.ToLower().Replace(" ", "_"));
                Assert.True(enemyData.MinLevel <= 7,
                    $"{enemy.BaseName} has MinLevel {enemyData.MinLevel} but player is level 7");
            }

            // Assert - Level 7 players should have access to all enemy types
            // (This is a weak assertion - just verifies no invalid enemies appear)
            Assert.True(enemies.Count == 50, "Should create 50 enemies successfully");
        }

        [Fact]
        public void EnemyFactory_RespectsMinLevelFiltering()
        {
            // Arrange & Act - Create enemies at various levels
            var level1Enemy = EnemyFactory.CreateEnemy(1);
            var level3Enemy = EnemyFactory.CreateEnemy(3);
            var level5Enemy = EnemyFactory.CreateEnemy(5);
            var level7Enemy = EnemyFactory.CreateEnemy(7);

            // Assert - All enemies should be valid for their respective player levels
            Assert.NotNull(level1Enemy);
            Assert.NotNull(level3Enemy);
            Assert.NotNull(level5Enemy);
            Assert.NotNull(level7Enemy);

            // Verify enemies have appropriate MinLevel
            var level1Data = DataLoader.GetEnemy(level1Enemy.BaseName.ToLower().Replace(" ", "_"));
            Assert.True(level1Data.MinLevel <= 1);

            var level3Data = DataLoader.GetEnemy(level3Enemy.BaseName.ToLower().Replace(" ", "_"));
            Assert.True(level3Data.MinLevel <= 3);

            var level5Data = DataLoader.GetEnemy(level5Enemy.BaseName.ToLower().Replace(" ", "_"));
            Assert.True(level5Data.MinLevel <= 5);

            var level7Data = DataLoader.GetEnemy(level7Enemy.BaseName.ToLower().Replace(" ", "_"));
            Assert.True(level7Data.MinLevel <= 7);
        }

        [Fact]
        public void AllEnemies_HaveValidMinLevel()
        {
            // Arrange - Load all enemies from data
            var allEnemies = DataLoader.GetEnemiesByLevel(0, 100).ToList();

            // Act & Assert - All enemies should have MinLevel >= 1
            foreach (var enemyData in allEnemies)
            {
                Assert.True(enemyData.MinLevel >= 1,
                    $"Enemy '{enemyData.Name}' has invalid MinLevel {enemyData.MinLevel}. Should be >= 1.");
                Assert.True(enemyData.MinLevel <= 10,
                    $"Enemy '{enemyData.Name}' has MinLevel {enemyData.MinLevel} which seems too high.");
            }
        }
    }
}
