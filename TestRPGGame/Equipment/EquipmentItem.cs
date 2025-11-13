using System;
using System.Collections.Generic;
using TestRPGGame.Combat;


namespace TestRPGGame.Equipment
{
    public class EquipmentItem
    {
        public string Name { get; set; }
        public EquipmentSlot Slot { get; set; }
        public ItemRarity Rarity { get; set; }
        public int Level { get; set; }

        // Stats
        public int AttackBonus { get; set; }
        public int DefenseBonus { get; set; }
        public int MagicBonus { get; set; }
        public int HPBonus { get; set; }
        public int ManaBonus { get; set; }
        public int AgilityBonus { get; set; }  // Renamed from SpeedBonus
        public double CritBonus { get; set; }

        // New: Min/Max damage system for weapons
        public int MinDamage { get; set; }
        public int MaxDamage { get; set; }
        public double Accuracy { get; set; } = 1.0;  // 1.0 = 100% accuracy

        // Backwards compatibility for existing code (with setter)
        public int SpeedBonus
        {
            get => AgilityBonus;
            set => AgilityBonus = value;
        }

        // Attack type for weapons
        public AttackType? WeaponAttackType { get; set; }

        public List<SpecialEffect> SpecialEffects { get; set; }
        public List<string> GrantedAbilityIds { get; set; }
        public int Price { get; set; }

        public EquipmentItem()
        {
            Name = "";
            SpecialEffects = new List<SpecialEffect>();
            GrantedAbilityIds = new List<string>();
        }

        public ConsoleColor GetRarityColor()
        {
            return Rarity switch
            {
                ItemRarity.Common => ConsoleColor.Gray,
                ItemRarity.Uncommon => ConsoleColor.Green,
                ItemRarity.Rare => ConsoleColor.Blue,
                ItemRarity.Epic => ConsoleColor.Magenta,
                ItemRarity.Legendary => ConsoleColor.Yellow,
                _ => ConsoleColor.White
            };
        }

        public string GetRarityMultiplierText()
        {
            return Rarity switch
            {
                ItemRarity.Common => "",
                ItemRarity.Uncommon => "+",
                ItemRarity.Rare => "++",
                ItemRarity.Epic => "+++",
                ItemRarity.Legendary => "⭐",
                _ => ""
            };
        }
    }
}
