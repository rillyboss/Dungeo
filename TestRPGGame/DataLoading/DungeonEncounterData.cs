using System.Collections.Generic;

namespace TestRPGGame.DataLoading
{
    public class DungeonEncounterData
    {
        public string Type { get; set; } = ""; // Combat, Choice, Story
        public string? Text { get; set; }
        public List<string>? EnemyPool { get; set; }
        public List<DungeonChoiceData>? Choices { get; set; }
    }
}
