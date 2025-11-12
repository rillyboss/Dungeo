using TestRPGGame.Entities.Player;

namespace TestRPGGame.Systems.Conditions
{
    /// <summary>
    /// Interface for achievement unlock conditions.
    /// Uses Strategy pattern to allow flexible achievement requirements.
    /// </summary>
    public interface IAchievementCondition
    {
        /// <summary>
        /// Checks if the condition is met based on current statistics.
        /// </summary>
        /// <param name="statistics">Current player statistics</param>
        /// <returns>True if the achievement condition is satisfied</returns>
        bool IsMet(PlayerStatistics statistics);

        /// <summary>
        /// Gets a human-readable description of progress toward this condition.
        /// </summary>
        /// <param name="statistics">Current player statistics</param>
        /// <returns>Progress description (e.g., "15/100 enemies killed")</returns>
        string GetProgress(PlayerStatistics statistics);
    }
}
