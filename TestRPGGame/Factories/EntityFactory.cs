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

namespace TestRPGGame.Factories
{
    /// <summary>
    /// Factory for creating game entities from data definitions.
    /// The code has zero knowledge of specific entities - it only knows how to interpret data.
    /// </summary>
    public static class EntityFactory
    {
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
                data.PurchaseCost
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
            return data.Type.ToLower() switch
            {
                "damage" => new DamageEffect(data.Multiplier, usesMagic: data.Type == "Magic", guaranteedCrit: data.GuaranteedCrit),
                "buff" => new BuffApplicator(data.BuffName ?? "Unknown Buff", data.Duration),
                "restore" => new RestoreEffect(data.Value, isMana: data.Value > 0),
                "statmod" => new StatModEffect("speed", data.Value),
                "dodge" => new DodgeEffect(),
                "poison" => new PoisonEffect(data.DamagePerTurn, data.Duration, data.Value),
                "damageovertime" => new PoisonEffect(data.Value, data.Duration, data.Value), // Burning/DOT effect
                "healovertime" => new RegenerationApplicator(data.Value, data.Duration),
                "stun" => new StunApplicator(data.Duration),
                "thorns" => new ThornsApplicator(data.Value, data.Duration),
                "shield" => new ShieldApplicator(data.Value, data.Duration),
                "lifesteal" => new LifeStealEffect(data.Multiplier, data.Value),
                _ => null
            };
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
                var random = new Random();
                maxHP += random.Next(-10, 11);
                attack += random.Next(-2, 4);
                defense += random.Next(-1, 3);
                speed += random.Next(-2, 4);
                goldReward += random.Next(-5, 16);
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
            var minibossData = DataLoader.GetEnemy(data.MinibossId);
            var bossData = DataLoader.GetEnemy(data.BossId);

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
