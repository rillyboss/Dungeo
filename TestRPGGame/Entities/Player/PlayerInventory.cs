using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TestRPGGame.Equipment;
using TestRPGGame.UI;

namespace TestRPGGame.Entities.Player
{
    public class PlayerInventory
    {
        // Equipment slots
        public EquipmentItem? Weapon { get; set; }
        public EquipmentItem? Armor { get; set; }
        public EquipmentItem? Helmet { get; set; }
        public EquipmentItem? Boots { get; set; }
        public EquipmentItem? Gloves { get; set; }
        public EquipmentItem? Ring1 { get; set; }
        public EquipmentItem? Ring2 { get; set; }
        public EquipmentItem? Amulet { get; set; }
        public EquipmentItem? Relic { get; set; }

        public List<EquipmentItem> BackpackItems { get; set; }

        public PlayerInventory()
        {
            BackpackItems = new List<EquipmentItem>();
        }

        public void DisplayInventory(Player player)
        {
            bool managing = true;

            while (managing)
            {
                Console.Clear();
                UIHelper.PrintColoredLine("╔════════════════════════════════════════════════════════╗", ConsoleColor.Cyan);
                UIHelper.PrintColoredLine("║                    INVENTORY                           ║", ConsoleColor.Yellow);
                UIHelper.PrintColoredLine("╚════════════════════════════════════════════════════════╝\n", ConsoleColor.Cyan);

                // Display equipped items
                Console.WriteLine("═══ EQUIPPED ITEMS ═══\n");
                DisplaySlot("Weapon", Weapon);
                DisplaySlot("Armor", Armor);
                DisplaySlot("Helmet", Helmet);
                DisplaySlot("Boots", Boots);
                DisplaySlot("Gloves", Gloves);
                DisplaySlot("Ring 1", Ring1);
                DisplaySlot("Ring 2", Ring2);
                DisplaySlot("Amulet", Amulet);
                DisplaySlot("Relic", Relic);

                Console.WriteLine("\n═══ BACKPACK ═══\n");
                if (BackpackItems.Count == 0)
                {
                    Console.WriteLine("  (Empty)\n");
                }
                else
                {
                    for (int i = 0; i < BackpackItems.Count; i++)
                    {
                        var item = BackpackItems[i];
                        Console.Write($"  {i + 1}. ");
                        UIHelper.PrintColored($"[{item.Rarity}] {item.Name}", item.GetRarityColor());
                        Console.WriteLine($" (Lv {item.Level}) - {item.Slot}");
                    }
                    Console.WriteLine();
                }

                // Display total stats
                DisplayTotalStats(player);

                Console.WriteLine("\n1. Equip item from backpack");
                Console.WriteLine("2. Unequip item");
                Console.WriteLine("3. View item details");
                Console.WriteLine("4. Drop item");
                Console.WriteLine("5. Back to main menu");

                Console.Write("\nChoose option: ");
                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1":
                        EquipItem(player);
                        break;
                    case "2":
                        UnequipItem();
                        break;
                    case "3":
                        ViewItemDetails();
                        break;
                    case "4":
                        DropItem();
                        break;
                    case "5":
                        managing = false;
                        break;
                    default:
                        UIHelper.PrintColoredLine("\n❌ Invalid choice!", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        break;
                }
            }
        }

        private void DisplaySlot(string slotName, EquipmentItem? item)
        {
            Console.Write($"  {slotName,-10}: ");
            if (item != null)
            {
                UIHelper.PrintColoredLine($"[{item.Rarity}] {item.Name}", item.GetRarityColor());
            }
            else
            {
                UIHelper.PrintColoredLine("(Empty)", ConsoleColor.DarkGray);
            }
        }

