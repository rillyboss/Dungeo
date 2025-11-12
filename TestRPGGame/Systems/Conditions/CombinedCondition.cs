using System.Collections.Generic;
using System.Linq;
using TestRPGGame.Entities.Player;

namespace TestRPGGame.Systems.Conditions
{
    /// <summary>
    /// Achievement condition that combines multiple conditions with AND/OR logic.
    /// </summary>
    public class CombinedCondition : IAchievementCondition
    {
        public enum LogicType { AND, OR }

        public List<IAchievementCondition> Conditions { get; set; } = new();
        public LogicType Logic { get; set; } = LogicType.AND;

        public bool IsMet(PlayerStatistics statistics)
        {
            if (Conditions.Count == 0)
                return false;

            return Logic == LogicType.AND
                ? Conditions.All(c => c.IsMet(statistics))
                : Conditions.Any(c => c.IsMet(statistics));
        }

        public string GetProgress(PlayerStatistics statistics)
        {
            var metCount = Conditions.Count(c => c.IsMet(statistics));
            var totalCount = Conditions.Count;

            if (Logic == LogicType.AND)
            {
                return $"{metCount}/{totalCount} requirements met";
            }
            else
            {
                return metCount > 0 ? "✓ Requirement met" : $"0/{totalCount} requirements met";
            }
        }
    }
}
