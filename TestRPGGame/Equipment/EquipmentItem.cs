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
            Console.Write($"[{Rarity}] {Name}", GetRarityColor());
            Console.WriteLine($" (Lv {Level})");
            Console.WriteLine($"Slot: {Slot.GetDisplayName()}");

            // Display attack type for weapons
            if (WeaponAttackType.HasValue)
            {
                Console.Write("  Attack Type: ");
                Console.Write($"{AttackTypeSystem.GetAttackTypeIcon(WeaponAttackType.Value)} {WeaponAttackType.Value}",
                    AttackTypeSystem.GetAttackTypeColor(WeaponAttackType.Value));
                Console.WriteLine();
            }

            Console.WriteLine();

            // Display stats
            // Show damage range for weapons (if it's a weapon)
            if (Slot == EquipmentSlot.Weapon && MaxDamage > 0)
            {
                Console.WriteLine($"  ⚔️  Damage: {MinDamage}-{MaxDamage}");
                if (Accuracy < 1.0)
                {
                    Console.WriteLine($"  🎯 Accuracy: {Accuracy:P0}");
                }
            }
            else if (AttackBonus > 0)
            {
                Console.WriteLine($"  ⚔️  Attack: +{AttackBonus}");
            }

            if (DefenseBonus > 0) Console.WriteLine($"  🛡️  Defense: +{DefenseBonus}");
            if (MagicBonus > 0) Console.WriteLine($"  🔮 Magic: +{MagicBonus}");
            if (HPBonus > 0) Console.WriteLine($"  ❤️  HP: +{HPBonus}");
            if (ManaBonus > 0) Console.WriteLine($"  💙 Mana: +{ManaBonus}");
            if (AgilityBonus > 0) Console.WriteLine($"  ⚡ Agility: +{AgilityBonus}");
            if (CritBonus > 0) Console.WriteLine($"  💥 Crit Chance: +{CritBonus:P0}");

            // Display special effects
            if (SpecialEffects.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("  ✨ SPECIAL EFFECTS:");
                foreach (var effect in SpecialEffects)
                {
                    Console.WriteLine($"    • {effect.Description}");
                }
            }

            Console.WriteLine();
            Console.WriteLine($"  💰 Value: {Price} gold");
        }
    }
}
