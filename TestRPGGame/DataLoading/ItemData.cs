using System.Collections.Generic;

namespace TestRPGGame.DataLoading
{
    public class ItemData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Type { get; set; } = ""; // Weapon, Armor, Accessory
        public string? Slot { get; set; } // Head, Chest, Legs, Feet, Accessory
        public int Price { get; set; }
        public int RequiredLevel { get; set; }
        public bool AvailableInShop { get; set; }
        public ItemStatsData Stats { get; set; } = new();
        public List<SpecialEffectData> SpecialEffects { get; set; } = new();
        public string? AttackType { get; set; }
    }
}
