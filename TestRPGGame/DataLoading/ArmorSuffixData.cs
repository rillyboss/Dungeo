using System.Collections.Generic;

namespace TestRPGGame.DataLoading
{
    public class ArmorSuffixData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public List<SpecialEffectData> Effects { get; set; } = new();
    }
}
