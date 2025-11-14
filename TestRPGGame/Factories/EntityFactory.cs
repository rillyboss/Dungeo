using System;
using System.Collections.Generic;
using System.Linq;
using TestRPGGame.DataLoading;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.Entities.Dungeon;
using TestRPGGame.Abilities;
using TestRPGGame.Abilities.Effects;
using TestRPGGame.Abilities.Applicators;
using TestRPGGame.Combat;
using TestRPGGame.Equipment;
using TestRPGGame.Utils;

namespace TestRPGGame.Factories
{
    /// <summary>
    /// Factory for creating game entities from data definitions.
    /// The code has zero knowledge of specific entities - it only knows how to interpret data.
    /// </summary>
    public static class EntityFactory
    {
        private static IDataRepository _repository = new JsonDataRepository(); // Default repository

        /// <summary>
        /// Sets the data repository. Use for dependency injection (e.g., mock repository for tests).
        /// </summary>
        public static void SetRepository(IDataRepository repository)
        {
            _repository = repository;
        }

        #region Ability Factory

        public static Ability CreateAbility(AbilityData data)
        {
            if (!Enum.TryParse<AbilityType>(data.Type, out var abilityType))
            {
                throw new ArgumentException($"Invalid ability type: {data.Type}");
            }

            var ability = new Ability(
                data.Name,
                data.ManaCost,
                data.Cooldown,
                data.Description,
                abilityType,
                data.UnlockLevel,
                (int)(data.PurchaseCost * Systems.GameConfig.Config.AbilityPurchasePriceMultiplier),
                data.Id
            )
            {
                IsUnlocked = data.IsStarting,
                Priority = data.Priority
            };

            // Convert data effects to game effects
            foreach (var effectData in data.Effects)
            {
                var effect = CreateAbilityEffect(effectData);
                if (effect != null)
                {
                    ability.Effects.Add(effect);
                }
            }

            return ability;
        }

        private static IAbilityEffect? CreateAbilityEffect(AbilityEffectData data)
        {
            // All effects now use the composable EffectApplicator system
            if (data.Type.ToLower() == "effect" && !string.IsNullOrEmpty(data.EffectKind))
            {
                return CreateComposableEffect(data);
            }

            // No legacy effects remain - all should be migrated to Effect format
            return null;
        }

        /// <summary>
        /// Creates a composable effect using the new data-driven system.
        /// Parses EffectKind (generic type) and creates effect with custom DisplayName.
        /// </summary>
        private static IAbilityEffect? CreateComposableEffect(AbilityEffectData data)
        {
            if (string.IsNullOrEmpty(data.EffectKind))
                return null;

            // Parse the generic effect kind
            if (!Enum.TryParse<Constants.EffectKind>(data.EffectKind, true, out var effectKind))
            {
                throw new ArgumentException($"Unknown EffectKind: {data.EffectKind}");
            }

            // Create the effect applicator with custom display name and parameters
            var applicator = new EffectApplicator(
                effectKind,
                data.DisplayName ?? data.EffectKind, // Use EffectKind as fallback
                data.Duration,
                data.Multiplier,
                data.Value
            );

            // Set random damage range if specified (e.g., 2.2x-2.8x)
            if (data.MinMultiplier > 0 && data.MaxMultiplier > 0)
            {
                applicator.MinMultiplier = data.MinMultiplier;
                applicator.MaxMultiplier = data.MaxMultiplier;
            }

            // Parse composite effects if present (e.g., Banner = AttackBoost + DefenseBoost + SpeedBoost)
            if (data.CompositeEffects != null && data.CompositeEffects.Count > 0)
            {
                applicator.CompositeEffects = new List<(Constants.EffectKind, double, int)>();
                foreach (var comp in data.CompositeEffects)
                {
                    if (Enum.TryParse<Constants.EffectKind>(comp.EffectKind, true, out var compKind))
                    {
                        applicator.CompositeEffects.Add((compKind, comp.Multiplier, comp.FlatValue));
                    }
                }
            }

            return applicator;
        }

        #endregion

        #region Enemy Factory

        public static Enemy CreateEnemy(EnemyData data, int playerLevel)
        {
            if (!Enum.TryParse<EnemyType>(data.Type, out var enemyType))
            {
                throw new ArgumentException($"Invalid enemy type: {data.Type}");
            }

            // Apply stats - if base stats are provided, use them, otherwise scale with level
            int level = data.Level > 0 ? data.Level : playerLevel;
            int maxHP = data.MaxHP > 0 ? data.MaxHP : 60 + (level * 15);
            int attack = data.Attack > 0 ? data.Attack : 8 + (level * 2);
            int defense = data.Defense > 0 ? data.Defense : 3 + level;
            int speed = data.Speed > 0 ? data.Speed : 5 + level;
            int goldReward = data.GoldReward > 0 ? data.GoldReward : 30 + (level * 10);
            int expReward = data.ExpReward > 0 ? data.ExpReward : 50 + (level * 20);

            // Add random variance for regular enemies (scaling enemies)
            if (data.Level == 0) // Level 0 indicates scaling enemy
            {
                maxHP += RandomProvider.Next(-10, 11);
                attack += RandomProvider.Next(-2, 4);
                defense += RandomProvider.Next(-1, 3);
                speed += RandomProvider.Next(-2, 4);
                goldReward += RandomProvider.Next(-5, 16);
            }

            var enemy = new Enemy(data.Name, enemyType, maxHP, attack, defense, speed, goldReward, expReward);

            return enemy;
        }

