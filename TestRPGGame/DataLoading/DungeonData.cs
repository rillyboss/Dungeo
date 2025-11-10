using System.Collections.Generic;

namespace TestRPGGame.DataLoading
{
    public class DungeonData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = ""; // Friendly display name (falls back to Name if empty)
        public string Description { get; set; } = "";
        public int RecommendedLevel { get; set; }
        public int RequiredDungeonsCompleted { get; set; }
        public List<string> RequiredDungeonIds { get; set; } = new();
        public string MinibossId { get; set; } = "";
        public string BossId { get; set; } = "";

        // Legacy: Fixed encounters (deprecated in favor of encounter pools)
        public List<DungeonEncounterData> Encounters { get; set; } = new();

        // New: Random encounter system for replayability
        public DungeonEncounterConfig? EncounterConfig { get; set; }
        public List<DungeonEncounterData> EncounterPool { get; set; } = new();

        public DungeonRewardsData Rewards { get; set; } = new();
    }

    /// <summary>
    /// Configuration for random encounter generation
    /// </summary>
    public class DungeonEncounterConfig
    {
        /// <summary>
        /// Minimum number of encounters before miniboss (excluding combat)
        /// </summary>
        public int MinEncounters { get; set; } = 2;

        /// <summary>
        /// Maximum number of encounters before miniboss (excluding combat)
        /// </summary>
        public int MaxEncounters { get; set; } = 4;

        /// <summary>
        /// Minimum number of combat encounters with random enemies
        /// </summary>
        public int MinCombatEncounters { get; set; } = 2;

        /// <summary>
        /// Maximum number of combat encounters with random enemies
        /// </summary>
        public int MaxCombatEncounters { get; set; } = 4;

        /// <summary>
        /// Chance (0.0-1.0) for an extra random combat between encounters
        /// </summary>
        public double RandomCombatChance { get; set; } = 0.3;
    }
}
