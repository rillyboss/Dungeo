using TestRPGGame.Entities.Player;

namespace TestRPGGame.Systems.Conditions
{
    /// <summary>
    /// Achievement condition for completing a specific dungeon or number of dungeon completions.
    /// </summary>
    public class DungeonCompletionCondition : IAchievementCondition
    {
        public string? DungeonName { get; set; } = null; // Null = any dungeon
        public int RequiredCompletions { get; set; } = 1;

        public bool IsMet(PlayerStatistics statistics)
        {
            if (string.IsNullOrEmpty(DungeonName))
            {
                // Any dungeon - check total
                return statistics.TotalDungeonsCompleted >= RequiredCompletions;
            }
            else
            {
                // Specific dungeon
                var completions = statistics.CompletionsByDungeon.GetValueOrDefault(DungeonName, 0);
                return completions >= RequiredCompletions;
            }
        }

        public string GetProgress(PlayerStatistics statistics)
        {
            if (string.IsNullOrEmpty(DungeonName))
            {
                return $"{statistics.TotalDungeonsCompleted}/{RequiredCompletions} dungeons completed";
            }
            else
            {
                var completions = statistics.CompletionsByDungeon.GetValueOrDefault(DungeonName, 0);
                return $"{completions}/{RequiredCompletions} {DungeonName} completions";
            }
        }
    }
}
