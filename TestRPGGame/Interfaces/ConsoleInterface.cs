using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TestRPGGame.Entities.Player;
using TestRPGGame.Equipment;
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

                    // Show character art based on class
                    switch (e.Class)
                    {
                        case PlayerClass.Warrior:
                            AsciiArt.DrawWarrior();
                            break;
                        case PlayerClass.Mage:
                            AsciiArt.DrawMage();
                            break;
                        case PlayerClass.Rogue:
                            AsciiArt.DrawRogue();
                            break;
                    }

                    Thread.Sleep(1000);
                    UIHelper.PrintColoredLine("\nYour journey begins now...\n", ConsoleColor.Gray);
                    Thread.Sleep(1500);
                    break;

                case GameEvents.CombatStartedEvent e:
                    Console.Clear();
                    UIHelper.PrintColoredLine("═══════════════════════════════════════════", ConsoleColor.Red);
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
                    var color = e.Type switch
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
                Console.Write("\nSelect slot (1-3) or 0 to exit: ");
                string input = Console.ReadLine() ?? "";

                if (input == "0")
                {
                    Environment.Exit(0);
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
            Console.WriteLine("7. 😴 Rest (Restore HP & Mana)");
            Console.WriteLine("8. 💾 Save Game");
            Console.WriteLine("9. ❓ Help");
            Console.WriteLine("0. 🚪 Exit Game");

            Console.Write("\nChoose an option: ");
            string choice = Console.ReadLine() ?? "";

            return choice switch
            {
                "1" => MainMenuChoice.Combat,
                "2" => MainMenuChoice.Dungeon,
                "3" => MainMenuChoice.Shop,
                "4" => MainMenuChoice.Inventory,
                "5" => MainMenuChoice.CharacterSheet,
                "6" => MainMenuChoice.UnlockAbilities,
                "7" => MainMenuChoice.Rest,
                "8" => MainMenuChoice.Save,
                "9" => MainMenuChoice.Help,
                "0" => MainMenuChoice.Exit,
                _ => MainMenuChoice.Combat // Default
            };
        }

        public CombatAction RequestCombatAction(CombatState state)
        {
            UIHelper.PrintColoredLine("\nYOUR TURN:", ConsoleColor.Yellow);
            Console.WriteLine("1. ⚔️  Attack");
            Console.WriteLine("2. 🎯 Use Ability");
            Console.WriteLine("3. 🧪 Use Potion");
            if (state.CanFlee)
            {
                Console.WriteLine("4. 🏃 Flee");
            }

            Console.Write("\nChoose action: ");
            string choice = Console.ReadLine() ?? "";

            return choice switch
            {
                "1" => new CombatAction { ActionType = CombatActionType.Attack },
                "2" => new CombatAction { ActionType = CombatActionType.UseAbility },
                "3" => new CombatAction { ActionType = CombatActionType.UsePotion },
                "4" when state.CanFlee => new CombatAction { ActionType = CombatActionType.Flee },
                _ => new CombatAction { ActionType = CombatActionType.Attack }
            };
        }

        public int RequestAbilitySelection(List<AbilityInfo> abilities)
        {
            Console.WriteLine("\n╔════ ABILITIES ════╗");
            for (int i = 0; i < abilities.Count; i++)
            {
                var ability = abilities[i];
                string status = ability.CanUse ? "✓" : "✗";
                string cdInfo = ability.CurrentCooldown > 0 ? $" (CD: {ability.CurrentCooldown})" : "";
                string priorityIcon = ability.Priority ? "⚡" : "";
                Console.WriteLine($"{i + 1}. {status} {priorityIcon}{ability.Name} ({ability.ManaCost} mana){cdInfo}");
            }
            Console.WriteLine("0. Cancel");
            Console.WriteLine("╚═══════════════════╝");

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
            // For now, just exit shop - full shop UI would be implemented here
            return new ShopAction { ActionType = ShopActionType.Exit };
        }

        public InventoryAction RequestInventoryAction(List<EquipmentItem> backpack, Dictionary<EquipmentSlot, EquipmentItem?> equipped)
        {
            // For now, just exit inventory - full inventory UI would be implemented here
            return new InventoryAction { ActionType = InventoryActionType.Exit };
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
    }
}
