using System;
using System.Collections.Generic;
using System.Threading;
using TestRPGGame.Entities.Player;
using TestRPGGame.DataLoading;
using TestRPGGame.Equipment;
using TestRPGGame.Abilities;
using TestRPGGame.UI;

namespace TestRPGGame.Systems
{
    public class Shop
    {
        private List<EquipmentItem> shopInventory;
        private Random random = new Random();

        public Shop()
        {
            shopInventory = new List<EquipmentItem>();
        }

        public void Enter(Player player)
        {
            // Generate shop inventory if empty or refresh
            RefreshShopInventory(player.Level);

            bool shopping = true;

            while (shopping)
            {
                Console.Clear();
                AsciiArt.DrawShop();
                Console.WriteLine();
                UIHelper.PrintColoredLine($"💰 Your Gold: {player.Gold}\n", ConsoleColor.Yellow);

                Console.WriteLine("1. 🛒 Buy Items");
                Console.WriteLine("2. 💵 Sell Items");
                Console.WriteLine("3. 🔄 Refresh Shop (costs 50 gold)");
                Console.WriteLine("4. 🧪 Buy Potions (50 gold each)");
                Console.WriteLine("5. 🚪 Leave Shop");

                Console.Write("\nWhat would you like to do? ");
                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1":
                        BuyItems(player);
                        break;
                    case "2":
                        SellItems(player);
                        break;
                    case "3":
                        RefreshShop(player);
                        break;
                    case "4":
                        BuyPotions(player);
                        break;
                    case "5":
                        shopping = false;
                        break;
                    default:
                        UIHelper.PrintColoredLine("\n❌ Invalid choice!", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        break;
                }
            }
        }

        private void RefreshShopInventory(int playerLevel)
        {
            shopInventory.Clear();

            // Generate 8-12 random items
            int itemCount = 8 + random.Next(5);

            for (int i = 0; i < itemCount; i++)
            {
                // Items can be slightly above or below player level
                EquipmentItem item = EquipmentGenerator.GenerateItem(playerLevel);
                shopInventory.Add(item);
            }
        }

        private void RefreshShop(Player player)
        {
            if (player.Gold >= 50)
            {
                player.Gold -= 50;
                RefreshShopInventory(player.Level);
                UIHelper.PrintColoredLine("\n✅ Shop inventory refreshed!", ConsoleColor.Green);
                Thread.Sleep(1500);
            }
            else
            {
                UIHelper.PrintColoredLine("\n❌ Not enough gold! Need 50 gold.", ConsoleColor.Red);
                Thread.Sleep(1500);
            }
        }

        private void BuyItems(Player player)
        {
            bool browsing = true;

            while (browsing)
            {
                Console.Clear();
                UIHelper.PrintColoredLine("═══════════════ SHOP INVENTORY ═══════════════\n", ConsoleColor.Cyan);
                UIHelper.PrintColoredLine($"💰 Your Gold: {player.Gold}\n", ConsoleColor.Yellow);

                if (shopInventory.Count == 0)
                {
                    Console.WriteLine("Shop is empty! Try refreshing.\n");
                }
                else
                {
                    // Display items with their actual index
                    for (int i = 0; i < shopInventory.Count; i++)
                    {
                        var item = shopInventory[i];
                        Console.Write($"  {i + 1}. ");
                        UIHelper.PrintColored($"[{item.Rarity}] {item.Name}", item.GetRarityColor());
                        Console.Write($" ({item.Slot})");
                        Console.WriteLine($" (Lv {item.Level}) - {item.Price} gold");

                        // Show key stats
                        Console.Write("     ");
                        if (item.AttackBonus > 0) Console.Write($"⚔️ +{item.AttackBonus} ");
                        if (item.DefenseBonus > 0) Console.Write($"🛡️ +{item.DefenseBonus} ");
                        if (item.MagicBonus > 0) Console.Write($"🔮 +{item.MagicBonus} ");
                        if (item.HPBonus > 0) Console.Write($"❤️ +{item.HPBonus} ");
                        if (item.SpecialEffects.Count > 0) Console.Write($"✨ x{item.SpecialEffects.Count} ");
                        Console.WriteLine();
                    }
                    Console.WriteLine();
                }

                Console.WriteLine("0. Back");
                Console.Write("\nSelect item to buy (or 'v' + number to view details): ");
                string input = Console.ReadLine() ?? "";

                if (input == "0")
                {
                    browsing = false;
                }
                else if (input.StartsWith("v") && int.TryParse(input.Substring(1), out int viewIndex) && viewIndex > 0 && viewIndex <= shopInventory.Count)
                {
                    Console.Clear();
                    Console.WriteLine();
                    shopInventory[viewIndex - 1].DisplayDetails();
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey(true);
                }
                else if (int.TryParse(input, out int buyIndex) && buyIndex > 0 && buyIndex <= shopInventory.Count)
                {
                    var item = shopInventory[buyIndex - 1];

                    if (player.Gold >= item.Price)
                    {
                        player.Gold -= item.Price;
                        player.Inventory.BackpackItems.Add(item);
                        shopInventory.RemoveAt(buyIndex - 1);

                        UIHelper.PrintColored($"\n✅ Purchased ", ConsoleColor.Green);
                        UIHelper.PrintColored($"[{item.Rarity}] {item.Name}", item.GetRarityColor());
                        Console.WriteLine("!");
                        UIHelper.PrintColoredLine($"Item added to your backpack. Remaining gold: {player.Gold}", ConsoleColor.Gray);
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        int needed = item.Price - player.Gold;
                        UIHelper.PrintColoredLine($"\n❌ Not enough gold! You have {player.Gold}, need {needed} more gold.", ConsoleColor.Red);
                        Thread.Sleep(1500);
                    }
                }
            }
        }

