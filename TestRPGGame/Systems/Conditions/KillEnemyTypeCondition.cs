using TestRPGGame.Entities.Player;

namespace TestRPGGame.Systems.Conditions
{
    /// <summary>
    /// Achievement condition for killing a specific number of a particular enemy type.
    /// </summary>
    public class KillEnemyTypeCondition : IAchievementCondition
    {
        public string EnemyType { get; set; } = string.Empty;
        public int RequiredKills { get; set; }

        public bool IsMet(PlayerStatistics statistics)
        {
            var kills = statistics.KillsByEnemyType.GetValueOrDefault(EnemyType, 0);
            return kills >= RequiredKills;
        }

        public string GetProgress(PlayerStatistics statistics)
        {
            var kills = statistics.KillsByEnemyType.GetValueOrDefault(EnemyType, 0);
            return $"{kills}/{RequiredKills} {EnemyType} killed";
        }
    }
}
