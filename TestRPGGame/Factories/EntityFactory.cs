using System;
using System.Collections.Generic;
using System.Linq;
using TestRPGGame.DataLoading;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.Entities.Dungeon;
using TestRPGGame.Entities.Boss;
using TestRPGGame.Abilities;
using TestRPGGame.Abilities.Effects;
using TestRPGGame.Combat;

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
                IsUnlocked = data.IsStarting
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
                "damage" => new DamageEffect(data.Multiplier, usesMagic: data.Type == "Magic"),
                "buff" => new BuffEffect(data.BuffName ?? "Unknown Buff", data.Duration),
                "restore" => new RestoreEffect(data.Value, isMana: data.Value > 0),
                "statmod" => new StatModEffect("speed", data.Value),
                "dodge" => new DodgeEffect(),
                "poison" => new PoisonEffect(data.DamagePerTurn, data.Duration, data.Value),
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

            var enemy = new Enemy(data.Name, data.Level > 0 ? data.Level : playerLevel, enemyType);

            // Apply stats - if base stats are provided, use them, otherwise scale with level
            int level = data.Level > 0 ? data.Level : playerLevel;
            enemy.MaxHP = data.MaxHP > 0 ? data.MaxHP : 60 + (level * 15);
            enemy.CurrentHP = enemy.MaxHP;
            enemy.Attack = data.Attack > 0 ? data.Attack : 8 + (level * 2);
            enemy.Defense = data.Defense > 0 ? data.Defense : 3 + level;
            enemy.Speed = data.Speed > 0 ? data.Speed : 5 + level;
            enemy.GoldReward = data.GoldReward > 0 ? data.GoldReward : 30 + (level * 10);
            enemy.ExpReward = data.ExpReward > 0 ? data.ExpReward : 50 + (level * 20);

            // Add random variance for regular enemies (non-boss)
            if (data.Level == 0) // Level 0 indicates scaling enemy
            {
                var random = new Random();
                enemy.MaxHP += random.Next(-10, 11);
                enemy.CurrentHP = enemy.MaxHP;
                enemy.Attack += random.Next(-2, 4);
                enemy.Defense += random.Next(-1, 3);
                enemy.Speed += random.Next(-2, 4);
                enemy.GoldReward += random.Next(-5, 16);
            }

            return enemy;
        }

        public static Enemy CreateBoss(BossData data)
        {
            if (!Enum.TryParse<EnemyType>(data.Type, out var enemyType))
            {
                throw new ArgumentException($"Invalid enemy type: {data.Type}");
            }

            var boss = new Enemy(data.Name, data.Level, enemyType);
            boss.MaxHP = data.MaxHP;
            boss.CurrentHP = data.MaxHP;
            boss.Attack = data.Attack;
            boss.Defense = data.Defense;
            boss.Speed = data.Speed;
            boss.GoldReward = data.GoldReward;
            boss.ExpReward = data.ExpReward;

            // Create boss abilities
            boss.BossAbilities = new List<BossAbility>();
            boss.StatusEffects = new BossStatusEffects();

            foreach (var abilityData in data.BossAbilities)
            {
                var ability = CreateBossAbility(abilityData);
                boss.BossAbilities.Add(ability);
            }

            return boss;
        }

        private static BossAbility CreateBossAbility(BossAbilityData data)
        {
            var effects = new List<BossAbilityEffect>();

            // Support both single Effect and multiple Effects
            var effectDataList = data.Effects.Count > 0 ? data.Effects :
                                 (data.Effect != null ? new List<BossAbilityEffectData> { data.Effect } : new List<BossAbilityEffectData>());

            foreach (var effectData in effectDataList)
            {
                if (!Enum.TryParse<BossAbilityEffectType>(effectData.Type, out var effectType))
                {
                    throw new ArgumentException($"Invalid boss ability effect type: {effectData.Type}");
                }

                var effect = new BossAbilityEffect(
                    effectType,
                    effectData.Value,
                    effectData.Duration,
                    effectData.Multiplier
                );

                effects.Add(effect);
            }

            return new BossAbility(data.Name, data.Description, data.Cooldown, effects);
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

            var dungeon = new Dungeon(data.Name, dungeonType, data.Description, data.RecommendedLevel);

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

            // Create encounters
            dungeon.Encounters = new List<DungeonEncounter>();
            foreach (var encounterData in data.Encounters)
            {
                var encounter = CreateDungeonEncounter(encounterData);
                dungeon.Encounters.Add(encounter);
            }

            // Load miniboss and boss
            var minibossData = DataLoader.GetBoss(data.MinibossId);
            var bossData = DataLoader.GetBoss(data.BossId);

            dungeon.Miniboss = CreateBoss(minibossData);
            dungeon.Boss = CreateBoss(bossData);
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
                foreach (var choice in data.Choices)
                {
                    encounter.Choices.Add(choice.Text);
                    encounter.ChoiceResults.Add($"{choice.Effect}: {choice.Value}");
                }
            }

            encounter.IsCombat = data.Type?.ToLower() == "combat";

            return encounter;
        }

        #endregion

        #region Item Factory
        // Item generation remains procedural in EquipmentGenerator
        // Items are configured via Data/items.json templates
        // Future enhancement: Create items from data definitions
        #endregion
    }
}
