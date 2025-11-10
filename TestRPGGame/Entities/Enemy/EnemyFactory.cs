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
            // Load enemy pool from data (cached), excluding bosses
            if (_enemyPool == null)
            {
                // Get all enemies but exclude bosses (IsBoss = true)
                _enemyPool = DataLoader.GetEnemiesByLevel(0, 100)
                    .Where(e => !e.IsBoss)
                    .ToList();
            }

            if (_enemyPool.Count == 0)
            {
                throw new InvalidOperationException("No enemies defined in data!");
            }

            // Filter enemies by MinLevel requirement
            var availableEnemies = _enemyPool.Where(e => playerLevel >= e.MinLevel).ToList();

            if (availableEnemies.Count == 0)
            {
                throw new InvalidOperationException($"No enemies available for player level {playerLevel}!");
            }

            // Select random enemy template from available enemies
            var enemyData = availableEnemies[random.Next(availableEnemies.Count)];

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
            // Chance to get modifiers increases with level, but limited at low levels
            // At level 1-2: rarely get multiple modifiers
            // At level 5+: can get 2-3 modifiers more commonly
            // At level 10+: can get all 3 modifiers

            int prefixChance = Math.Min(70, 10 + (level * 4));  // 10-70% (reduced from 20-80%)
            int suffixChance = Math.Min(60, 5 + (level * 3));   // 5-60% (reduced from 15-70%)

            EnemyPrefixData? prefix = null;
            EnemySuffixData? suffix = null;
            EnemyBehaviorData? behaviorModifier = null;
            int modifierCount = 0;
            int maxModifiers = Math.Min(3, 1 + (level / 3)); // Level 1-2: max 1, Level 3-5: max 2, Level 6+: max 3

            // Roll for behavior modifier (only for non-boss enemies)
            bool isBoss = enemyData.IsBoss;
            if (!isBoss && level >= 2) // Behaviors only appear at level 2+
            {
                var availableBehaviors = DataLoader.GetBehaviorsWithSpawnChance().ToList();
                if (availableBehaviors.Count > 0 && modifierCount < maxModifiers)
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
                                modifierCount++;
                                break;
                            }
                        }
                    }
                }
            }

            // Roll for prefix
            if (modifierCount < maxModifiers && random.Next(100) < prefixChance)
            {
                var availablePrefixes = DataLoader.GetEnemyPrefixesByLevel(level).ToList();
                if (availablePrefixes.Count > 0)
                {
                    prefix = availablePrefixes[random.Next(availablePrefixes.Count)];
                    modifierCount++;
                }
            }

            // Roll for suffix (less likely if already has prefix)
            if (modifierCount < maxModifiers && random.Next(100) < suffixChance)
            {
                var availableSuffixes = DataLoader.GetEnemySuffixesByLevel(level).ToList();
                if (availableSuffixes.Count > 0)
                {
                    suffix = availableSuffixes[random.Next(availableSuffixes.Count)];
                    modifierCount++;
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

            // Bosses use phases (if defined) and their default behavior
            if (enemyData.IsBoss)
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
