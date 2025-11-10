using System.Collections.Generic;

namespace TestRPGGame.DataLoading
{
    public class BossData
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
        public List<BossAbilityData> BossAbilities { get; set; } = new();
        public List<string> GuaranteedDrops { get; set; } = new();
    }
}