        #endregion

        #region Dungeon Factory

        public static Dungeon CreateDungeon(DungeonData data)
        {
            if (!Enum.TryParse<DungeonType>(data.Id.Replace("_", ""), true, out var dungeonType))
            {
                // Fallback to first dungeon type if parsing fails
                dungeonType = DungeonType.GoblinCaves;
            }

            // Use DisplayName if provided, otherwise fall back to Name
            string displayName = !string.IsNullOrEmpty(data.DisplayName) ? data.DisplayName : data.Name;
            var dungeon = new Dungeon(displayName, dungeonType, data.Description, data.RecommendedLevel);

            // Set requirements
            dungeon.Requirements = new DungeonRequirements
            {
                MinLevel = data.RecommendedLevel,
                GoldCost = CalculateDungeonCost(data.RecommendedLevel),
                PreviousDungeonRequired = data.RequiredDungeonIds.FirstOrDefault()
            };

            // Set rewards
            dungeon.MinibossReward = new DungeonReward
            {
                GoldMin = (int)(data.Rewards.MinibossGold * 0.8),
                GoldMax = (int)(data.Rewards.MinibossGold * 1.2),
                Experience = data.RecommendedLevel * 150,
                GuaranteedLootCount = Math.Max(1, data.RecommendedLevel / 3),
                MinLootRarity = Math.Min(1, data.RecommendedLevel / 3)
            };

            dungeon.BossReward = new DungeonReward
            {
                GoldMin = (int)(data.Rewards.BossGold * 0.8),
                GoldMax = (int)(data.Rewards.BossGold * 1.2),
                Experience = data.RecommendedLevel * 250,
                GuaranteedLootCount = Math.Max(2, data.RecommendedLevel / 2),
                MinLootRarity = Math.Min(2, data.RecommendedLevel / 3)
            };

            // Create encounters (legacy fixed encounters)
            dungeon.Encounters = new List<DungeonEncounter>();
            foreach (var encounterData in data.Encounters)
            {
                var encounter = CreateDungeonEncounter(encounterData);
                dungeon.Encounters.Add(encounter);
            }

            // Create encounter pool for random generation
            dungeon.EncounterPool = new List<DungeonEncounter>();
            foreach (var encounterData in data.EncounterPool)
            {
                var encounter = CreateDungeonEncounter(encounterData);
                dungeon.EncounterPool.Add(encounter);
            }

            // Copy encounter configuration
            dungeon.EncounterConfig = data.EncounterConfig;

            // Load miniboss and boss using unified enemy system
            var minibossData = _repository.GetEnemy(data.MinibossId);
            var bossData = _repository.GetEnemy(data.BossId);

            dungeon.Miniboss = CreateEnemy(minibossData, data.RecommendedLevel);
            dungeon.Boss = CreateEnemy(bossData, data.RecommendedLevel);
            dungeon.Difficulty = data.RecommendedLevel;

            return dungeon;
        }

        private static int CalculateDungeonCost(int level)
        {
            if (level <= 1) return 0;
            if (level <= 3) return 50;
            if (level <= 6) return 100;
            if (level <= 9) return 200;
            return 500;
        }

        private static DungeonEncounter CreateDungeonEncounter(DungeonEncounterData data)
        {
            var encounter = new DungeonEncounter(data.Text ?? "");

            if (data.Choices != null)
            {
                // Store all choices for potential random selection
                encounter.AllChoices = data.Choices;
                encounter.RandomChoiceCount = data.RandomChoiceCount;

                // Pre-populate with all choices (will be randomized at runtime if needed)
                foreach (var choice in data.Choices)
                {
                    encounter.Choices.Add(choice.Text);
                    string friendlyEffect = GetFriendlyEffectName(choice.Effect);
                    encounter.ChoiceResults.Add($"{friendlyEffect}: {choice.Value}");
                }
            }

            encounter.IsCombat = data.Type?.ToLower() == "combat";

            return encounter;
        }

        private static string GetFriendlyEffectName(string effect)
        {
            return effect switch
            {
                "TakeDamage" => "Take Damage",
                "GainGold" => "Gain Gold",
                "GainGoldAndMana" => "Gain Gold and Mana",
                "RestoreManaAndTakeDamage" => "Restore Mana but Take Damage",
                "TakeDamageAndLoseMana" => "Take Damage and Lose Mana",
                "Heal" => "Heal",
                "RestoreMana" => "Restore Mana",
                "GainExperience" => "Gain Experience",
                "FindItem" => "Find Item",
                "Combat" => "Combat",
                _ => effect // Fallback to original if not mapped
            };
        }

        #endregion

        #region Item Factory
        // Item generation remains procedural in EquipmentGenerator
        // Items are configured via Data/items.json templates
        // TODO: Implement starting equipment system properly
        #endregion
    }
}
