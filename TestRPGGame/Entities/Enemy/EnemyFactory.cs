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

            // Apply random modifiers based on level
            ApplyRandomModifiers(enemy, playerLevel);

            // Load base abilities from data
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

        private static void ApplyRandomModifiers(Enemy enemy, int level)
        {
            // Chance to get prefix/suffix increases with level
            // Prefix: 20% base + 5% per level (capped at 80%)
            // Suffix: 15% base + 4% per level (capped at 70%)
            int prefixChance = Math.Min(80, 20 + (level * 5));
            int suffixChance = Math.Min(70, 15 + (level * 4));

            EnemyPrefixData? prefix = null;
            EnemySuffixData? suffix = null;

            // Roll for prefix
            if (random.Next(100) < prefixChance)
            {
                var availablePrefixes = DataLoader.GetEnemyPrefixesByLevel(level).ToList();
                if (availablePrefixes.Count > 0)
                {
                    prefix = availablePrefixes[random.Next(availablePrefixes.Count)];
                }
            }

            // Roll for suffix
            if (random.Next(100) < suffixChance)
            {
                var availableSuffixes = DataLoader.GetEnemySuffixesByLevel(level).ToList();
                if (availableSuffixes.Count > 0)
                {
                    suffix = availableSuffixes[random.Next(availableSuffixes.Count)];
                }
            }

            // Apply prefix modifiers
            if (prefix != null && !string.IsNullOrEmpty(prefix.Name))
            {
                enemy.Prefix = prefix.Name;
                enemy.MaxHP = (int)(enemy.MaxHP * prefix.HPMultiplier);
                enemy.CurrentHP = enemy.MaxHP;
                enemy.Attack = (int)(enemy.Attack * prefix.AttackMultiplier);
                enemy.Defense = (int)(enemy.Defense * prefix.DefenseMultiplier);
                enemy.Speed = (int)(enemy.Speed * prefix.SpeedMultiplier);
                enemy.GoldReward = (int)(enemy.GoldReward * prefix.GoldMultiplier);
                enemy.ExpReward = (int)(enemy.ExpReward * prefix.ExpMultiplier);
            }

            // Apply suffix bonuses and abilities
            if (suffix != null && !string.IsNullOrEmpty(suffix.Name))
            {
                enemy.Suffix = suffix.Name;
                enemy.GoldReward += suffix.GoldBonus;
                enemy.ExpReward += suffix.ExpBonus;

                // Add additional abilities from suffix
                foreach (var abilityId in suffix.AdditionalAbilities)
                {
                    try
                    {
                        var ability = DataLoader.GetAbility(abilityId);
                        var abilityInstance = EntityFactory.CreateAbility(ability);
                        // Suffix abilities can be used at any HP
                        enemy.Abilities.Add(new EnemyAbility(abilityInstance, 100));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Failed to load suffix ability '{abilityId}': {ex.Message}");
                    }
                }
            }

            // Build full name with modifiers
            string fullName = enemy.BaseName;
            if (!string.IsNullOrEmpty(enemy.Prefix))
            {
                fullName = $"{enemy.Prefix} {fullName}";
            }
            if (!string.IsNullOrEmpty(enemy.Suffix))
            {
                fullName = $"{fullName} {enemy.Suffix}";
            }
            enemy.Name = fullName;
        }
    }
}
