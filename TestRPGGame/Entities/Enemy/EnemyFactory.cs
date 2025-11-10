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

            // Add abilities to make combat interesting
            if (random.Next(100) < 50) // 50% chance for special ability
            {
                string[] abilityNames = { "Fierce Strike", "Rage", "Heavy Blow", "Quick Attack", "Power Up" };
                enemy.Abilities.Add(new EnemyAbility(
                    abilityNames[random.Next(abilityNames.Length)],
                    1.5 + (random.NextDouble() * 0.5),
                    3 + random.Next(0, 2)
                ));
            }

            return enemy;
        }
    }
}
