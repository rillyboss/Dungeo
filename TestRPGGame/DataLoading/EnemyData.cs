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
        public List<string> Abilities { get; set; } = new();
        public List<string> PossibleDrops { get; set; } = new();
    }
}
