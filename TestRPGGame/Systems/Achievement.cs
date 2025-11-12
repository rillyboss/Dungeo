using System;
using TestRPGGame.Systems.Conditions;

namespace TestRPGGame.Systems
{
    /// <summary>
    /// Represents an unlockable achievement with conditions and rewards.
    /// </summary>
    public class Achievement
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Combat, Exploration, Collection, Progression, Special
        public IAchievementCondition? Condition { get; set; }

        // Rewards
        public int GoldReward { get; set; } = 0;
        public int ExperienceReward { get; set; } = 0;
        public string? ItemReward { get; set; } = null; // Item name to grant
        public string? TitleReward { get; set; } = null; // Title/badge name

        // Tracking
        public bool IsUnlocked { get; set; } = false;
        public DateTime? UnlockTime { get; set; } = null;
        public bool IsHidden { get; set; } = false; // Hidden until unlocked

        /// <summary>
        /// Points value for achievement completion percentage.
        /// </summary>
        public int Points { get; set; } = 10;

        /// <summary>
        /// Checks if this achievement's condition is met.
        /// </summary>
        public bool CheckCondition(Entities.Player.PlayerStatistics statistics)
        {
            if (IsUnlocked)
                return true;

            return Condition?.IsMet(statistics) ?? false;
        }

        /// <summary>
        /// Gets the progress string for this achievement.
        /// </summary>
        public string GetProgressString(Entities.Player.PlayerStatistics statistics)
        {
            if (IsUnlocked)
                return "✓ Unlocked";

            return Condition?.GetProgress(statistics) ?? "No condition";
        }

        /// <summary>
        /// Unlocks the achievement and records the unlock time.
        /// </summary>
        public void Unlock()
        {
            if (!IsUnlocked)
            {
                IsUnlocked = true;
                UnlockTime = DateTime.Now;
            }
        }
    }

    /// <summary>
    /// Achievement categories for organization.
    /// </summary>
    public static class AchievementCategories
    {
        public const string Combat = "Combat";
        public const string Exploration = "Exploration";
        public const string Collection = "Collection";
        public const string Progression = "Progression";
        public const string Special = "Special";
        public const string Economy = "Economy";
    }
}
