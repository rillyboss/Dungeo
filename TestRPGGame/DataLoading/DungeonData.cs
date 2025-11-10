using System.Collections.Generic;

namespace TestRPGGame.DataLoading
{
    public class DungeonData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int RecommendedLevel { get; set; }
        public int RequiredDungeonsCompleted { get; set; }
        public List<string> RequiredDungeonIds { get; set; } = new();
        public string MinibossId { get; set; } = "";
        public string BossId { get; set; } = "";
        public List<DungeonEncounterData> Encounters { get; set; } = new();
        public DungeonRewardsData Rewards { get; set; } = new();
    }
}
