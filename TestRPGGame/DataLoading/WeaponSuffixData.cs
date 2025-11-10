using System.Collections.Generic;

namespace TestRPGGame.DataLoading
{
    public class WeaponSuffixData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public List<SpecialEffectData> Effects { get; set; } = new();
    }
}
