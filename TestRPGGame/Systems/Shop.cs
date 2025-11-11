using System;
using System.Collections.Generic;
using System.Threading;
using TestRPGGame.Entities.Player;
using TestRPGGame.DataLoading;
using TestRPGGame.Equipment;
using TestRPGGame.Abilities;
using TestRPGGame.UI;
using TestRPGGame.Interfaces;


namespace TestRPGGame.Systems
{
    public class Shop
    {
        private List<EquipmentItem> shopInventory;
        private Random random = new Random();
        private IGameInterface gameInterface;

        public Shop(IGameInterface gameInterface)
        {
            this.gameInterface = gameInterface;
            shopInventory = new List<EquipmentItem>();
        }

        private void SendMessage(string message, ConsoleColor color = ConsoleColor.White)
        {
            gameInterface?.OnEvent(new GameEvents.InfoMessageEvent
            {
                Message = message,
                Type = GameEvents.MessageType.Info,
                Color = color
            });
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
                SendMessage("");
                SendMessage($"💰 Your Gold: {player.Gold}\n");

                SendMessage("1. 🛒 Buy Items");
                SendMessage("2. 💵 Sell Items");
                SendMessage("3. 🔄 Refresh Shop (costs 50 gold)");
                SendMessage("4. 🧪 Buy Potions (50 gold each)");
                SendMessage("5. 🚪 Leave Shop");

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
                        SendMessage("\n❌ Invalid choice!");
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
                SendMessage("\n✅ Shop inventory refreshed!");
                Thread.Sleep(1500);
            }
            else
            {
                SendMessage("\n❌ Not enough gold! Need 50 gold.");
                Thread.Sleep(1500);
            }
        }

        private void BuyItems(Player player)
        {
            bool browsing = true;

            while (browsing)
            {
                Console.Clear();
                SendMessage("═══════════════ SHOP INVENTORY ═══════════════\n");
                SendMessage($"💰 Your Gold: {player.Gold}\n");

                if (shopInventory.Count == 0)
                {
                    SendMessage("Shop is empty! Try refreshing.\n");
                }
                else
                {
                    // Display items with their actual index
                    for (int i = 0; i < shopInventory.Count; i++)
                    {
                        var item = shopInventory[i];
                        Console.Write($"  {i + 1}. ");
                        Console.Write($"[{item.Rarity}] {item.Name}", item.GetRarityColor());
                        Console.Write($" ({item.Slot.GetDisplayName()})");
                        SendMessage($" (Lv {item.Level}) - {item.Price} gold");

                        // Show key stats
                        Console.Write("     ");
                        if (item.AttackBonus > 0) Console.Write($"⚔️ +{item.AttackBonus} ");
                        if (item.DefenseBonus > 0) Console.Write($"🛡️ +{item.DefenseBonus} ");
                        if (item.MagicBonus > 0) Console.Write($"🔮 +{item.MagicBonus} ");
                        if (item.HPBonus > 0) Console.Write($"❤️ +{item.HPBonus} ");
                        if (item.SpecialEffects.Count > 0) Console.Write($"✨ x{item.SpecialEffects.Count} ");
                        SendMessage("");
                    }
                    SendMessage("");
                }

                SendMessage("0. Back");
                Console.Write("\nSelect item to buy (or 'v' + number to view details): ");
                string input = Console.ReadLine() ?? "";

                if (input == "0")
                {
                    browsing = false;
                }
                else if (input.StartsWith("v") && int.TryParse(input.Substring(1), out int viewIndex) && viewIndex > 0 && viewIndex <= shopInventory.Count)
                {
                    Console.Clear();
                    SendMessage("");
                    shopInventory[viewIndex - 1].DisplayDetails();
                    SendMessage("\nPress any key to continue...");
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

                        Console.Write($"\n✅ Purchased ");
                        Console.Write($"[{item.Rarity}] {item.Name}", item.GetRarityColor());
                        SendMessage("!");
                        SendMessage($"Item added to your backpack. Remaining gold: {player.Gold}");
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        int needed = item.Price - player.Gold;
                        SendMessage($"\n❌ Not enough gold! You have {player.Gold}, need {needed} more gold.");
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
                SendMessage("═══════════════ SELL ITEMS ═══════════════\n");
                SendMessage($"💰 Your Gold: {player.Gold}\n");

                if (player.Inventory.BackpackItems.Count == 0)
                {
                    SendMessage("Your backpack is empty!\n");
                    SendMessage("Press any key to go back...");
                    Console.ReadKey(true);
                    return;
                }

                SendMessage("╔═══ BACKPACK ═══╗\n");
                for (int i = 0; i < player.Inventory.BackpackItems.Count; i++)
                {
                    var item = player.Inventory.BackpackItems[i];
                    int sellPrice = (int)(item.Price * 0.6); // Sell for 60% of buy price

                    Console.Write($"  {i + 1}. ");
                    Console.Write($"[{item.Rarity}] {item.Name}", item.GetRarityColor());
                    SendMessage($" - Sell for {sellPrice} gold");
                }
                SendMessage("");

                SendMessage("0. Back");
                Console.Write("\nSelect item to sell (or 'v' + number to view details): ");
                string input = Console.ReadLine() ?? "";

                if (input == "0")
                {
                    selling = false;
                }
                else if (input.StartsWith("v") && int.TryParse(input.Substring(1), out int viewIndex) && viewIndex > 0 && viewIndex <= player.Inventory.BackpackItems.Count)
                {
                    Console.Clear();
                    SendMessage("");
                    player.Inventory.BackpackItems[viewIndex - 1].DisplayDetails();
                    SendMessage("\nPress any key to continue...");
                    Console.ReadKey(true);
                }
                else if (int.TryParse(input, out int sellIndex) && sellIndex > 0 && sellIndex <= player.Inventory.BackpackItems.Count)
                {
                    var item = player.Inventory.BackpackItems[sellIndex - 1];
                    int sellPrice = (int)(item.Price * 0.6);

                    Console.Write($"\nSell ");
                    Console.Write($"[{item.Rarity}] {item.Name}", item.GetRarityColor());
                    Console.Write($" for {sellPrice} gold? (y/n): ");

                    string confirm = Console.ReadLine() ?? "";

                    if (confirm.ToLower() == "y")
                    {
                        player.Gold += sellPrice;
                        player.Inventory.BackpackItems.RemoveAt(sellIndex - 1);

                        SendMessage($"\n✅ Sold for {sellPrice} gold!");
                        Thread.Sleep(1500);
                    }
                }
            }
        }

        private void BuyPotions(Player player)
        {
            Console.Clear();
            SendMessage("═══════════════ POTIONS ═══════════════\n");

            SendMessage($"🧪 Health Potion - Restores 50% HP");
            SendMessage($"💰 Price: 50 gold each");
            SendMessage($"\nYou currently have: {player.PotionCount} potions");
            SendMessage($"Your gold: {player.Gold}");

            Console.Write("\nHow many potions? (0 to cancel): ");
            string input = Console.ReadLine() ?? "";

            if (int.TryParse(input, out int amount) && amount > 0)
            {
                int totalCost = amount * 50;
                if (player.Gold >= totalCost)
                {
                    player.Gold -= totalCost;
                    player.PotionCount += amount;
                    SendMessage($"\n✅ Purchased {amount} potion(s)!");
                }
                else
                {
                    SendMessage("\n❌ Not enough gold!");
                }
            }
            else if (amount < 0)
            {
                SendMessage("\n❌ Invalid amount!");
            }

            Thread.Sleep(2000);
        }
    }
}
