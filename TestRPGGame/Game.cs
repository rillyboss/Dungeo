using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Dungeon;
using TestRPGGame.DataLoading;
using TestRPGGame.Factories;
using TestRPGGame.Systems;
using TestRPGGame.Combat;
using TestRPGGame.Equipment;
using TestRPGGame.UI;
using EnemyEntity = TestRPGGame.Entities.Enemy.Enemy;

namespace TestRPGGame
{
    public class Game
    {
        private Player? player;
        private Shop shop;
        private CombatSystem combat;
        private bool isRunning;
        private int? lastSaveSlot; // Track the last save slot used for autosave
        private List<Dungeon> dungeons;
        private DungeonProgress dungeonProgress;
        private DungeonRunner dungeonRunner;
        private TestRPGGame.Interfaces.ConsoleInterface consoleInterface;

        public Game()
        {
            consoleInterface = new TestRPGGame.Interfaces.ConsoleInterface();
            shop = new Shop(consoleInterface);
            combat = new CombatSystem();
            isRunning = true;
            lastSaveSlot = null;
            dungeons = DungeonFactory.CreateAllDungeons();
            dungeonProgress = new DungeonProgress();
            dungeonRunner = new DungeonRunner(consoleInterface);
        }

        public void Start()
        {
            DisplayTitleScreen();

            // Show load/new game menu
            bool loadedGame = ShowLoadOrNewMenu();

            if (!loadedGame)
            {
                player = CreateCharacter();
            }

            MainGameLoop();
        }

        private bool ShowLoadOrNewMenu()
        {
            while (true)
            {
                Console.Clear();
                UIHelper.PrintColoredLine("═══════════════════════════════════════════", ConsoleColor.Cyan);
                UIHelper.PrintColoredLine("          SELECT CHARACTER", ConsoleColor.Yellow);
                UIHelper.PrintColoredLine("═══════════════════════════════════════════\n", ConsoleColor.Cyan);

                var slots = SaveSystem.GetAllSaveSlots();

                // Display all save slots
                foreach (var saveSlot in slots)
                {
                    Console.Write($"{saveSlot.SlotNumber}. ");

                    if (saveSlot.IsEmpty)
                    {
                        UIHelper.PrintColoredLine("[EMPTY SLOT - Create New Character]", ConsoleColor.DarkGray);
                    }
                    else
                    {
                        UIHelper.PrintColored($"{saveSlot.Name}", ConsoleColor.Yellow);
                        Console.Write($" - Level {saveSlot.Level} {saveSlot.Class}");
                        Console.Write($" (Saved: {saveSlot.SaveTime:MM/dd/yyyy HH:mm})");
                        Console.WriteLine();
                    }
                }

                Console.WriteLine("\n0. Exit Game");
                Console.WriteLine("\nTo delete a save: Type 'D' + slot number (e.g., D1, D2, D3)");
                Console.Write("Select slot (1-3), delete (D1-D3), or 0 to exit: ");
                string input = Console.ReadLine() ?? "";

                if (input == "0")
                {
                    Environment.Exit(0);
                    return false;
                }

                // Check for delete command (D1, D2, D3)
                if (input.Length == 2 && (input.ToUpper().StartsWith("D")))
                {
                    if (int.TryParse(input.Substring(1), out int delSlot) && delSlot >= 1 && delSlot <= 3)
                    {
                        var slotToDelete = slots.FirstOrDefault(s => s.SlotNumber == delSlot);
                        if (slotToDelete != null && !slotToDelete.IsEmpty)
                        {
                            Console.Write($"\n⚠️  Delete {slotToDelete.Name} (Level {slotToDelete.Level} {slotToDelete.Class})? This cannot be undone! (y/n): ");
                            string confirm = Console.ReadLine() ?? "";
                            if (confirm.ToLower() == "y")
                            {
                                SaveSystem.DeleteSave(delSlot);
                                UIHelper.PrintColoredLine($"\n🗑️  Slot {delSlot} deleted successfully!", ConsoleColor.Yellow);
                                Thread.Sleep(1500);
                            }
                        }
                        else
                        {
                            UIHelper.PrintColoredLine($"\n❌ Slot {delSlot} is already empty!", ConsoleColor.Red);
                            Thread.Sleep(1000);
                        }
                        continue; // Redisplay menu
                    }
                }

                if (int.TryParse(input, out int slot) && slot >= 1 && slot <= 3)
                {
                    var slotInfo = slots.FirstOrDefault(s => s.SlotNumber == slot);

                    if (slotInfo != null && slotInfo.IsEmpty)
                    {
                        // Create new character in this slot
                        lastSaveSlot = slot;
                        return false; // Signal to create new character
                    }
                    else
                    {
                        // Load existing character
                        var (loadedPlayer, loadedProgress) = SaveSystem.LoadGame(slot);
                        if (loadedPlayer != null)
                        {
                            lastSaveSlot = slot;
                            player = loadedPlayer;
                            if (loadedProgress != null)
                            {
                                dungeonProgress = loadedProgress;
                            }
                            UIHelper.PrintColoredLine("\n✅ Game loaded successfully!", ConsoleColor.Green);
                            Thread.Sleep(1500);
                            return true;
                        }
                        else
                        {
                            UIHelper.PrintColoredLine("\n❌ Failed to load save!", ConsoleColor.Red);
                            Thread.Sleep(1500);
                        }
                    }
                }
                else
                {
                    UIHelper.PrintColoredLine("\n❌ Invalid choice!", ConsoleColor.Red);
                    Thread.Sleep(1000);
                }
            }
        }

