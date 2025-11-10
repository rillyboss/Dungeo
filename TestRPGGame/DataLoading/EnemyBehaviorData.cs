using System.Collections.Generic;

namespace TestRPGGame.DataLoading
{
    /// <summary>
    /// Defines an enemy behavior pattern that controls AI decision making.
    /// Behaviors can be applied as modifiers to regular enemies or used in boss phases.
    /// </summary>
    public class EnemyBehaviorData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        /// <summary>
        /// Weights for different ability types. Higher = more likely to use.
        /// </summary>
        public Dictionary<string, double> AbilityWeights { get; set; } = new();

        /// <summary>
        /// Multipliers applied to enemy stats when this behavior is active.
        /// </summary>
        public Dictionary<string, double> StatModifiers { get; set; } = new();

        /// <summary>
        /// Contextual decision factors that modify ability selection.
        /// </summary>
        public Dictionary<string, double> DecisionFactors { get; set; } = new();

        /// <summary>
        /// Chance (0.0-1.0) this behavior appears as a random modifier on enemies.
        /// </summary>
        public double SpawnChance { get; set; } = 0.0;
    }
}
