using System.Linq;
using TestRPGGame.Entities.Player;
using TestRPGGame.Interfaces;

namespace TestRPGGame.Systems
{
    /// <summary>
    /// Manages player statistics display and interaction.
    /// Follows the same pattern as ProgressionManager and SaveManager.
    /// </summary>
    public class StatisticsManager
    {
        private readonly IGameInterface _gameInterface;

        public StatisticsManager(IGameInterface gameInterface)
        {
            _gameInterface = gameInterface;
        }

        /// <summary>
        /// Displays the player's statistics.
        /// </summary>
        public void DisplayStatistics(PlayerStatistics statistics)
        {
            var info = BuildStatisticsInfo(statistics);
            _gameInterface.DisplayStatistics(info);
        }

        /// <summary>
        /// Builds a StatisticsInfo object from PlayerStatistics.
        /// </summary>
        private StatisticsInfo BuildStatisticsInfo(PlayerStatistics stats)
        {
            return new StatisticsInfo
            {
                // Combat Stats
                TotalKills = stats.TotalKills,
                TotalDeaths = stats.TotalDeaths,
                KillsByEnemyType = stats.KillsByEnemyType,
                BossesDefeated = stats.BossesDefeated,
                CombatsWon = stats.CombatsWon,
                CombatsFled = stats.CombatsFled,

                // Damage Stats
                TotalDamageDealt = stats.TotalDamageDealt,
                TotalDamageTaken = stats.TotalDamageTaken,
                TotalHealingDone = stats.TotalHealingDone,
                CriticalHitsDealt = stats.CriticalHitsDealt,
                AttacksMissed = stats.AttacksMissed,
                AttacksDodged = stats.AttacksDodged,

                // Gold & Economy
                TotalGoldEarned = stats.TotalGoldEarned,
                TotalGoldSpent = stats.TotalGoldSpent,
                ItemsBought = stats.ItemsBought,
                ItemsSold = stats.ItemsSold,
                TimesRested = stats.TimesRested,

                // Dungeon Progress
                TotalDungeonsCompleted = stats.TotalDungeonsCompleted,
                CompletionsByDungeon = stats.CompletionsByDungeon,
                DungeonAttempts = stats.DungeonAttempts,
                DungeonFailures = stats.DungeonFailures,

                // Ability Usage
                TotalAbilitiesUsed = stats.TotalAbilitiesUsed,
                UsageByAbility = stats.UsageByAbility,
                TotalManaSpent = stats.TotalManaSpent,

                // Items & Consumables
                PotionsUsed = stats.PotionsUsed,
                EquipmentUpgrades = stats.EquipmentUpgrades,
                LegendaryItemsFound = stats.LegendaryItemsFound,
                EpicItemsFound = stats.EpicItemsFound,

                // Progression
                TotalLevelsGained = stats.TotalLevelsGained,
                AbilitiesUnlocked = stats.AbilitiesUnlocked,
                HighestLevelReached = stats.HighestLevelReached,
                TotalExperienceGained = stats.TotalExperienceGained,

                // Miscellaneous
                ShopRefreshes = stats.ShopRefreshes,
                GameSaves = stats.GameSaves,
                TotalTurnsInCombat = stats.TotalTurnsInCombat,
                StatusEffectsApplied = stats.StatusEffectsApplied,
                StatusEffectsReceived = stats.StatusEffectsReceived,

                // High Scores
                HighestDamageInOneTurn = stats.HighestDamageInOneTurn,
                LongestCombat = stats.LongestCombat,
                MostGoldAtOnce = stats.MostGoldAtOnce,

                // Calculated/Derived Stats
                KillDeathRatio = stats.TotalDeaths > 0 ? (double)stats.TotalKills / stats.TotalDeaths : stats.TotalKills,
                AverageDamagePerCombat = stats.CombatsWon > 0 ? (double)stats.TotalDamageDealt / stats.CombatsWon : 0,
                DungeonSuccessRate = stats.DungeonAttempts > 0 ? (double)stats.TotalDungeonsCompleted / stats.DungeonAttempts * 100 : 0,
                NetGold = stats.TotalGoldEarned - stats.TotalGoldSpent,
                CritRate = (stats.TotalDamageDealt > 0 && stats.CombatsWon > 0) ? (double)stats.CriticalHitsDealt / stats.CombatsWon * 100 : 0
            };
        }
    }

    /// <summary>
    /// Data transfer object for displaying statistics.
    /// Used by IGameInterface.DisplayStatistics().
    /// </summary>
    public class StatisticsInfo
    {
        // Combat Stats
        public int TotalKills { get; set; }
        public int TotalDeaths { get; set; }
        public System.Collections.Generic.Dictionary<string, int> KillsByEnemyType { get; set; } = new();
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
        public System.Collections.Generic.Dictionary<string, int> CompletionsByDungeon { get; set; } = new();
        public int DungeonAttempts { get; set; }
        public int DungeonFailures { get; set; }

        // Ability Usage
        public int TotalAbilitiesUsed { get; set; }
        public System.Collections.Generic.Dictionary<string, int> UsageByAbility { get; set; } = new();
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
        public int LongestCombat { get; set; }
        public int MostGoldAtOnce { get; set; }

        // Calculated/Derived Stats
        public double KillDeathRatio { get; set; }
        public double AverageDamagePerCombat { get; set; }
        public double DungeonSuccessRate { get; set; }
        public long NetGold { get; set; }
        public double CritRate { get; set; }
    }
}
