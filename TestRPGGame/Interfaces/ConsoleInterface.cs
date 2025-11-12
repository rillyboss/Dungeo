using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TestRPGGame.Entities.Player;
using TestRPGGame.Equipment;
using TestRPGGame.Systems;
using TestRPGGame.UI;

namespace TestRPGGame.Interfaces
{
    /// <summary>
    /// Console-based game interface - the original UI experience
    /// </summary>
    public class ConsoleInterface : IGameInterface
    {
        public ConsoleInterface()
        {
            // Show title screen
            DisplayTitleScreen();
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

        public void OnEvent<T>(T gameEvent) where T : class
        {
            // Console interface displays events as they happen
            switch (gameEvent)
            {
                case GameEvents.CharacterCreatedEvent e:
                    Console.Clear();
                    UIHelper.PrintColoredLine($"\n⚡ Welcome, {e.Name} the {e.Class}! ⚡\n", ConsoleColor.Yellow);

                    // Show character art based on class (data-driven)
                    AsciiArt.DrawClass(e.Class.ToString());

                    Thread.Sleep(1000);
                    UIHelper.PrintColoredLine("\nYour journey begins now...\n", ConsoleColor.Gray);
                    Thread.Sleep(1500);
                    break;

                case GameEvents.CombatStartedEvent e:
                    Console.Clear();

                    // Display enemy ASCII art
                    AsciiArt.DrawEnemy(e.EnemyName);

                    UIHelper.PrintColoredLine("\n═══════════════════════════════════════════", ConsoleColor.Red);
                    UIHelper.PrintColoredLine($"      BATTLE: {e.EnemyName}", ConsoleColor.Yellow);
                    UIHelper.PrintColoredLine("═══════════════════════════════════════════\n", ConsoleColor.Red);
                    UIHelper.PrintColoredLine($"Level {e.EnemyLevel} | HP: {e.EnemyMaxHP} | ATK: {e.EnemyAttack} | DEF: {e.EnemyDefense}", ConsoleColor.Gray);
                    break;

                case GameEvents.CombatTurnStartEvent e:
                    Console.WriteLine("\n" + new string('─', 43));
                    UIHelper.PrintColoredLine($"TURN {e.TurnNumber}", ConsoleColor.Cyan);
                    Console.WriteLine(new string('─', 43));

                    UIHelper.PrintColored($"❤️  {e.PlayerHP}/{e.PlayerMaxHP} HP", ConsoleColor.Green);
                    Console.Write(" | ");
                    UIHelper.PrintColored($"💙 {e.PlayerMana}/{e.PlayerMaxMana} Mana", ConsoleColor.Cyan);
                    Console.WriteLine();

                    UIHelper.PrintColoredLine($"👹 Enemy: {e.EnemyHP}/{e.EnemyMaxHP} HP", ConsoleColor.Red);

                    if (e.PlayerActiveEffects.Any())
                    {
                        UIHelper.PrintColored("✨ Your effects: ", ConsoleColor.Yellow);
                        Console.WriteLine(string.Join(", ", e.PlayerActiveEffects));
                    }

                    if (e.EnemyActiveEffects.Any())
                    {
                        UIHelper.PrintColored("💀 Enemy effects: ", ConsoleColor.Magenta);
                        Console.WriteLine(string.Join(", ", e.EnemyActiveEffects));
                    }
                    break;

                case GameEvents.DamageDealtEvent e:
                    ConsoleColor damageColor = e.IsCritical ? ConsoleColor.Yellow : ConsoleColor.White;
                    string critText = e.IsCritical ? " ⚡CRITICAL!⚡" : "";
                    UIHelper.PrintColoredLine($"💥 {e.Attacker} deals {e.Damage} {e.AttackType} damage to {e.Target}{critText}", damageColor);
                    Thread.Sleep(500);
                    break;

                case GameEvents.AbilityUsedEvent e:
                    UIHelper.PrintColoredLine($"✨ {e.User} used {e.AbilityName}!", ConsoleColor.Cyan);
                    UIHelper.PrintColoredLine($"   {e.Description}", ConsoleColor.Gray);
                    Thread.Sleep(800);
                    break;

                case GameEvents.EffectAppliedEvent e:
                    UIHelper.PrintColoredLine($"🎯 {e.EffectName} applied to {e.Target} ({e.Duration} turns)", ConsoleColor.Magenta);
                    UIHelper.PrintColoredLine($"   {e.Description}", ConsoleColor.Gray);
                    Thread.Sleep(500);
                    break;

                case GameEvents.PotionUsedEvent e:
                    UIHelper.PrintColoredLine($"\n🧪 Potion consumed! Restored {e.HPRestored} HP", ConsoleColor.Green);
                    UIHelper.PrintColoredLine($"Potions remaining: {e.PotionsRemaining}", ConsoleColor.Gray);
                    Thread.Sleep(1000);
                    break;

                case GameEvents.CombatEndedEvent e:
                    if (e.PlayerVictory)
                    {
                        AsciiArt.DrawVictory();
                        Thread.Sleep(1000);

                        Console.WriteLine();
                        AsciiArt.DrawGoldReward();
                        UIHelper.PrintColoredLine($"💰 Gold earned: {e.GoldEarned}", ConsoleColor.Yellow);
                        UIHelper.PrintColoredLine($"⭐ Experience gained: {e.ExperienceEarned}", ConsoleColor.Cyan);

                        if (e.LootDropped != null)
                        {
                            Console.WriteLine();
                            AsciiArt.DrawLootDrop();
                            UIHelper.PrintColored($"✨ [{e.LootDropped.Rarity}] {e.LootDropped.Name} ", e.LootDropped.GetRarityColor());
                            Console.WriteLine("dropped!");
                        }
                    }
                    else
                    {
                        AsciiArt.DrawDefeat();
                        Thread.Sleep(1000);

                        Console.WriteLine();
                        UIHelper.PrintColoredLine("You have been defeated in battle!", ConsoleColor.Red);
                        UIHelper.PrintColoredLine("You lost some gold and crawled back to safety...", ConsoleColor.Gray);
                        UIHelper.PrintColoredLine($"💰 Gold lost: {e.GoldLost}", ConsoleColor.Yellow);
                    }

                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey(true);
                    break;

                case GameEvents.PlayerLeveledUpEvent e:
                    Console.WriteLine();
                    AsciiArt.DrawLevelUp();
                    UIHelper.PrintColoredLine($"\n🎉 You are now level {e.NewLevel}!", ConsoleColor.Magenta);
                    UIHelper.PrintColoredLine($"HP: {e.NewMaxHP} | Mana: {e.NewMaxMana} | ATK: {e.NewAttack} | DEF: {e.NewDefense}", ConsoleColor.Green);
                    Thread.Sleep(1500);
                    break;

                case GameEvents.GameSavedEvent e:
                    if (e.Success)
                    {
                        UIHelper.PrintColoredLine($"💾 Auto-saved to slot {e.SlotNumber}", ConsoleColor.DarkGray);
                    }
                    break;

                case GameEvents.InfoMessageEvent e:
                    var color = e.Color ?? e.Type switch
                    {
                        GameEvents.MessageType.Success => ConsoleColor.Green,
                        GameEvents.MessageType.Warning => ConsoleColor.Yellow,
                        GameEvents.MessageType.Error => ConsoleColor.Red,
                        _ => ConsoleColor.White
                    };
                    UIHelper.PrintColoredLine(e.Message, color);
                    break;
            }
        }

        public (int slotNumber, bool isNewCharacter) RequestSaveSlotSelection(List<InterfaceSaveSlotInfo> slots)
        {
            while (true)
            {
                Console.Clear();
                UIHelper.PrintColoredLine("═══════════════════════════════════════════", ConsoleColor.Cyan);
                UIHelper.PrintColoredLine("          SELECT CHARACTER", ConsoleColor.Yellow);
                UIHelper.PrintColoredLine("═══════════════════════════════════════════\n", ConsoleColor.Cyan);

                foreach (var slot in slots)
                {
                    Console.Write($"{slot.SlotNumber}. ");

                    if (slot.IsEmpty)
                    {
                        UIHelper.PrintColoredLine("[EMPTY SLOT - Create New Character]", ConsoleColor.DarkGray);
                    }
                    else
                    {
                        UIHelper.PrintColored($"{slot.Name}", ConsoleColor.Yellow);
                        Console.Write($" - Level {slot.Level} {slot.Class}");
                        Console.Write($" (Saved: {slot.SaveTime:MM/dd/yyyy HH:mm})");
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
                }

                // Check for delete command (D1, D2, D3)
                if (input.Length == 2 && input.ToUpper().StartsWith("D"))
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

                                // Refresh the slots list
                                var systemSlots = SaveSystem.GetAllSaveSlots();
                                slots = systemSlots.Select(s => new InterfaceSaveSlotInfo
                                {
                                    SlotNumber = s.SlotNumber,
                                    IsEmpty = s.IsEmpty,
                                    Name = s.Name,
                                    Level = s.Level,
                                    Class = s.Class,
                                    SaveTime = s.SaveTime
                                }).ToList();
                            }
                        }
                        else
                        {
                            UIHelper.PrintColoredLine("\n❌ That slot is already empty!", ConsoleColor.Red);
                            Thread.Sleep(1500);
                        }
                        continue; // Re-display the menu
                    }
                }

