using System.Collections.Generic;

namespace TestRPGGame.DataLoading
{
    public class ItemGenerationData
    {
        public Dictionary<string, WeaponPrefixData> weapon_prefixes { get; set; } = new();
        public Dictionary<string, WeaponTypeData> weapon_types { get; set; } = new();
        public Dictionary<string, WeaponSuffixData> weapon_suffixes { get; set; } = new();
        public Dictionary<string, ArmorPrefixData> armor_prefixes { get; set; } = new();
        public Dictionary<string, ArmorSuffixData> armor_suffixes { get; set; } = new();
        public Dictionary<string, double> rarity_multipliers { get; set; } = new();

        // C# friendly properties
        public Dictionary<string, WeaponPrefixData> WeaponPrefixes => weapon_prefixes;
        public Dictionary<string, WeaponTypeData> WeaponTypes => weapon_types;
        public Dictionary<string, WeaponSuffixData> WeaponSuffixes => weapon_suffixes;
        public Dictionary<string, ArmorPrefixData> ArmorPrefixes => armor_prefixes;
        public Dictionary<string, ArmorSuffixData> ArmorSuffixes => armor_suffixes;
        public Dictionary<string, double> RarityMultipliers => rarity_multipliers;
    }
}
