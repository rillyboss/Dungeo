using System.Collections.Generic;

namespace TestRPGGame.DataLoading
{
    public class DungeonEncounterData
    {
        public string Type { get; set; } = ""; // Combat, Choice, Story
        public string? Text { get; set; }
        public List<string>? EnemyPool { get; set; }
        public List<DungeonChoiceData>? Choices { get; set; }

        /// <summary>
        /// Number of choices to randomly select from the Choices list.
        /// If 0 or not specified, all choices are shown.
        /// Allows defining many choices but only presenting a random subset.
        /// </summary>
        public int RandomChoiceCount { get; set; } = 0;
    }
}
