using System.Collections.Generic;

namespace TestRPGGame.DataLoading
{
    public class EnemyData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int Level { get; set; }
        public string Type { get; set; } = "";
        public int MaxHP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Speed { get; set; }
        public int GoldReward { get; set; }
        public int ExpReward { get; set; }
        public int MinLevel { get; set; } = 1; // Minimum player level to encounter this enemy
        public bool IsBoss { get; set; } = false; // True for dungeon bosses, false for random encounters
        public List<EnemyAbilityData> Abilities { get; set; } = new();
        public List<string> PossibleDrops { get; set; } = new();

        /// <summary>
        /// Default behavior for this enemy (optional). If not set, uses "default" behavior.
        /// </summary>
        public string? DefaultBehavior { get; set; }

        /// <summary>
        /// Phases for this enemy (primarily for bosses, but any enemy can have phases).
        /// </summary>
        public List<EnemyPhaseData>? Phases { get; set; }
    }
}
