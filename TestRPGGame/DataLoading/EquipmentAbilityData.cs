using System.Collections.Generic;

namespace TestRPGGame.DataLoading
{
    public class EquipmentAbilityPools
    {
        public WeaponAbilityPools? WeaponAbilities { get; set; }
        public ArmorAbilityPools? ArmorAbilities { get; set; }
        public AccessoryAbilityPools? AccessoryAbilities { get; set; }
        public Dictionary<string, double>? RarityChances { get; set; }
    }

    public class WeaponAbilityPools
    {
        public List<string>? Physical { get; set; }
        public List<string>? Fire { get; set; }
        public List<string>? Ice { get; set; }
        public List<string>? Poison { get; set; }
        public List<string>? Shadow { get; set; }
    }

    public class ArmorAbilityPools
    {
        public List<string>? Defensive { get; set; }
        public List<string>? Offensive { get; set; }
    }

    public class AccessoryAbilityPools
    {
        public List<string>? Utility { get; set; }
    }
}