        private void DisplayTitleScreen()
        {
            Console.Clear();
            UIHelper.PrintColoredLine(@"
╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║   ██████╗ ██╗   ██╗███╗   ██╗ ██████╗ ███████╗ ██████╗    ║
║   ██╔══██╗██║   ██║████╗  ██║██╔════╝ ██╔════╝██╔═══██╗   ║
║   ██║  ██║██║   ██║██╔██╗ ██║██║  ███╗█████╗  ██║   ██║   ║
║   ██║  ██║██║   ██║██║╚██╗██║██║   ██║██╔══╝  ██║   ██║   ║
║   ██████╔╝╚██████╔╝██║ ╚████║╚██████╔╝███████╗╚██████╔╝   ║
║   ╚═════╝  ╚═════╝ ╚═╝  ╚═══╝ ╚═════╝ ╚══════╝ ╚═════╝    ║
║                                                           ║
║                  LEGENDS OF THE CONSOLE                   ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝
", ConsoleColor.Cyan);

            UIHelper.PrintColoredLine("\n      Press any key to begin your adventure...", ConsoleColor.Yellow);
            Console.ReadKey(true);
        }

        private Player CreateCharacter()
        {
            Console.Clear();
            UIHelper.PrintColoredLine("═══════════════════════════════════════════", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("          CHARACTER CREATION", ConsoleColor.Yellow);
            UIHelper.PrintColoredLine("═══════════════════════════════════════════\n", ConsoleColor.Cyan);

            Console.Write("Enter your hero's name: ");
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name)) name = "Hero";

            Console.WriteLine("\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                   CHOOSE YOUR CLASS                    ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");

            UIHelper.PrintColoredLine("⚔️  1. WARRIOR", ConsoleColor.Red);
            Console.WriteLine("   High HP and Defense | Powerful physical attacks");
            Console.WriteLine("   Abilities: Power Strike, Shield Wall, Battle Rage\n");

            UIHelper.PrintColoredLine("🔮 2. MAGE", ConsoleColor.Magenta);
            Console.WriteLine("   High Magic Power | Devastating spells");
            Console.WriteLine("   Abilities: Fireball, Ice Lance, Mana Surge\n");

            UIHelper.PrintColoredLine("🏹 3. ROGUE", ConsoleColor.Green);
            Console.WriteLine("   High Speed and Crit | Quick deadly strikes");
            Console.WriteLine("   Abilities: Backstab, Poison Blade, Shadow Step\n");

            Console.Write("Choose your class (1-3): ");
            string choice = Console.ReadLine();

            PlayerClass playerClass = choice switch
            {
                "1" => PlayerClass.Warrior,
                "2" => PlayerClass.Mage,
                "3" => PlayerClass.Rogue,
                _ => PlayerClass.Warrior
            };

            var newPlayer = new Player(name, playerClass);

            Console.Clear();
            UIHelper.PrintColoredLine($"\n⚡ Welcome, {name} the {playerClass}! ⚡\n", ConsoleColor.Yellow);

            // Show character art (data-driven)
            AsciiArt.DrawClass(playerClass.ToString());

            Thread.Sleep(1000);
            UIHelper.PrintColoredLine("\nYour journey begins now...\n", ConsoleColor.Gray);
            Thread.Sleep(1500);

            return newPlayer;
        }

