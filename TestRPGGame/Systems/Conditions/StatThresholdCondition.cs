using System;
using TestRPGGame.Entities.Player;

namespace TestRPGGame.Systems.Conditions
{
    /// <summary>
    /// Achievement condition that checks if a statistic meets or exceeds a threshold.
    /// </summary>
    public class StatThresholdCondition : IAchievementCondition
    {
        public string StatName { get; set; } = string.Empty;
        public int Threshold { get; set; }
        public string DisplayName { get; set; } = string.Empty;

        public bool IsMet(PlayerStatistics statistics)
        {
            var currentValue = GetStatValue(statistics, StatName);
            return currentValue >= Threshold;
        }

        public string GetProgress(PlayerStatistics statistics)
        {
            var currentValue = GetStatValue(statistics, StatName);
            var displayName = !string.IsNullOrEmpty(DisplayName) ? DisplayName : StatName;
            return $"{currentValue}/{Threshold} {displayName}";
        }

        private int GetStatValue(PlayerStatistics stats, string statName)
        {
            return statName switch
            {
                "TotalKills" => stats.TotalKills,
                "TotalDeaths" => stats.TotalDeaths,
                "BossesDefeated" => stats.BossesDefeated,
                "TotalDungeonsCompleted" => stats.TotalDungeonsCompleted,
                "DungeonAttempts" => stats.DungeonAttempts,
                "CombatsWon" => stats.CombatsWon,
                "TotalAbilitiesUsed" => stats.TotalAbilitiesUsed,
                "PotionsUsed" => stats.PotionsUsed,
                "ItemsBought" => stats.ItemsBought,
                "ItemsSold" => stats.ItemsSold,
                "TimesRested" => stats.TimesRested,
                "CriticalHitsDealt" => stats.CriticalHitsDealt,
                "LegendaryItemsFound" => stats.LegendaryItemsFound,
                "EpicItemsFound" => stats.EpicItemsFound,
                "HighestLevelReached" => stats.HighestLevelReached,
                "TotalLevelsGained" => stats.TotalLevelsGained,
                "GameSaves" => stats.GameSaves,
                "TotalGoldEarned" => (int)(stats.TotalGoldEarned / 1000), // Convert to thousands
                "TotalGoldSpent" => (int)(stats.TotalGoldSpent / 1000),
                "TotalDamageDealt" => (int)(stats.TotalDamageDealt / 1000),
                "TotalDamageTaken" => (int)(stats.TotalDamageTaken / 1000),
                "MostGoldAtOnce" => stats.MostGoldAtOnce,
                "HighestDamageInOneTurn" => stats.HighestDamageInOneTurn,
                "LongestCombat" => stats.LongestCombat,
                _ => 0
            };
        }
    }
}
