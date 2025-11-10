using System;
using System.Collections.Generic;
using System.Linq;
using TestRPGGame.DataLoading;
using TestRPGGame.Factories;
using TestRPGGame.Combat;

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
            ApplyRandomModifiers(enemy, enemyData, playerLevel);

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

            // Create AI for this enemy
            CreateAI(enemy, enemyData);

            return enemy;
        }

        private static void ApplyRandomModifiers(Enemy enemy, EnemyData enemyData, int level)
        {
            // Chance to get prefix/suffix increases with level
            // Prefix: 20% base + 5% per level (capped at 80%)
            // Suffix: 15% base + 4% per level (capped at 70%)
            // Behavior: Rolled based on behavior spawn chances (only for regular enemies, not bosses)
            int prefixChance = Math.Min(80, 20 + (level * 5));
            int suffixChance = Math.Min(70, 15 + (level * 4));

            EnemyPrefixData? prefix = null;
            EnemySuffixData? suffix = null;
            EnemyBehaviorData? behaviorModifier = null;

            // Roll for behavior modifier (only for non-boss enemies)
            // Bosses are identified by having Phases defined in their data
            bool isBoss = (enemyData.Phases != null && enemyData.Phases.Count > 0);
            if (!isBoss)
            {
                var availableBehaviors = DataLoader.GetBehaviorsWithSpawnChance().ToList();
                if (availableBehaviors.Count > 0)
                {
                    double totalChance = availableBehaviors.Sum(b => b.SpawnChance);
                    double roll = random.NextDouble();

                    if (roll < totalChance)
                    {
                        // Weighted selection
                        double cumulative = 0;
                        foreach (var behavior in availableBehaviors)
                        {
                            cumulative += behavior.SpawnChance;
                            if (roll < cumulative)
                            {
                                behaviorModifier = behavior;
                                break;
                            }
                        }
                    }
                }
            }

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

            // Apply behavior modifier stats (only for non-boss enemies)
            if (behaviorModifier != null && !string.IsNullOrEmpty(behaviorModifier.Name))
            {
                enemy.Behavior = behaviorModifier.Name;

                // Apply stat modifiers from behavior
                if (behaviorModifier.StatModifiers.TryGetValue("Attack", out double attackMod))
                {
                    enemy.Attack = (int)(enemy.Attack * attackMod);
                }
                if (behaviorModifier.StatModifiers.TryGetValue("Defense", out double defenseMod))
                {
                    enemy.Defense = (int)(enemy.Defense * defenseMod);
                }
                if (behaviorModifier.StatModifiers.TryGetValue("Speed", out double speedMod))
                {
                    enemy.Speed = (int)(enemy.Speed * speedMod);
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
            // For regular enemies: [Behavior] [Prefix] Name [Suffix]
            // For bosses: Name only (no behavior shown)
            string fullName = enemy.BaseName;

            if (!isBoss)
            {
                // Regular enemies show behavior in name
                if (!string.IsNullOrEmpty(enemy.Behavior))
                {
                    fullName = $"{enemy.Behavior} {fullName}";
                }
            }

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

        /// <summary>
        /// Create AI for the enemy based on its data and any behavior modifiers.
        /// </summary>
        private static void CreateAI(Enemy enemy, EnemyData enemyData)
        {
            EnemyBehaviorData? behavior = null;
            List<EnemyPhaseData>? phases = null;

            // Bosses are identified by having Phases defined in their data
            bool isBoss = (enemyData.Phases != null && enemyData.Phases.Count > 0);

            // Bosses use phases (if defined) and their default behavior
            if (isBoss)
            {
                // Load phases if defined in data
                if (enemyData.Phases != null && enemyData.Phases.Count > 0)
                {
                    phases = enemyData.Phases;
                }

                // Load default behavior for boss (or use "default" if not specified)
                string behaviorId = enemyData.DefaultBehavior ?? "default";
                behavior = DataLoader.GetBehavior(behaviorId);
            }
            else
            {
                // Regular enemies: use behavior modifier if present, otherwise use default behavior
                string behaviorId;
                if (!string.IsNullOrEmpty(enemy.Behavior))
                {
                    // Enemy has a behavior modifier applied
                    behaviorId = enemy.Behavior.ToLower();
                }
                else if (!string.IsNullOrEmpty(enemyData.DefaultBehavior))
                {
                    // Enemy has a default behavior in data
                    behaviorId = enemyData.DefaultBehavior;
                }
                else
                {
                    // Fall back to "default" behavior
                    behaviorId = "default";
                }

                behavior = DataLoader.GetBehavior(behaviorId);
            }

            // Create AI with behavior and phases
            enemy.AI = new EnemyAI(behavior, phases);
        }
    }
}