        private void DisplayTotalStats(Player player)
        {
            Console.WriteLine("═══ TOTAL STATS FROM EQUIPMENT ═══\n");
            var stats = GetTotalStats();

            if (stats.Attack > 0) Console.WriteLine($"  ⚔️  Attack: +{stats.Attack}");
            if (stats.Defense > 0) Console.WriteLine($"  🛡️  Defense: +{stats.Defense}");
            if (stats.Magic > 0) Console.WriteLine($"  🔮 Magic: +{stats.Magic}");
            if (stats.HP > 0) Console.WriteLine($"  ❤️  HP: +{stats.HP}");
            if (stats.Mana > 0) Console.WriteLine($"  💙 Mana: +{stats.Mana}");
            if (stats.Speed > 0) Console.WriteLine($"  ⚡ Speed: +{stats.Speed}");
            if (stats.Crit > 0) Console.WriteLine($"  💥 Crit: +{stats.Crit:P0}");

            // List all special effects
            var allEffects = GetAllSpecialEffects();
            if (allEffects.Count > 0)
            {
                Console.WriteLine();
                UIHelper.PrintColoredLine("  ✨ ACTIVE SPECIAL EFFECTS:", ConsoleColor.Yellow);
                foreach (var effect in allEffects)
                {
                    UIHelper.PrintColoredLine($"    • {effect.Description}", ConsoleColor.Cyan);
                }
            }
        }

        public (int Attack, int Defense, int Magic, int HP, int Mana, int Speed, double Crit) GetTotalStats()
        {
            int attack = 0, defense = 0, magic = 0, hp = 0, mana = 0, speed = 0;
            double crit = 0;

            var allEquipment = new[] { Weapon, Armor, Helmet, Boots, Gloves, Ring1, Ring2, Amulet, Relic };

            foreach (var item in allEquipment)
            {
                if (item != null)
                {
                    attack += item.AttackBonus;
                    defense += item.DefenseBonus;
                    magic += item.MagicBonus;
                    hp += item.HPBonus;
                    mana += item.ManaBonus;
                    speed += item.SpeedBonus;
                    crit += item.CritBonus;

                    // Add passive bonuses from special effects
                    foreach (var effect in item.SpecialEffects)
                    {
                        if (effect.Type == EffectType.AttackBonus && effect.ProcChance >= 1.0)
                            attack += effect.Value;
                        if (effect.Type == EffectType.DefenseBonus && effect.ProcChance >= 1.0)
                            defense += effect.Value;
                        if (effect.Type == EffectType.SpeedBonus && effect.ProcChance >= 1.0)
                            speed += effect.Value;
                        if (effect.Type == EffectType.CritBonus && effect.ProcChance >= 1.0)
                            crit += effect.Value / 100.0;
                    }
                }
            }

            return (attack, defense, magic, hp, mana, speed, crit);
        }

        public List<SpecialEffect> GetAllSpecialEffects()
        {
            var effects = new List<SpecialEffect>();
            var allEquipment = new[] { Weapon, Armor, Helmet, Boots, Gloves, Ring1, Ring2, Amulet, Relic };

            foreach (var item in allEquipment)
            {
                if (item != null)
                {
                    effects.AddRange(item.SpecialEffects);
                }
            }

            return effects;
        }

        private void EquipItem(Player player)
        {
            if (BackpackItems.Count == 0)
            {
                UIHelper.PrintColoredLine("\n❌ No items in backpack!", ConsoleColor.Red);
                Thread.Sleep(1500);
                return;
            }

            Console.Write("\nEnter item number to equip (0 to cancel): ");
            string input = Console.ReadLine() ?? "";

            if (int.TryParse(input, out int index) && index > 0 && index <= BackpackItems.Count)
            {
                var item = BackpackItems[index - 1];
                BackpackItems.RemoveAt(index - 1);

                EquipmentItem? unequipped = null;

                switch (item.Slot)
                {
                    case EquipmentSlot.Weapon:
                        unequipped = Weapon;
                        Weapon = item;
                        break;
                    case EquipmentSlot.Armor:
                        unequipped = Armor;
                        Armor = item;
                        break;
                    case EquipmentSlot.Helmet:
                        unequipped = Helmet;
                        Helmet = item;
                        break;
                    case EquipmentSlot.Boots:
                        unequipped = Boots;
                        Boots = item;
                        break;
                    case EquipmentSlot.Gloves:
                        unequipped = Gloves;
                        Gloves = item;
                        break;
                    case EquipmentSlot.Ring1:
                        if (Ring1 == null)
                        {
                            Ring1 = item;
                        }
                        else if (Ring2 == null)
                        {
                            Ring2 = item;
                        }
                        else
                        {
                            // Ask which ring to replace
                            Console.WriteLine("\nBoth ring slots are full. Which ring to replace?");
                            Console.WriteLine("1. Ring 1");
                            Console.WriteLine("2. Ring 2");
                            Console.Write("Choice: ");
                            string ringChoice = Console.ReadLine() ?? "";
                            if (ringChoice == "1")
                            {
                                unequipped = Ring1;
                                Ring1 = item;
                            }
                            else
                            {
                                unequipped = Ring2;
                                Ring2 = item;
                            }
                        }
                        break;
                    case EquipmentSlot.Ring2:
                        if (Ring1 == null)
                        {
                            Ring1 = item;
                        }
                        else if (Ring2 == null)
                        {
                            Ring2 = item;
                        }
                        else
                        {
                            unequipped = Ring2;
                            Ring2 = item;
                        }
                        break;
                    case EquipmentSlot.Amulet:
                        unequipped = Amulet;
                        Amulet = item;
                        break;
                    case EquipmentSlot.Relic:
                        unequipped = Relic;
                        Relic = item;
                        break;
                }

                if (unequipped != null)
                {
                    BackpackItems.Add(unequipped);
                }

                // Update player stats
                player.UpdateStatsFromEquipment();

                UIHelper.PrintColoredLine($"\n✅ Equipped {item.Name}!", ConsoleColor.Green);
                Thread.Sleep(1500);
            }
        }

