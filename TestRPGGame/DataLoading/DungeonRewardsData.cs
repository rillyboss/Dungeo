using System.Collections.Generic;

namespace TestRPGGame.DataLoading
{
    public class DungeonRewardsData
    {
        public int MinibossGold { get; set; }
        public int BossGold { get; set; }
        public List<string> GuaranteedItems { get; set; } = new();
        public List<string> PossibleItems { get; set; } = new();
        public double ItemDropChance { get; set; }
    }
}
