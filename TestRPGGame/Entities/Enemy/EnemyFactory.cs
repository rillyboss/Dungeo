using System;
using System.Collections.Generic;
using System.Linq;
using TestRPGGame.DataLoading;
using TestRPGGame.Factories;

namespace TestRPGGame.Entities.Enemy
{
    public static class EnemyFactory
    {
        private static Random random = new Random();
        private static List<EnemyData>? _enemyPool = null;

        public static Enemy CreateEnemy(int playerLevel)
        {
            // Load enemy pool from data (cached)
            if (_enemyPool == null)
            {
                _enemyPool = DataLoader.GetEnemiesByLevel(0, 100).ToList();
            }

            if (_enemyPool.Count == 0)
            {
                throw new InvalidOperationException("No enemies defined in data!");
            }

            // Select random enemy template from data
            var enemyData = _enemyPool[random.Next(_enemyPool.Count)];

            // Create enemy using EntityFactory (handles scaling and variance)
            Enemy enemy = EntityFactory.CreateEnemy(enemyData, playerLevel);

            // Load abilities from data
            foreach (var abilityData in enemyData.Abilities)
            {
                try
                {
                    var ability = DataLoader.GetAbility(abilityData.AbilityId);
                    var abilityInstance = EntityFactory.CreateAbility(ability);
                    enemy.Abilities.Add(new EnemyAbility(abilityInstance, abilityData.UseThreshold));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to load ability '{abilityData.AbilityId}' for enemy '{enemyData.Name}': {ex.Message}");
                }
            }

            return enemy;
        }
    }
}
