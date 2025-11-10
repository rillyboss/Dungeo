using System.Collections.Generic;

namespace TestRPGGame.DataLoading
{
    public class EnemySuffixData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int MinLevel { get; set; }
        public List<string> AdditionalAbilities { get; set; } = new();
        public int GoldBonus { get; set; }
        public int ExpBonus { get; set; }
    }
}