                if (int.TryParse(input, out int slotNum) && slotNum >= 1 && slotNum <= 3)
                {
                    var selectedSlot = slots.FirstOrDefault(s => s.SlotNumber == slotNum);
                    if (selectedSlot != null)
                    {
                        return (slotNum, selectedSlot.IsEmpty);
                    }
                }

                UIHelper.PrintColoredLine("\n❌ Invalid choice!", ConsoleColor.Red);
                Thread.Sleep(1000);
            }
        }

        public (string name, PlayerClass playerClass) RequestCharacterCreation()
        {
            Console.Clear();
            UIHelper.PrintColoredLine("═══════════════════════════════════════════", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("          CHARACTER CREATION", ConsoleColor.Yellow);
            UIHelper.PrintColoredLine("═══════════════════════════════════════════\n", ConsoleColor.Cyan);

            Console.Write("Enter your hero's name: ");
            string name = Console.ReadLine() ?? "";
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
            string choice = Console.ReadLine() ?? "";

            PlayerClass playerClass = choice switch
            {
                "1" => PlayerClass.Warrior,
                "2" => PlayerClass.Mage,
                "3" => PlayerClass.Rogue,
                _ => PlayerClass.Warrior
            };

            return (name, playerClass);
        }

        public MainMenuChoice RequestMainMenuChoice(string playerName, int level, PlayerClass playerClass, int currentHP, int maxHP, int gold)
        {
            Console.Clear();
            UIHelper.PrintColoredLine("═══════════════════════════════════════════", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine($"  {playerName} | Level {level} {playerClass}", ConsoleColor.Yellow);
            UIHelper.PrintColoredLine($"  💰 Gold: {gold} | ❤️  HP: {currentHP}/{maxHP}", ConsoleColor.White);
            UIHelper.PrintColoredLine("═══════════════════════════════════════════\n", ConsoleColor.Cyan);

            Console.WriteLine("1. ⚔️  Enter Combat");
            Console.WriteLine("2. 🏰 Enter Dungeon");
            Console.WriteLine("3. 🏪 Visit Shop");
            Console.WriteLine("4. 🎒 Inventory & Equipment");
            Console.WriteLine("5. 📖 Character Sheet");
            Console.WriteLine("6. 🌟 Unlock Abilities");
            Console.WriteLine("7. 📊 View Statistics");
            Console.WriteLine("8. 🏆 View Achievements");
            Console.WriteLine("9. 😴 Rest (Restore HP & Mana)");
            Console.WriteLine("A. 💾 Save Game");
            Console.WriteLine("H. ❓ Help");
            Console.WriteLine("0. 🚪 Exit Game");

            Console.Write("\nChoose an option: ");
            string choice = Console.ReadLine()?.ToUpper() ?? "";

            return choice switch
            {
                "1" => MainMenuChoice.Combat,
                "2" => MainMenuChoice.Dungeon,
                "3" => MainMenuChoice.Shop,
                "4" => MainMenuChoice.Inventory,
                "5" => MainMenuChoice.CharacterSheet,
                "6" => MainMenuChoice.UnlockAbilities,
                "7" => MainMenuChoice.Statistics,
                "8" => MainMenuChoice.Achievements,
                "9" => MainMenuChoice.Rest,
                "A" => MainMenuChoice.Save,
                "H" => MainMenuChoice.Help,
                "0" => MainMenuChoice.Exit,
                _ => MainMenuChoice.Combat // Default
            };
        }

        public CombatAction RequestCombatAction(CombatState state)
        {
            // Display combat status
            Console.WriteLine("\n" + new string('═', 60));

            // Player status
            UIHelper.PrintColoredLine($"👤 YOU", ConsoleColor.Cyan);
            Console.Write("   ");
            DrawHealthBar(state.PlayerCurrentHP, state.PlayerMaxHP, ConsoleColor.Green);
            Console.Write("   ");
            DrawManaBar(state.PlayerCurrentMana, state.PlayerMaxMana, ConsoleColor.Blue);

            // Show player buffs/debuffs
            if (state.PlayerActiveEffects.Count > 0)
            {
                Console.Write("   🛡️  ");
                UIHelper.PrintColored(string.Join(", ", state.PlayerActiveEffects), ConsoleColor.Cyan);
                Console.WriteLine();
            }

            Console.WriteLine();

            // Enemy status
            UIHelper.PrintColoredLine($"👹 {state.EnemyName}", ConsoleColor.Red);
            Console.Write("   ");
            DrawHealthBar(state.EnemyCurrentHP, state.EnemyMaxHP, ConsoleColor.Red);

            // Show enemy buffs/debuffs
            if (state.EnemyActiveEffects.Count > 0)
            {
                Console.Write("   💀 ");
                UIHelper.PrintColored(string.Join(", ", state.EnemyActiveEffects), ConsoleColor.Yellow);
                Console.WriteLine();
            }

            Console.WriteLine("\n" + new string('═', 60));

            UIHelper.PrintColoredLine("\nYOUR TURN:", ConsoleColor.Yellow);
            Console.WriteLine("1. ⚔️  Attack");
            Console.WriteLine("2. 🎯 Use Ability");
            Console.WriteLine($"3. 🧪 Use Potion ({state.PlayerPotions} remaining)");
            if (state.CanFlee)
            {
                Console.WriteLine("4. 🏃 Flee");
            }

            Console.Write("\nChoose action: ");
            string choice = Console.ReadLine() ?? "";

            switch (choice)
            {
                case "1":
                    return new CombatAction { ActionType = CombatActionType.Attack };

                case "2":
                    // Show ability list and get selection
                    int abilityIndex = RequestAbilitySelection(state.AvailableAbilities);
                    if (abilityIndex >= 0)
                    {
                        return new CombatAction
                        {
                            ActionType = CombatActionType.UseAbility,
                            AbilityIndex = abilityIndex
                        };
                    }
                    // If cancelled, ask again
                    return RequestCombatAction(state);

                case "3":
                    return new CombatAction { ActionType = CombatActionType.UsePotion };

                case "4" when state.CanFlee:
                    return new CombatAction { ActionType = CombatActionType.Flee };

                default:
                    UIHelper.PrintColoredLine("\n❌ Invalid choice!", ConsoleColor.Red);
                    System.Threading.Thread.Sleep(1000);
                    return RequestCombatAction(state);
            }
        }

        public int RequestAbilitySelection(List<AbilityInfo> abilities)
        {
            Console.WriteLine("\n╔════════════════════ ABILITIES ═══════════════════════╗");
            for (int i = 0; i < abilities.Count; i++)
            {
                var ability = abilities[i];
                string status = ability.CanUse ? "✓" : "✗";
                string cdInfo = ability.CurrentCooldown > 0 ? $" (CD: {ability.CurrentCooldown})" : "";
                string priorityIcon = ability.Priority ? "⚡" : "";

                // Ability name and cost
                ConsoleColor nameColor = ability.CanUse ? ConsoleColor.White : ConsoleColor.DarkGray;
                UIHelper.PrintColored($"{i + 1}. {status} {priorityIcon}{ability.Name}", nameColor);
                Console.WriteLine($" ({ability.ManaCost} mana){cdInfo}");

                // Description with indentation
                UIHelper.PrintColoredLine($"   {ability.Description}", ConsoleColor.Gray);

                if (i < abilities.Count - 1)
                    Console.WriteLine(); // Spacing between abilities
            }
            Console.WriteLine("\n0. Cancel");
            Console.WriteLine("╚═══════════════════════════════════════════════════════╝");

            Console.Write("\nChoose ability: ");
            string choice = Console.ReadLine() ?? "";

            if (int.TryParse(choice, out int index) && index >= 1 && index <= abilities.Count)
            {
                return index - 1;
            }

            return -1; // Cancel
        }

        public ShopAction RequestShopAction(List<ShopItemInfo> forSale, List<EquipmentItem> inventory, int playerGold)
        {
            Console.Clear();
            AsciiArt.DrawShop();
            Console.WriteLine();
            UIHelper.PrintColoredLine($"💰 Your Gold: {playerGold}\n", ConsoleColor.Yellow);

            UIHelper.PrintColoredLine("1. 🛒 Buy Items", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("2. 💵 Sell Items", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("3. 🔄 Refresh Shop (costs 50 gold)", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("4. 🧪 Buy Potions (50 gold each)", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("5. 🚪 Leave Shop", ConsoleColor.Gray);

            Console.Write("\nWhat would you like to do? ");
            string choice = Console.ReadLine() ?? "";

            switch (choice)
            {
                case "1":
                    return RequestBuyItem(forSale, playerGold);
                case "2":
                    return RequestSellItem(inventory);
                case "3":
                    return new ShopAction { ActionType = ShopActionType.RefreshShop };
                case "4":
                    return RequestBuyPotions(playerGold);
                case "5":
                    return new ShopAction { ActionType = ShopActionType.Exit };
                default:
                    UIHelper.PrintColoredLine("\n❌ Invalid choice!", ConsoleColor.Red);
                    Thread.Sleep(1000);
                    return RequestShopAction(forSale, inventory, playerGold); // Recurse
            }
        }

        private ShopAction RequestBuyItem(List<ShopItemInfo> forSale, int playerGold)
        {
            Console.Clear();
            UIHelper.PrintColoredLine("═══════════════ SHOP INVENTORY ═══════════════\n", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine($"💰 Your Gold: {playerGold}\n", ConsoleColor.Yellow);

            if (forSale.Count == 0)
            {
                Console.WriteLine("Shop is empty! Try refreshing.\n");
                Console.WriteLine("Press any key to go back...");
                Console.ReadKey(true);
                return new ShopAction { ActionType = ShopActionType.Exit };
            }

            for (int i = 0; i < forSale.Count; i++)
            {
                var item = forSale[i].Item;
                if (item != null)
                {
                    Console.Write($"  {i + 1}. ");
                    UIHelper.PrintColored($"[{item.Rarity}] {item.Name}", item.GetRarityColor());
                    Console.Write($" ({item.Slot.GetDisplayName()})");
                    Console.WriteLine($" (Lv {item.Level}) - {item.Price} gold");

                    Console.Write("     ");
                    if (item.AttackBonus > 0) Console.Write($"⚔️ +{item.AttackBonus} ");
                    if (item.DefenseBonus > 0) Console.Write($"🛡️ +{item.DefenseBonus} ");
                    if (item.MagicBonus > 0) Console.Write($"🔮 +{item.MagicBonus} ");
                    if (item.HPBonus > 0) Console.Write($"❤️ +{item.HPBonus} ");
                    if (item.SpecialEffects.Count > 0) Console.Write($"✨ x{item.SpecialEffects.Count} ");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("\n0. Back");
            Console.Write("\nSelect item to buy (or 'v' + number to view details): ");
            string input = Console.ReadLine() ?? "";

            if (input == "0")
            {
                return new ShopAction { ActionType = ShopActionType.Exit };
            }
            else if (input.StartsWith("v") && int.TryParse(input.Substring(1), out int viewIndex) &&
                     viewIndex > 0 && viewIndex <= forSale.Count && forSale[viewIndex - 1].Item != null)
            {
                DisplayEquipmentDetails(forSale[viewIndex - 1].Item!);
                return RequestBuyItem(forSale, playerGold); // Recurse
            }
            else if (int.TryParse(input, out int buyIndex) && buyIndex > 0 && buyIndex <= forSale.Count)
            {
                return new ShopAction
                {
                    ActionType = ShopActionType.BuyItem,
                    ItemIndex = buyIndex - 1
                };
            }

            return new ShopAction { ActionType = ShopActionType.Exit };
        }

        private ShopAction RequestSellItem(List<EquipmentItem> inventory)
        {
            Console.Clear();
            UIHelper.PrintColoredLine("═══════════════ SELL ITEMS ═══════════════\n", ConsoleColor.Cyan);

            if (inventory.Count == 0)
            {
                Console.WriteLine("Your backpack is empty!\n");
                Console.WriteLine("Press any key to go back...");
                Console.ReadKey(true);
                return new ShopAction { ActionType = ShopActionType.Exit };
            }

            UIHelper.PrintColoredLine("╔═══ BACKPACK ═══╗\n", ConsoleColor.Cyan);
            for (int i = 0; i < inventory.Count; i++)
            {
                var item = inventory[i];
                int sellPrice = (int)(item.Price * 0.6);

                Console.Write($"  {i + 1}. ");
                UIHelper.PrintColored($"[{item.Rarity}] {item.Name}", item.GetRarityColor());
                Console.WriteLine($" - Sell for {sellPrice} gold");
            }

            Console.WriteLine("\n0. Back");
            Console.Write("\nSelect item to sell (or 'v' + number to view details): ");
            string input = Console.ReadLine() ?? "";

            if (input == "0")
            {
                return new ShopAction { ActionType = ShopActionType.Exit };
            }
            else if (input.StartsWith("v") && int.TryParse(input.Substring(1), out int viewIndex) &&
                     viewIndex > 0 && viewIndex <= inventory.Count)
            {
                DisplayEquipmentDetails(inventory[viewIndex - 1]);
                return RequestSellItem(inventory); // Recurse
            }
            else if (int.TryParse(input, out int sellIndex) && sellIndex > 0 && sellIndex <= inventory.Count)
            {
                var item = inventory[sellIndex - 1];
                int sellPrice = (int)(item.Price * 0.6);

                Console.Write($"\nSell ");
                UIHelper.PrintColored($"[{item.Rarity}] {item.Name}", item.GetRarityColor());
                Console.Write($" for {sellPrice} gold? (y/n): ");

                string confirm = Console.ReadLine() ?? "";
                if (confirm.ToLower() == "y")
                {
                    return new ShopAction
                    {
                        ActionType = ShopActionType.SellItem,
                        ItemIndex = sellIndex - 1
                    };
                }
            }

            return new ShopAction { ActionType = ShopActionType.Exit };
        }

        private ShopAction RequestBuyPotions(int playerGold)
        {
            Console.Clear();
            UIHelper.PrintColoredLine("═══════════════ POTIONS ═══════════════\n", ConsoleColor.Cyan);

            Console.WriteLine($"🧪 Health Potion - Restores 50% HP");
            UIHelper.PrintColoredLine($"💰 Price: 50 gold each", ConsoleColor.Yellow);
            Console.WriteLine($"\nYour gold: {playerGold}");

            Console.Write("\nHow many potions? (0 to cancel): ");
            string input = Console.ReadLine() ?? "";

            if (int.TryParse(input, out int amount) && amount > 0)
            {
                return new ShopAction
                {
                    ActionType = ShopActionType.BuyPotion,
                    Quantity = amount
                };
            }

            return new ShopAction { ActionType = ShopActionType.Exit };
        }

        public InventoryAction RequestInventoryAction(List<EquipmentItem> backpack, Dictionary<EquipmentSlot, EquipmentItem?> equipped)
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    INVENTORY                           ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");

            // Display equipped items
            UIHelper.PrintColoredLine("═══ EQUIPPED ITEMS ═══\n", ConsoleColor.Cyan);
            DisplayInventorySlot("Weapon", equipped.GetValueOrDefault(EquipmentSlot.Weapon));
            DisplayInventorySlot("Armor", equipped.GetValueOrDefault(EquipmentSlot.Armor));
            DisplayInventorySlot("Helmet", equipped.GetValueOrDefault(EquipmentSlot.Helmet));
            DisplayInventorySlot("Boots", equipped.GetValueOrDefault(EquipmentSlot.Boots));
            DisplayInventorySlot("Gloves", equipped.GetValueOrDefault(EquipmentSlot.Gloves));
            DisplayInventorySlot("Ring 1", equipped.GetValueOrDefault(EquipmentSlot.Ring1));
            DisplayInventorySlot("Ring 2", equipped.GetValueOrDefault(EquipmentSlot.Ring2));
            DisplayInventorySlot("Amulet", equipped.GetValueOrDefault(EquipmentSlot.Amulet));
            DisplayInventorySlot("Relic", equipped.GetValueOrDefault(EquipmentSlot.Relic));

            // Display backpack
            UIHelper.PrintColoredLine("\n═══ BACKPACK ═══\n", ConsoleColor.Cyan);
            if (backpack.Count == 0)
            {
                Console.WriteLine("  (Empty)\n");
            }
            else
            {
                for (int i = 0; i < backpack.Count; i++)
                {
                    var item = backpack[i];
                    Console.Write($"  {i + 1}. ");
                    UIHelper.PrintColored($"[{item.Rarity}] {item.Name}", item.GetRarityColor());
                    Console.WriteLine($" (Lv {item.Level}) - {item.Slot.GetDisplayName()}");
                }
                Console.WriteLine();
            }

            // Display total stats
            DisplayInventoryStats(equipped);

            // Menu
            Console.WriteLine("\n1. Equip item from backpack");
            Console.WriteLine("2. Unequip item");
            Console.WriteLine("3. View item details");
            Console.WriteLine("4. Back to main menu");

            Console.Write("\nChoose option: ");
            string choice = Console.ReadLine() ?? "";

            switch (choice)
            {
                case "1":
                    return RequestEquipItem(backpack);
                case "2":
                    return RequestUnequipItem(equipped);
                case "3":
                    return RequestViewItemDetails(backpack);
                case "4":
                    return new InventoryAction { ActionType = InventoryActionType.Exit };
                default:
                    UIHelper.PrintColoredLine("\n❌ Invalid choice!", ConsoleColor.Red);
                    Thread.Sleep(1000);
                    return RequestInventoryAction(backpack, equipped); // Recurse
            }
        }

        private void DisplayInventorySlot(string slotName, EquipmentItem? item)
        {
            Console.Write($"  {slotName,-10}: ");
            if (item != null)
            {
                UIHelper.PrintColoredLine($"[{item.Rarity}] {item.Name}", item.GetRarityColor());
            }
            else
            {
                Console.WriteLine("(Empty)");
            }
        }

        private void DisplayInventoryStats(Dictionary<EquipmentSlot, EquipmentItem?> equipped)
        {
            UIHelper.PrintColoredLine("═══ TOTAL STATS FROM EQUIPMENT ═══\n", ConsoleColor.Cyan);

            int attack = 0, defense = 0, magic = 0, hp = 0, mana = 0, speed = 0;
            double crit = 0;

            foreach (var item in equipped.Values)
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
                }
            }

            if (attack > 0) UIHelper.PrintColoredLine($"  ⚔️  Attack: +{attack}", ConsoleColor.Red);
            if (defense > 0) UIHelper.PrintColoredLine($"  🛡️  Defense: +{defense}", ConsoleColor.Blue);
            if (magic > 0) UIHelper.PrintColoredLine($"  🔮 Magic: +{magic}", ConsoleColor.Magenta);
            if (hp > 0) UIHelper.PrintColoredLine($"  ❤️  HP: +{hp}", ConsoleColor.Green);
            if (mana > 0) UIHelper.PrintColoredLine($"  💙 Mana: +{mana}", ConsoleColor.Cyan);
            if (speed > 0) UIHelper.PrintColoredLine($"  ⚡ Speed: +{speed}", ConsoleColor.Yellow);
            if (crit > 0) UIHelper.PrintColoredLine($"  💥 Crit: +{crit:P0}", ConsoleColor.Yellow);

            // Show special effects
            var allEffects = new List<SpecialEffect>();
            foreach (var item in equipped.Values)
            {
                if (item != null)
                {
                    allEffects.AddRange(item.SpecialEffects);
                }
            }

            if (allEffects.Count > 0)
            {
                Console.WriteLine();
                UIHelper.PrintColoredLine("  ✨ ACTIVE SPECIAL EFFECTS:", ConsoleColor.Magenta);
                foreach (var effect in allEffects)
                {
                    Console.WriteLine($"    • {effect.Description}");
                }
            }
        }

        private InventoryAction RequestEquipItem(List<EquipmentItem> backpack)
        {
            if (backpack.Count == 0)
            {
                UIHelper.PrintColoredLine("\n❌ No items in backpack!", ConsoleColor.Red);
                Thread.Sleep(1500);
                return new InventoryAction { ActionType = InventoryActionType.Exit };
            }

            Console.Write("\nEnter item number to equip (0 to cancel): ");
            string input = Console.ReadLine() ?? "";

            if (int.TryParse(input, out int index) && index > 0 && index <= backpack.Count)
            {
                return new InventoryAction
                {
                    ActionType = InventoryActionType.EquipItem,
                    ItemIndex = index - 1
                };
            }

            return new InventoryAction { ActionType = InventoryActionType.Exit };
        }

        private InventoryAction RequestUnequipItem(Dictionary<EquipmentSlot, EquipmentItem?> equipped)
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

            EquipmentSlot? slot = choice switch
            {
                "1" => EquipmentSlot.Weapon,
                "2" => EquipmentSlot.Armor,
                "3" => EquipmentSlot.Helmet,
                "4" => EquipmentSlot.Boots,
                "5" => EquipmentSlot.Gloves,
                "6" => EquipmentSlot.Ring1,
                "7" => EquipmentSlot.Ring2,
                "8" => EquipmentSlot.Amulet,
                "9" => EquipmentSlot.Relic,
                _ => null
            };

            if (slot.HasValue)
            {
                return new InventoryAction
                {
                    ActionType = InventoryActionType.UnequipItem,
                    Slot = slot.Value
                };
            }

            return new InventoryAction { ActionType = InventoryActionType.Exit };
        }

        private InventoryAction RequestViewItemDetails(List<EquipmentItem> backpack)
        {
            if (backpack.Count == 0)
            {
                UIHelper.PrintColoredLine("\n❌ No items in backpack!", ConsoleColor.Red);
                Thread.Sleep(1500);
                return new InventoryAction { ActionType = InventoryActionType.Exit };
            }

            Console.Write("\nEnter item number to view (0 to cancel): ");
            string input = Console.ReadLine() ?? "";

            if (int.TryParse(input, out int index) && index > 0 && index <= backpack.Count)
            {
                DisplayEquipmentDetails(backpack[index - 1]);

                return new InventoryAction
                {
                    ActionType = InventoryActionType.ViewDetails,
                    ItemIndex = index - 1
                };
            }

            return new InventoryAction { ActionType = InventoryActionType.Exit };
        }

        public void DisplayCharacterSheet(CharacterSheetInfo info)
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine("          CHARACTER SHEET");
            Console.WriteLine("═══════════════════════════════════════════\n");

            Console.WriteLine($"Name: {info.Name}");
            Console.WriteLine($"Class: {info.Class}");
            Console.WriteLine($"Level: {info.Level}");
            Console.WriteLine($"Experience: {info.Experience}/{info.ExperienceToNextLevel}\n");

            Console.WriteLine("╔════════════ STATS ════════════╗");
            Console.WriteLine($"  ❤️  HP: {info.CurrentHP}/{info.MaxHP}");
            Console.WriteLine($"  💙 Mana: {info.CurrentMana}/{info.MaxMana}");
            Console.WriteLine($"  ⚔️  Attack: {info.Attack}");
            Console.WriteLine($"  🛡️  Defense: {info.Defense}");
            Console.WriteLine($"  🔮 Magic: {info.MagicPower}");
            Console.WriteLine($"  ⚡ Speed: {info.Speed}");
            Console.WriteLine($"  💥 Crit Chance: {info.CritChance:P0}");
            Console.WriteLine("╚═══════════════════════════════╝\n");

            Console.WriteLine("╔════════════ RESOURCES ════════════╗");
            Console.WriteLine($"  💰 Gold: {info.Gold}");
            Console.WriteLine($"  🧪 Potions: {info.Potions}");
            Console.WriteLine("╚═══════════════════════════════════╝\n");

            Console.WriteLine("╔════════════ ABILITIES ════════════╗");
            foreach (var ability in info.Abilities)
            {
                if (ability.IsUnlocked)
                {
                    Console.Write($"  ✓ {ability.Name}");
                    Console.WriteLine($" (Cost: {ability.ManaCost} mana, CD: {ability.Cooldown})");
                    Console.WriteLine($"    {ability.Description}");
                }
                else
                {
                    Console.Write($"  🔒 {ability.Name}");
                    Console.WriteLine($" - Unlock at Level {ability.UnlockLevel} for {ability.PurchaseCost} gold");
                }
            }
            Console.WriteLine("╚═══════════════════════════════════╝");

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
        }

        public int RequestDungeonSelection(List<DungeonSelectionInfo> dungeons)
        {
            Console.Clear();
            UIHelper.PrintColoredLine("═══════════════════════════════════════════", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("          DUNGEON SELECTION", ConsoleColor.Yellow);
            UIHelper.PrintColoredLine("═══════════════════════════════════════════\n", ConsoleColor.Cyan);

            Console.WriteLine("╔════════════ AVAILABLE DUNGEONS ════════════╗");

            for (int i = 0; i < dungeons.Count; i++)
            {
                var dungeon = dungeons[i];
                Console.Write($"\n{i + 1}. ");

                if (dungeon.IsCompleted)
                {
                    UIHelper.PrintColored($"✓ {dungeon.Name}", ConsoleColor.Green);
                    Console.WriteLine(" (COMPLETED)");
                }
                else if (dungeon.CanEnter)
                {
                    UIHelper.PrintColored($"🏰 {dungeon.Name}", ConsoleColor.Yellow);
                    Console.WriteLine($" - Level {dungeon.MinLevel}");
                }
                else
                {
                    UIHelper.PrintColored($"🔒 {dungeon.Name}", ConsoleColor.DarkGray);
                    Console.WriteLine($" - Level {dungeon.MinLevel}");
                }

                Console.WriteLine($"   Difficulty: {new string('★', dungeon.Difficulty)}");
                Console.WriteLine($"   Entry Cost: {dungeon.GoldCost} gold");

                if (!dungeon.CanEnter && !string.IsNullOrEmpty(dungeon.BlockingReason))
                {
                    UIHelper.PrintColoredLine($"   ⚠️  {dungeon.BlockingReason}", ConsoleColor.Red);
                }
            }

            Console.WriteLine("\n╚════════════════════════════════════════════╝");
            Console.WriteLine("\n0. Back to Main Menu");
            Console.Write("\nSelect dungeon: ");

            string input = Console.ReadLine() ?? "";

            if (int.TryParse(input, out int index) && index >= 1 && index <= dungeons.Count)
            {
                return index - 1;
            }

            return -1; // Cancel
        }

        public int RequestAbilityUnlock(List<AbilityInfo> lockedAbilities, int playerGold, int playerLevel)
        {
            Console.Clear();
            UIHelper.PrintColoredLine("═══════════════════════════════════════════", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("          UNLOCK ABILITIES", ConsoleColor.Yellow);
            UIHelper.PrintColoredLine("═══════════════════════════════════════════\n", ConsoleColor.Cyan);

            UIHelper.PrintColoredLine($"Your Level: {playerLevel} | Gold: {playerGold}", ConsoleColor.White);
            Console.WriteLine();

            Console.WriteLine("╔════════════ LOCKED ABILITIES ════════════╗");
            for (int i = 0; i < lockedAbilities.Count; i++)
            {
                var ability = lockedAbilities[i];
                Console.Write($"{i + 1}. ");

                bool canUnlock = playerLevel >= ability.UnlockLevel && playerGold >= ability.PurchaseCost;
                ConsoleColor color = canUnlock ? ConsoleColor.Yellow : ConsoleColor.DarkGray;

                UIHelper.PrintColored($"🔒 {ability.Name}", color);
                Console.WriteLine($" - Level {ability.UnlockLevel} | {ability.PurchaseCost} gold");
                Console.WriteLine($"   {ability.Description}");

                if (!canUnlock)
                {
                    if (playerLevel < ability.UnlockLevel)
                    {
                        UIHelper.PrintColoredLine($"   ⚠️  Need level {ability.UnlockLevel}", ConsoleColor.Red);
                    }
                    if (playerGold < ability.PurchaseCost)
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

            if (int.TryParse(input, out int index) && index >= 1 && index <= lockedAbilities.Count)
            {
                return index - 1;
            }

            return -1;
        }

        public bool RequestConfirmation(string message)
        {
            Console.WriteLine();
            UIHelper.PrintColoredLine(message, ConsoleColor.Yellow);
            Console.Write("Confirm (y/n): ");
            string confirm = Console.ReadLine() ?? "";
            return confirm.ToLower() == "y";
        }

        public int RequestSaveSlot(List<InterfaceSaveSlotInfo> slots)
        {
            Console.Clear();
            UIHelper.PrintColoredLine("═══════════════════════════════════════════", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("          SAVE GAME", ConsoleColor.Yellow);
            UIHelper.PrintColoredLine("═══════════════════════════════════════════\n", ConsoleColor.Cyan);

            foreach (var slot in slots)
            {
                Console.Write($"{slot.SlotNumber}. ");

                if (slot.IsEmpty)
                {
                    UIHelper.PrintColoredLine("[EMPTY SLOT]", ConsoleColor.DarkGray);
                }
                else
                {
                    UIHelper.PrintColored($"{slot.Name}", ConsoleColor.Yellow);
                    Console.WriteLine($" - Level {slot.Level} {slot.Class} (Saved: {slot.SaveTime:MM/dd/yyyy HH:mm})");
                }
            }

            Console.WriteLine("\n0. Cancel");
            Console.Write("\nSelect save slot (1-3): ");
            string input = Console.ReadLine() ?? "";

            if (int.TryParse(input, out int slotNum) && slotNum >= 1 && slotNum <= 3)
            {
                return slotNum;
            }

            return -1; // Cancel
        }

        public void WaitForAcknowledgment()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
        }

        // Helper methods for visual displays
        private void DrawHealthBar(int current, int max, ConsoleColor color)
        {
            int barLength = 20;
            int filledLength = (int)((double)current / max * barLength);
            filledLength = Math.Max(0, Math.Min(barLength, filledLength));

            Console.Write("❤️  [");
            UIHelper.PrintColored(new string('█', filledLength), color);
            Console.Write(new string('░', barLength - filledLength));
            Console.Write($"] {current}/{max}");
        }

        private void DrawManaBar(int current, int max, ConsoleColor color)
        {
            int barLength = 20;
            int filledLength = (int)((double)current / max * barLength);
            filledLength = Math.Max(0, Math.Min(barLength, filledLength));

            Console.Write("💙 [");
            UIHelper.PrintColored(new string('█', filledLength), color);
            Console.Write(new string('░', barLength - filledLength));
            Console.Write($"] {current}/{max}");
        }

        public int RequestEncounterChoice(string description, List<string> choices)
        {
            Console.Clear();
            UIHelper.PrintColoredLine("═══════════════════════════════════════════", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("          DUNGEON ENCOUNTER", ConsoleColor.Yellow);
            UIHelper.PrintColoredLine("═══════════════════════════════════════════\n", ConsoleColor.Cyan);

            UIHelper.PrintColoredLine(description + "\n", ConsoleColor.White);

            for (int i = 0; i < choices.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {choices[i]}");
            }

            Console.Write("\nYour choice: ");
            string input = Console.ReadLine() ?? "";

            if (int.TryParse(input, out int choice) && choice > 0 && choice <= choices.Count)
            {
                return choice - 1; // Return 0-based index
            }

            // Invalid choice, default to first option
            return 0;
        }

        /// <summary>
        /// Displays detailed information about an equipment item
        /// </summary>
        private void DisplayEquipmentDetails(EquipmentItem item)
        {
            Console.Clear();
            Console.WriteLine();

            // Display name with rarity color
            UIHelper.PrintColored($"[{item.Rarity}] {item.Name}", item.GetRarityColor());
            Console.WriteLine($" (Lv {item.Level})");
            Console.WriteLine($"Slot: {item.Slot.GetDisplayName()}");

            // Display attack type for weapons
            if (item.WeaponAttackType.HasValue)
            {
                Console.Write("  Attack Type: ");
                UIHelper.PrintColored(
                    $"{Combat.AttackTypeSystem.GetAttackTypeIcon(item.WeaponAttackType.Value)} {item.WeaponAttackType.Value}",
                    Combat.AttackTypeSystem.GetAttackTypeColor(item.WeaponAttackType.Value));
                Console.WriteLine();
            }

            Console.WriteLine();

            // Display stats - show damage range for weapons (if it's a weapon)
            if (item.Slot == EquipmentSlot.Weapon && item.MaxDamage > 0)
            {
                Console.WriteLine($"  ⚔️  Damage: {item.MinDamage}-{item.MaxDamage}");
                if (item.Accuracy < 1.0)
                {
                    Console.WriteLine($"  🎯 Accuracy: {item.Accuracy:P0}");
                }
            }
            else if (item.AttackBonus > 0)
            {
                Console.WriteLine($"  ⚔️  Attack: +{item.AttackBonus}");
            }

            if (item.DefenseBonus > 0) Console.WriteLine($"  🛡️  Defense: +{item.DefenseBonus}");
            if (item.MagicBonus > 0) Console.WriteLine($"  🔮 Magic: +{item.MagicBonus}");
            if (item.HPBonus > 0) Console.WriteLine($"  ❤️  HP: +{item.HPBonus}");
            if (item.ManaBonus > 0) Console.WriteLine($"  💙 Mana: +{item.ManaBonus}");
            if (item.AgilityBonus > 0) Console.WriteLine($"  ⚡ Agility: +{item.AgilityBonus}");
            if (item.CritBonus > 0) Console.WriteLine($"  💥 Crit Chance: +{item.CritBonus:P0}");

            // Display special effects
            if (item.SpecialEffects.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("  ✨ SPECIAL EFFECTS:");
                foreach (var effect in item.SpecialEffects)
                {
                    Console.WriteLine($"    • {effect.Description}");
                }
            }

            Console.WriteLine();
            Console.WriteLine($"  💰 Value: {item.Price} gold");

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
        }

        public void DisplayStatistics(Systems.StatisticsInfo info)
        {
            Console.Clear();
            UIHelper.PrintColoredLine("═══════════════════════════════════════════", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("          PLAYER STATISTICS", ConsoleColor.Yellow);
            UIHelper.PrintColoredLine("═══════════════════════════════════════════\n", ConsoleColor.Cyan);

            // Combat Stats
            UIHelper.PrintColoredLine("⚔️  COMBAT STATS", ConsoleColor.Green);
            Console.WriteLine($"  Total Kills: {info.TotalKills}");
            Console.WriteLine($"  Bosses Defeated: {info.BossesDefeated}");
            Console.WriteLine($"  Total Deaths: {info.TotalDeaths}");
            Console.WriteLine($"  K/D Ratio: {info.KillDeathRatio:F2}");
            Console.WriteLine($"  Combats Won: {info.CombatsWon}");
            Console.WriteLine($"  Combats Fled: {info.CombatsFled}");
            Console.WriteLine();

            // Damage Stats
            UIHelper.PrintColoredLine("💥 DAMAGE STATS", ConsoleColor.Red);
            Console.WriteLine($"  Total Damage Dealt: {info.TotalDamageDealt:N0}");
            Console.WriteLine($"  Total Damage Taken: {info.TotalDamageTaken:N0}");
            Console.WriteLine($"  Total Healing: {info.TotalHealingDone:N0}");
            Console.WriteLine($"  Critical Hits: {info.CriticalHitsDealt}");
            Console.WriteLine($"  Attacks Missed: {info.AttacksMissed}");
            Console.WriteLine($"  Attacks Dodged: {info.AttacksDodged}");
            Console.WriteLine($"  Avg Damage/Combat: {info.AverageDamagePerCombat:F1}");
            Console.WriteLine($"  Highest Single Hit: {info.HighestDamageInOneTurn}");
            Console.WriteLine();

            // Gold & Economy
            UIHelper.PrintColoredLine("💰 ECONOMY", ConsoleColor.Yellow);
            Console.WriteLine($"  Total Gold Earned: {info.TotalGoldEarned:N0}");
            Console.WriteLine($"  Total Gold Spent: {info.TotalGoldSpent:N0}");
            Console.WriteLine($"  Net Gold: {info.NetGold:N0}");
            Console.WriteLine($"  Most Gold at Once: {info.MostGoldAtOnce:N0}");
            Console.WriteLine($"  Items Bought: {info.ItemsBought}");
            Console.WriteLine($"  Items Sold: {info.ItemsSold}");
            Console.WriteLine($"  Times Rested: {info.TimesRested}");
            Console.WriteLine();

            // Dungeon Stats
            UIHelper.PrintColoredLine("🏰 DUNGEON STATS", ConsoleColor.Magenta);
            Console.WriteLine($"  Dungeons Completed: {info.TotalDungeonsCompleted}");
            Console.WriteLine($"  Dungeon Attempts: {info.DungeonAttempts}");
            Console.WriteLine($"  Success Rate: {info.DungeonSuccessRate:F1}%");
            Console.WriteLine();

            // Progression
            UIHelper.PrintColoredLine("📈 PROGRESSION", ConsoleColor.Cyan);
            Console.WriteLine($"  Highest Level: {info.HighestLevelReached}");
            Console.WriteLine($"  Total Levels Gained: {info.TotalLevelsGained}");
            Console.WriteLine($"  Total XP Gained: {info.TotalExperienceGained:N0}");
            Console.WriteLine($"  Abilities Unlocked: {info.AbilitiesUnlocked}");
            Console.WriteLine($"  Legendary Items Found: {info.LegendaryItemsFound}");
            Console.WriteLine($"  Epic Items Found: {info.EpicItemsFound}");
            Console.WriteLine();

            // Misc Stats
            UIHelper.PrintColoredLine("📊 MISCELLANEOUS", ConsoleColor.White);
            Console.WriteLine($"  Potions Used: {info.PotionsUsed}");
            Console.WriteLine($"  Total Combat Turns: {info.TotalTurnsInCombat}");
            Console.WriteLine($"  Longest Combat: {info.LongestCombat} turns");
            Console.WriteLine($"  Game Saves: {info.GameSaves}");

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
        }

        public void DisplayAchievements(AchievementDisplayInfo info)
        {
            Console.Clear();
            UIHelper.PrintColoredLine("═══════════════════════════════════════════", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("            ACHIEVEMENTS", ConsoleColor.Yellow);
            UIHelper.PrintColoredLine("═══════════════════════════════════════════\n", ConsoleColor.Cyan);

            // Overall progress
            Console.WriteLine($"Progress: {info.UnlockedAchievements}/{info.TotalAchievements} ({info.CompletionPercentage:F1}%)");
            Console.WriteLine($"Points: {info.EarnedPoints}/{info.TotalPoints}\n");

            // Display by category
            foreach (var category in info.Categories)
            {
                if (!info.AchievementsByCategory.ContainsKey(category))
                    continue;

                var achievements = info.AchievementsByCategory[category];
                var unlockedCount = achievements.Count(a => a.IsUnlocked);

                UIHelper.PrintColoredLine($"\n🏆 {category.ToUpper()} ({unlockedCount}/{achievements.Count})", ConsoleColor.Green);

                foreach (var achievement in achievements)
                {
                    // Skip hidden achievements if not unlocked
                    if (achievement.IsHidden && !achievement.IsUnlocked)
                        continue;

                    var statusIcon = achievement.IsUnlocked ? "✓" : " ";
                    var nameColor = achievement.IsUnlocked ? ConsoleColor.Yellow : ConsoleColor.Gray;

                    Console.ForegroundColor = nameColor;
                    Console.WriteLine($"  [{statusIcon}] {achievement.Name} ({achievement.Points} pts)");
                    Console.ResetColor();

                    Console.WriteLine($"      {achievement.Description}");

                    if (!achievement.IsUnlocked)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"      Progress: {achievement.ProgressText}");
                        Console.ResetColor();
                    }
                    else if (achievement.GoldReward > 0 || achievement.ExperienceReward > 0 || !string.IsNullOrEmpty(achievement.TitleReward))
                    {
                        var rewards = new List<string>();
                        if (achievement.GoldReward > 0) rewards.Add($"{achievement.GoldReward}g");
                        if (achievement.ExperienceReward > 0) rewards.Add($"{achievement.ExperienceReward} XP");
                        if (!string.IsNullOrEmpty(achievement.TitleReward)) rewards.Add($"Title: {achievement.TitleReward}");

                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine($"      Rewards: {string.Join(", ", rewards)}");
                        Console.ResetColor();
                    }
                }
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
        }
    }
}