        private void MainGameLoop()
        {
            while (isRunning)
            {
                DisplayMainMenu();
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        EnterCombat();
                        break;
                    case "2":
                        EnterDungeon();
                        break;
                    case "3":
                        shop.Enter(player);
                        // Auto-save after shop visit
                        AutoSave("after shop");
                        break;
                    case "4":
                        player.Inventory.DisplayInventory(player);
                        break;
                    case "5":
                        player.DisplayCharacterSheet();
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey(true);
                        break;
                    case "6":
                        UnlockAbilities();
                        break;
                    case "7":
                        Rest();
                        break;
                    case "8":
                        SaveGame();
                        break;
                    case "9":
                        DisplayHelp();
                        break;
                    case "0":
                        isRunning = false;
                        Console.Clear();
                        UIHelper.PrintColoredLine("\n⚔️  Thanks for playing! May your legend live on...  ⚔️\n", ConsoleColor.Cyan);
                        break;
                    default:
                        UIHelper.PrintColoredLine("\n❌ Invalid choice!", ConsoleColor.Red);
                        Thread.Sleep(1000);
                        break;
                }
            }
        }

        private void DisplayMainMenu()
        {
            Console.Clear();
            UIHelper.PrintColoredLine("═══════════════════════════════════════════", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine($"  {player.Name} | Level {player.Level} {player.Class}", ConsoleColor.Yellow);
            UIHelper.PrintColoredLine($"  💰 Gold: {player.Gold} | ❤️  HP: {player.CurrentHP}/{player.MaxHP}", ConsoleColor.White);
            UIHelper.PrintColoredLine("═══════════════════════════════════════════\n", ConsoleColor.Cyan);

            Console.WriteLine("1. ⚔️  Enter Combat");
            Console.WriteLine("2. 🏰 Enter Dungeon");
            Console.WriteLine("3. 🏪 Visit Shop");
            Console.WriteLine("4. 🎒 Inventory & Equipment");
            Console.WriteLine("5. 📖 Character Sheet");
            Console.WriteLine("6. 🌟 Unlock Abilities");
            Console.WriteLine("7. 😴 Rest (Restore HP & Mana)");
            Console.WriteLine("8. 💾 Save Game");
            Console.WriteLine("9. ❓ Help");
            Console.WriteLine("0. 🚪 Exit Game");

            Console.Write("\nChoose an option: ");
        }

        private void EnterCombat()
        {
            EnemyEntity enemy = Entities.Enemy.EnemyFactory.CreateEnemy(player.Level);
            bool victory = combat.StartBattle(player, enemy);

            if (victory)
            {
                int goldReward = enemy.GoldReward;
                int expReward = enemy.ExpReward;

                player.Gold += goldReward;
                bool leveledUp = player.GainExperience(expReward);

                AsciiArt.DrawVictory();
                Thread.Sleep(1000);

                Console.WriteLine();
                AsciiArt.DrawGoldReward();
                UIHelper.PrintColoredLine($"💰 Gold earned: {goldReward}", ConsoleColor.Yellow);
                UIHelper.PrintColoredLine($"⭐ Experience gained: {expReward}", ConsoleColor.Cyan);

                if (leveledUp)
                {
                    Console.WriteLine();
                    AsciiArt.DrawLevelUp();
                    UIHelper.PrintColoredLine($"\n🎉 You are now level {player.Level}!", ConsoleColor.Magenta);
                    Thread.Sleep(1500);
                }

                // Random loot drop
                if (new Random().Next(100) < 40) // 40% chance
                {
                    EquipmentItem loot = EquipmentGenerator.GenerateItem(player.Level);
                    Console.WriteLine();
                    AsciiArt.DrawLootDrop();
                    UIHelper.PrintColored($"✨ [{loot.Rarity}] {loot.Name} ", loot.GetRarityColor());
                    Console.WriteLine("dropped!");
                    player.Inventory.BackpackItems.Add(loot);
                }

                // Auto-save after combat
                Console.WriteLine();
                AutoSave("after combat");

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey(true);
            }
            else
            {
                AsciiArt.DrawDefeat();
                Thread.Sleep(1000);

                Console.WriteLine();
                UIHelper.PrintColoredLine("You have been defeated in battle!", ConsoleColor.Red);
                UIHelper.PrintColoredLine("You lost some gold and crawled back to safety...", ConsoleColor.Gray);

                int goldLost = Math.Min(player.Gold / 4, 100);
                player.Gold -= goldLost;
                player.CurrentHP = player.MaxHP / 2;

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey(true);
            }
        }

        private void DisplayHelp()
        {
            Console.Clear();
            UIHelper.PrintColoredLine("═══════════════════════════════════════════", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("                 GAME HELP", ConsoleColor.Yellow);
            UIHelper.PrintColoredLine("═══════════════════════════════════════════\n", ConsoleColor.Cyan);

            Console.WriteLine("⚔️  COMBAT:");
            Console.WriteLine("   Battle enemies to gain gold and experience");
            Console.WriteLine("   Use abilities strategically - they have cooldowns!");
            Console.WriteLine("   Potions can save your life in tough fights\n");

            Console.WriteLine("🏪 SHOP:");
            Console.WriteLine("   Buy better weapons, armor, and potions");
            Console.WriteLine("   Equipment improves your stats");
            Console.WriteLine("   Higher level gear costs more but is stronger\n");

            Console.WriteLine("📊 PROGRESSION:");
            Console.WriteLine("   Level up by gaining experience in combat");
            Console.WriteLine("   Each level increases your base stats");
            Console.WriteLine("   Enemies get stronger as you level up\n");

            Console.WriteLine("💡 TIPS:");
            Console.WriteLine("   Different classes have different playstyles");
            Console.WriteLine("   Save gold for better equipment");
            Console.WriteLine("   Use abilities when they're most effective");

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
        }

        private void SaveGame()
        {
            if (player == null)
            {
                UIHelper.PrintColoredLine("\n❌ No active player to save!", ConsoleColor.Red);
                Thread.Sleep(1500);
                return;
            }

            while (true)
            {
                // Display save slots inline (DisplaySaveSlots method removed from SaveSystem)
                Console.Clear();
                Console.WriteLine("═══════════════════════════════════════════");
                Console.WriteLine("              SAVE SLOTS");
                Console.WriteLine("═══════════════════════════════════════════\n");

                var allSlots = SaveSystem.GetAllSaveSlots();
                foreach (var slotInfo in allSlots)
                {
                    Console.Write($"Slot {slotInfo.SlotNumber}: ");
                    if (slotInfo.IsEmpty)
                    {
                        Console.WriteLine("[EMPTY]");
                    }
                    else
                    {
                        Console.WriteLine($"{slotInfo.Name} - Level {slotInfo.Level} {slotInfo.Class}");
                        Console.WriteLine($"         Saved: {slotInfo.SaveTime:MM/dd/yyyy HH:mm}");
                    }
                    Console.WriteLine();
                }

                Console.WriteLine("0. Cancel");
                Console.Write("\nSelect save slot (1-3): ");
                string input = Console.ReadLine() ?? "";

                if (input == "0")
                {
                    return;
                }

                if (int.TryParse(input, out int slot) && slot >= 1 && slot <= 3)
                {
                    // Check if slot is occupied
                    var slots = SaveSystem.GetAllSaveSlots();
                    var selectedSlot = slots.FirstOrDefault(s => s.SlotNumber == slot);

                    if (selectedSlot != null && !selectedSlot.IsEmpty)
                    {
                        Console.Write($"\n⚠️  Slot {slot} is occupied. Overwrite? (y/n): ");
                        string confirm = Console.ReadLine() ?? "";
                        if (confirm.ToLower() != "y")
                        {
                            continue;
                        }
                    }

                    if (SaveSystem.SaveGame(player, slot, dungeonProgress))
                    {
                        lastSaveSlot = slot; // Track the slot for autosave
                        Console.WriteLine();
                        UIHelper.PrintColoredLine("✅ Game saved successfully!", ConsoleColor.Green);
                        UIHelper.PrintColoredLine($"Saved to slot {slot}: {player.Name} (Level {player.Level} {player.Class})", ConsoleColor.Gray);
                    }
                    else
                    {
                        UIHelper.PrintColoredLine("\n❌ Failed to save game!", ConsoleColor.Red);
                    }

                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey(true);
                    return;
                }
                else
                {
                    UIHelper.PrintColoredLine("\n❌ Invalid slot number!", ConsoleColor.Red);
                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// Auto-save helper method - saves silently to the last used slot
        /// </summary>
        private void AutoSave(string context = "")
        {
            if (lastSaveSlot.HasValue && player != null)
            {
                if (SaveSystem.SaveGame(player, lastSaveSlot.Value, dungeonProgress))
                {
                    string message = string.IsNullOrEmpty(context) ? "💾 Auto-saved" : $"💾 Auto-saved ({context})";
                    UIHelper.PrintColoredLine(message, ConsoleColor.DarkGray);
                }
            }
        }

        private void Rest()
        {
            Console.Clear();
            AsciiArt.DrawRest();
            Console.WriteLine();

            if (player != null)
            {
                int restCost = GameConfig.Config.RestingCost;

                // Check if player can afford to rest
                if (player.Gold < restCost)
                {
                    UIHelper.PrintColoredLine($"😴 You want to rest at the inn...\n", ConsoleColor.Gray);
                    UIHelper.PrintColoredLine($"❌ Not enough gold! Resting costs {restCost} gold.", ConsoleColor.Red);
                    UIHelper.PrintColoredLine($"💰 You have: {player.Gold} gold", ConsoleColor.Yellow);

                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey(true);
                    return;
                }

                int hpBefore = player.CurrentHP;
                int manaBefore = player.CurrentMana;

                player.CurrentHP = player.MaxHP;
                player.CurrentMana = player.MaxMana;
                player.Gold -= restCost;

                int hpRestored = player.CurrentHP - hpBefore;
                int manaRestored = player.CurrentMana - manaBefore;

                UIHelper.PrintColoredLine("😴 You rest at the inn and recover...\n", ConsoleColor.Gray);
                UIHelper.PrintColoredLine($"💰 Paid {restCost} gold for lodging", ConsoleColor.Yellow);
                Thread.Sleep(1000);

                if (hpRestored > 0)
                {
                    UIHelper.PrintColoredLine($"❤️  HP restored: +{hpRestored} ({hpBefore} → {player.CurrentHP})", ConsoleColor.Green);
                }
                else
                {
                    UIHelper.PrintColoredLine("❤️  HP already at maximum", ConsoleColor.Gray);
                }

                if (manaRestored > 0)
                {
                    UIHelper.PrintColoredLine($"💙 Mana restored: +{manaRestored} ({manaBefore} → {player.CurrentMana})", ConsoleColor.Cyan);
                }
                else
                {
                    UIHelper.PrintColoredLine("💙 Mana already at maximum", ConsoleColor.Gray);
                }

                UIHelper.PrintColoredLine("\n✨ You feel refreshed and ready for battle!", ConsoleColor.Yellow);

                // Auto-save after resting
                AutoSave("after rest");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
        }

        private void EnterDungeon()
        {
            if (player == null) return;

            while (true)
            {
                Console.Clear();
                UIHelper.PrintColoredLine("═══════════════════════════════════════════", ConsoleColor.Cyan);
                UIHelper.PrintColoredLine("          DUNGEON SELECTION", ConsoleColor.Yellow);
                UIHelper.PrintColoredLine("═══════════════════════════════════════════\n", ConsoleColor.Cyan);

                UIHelper.PrintColoredLine($"Your Level: {player.Level} | Gold: {player.Gold}", ConsoleColor.White);
                Console.WriteLine();

                Console.WriteLine("╔════════════ AVAILABLE DUNGEONS ════════════╗");

                for (int i = 0; i < dungeons.Count; i++)
                {
                    var dungeon = dungeons[i];
                    bool completed = dungeonProgress.CompletedDungeons.GetValueOrDefault(dungeon.Name, false);
                    bool meetsReqs = dungeon.Requirements.MeetsRequirements(player, dungeonProgress.CompletedDungeons);

                    Console.Write($"\n{i + 1}. ");

                    if (completed)
                    {
                        UIHelper.PrintColored($"✓ {dungeon.Name}", ConsoleColor.Green);
                        Console.WriteLine($" (COMPLETED)");
                    }
                    else if (meetsReqs)
                    {
                        UIHelper.PrintColored($"🏰 {dungeon.Name}", ConsoleColor.Yellow);
                        Console.WriteLine($" - Level {dungeon.Requirements.MinLevel}");
                    }
                    else
                    {
                        UIHelper.PrintColored($"🔒 {dungeon.Name}", ConsoleColor.DarkGray);
                        Console.WriteLine($" - Level {dungeon.Requirements.MinLevel}");
                    }

                    Console.WriteLine($"   Difficulty: {new string('★', dungeon.Difficulty)}");
                    Console.WriteLine($"   Entry Cost: {dungeon.Requirements.GoldCost} gold");

                    // Show requirements
                    if (!meetsReqs)
                    {
                        if (player.Level < dungeon.Requirements.MinLevel)
                        {
                            UIHelper.PrintColoredLine($"   ⚠️  Requires Level {dungeon.Requirements.MinLevel}", ConsoleColor.Red);
                        }
                        if (player.Gold < dungeon.Requirements.GoldCost)
                        {
                            UIHelper.PrintColoredLine($"   ⚠️  Need {dungeon.Requirements.GoldCost - player.Gold} more gold", ConsoleColor.Red);
                        }
                        if (!string.IsNullOrEmpty(dungeon.Requirements.PreviousDungeonRequired) &&
                            !dungeonProgress.CompletedDungeons.GetValueOrDefault(dungeon.Requirements.PreviousDungeonRequired, false))
                        {
                            UIHelper.PrintColoredLine($"   ⚠️  Must complete {dungeon.Requirements.PreviousDungeonRequired} first", ConsoleColor.Red);
                        }
                    }
                    else
                    {
                        UIHelper.PrintColoredLine($"   ✓ Ready to enter!", ConsoleColor.Green);
                    }
                }

                Console.WriteLine("\n╚════════════════════════════════════════════╝");

                Console.WriteLine("\n0. Back to Main Menu");
                Console.Write("\nSelect dungeon: ");

                string input = Console.ReadLine() ?? "";

                if (input == "0")
                {
                    return;
                }

                if (int.TryParse(input, out int dungeonIndex) && dungeonIndex > 0 && dungeonIndex <= dungeons.Count)
                {
                    var dungeon = dungeons[dungeonIndex - 1];

                    if (!dungeon.Requirements.MeetsRequirements(player, dungeonProgress.CompletedDungeons))
                    {
                        UIHelper.PrintColoredLine("\n❌ You don't meet the requirements for this dungeon!", ConsoleColor.Red);
                        Thread.Sleep(2000);
                        continue;
                    }

                    // Confirm entry
                    Console.WriteLine();
                    UIHelper.PrintColoredLine($"Enter {dungeon.Name}?", ConsoleColor.Yellow);
                    if (dungeon.Requirements.GoldCost > 0)
                    {
                        UIHelper.PrintColoredLine($"This will cost {dungeon.Requirements.GoldCost} gold.", ConsoleColor.Gray);
                    }
                    Console.Write("Confirm (y/n): ");
                    string confirm = Console.ReadLine() ?? "";

                    if (confirm.ToLower() == "y")
                    {
                        // Enter the dungeon!
                        bool success = dungeonRunner.RunDungeon(player, dungeon, dungeonProgress);

                        // Auto-save after dungeon completion
                        if (success)
                        {
                            AutoSave("dungeon complete");
                        }

                        // Return to menu
                        return;
                    }
                }
                else
                {
                    UIHelper.PrintColoredLine("\n❌ Invalid choice!", ConsoleColor.Red);
                    Thread.Sleep(1000);
                }
            }
        }

        private void UnlockAbilities()
        {
            if (player == null) return;

            while (true)
            {
                Console.Clear();
                UIHelper.PrintColoredLine("═══════════════════════════════════════════", ConsoleColor.Cyan);
                UIHelper.PrintColoredLine("          UNLOCK ABILITIES", ConsoleColor.Yellow);
                UIHelper.PrintColoredLine("═══════════════════════════════════════════\n", ConsoleColor.Cyan);

                UIHelper.PrintColoredLine($"Your Level: {player.Level} | Gold: {player.Gold}", ConsoleColor.White);
                Console.WriteLine();

                var lockedAbilities = player.Abilities.Where(a => !a.IsUnlocked).ToList();

                if (lockedAbilities.Count == 0)
                {
                    UIHelper.PrintColoredLine("🎉 All abilities unlocked!", ConsoleColor.Green);
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey(true);
                    return;
                }

                Console.WriteLine("╔════════════ LOCKED ABILITIES ════════════╗");
                for (int i = 0; i < lockedAbilities.Count; i++)
                {
                    var ability = lockedAbilities[i];
                    Console.Write($"{i + 1}. ");

                    bool canUnlock = ability.CanUnlock(player.Level, player.Gold);
                    ConsoleColor color = canUnlock ? ConsoleColor.Yellow : ConsoleColor.DarkGray;

                    UIHelper.PrintColored($"🔒 {ability.Name}", color);
                    Console.WriteLine($" - Level {ability.UnlockLevel} | {ability.PurchaseCost} gold");
                    Console.WriteLine($"   {ability.Description}");

                    if (!canUnlock)
                    {
                        if (player.Level < ability.UnlockLevel)
                        {
                            UIHelper.PrintColoredLine($"   ⚠️  Need level {ability.UnlockLevel}", ConsoleColor.Red);
                        }
                        if (player.Gold < ability.PurchaseCost)
                        {
                            UIHelper.PrintColoredLine($"   ⚠️  Need {ability.PurchaseCost} gold", ConsoleColor.Red);
                        }
                    }
                    else
                    {
                        UIHelper.PrintColoredLine($"   ✓ Can unlock now!", ConsoleColor.Green);
                    }

                    Console.WriteLine();
                }
                Console.WriteLine("╚═══════════════════════════════════════════╝");

                Console.WriteLine("\n0. Back to Main Menu");
                Console.Write("\nSelect ability to unlock: ");

                string input = Console.ReadLine() ?? "";

                if (input == "0")
                {
                    return;
                }

                if (int.TryParse(input, out int abilityIndex) && abilityIndex > 0 && abilityIndex <= lockedAbilities.Count)
                {
                    var ability = lockedAbilities[abilityIndex - 1];

                    if (!ability.CanUnlock(player.Level, player.Gold))
                    {
                        UIHelper.PrintColoredLine("\n❌ Cannot unlock this ability yet!", ConsoleColor.Red);

                        if (player.Level < ability.UnlockLevel)
                        {
                            UIHelper.PrintColoredLine($"   Need to reach level {ability.UnlockLevel}", ConsoleColor.Gray);
                        }
                        if (player.Gold < ability.PurchaseCost)
                        {
                            UIHelper.PrintColoredLine($"   Need {ability.PurchaseCost - player.Gold} more gold", ConsoleColor.Gray);
                        }

                        Thread.Sleep(2000);
                        continue;
                    }

                    // Confirm purchase
                    Console.WriteLine();
                    UIHelper.PrintColoredLine($"Unlock {ability.Name} for {ability.PurchaseCost} gold?", ConsoleColor.Yellow);
                    Console.Write("Confirm (y/n): ");
                    string confirm = Console.ReadLine() ?? "";

                    if (confirm.ToLower() == "y")
                    {
                        player.Gold -= ability.PurchaseCost;
                        ability.Unlock();

                        Console.WriteLine();
                        UIHelper.PrintColoredLine($"🌟 {ability.Name} unlocked!", ConsoleColor.Green);
                        UIHelper.PrintColoredLine($"💰 {ability.PurchaseCost} gold spent. Remaining: {player.Gold}", ConsoleColor.Yellow);

                        Thread.Sleep(2000);
                    }
                }
                else
                {
                    UIHelper.PrintColoredLine("\n❌ Invalid choice!", ConsoleColor.Red);
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
