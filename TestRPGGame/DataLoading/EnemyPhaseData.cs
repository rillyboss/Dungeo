using System.Collections.Generic;

namespace TestRPGGame.DataLoading
{
    /// <summary>
    /// Defines an enemy phase that triggers at a specific HP threshold.
    /// Phases change behavior, enable/disable abilities, and can display messages.
    /// Any enemy can have phases - data determines if they're used.
    /// </summary>
    public class EnemyPhaseData
    {
        /// <summary>
        /// HP percentage (0-100) at which this phase activates.
        /// </summary>
        public int HPThreshold { get; set; } = 100;

        /// <summary>
        /// Behavior ID to use during this phase.
        /// </summary>
        public string BehaviorId { get; set; } = "";

        /// <summary>
        /// Message displayed when entering this phase.
        /// Use {name} as placeholder for enemy name.
        /// </summary>
        public string? PhaseMessage { get; set; }

        /// <summary>
        /// Ability IDs enabled during this phase. Empty = all abilities enabled.
        /// </summary>
        public List<string> EnabledAbilities { get; set; } = new();

        /// <summary>
        /// Ability IDs disabled during this phase.
        /// </summary>
        public List<string> DisabledAbilities { get; set; } = new();

        /// <summary>
        /// Priority boosts for specific abilities during this phase.
        /// Key = ability ID, Value = priority multiplier.
        /// </summary>
        public Dictionary<string, double> AbilityPriorityBoost { get; set; } = new();

        /// <summary>
        /// Whether this phase has been triggered yet.
        /// </summary>
        public bool HasTriggered { get; set; } = false;
    }
}
