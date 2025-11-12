using System.Collections.Generic;

namespace TestRPGGame.Entities.Player
{
    /// <summary>
    /// Tracks comprehensive player statistics across game sessions.
    /// Designed to be flexible and easily extendable for new stats.
    /// </summary>
    public class PlayerStatistics
    {
        // Combat Stats
        public int TotalKills { get; set; }
        public int TotalDeaths { get; set; }
        public Dictionary<string, int> KillsByEnemyType { get; set; } = new();
        public int BossesDefeated { get; set; }
        public int CombatsWon { get; set; }
        public int CombatsFled { get; set; }

        // Damage Stats
        public long TotalDamageDealt { get; set; }
        public long TotalDamageTaken { get; set; }
        public long TotalHealingDone { get; set; }
        public int CriticalHitsDealt { get; set; }
        public int AttacksMissed { get; set; }
        public int AttacksDodged { get; set; }

        // Gold & Economy
        public long TotalGoldEarned { get; set; }
        public long TotalGoldSpent { get; set; }
        public int ItemsBought { get; set; }
        public int ItemsSold { get; set; }
        public int TimesRested { get; set; }

        // Dungeon Progress
        public int TotalDungeonsCompleted { get; set; }
        public Dictionary<string, int> CompletionsByDungeon { get; set; } = new();
        public int DungeonAttempts { get; set; }
        public int DungeonFailures { get; set; }

        // Ability Usage
        public int TotalAbilitiesUsed { get; set; }
        public Dictionary<string, int> UsageByAbility { get; set; } = new();
        public int TotalManaSpent { get; set; }

        // Items & Consumables
        public int PotionsUsed { get; set; }
        public int EquipmentUpgrades { get; set; }
        public int LegendaryItemsFound { get; set; }
        public int EpicItemsFound { get; set; }

        // Progression
        public int TotalLevelsGained { get; set; }
        public int AbilitiesUnlocked { get; set; }
        public int HighestLevelReached { get; set; }
        public long TotalExperienceGained { get; set; }

        // Miscellaneous
        public int ShopRefreshes { get; set; }
        public int GameSaves { get; set; }
        public int TotalTurnsInCombat { get; set; }
        public int StatusEffectsApplied { get; set; }
        public int StatusEffectsReceived { get; set; }

        // High Scores
        public int HighestDamageInOneTurn { get; set; }
        public int LongestCombat { get; set; } // turns
        public int MostGoldAtOnce { get; set; }

        /// <summary>
        /// Increments a kill count for a specific enemy type.
        /// </summary>
        public void RecordKill(string enemyType)
        {
            TotalKills++;
            if (!KillsByEnemyType.ContainsKey(enemyType))
            {
                KillsByEnemyType[enemyType] = 0;
            }
            KillsByEnemyType[enemyType]++;
        }

        /// <summary>
        /// Increments ability usage count for a specific ability.
        /// </summary>
        public void RecordAbilityUse(string abilityName, int manaCost)
        {
            TotalAbilitiesUsed++;
            TotalManaSpent += manaCost;

            if (!UsageByAbility.ContainsKey(abilityName))
            {
                UsageByAbility[abilityName] = 0;
            }
            UsageByAbility[abilityName]++;
        }

        /// <summary>
        /// Records a dungeon completion for a specific dungeon.
        /// </summary>
        public void RecordDungeonCompletion(string dungeonName)
        {
            TotalDungeonsCompleted++;

            if (!CompletionsByDungeon.ContainsKey(dungeonName))
            {
                CompletionsByDungeon[dungeonName] = 0;
            }
            CompletionsByDungeon[dungeonName]++;
        }

        /// <summary>
        /// Records damage dealt and updates highest damage if applicable.
        /// </summary>
        public void RecordDamageDealt(int damage, bool isCritical)
        {
            TotalDamageDealt += damage;

            if (isCritical)
            {
                CriticalHitsDealt++;
            }

            if (damage > HighestDamageInOneTurn)
            {
                HighestDamageInOneTurn = damage;
            }
        }

        /// <summary>
        /// Resets all statistics to zero. Used for new characters or fresh starts.
        /// </summary>
        public void Reset()
        {
            TotalKills = 0;
            TotalDeaths = 0;
            KillsByEnemyType.Clear();
            BossesDefeated = 0;
            CombatsWon = 0;
            CombatsFled = 0;

            TotalDamageDealt = 0;
            TotalDamageTaken = 0;
            TotalHealingDone = 0;
            CriticalHitsDealt = 0;
            AttacksMissed = 0;
            AttacksDodged = 0;

            TotalGoldEarned = 0;
            TotalGoldSpent = 0;
            ItemsBought = 0;
            ItemsSold = 0;
            TimesRested = 0;

            TotalDungeonsCompleted = 0;
            CompletionsByDungeon.Clear();
            DungeonAttempts = 0;
            DungeonFailures = 0;

            TotalAbilitiesUsed = 0;
            UsageByAbility.Clear();
            TotalManaSpent = 0;

            PotionsUsed = 0;
            EquipmentUpgrades = 0;
            LegendaryItemsFound = 0;
            EpicItemsFound = 0;

            TotalLevelsGained = 0;
            AbilitiesUnlocked = 0;
            HighestLevelReached = 0;
            TotalExperienceGained = 0;

            ShopRefreshes = 0;
            GameSaves = 0;
            TotalTurnsInCombat = 0;
            StatusEffectsApplied = 0;
            StatusEffectsReceived = 0;

            HighestDamageInOneTurn = 0;
            LongestCombat = 0;
            MostGoldAtOnce = 0;
        }
    }
}