        private void SellItems(Player player)
        {
            bool selling = true;

            while (selling)
            {
                Console.Clear();
                UIHelper.PrintColoredLine("═══════════════ SELL ITEMS ═══════════════\n", ConsoleColor.Cyan);
                UIHelper.PrintColoredLine($"💰 Your Gold: {player.Gold}\n", ConsoleColor.Yellow);

                if (player.Inventory.BackpackItems.Count == 0)
                {
                    Console.WriteLine("Your backpack is empty!\n");
                    Console.WriteLine("Press any key to go back...");
                    Console.ReadKey(true);
                    return;
                }

                Console.WriteLine("╔═══ BACKPACK ═══╗\n");
                for (int i = 0; i < player.Inventory.BackpackItems.Count; i++)
                {
                    var item = player.Inventory.BackpackItems[i];
                    int sellPrice = (int)(item.Price * 0.6); // Sell for 60% of buy price

                    Console.Write($"  {i + 1}. ");
                    UIHelper.PrintColored($"[{item.Rarity}] {item.Name}", item.GetRarityColor());
                    Console.WriteLine($" - Sell for {sellPrice} gold");
                }
                Console.WriteLine();

                Console.WriteLine("0. Back");
                Console.Write("\nSelect item to sell (or 'v' + number to view details): ");
                string input = Console.ReadLine() ?? "";

                if (input == "0")
                {
                    selling = false;
                }
                else if (input.StartsWith("v") && int.TryParse(input.Substring(1), out int viewIndex) && viewIndex > 0 && viewIndex <= player.Inventory.BackpackItems.Count)
                {
                    Console.Clear();
                    Console.WriteLine();
                    player.Inventory.BackpackItems[viewIndex - 1].DisplayDetails();
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey(true);
                }
                else if (int.TryParse(input, out int sellIndex) && sellIndex > 0 && sellIndex <= player.Inventory.BackpackItems.Count)
                {
                    var item = player.Inventory.BackpackItems[sellIndex - 1];
                    int sellPrice = (int)(item.Price * 0.6);

                    Console.Write($"\nSell ");
                    UIHelper.PrintColored($"[{item.Rarity}] {item.Name}", item.GetRarityColor());
                    Console.Write($" for {sellPrice} gold? (y/n): ");

                    string confirm = Console.ReadLine() ?? "";

                    if (confirm.ToLower() == "y")
                    {
                        player.Gold += sellPrice;
                        player.Inventory.BackpackItems.RemoveAt(sellIndex - 1);

                        UIHelper.PrintColoredLine($"\n✅ Sold for {sellPrice} gold!", ConsoleColor.Green);
                        Thread.Sleep(1500);
                    }
                }
            }
        }

        private void BuyPotions(Player player)
        {
            Console.Clear();
            UIHelper.PrintColoredLine("═══════════════ POTIONS ═══════════════\n", ConsoleColor.Green);

            Console.WriteLine($"🧪 Health Potion - Restores 50% HP");
            Console.WriteLine($"💰 Price: 50 gold each");
            Console.WriteLine($"\nYou currently have: {player.PotionCount} potions");
            Console.WriteLine($"Your gold: {player.Gold}");

            Console.Write("\nHow many potions? (0 to cancel): ");
            string input = Console.ReadLine() ?? "";

            if (int.TryParse(input, out int amount) && amount > 0)
            {
                int totalCost = amount * 50;
                if (player.Gold >= totalCost)
                {
                    player.Gold -= totalCost;
                    player.PotionCount += amount;
                    UIHelper.PrintColoredLine($"\n✅ Purchased {amount} potion(s)!", ConsoleColor.Green);
                }
                else
                {
                    UIHelper.PrintColoredLine("\n❌ Not enough gold!", ConsoleColor.Red);
                }
            }
            else if (amount < 0)
            {
                UIHelper.PrintColoredLine("\n❌ Invalid amount!", ConsoleColor.Red);
            }

            Thread.Sleep(2000);
        }
    }
}