        private void UnequipItem()
        {
            Console.WriteLine("\nWhich item to unequip?");
            Console.WriteLine("1. Weapon");
            Console.WriteLine("2. Armor");
            Console.WriteLine("3. Helmet");
            Console.WriteLine("4. Boots");
            Console.WriteLine("5. Gloves");
            Console.WriteLine("6. Ring 1");
            Console.WriteLine("7. Ring 2");
            Console.WriteLine("8. Amulet");
            Console.WriteLine("9. Relic");
            Console.WriteLine("0. Cancel");

            Console.Write("\nChoice: ");
            string choice = Console.ReadLine() ?? "";

            EquipmentItem? item = null;

            switch (choice)
            {
                case "1":
                    item = Weapon;
                    Weapon = null;
                    break;
                case "2":
                    item = Armor;
                    Armor = null;
                    break;
                case "3":
                    item = Helmet;
                    Helmet = null;
                    break;
                case "4":
                    item = Boots;
                    Boots = null;
                    break;
                case "5":
                    item = Gloves;
                    Gloves = null;
                    break;
                case "6":
                    item = Ring1;
                    Ring1 = null;
                    break;
                case "7":
                    item = Ring2;
                    Ring2 = null;
                    break;
                case "8":
                    item = Amulet;
                    Amulet = null;
                    break;
                case "9":
                    item = Relic;
                    Relic = null;
                    break;
                default:
                    return;
            }

            if (item != null)
            {
                BackpackItems.Add(item);
                UIHelper.PrintColoredLine($"\n✅ Unequipped {item.Name}!", ConsoleColor.Green);
                Thread.Sleep(1500);
            }
            else
            {
                UIHelper.PrintColoredLine("\n❌ No item equipped in that slot!", ConsoleColor.Red);
                Thread.Sleep(1500);
            }
        }

        private void ViewItemDetails()
        {
            if (BackpackItems.Count == 0)
            {
                UIHelper.PrintColoredLine("\n❌ No items in backpack!", ConsoleColor.Red);
                Thread.Sleep(1500);
                return;
            }

            Console.Write("\nEnter item number to view (0 to cancel): ");
            string input = Console.ReadLine() ?? "";

            if (int.TryParse(input, out int index) && index > 0 && index <= BackpackItems.Count)
            {
                Console.Clear();
                Console.WriteLine();
                BackpackItems[index - 1].DisplayDetails();
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey(true);
            }
        }

        private void DropItem()
        {
            if (BackpackItems.Count == 0)
            {
                UIHelper.PrintColoredLine("\n❌ No items in backpack!", ConsoleColor.Red);
                Thread.Sleep(1500);
                return;
            }

            Console.Write("\nEnter item number to drop (0 to cancel): ");
            string input = Console.ReadLine() ?? "";

            if (int.TryParse(input, out int index) && index > 0 && index <= BackpackItems.Count)
            {
                var item = BackpackItems[index - 1];
                Console.Write($"\nAre you sure you want to drop {item.Name}? (y/n): ");
                string confirm = Console.ReadLine() ?? "";

                if (confirm.ToLower() == "y")
                {
                    BackpackItems.RemoveAt(index - 1);
                    UIHelper.PrintColoredLine($"\n✅ Dropped {item.Name}!", ConsoleColor.Green);
                    Thread.Sleep(1500);
                }
            }
        }
    }
}
