using System;
using System.Collections.Generic;
using TestRPGGame.Combat;
using TestRPGGame.UI;

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
        public int SpeedBonus { get; set; }
        public double CritBonus { get; set; }

        // Attack type for weapons
        public AttackType? WeaponAttackType { get; set; }

        public List<SpecialEffect> SpecialEffects { get; set; }
        public int Price { get; set; }

        public EquipmentItem()
        {
            Name = "";
            SpecialEffects = new List<SpecialEffect>();
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

        public void DisplayDetails()
        {
            UIHelper.PrintColored($"[{Rarity}] {Name}", GetRarityColor());
            Console.WriteLine($" (Lv {Level})");
            Console.WriteLine($"Slot: {Slot.GetDisplayName()}");

            // Display attack type for weapons
            if (WeaponAttackType.HasValue)
            {
                Console.Write("  Attack Type: ");
                UIHelper.PrintColored($"{AttackTypeSystem.GetAttackTypeIcon(WeaponAttackType.Value)} {WeaponAttackType.Value}",
                    AttackTypeSystem.GetAttackTypeColor(WeaponAttackType.Value));
                Console.WriteLine();
            }

            Console.WriteLine();

            // Display stats
            if (AttackBonus > 0) UIHelper.PrintColoredLine($"  ⚔️  Attack: +{AttackBonus}", ConsoleColor.White);
            if (DefenseBonus > 0) UIHelper.PrintColoredLine($"  🛡️  Defense: +{DefenseBonus}", ConsoleColor.White);
            if (MagicBonus > 0) UIHelper.PrintColoredLine($"  🔮 Magic: +{MagicBonus}", ConsoleColor.White);
            if (HPBonus > 0) UIHelper.PrintColoredLine($"  ❤️  HP: +{HPBonus}", ConsoleColor.Green);
            if (ManaBonus > 0) UIHelper.PrintColoredLine($"  💙 Mana: +{ManaBonus}", ConsoleColor.Cyan);
            if (SpeedBonus > 0) UIHelper.PrintColoredLine($"  ⚡ Speed: +{SpeedBonus}", ConsoleColor.Yellow);
            if (CritBonus > 0) UIHelper.PrintColoredLine($"  💥 Crit Chance: +{CritBonus:P0}", ConsoleColor.Magenta);

            // Display special effects
            if (SpecialEffects.Count > 0)
            {
                Console.WriteLine();
                UIHelper.PrintColoredLine("  ✨ SPECIAL EFFECTS:", ConsoleColor.Yellow);
                foreach (var effect in SpecialEffects)
                {
                    UIHelper.PrintColoredLine($"    • {effect.Description}", ConsoleColor.Cyan);
                }
            }

            Console.WriteLine();
            UIHelper.PrintColoredLine($"  💰 Value: {Price} gold", ConsoleColor.Yellow);
        }
    }
}
