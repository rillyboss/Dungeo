using System;
using System.IO;
using System.Linq;
using TestRPGGame.Entities.Player;
using TestRPGGame.Interfaces;
using TestRPGGame.DataLoading;

namespace TestRPGGame
{
    /// <summary>
    /// Automated boss fight tester for each class
    /// WARNING: This will temporarily use save slots for testing
    /// Backs up existing saves and restores them after testing
    /// </summary>
    public class BossFightRunner
    {
        private static readonly string SaveDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TestRPGGame",
            "Saves"
        );

        private static readonly string BackupDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TestRPGGame",
            "SavesBackup"
        );

        // Commented out to avoid conflicts with main entry point
        // Uncomment and run with: dotnet run --project TestRPGGame --property:StartupObject=TestRPGGame.BossFightRunner
        /*public static void Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║        RPG BOSS FIGHT TESTER - Damage Formula Test        ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

            // Backup existing saves
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Backing up existing save files...");
            Console.ResetColor();
            BackupSaves();

            try
            {
                // Load game data
                DataLoader.LoadAllData();

                // Test each class against the first boss
                TestClass(PlayerClass.Warrior, "BossTest_Warrior");
                Console.WriteLine("\n" + new string('=', 60) + "\n");

                TestClass(PlayerClass.Mage, "BossTest_Mage");
                Console.WriteLine("\n" + new string('=', 60) + "\n");

                TestClass(PlayerClass.Rogue, "BossTest_Rogue");

                Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                    TESTING COMPLETE!                       ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
            }
            finally
            {
                // Restore saves
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Restoring original save files...");
                Console.ResetColor();
                RestoreSaves();
            }
        }*/

        private static void BackupSaves()
        {
            if (!Directory.Exists(SaveDirectory))
                return;

            if (Directory.Exists(BackupDirectory))
                Directory.Delete(BackupDirectory, true);

            Directory.CreateDirectory(BackupDirectory);

            foreach (var file in Directory.GetFiles(SaveDirectory))
            {
                var fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(BackupDirectory, fileName));
            }

            // Clear save directory for testing
            foreach (var file in Directory.GetFiles(SaveDirectory))
            {
                File.Delete(file);
            }

            Console.WriteLine("  ✓ Saves backed up successfully\n");
        }

        private static void RestoreSaves()
        {
            if (!Directory.Exists(BackupDirectory))
                return;

            // Delete test saves
            if (Directory.Exists(SaveDirectory))
            {
                foreach (var file in Directory.GetFiles(SaveDirectory))
                {
                    File.Delete(file);
                }
            }

            // Restore original saves
            foreach (var file in Directory.GetFiles(BackupDirectory))
            {
                var fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(SaveDirectory, fileName));
            }

            // Clean up backup directory
            Directory.Delete(BackupDirectory, true);

            Console.WriteLine("  ✓ Original saves restored\n");
        }

        private static void TestClass(PlayerClass playerClass, string characterName)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Testing {playerClass} vs Goblin King Boss");
            Console.ResetColor();
            Console.WriteLine(new string('-', 60));

            // Create boss-focused strategy for this class
            var strategy = new BossFightStrategy(playerClass, characterName);
            var autoInterface = new AutomatedInterface(strategy);

            // Create and run game
            var game = new GameCore(autoInterface);

            try
            {
                game.Start();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error during automated run: {ex.Message}");
                Console.ResetColor();
            }

            // Print summary from log
            Console.WriteLine("\n--- RUN SUMMARY ---");
            var log = autoInterface.GetLog();
            PrintBossFightSummary(log);
        }

        private static void PrintBossFightSummary(string log)
        {
            var lines = log.Split('\n');
            bool inBossFight = false;
            int totalDamageDealt = 0;
            int totalDamageTaken = 0;
            int turnsToKill = 0;

            foreach (var line in lines)
            {
                // Detect boss fight start
                if (line.Contains("COMBAT") && line.Contains("King"))
                {
                    inBossFight = true;
                    Console.WriteLine(line);
                }

                // Track damage during boss fight
                if (inBossFight)
                {
                    // Player damage to boss
                    if (line.Contains("→") && line.Contains("King") && line.Contains("damage"))
                    {
                        // Try to extract damage number
                        var parts = line.Split(new[] { '→', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                        {
                            var damagePart = parts[1].Trim();
                            var damageStr = damagePart.Split(' ')[0];
                            if (int.TryParse(damageStr, out int damage))
                            {
                                totalDamageDealt += damage;
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine(line);
                                Console.ResetColor();
                            }
                        }
                    }

                    // Boss damage to player
                    if (line.Contains("King") && line.Contains("→") && line.Contains("damage") && !line.Contains("King:"))
                    {
                        var parts = line.Split(new[] { '→', ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2)
                        {
                            var damagePart = parts[1].Trim();
                            var damageStr = damagePart.Split(' ')[0];
                            if (int.TryParse(damageStr, out int damage))
                            {
                                totalDamageTaken += damage;
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(line);
                                Console.ResetColor();
                            }
                        }
                    }

                    // Track turns
                    if (line.Contains("--- Turn"))
                    {
                        turnsToKill++;
                    }

                    // Detect boss fight end
                    if (line.Contains("VICTORY") || line.Contains("DEFEAT"))
                    {
                        Console.ForegroundColor = line.Contains("VICTORY") ? ConsoleColor.Green : ConsoleColor.Red;
                        Console.WriteLine(line);
                        Console.ResetColor();
                        inBossFight = false;
                    }
                }
            }

            // Print statistics
            if (totalDamageDealt > 0)
            {
                Console.WriteLine($"\nBoss Fight Statistics:");
                Console.WriteLine($"  Total Damage Dealt: {totalDamageDealt}");
                Console.WriteLine($"  Total Damage Taken: {totalDamageTaken}");
                Console.WriteLine($"  Turns to Kill: {turnsToKill}");
                Console.WriteLine($"  Average Damage/Turn: {(turnsToKill > 0 ? totalDamageDealt / turnsToKill : 0)}");
            }
        }
    }

    /// <summary>
    /// Strategy specifically designed to rush to the first boss dungeon
    /// </summary>
    public class BossFightStrategy : AutomatedStrategy
    {
        private readonly PlayerClass playerClass;
        private readonly string characterName;
        private bool hasEnteredDungeon = false;
        private bool hasCompletedDungeon = false;

        public BossFightStrategy(PlayerClass playerClass, string characterName)
        {
            this.playerClass = playerClass;
            this.characterName = characterName;
        }

        public override (string name, PlayerClass playerClass) ChooseCharacterClass()
        {
            return (characterName, playerClass);
        }

        public override MainMenuChoice ChooseMainMenuAction(int combatCount, int level, int gold, int hp, int maxHp)
        {
            // Exit after completing the dungeon with boss fight
            if (hasCompletedDungeon)
                return MainMenuChoice.Exit;

            // Heal if critically low HP
            if (hp < maxHp * 0.4)
                return MainMenuChoice.Rest;

            // Enter dungeon once
            if (!hasEnteredDungeon)
            {
                hasEnteredDungeon = true;
                return MainMenuChoice.Dungeon;
            }

            // Should never reach here in a dungeon run, but just in case
            return MainMenuChoice.Exit;
        }

        // Called when dungeon is complete
        public void MarkDungeonComplete()
        {
            hasCompletedDungeon = true;
        }

        public override CombatAction ChooseCombatAction(CombatState state)
        {
            // Use potion if critically low
            if (state.PlayerCurrentHP < state.PlayerMaxHP * 0.25 && state.PlayerPotions > 0)
            {
                return new CombatAction { ActionType = CombatActionType.UsePotion };
            }

            // Use highest damage abilities when available
            var damageAbilities = state.AvailableAbilities
                .Where(a => a.CanUse &&
                           a.CurrentCooldown == 0 &&
                           (a.Name.Contains("Strike") ||
                            a.Name.Contains("Blast") ||
                            a.Name.Contains("Backstab") ||
                            a.Name.Contains("Smash")))
                .OrderByDescending(a => a.ManaCost)
                .ToList();

            if (damageAbilities.Any() && state.PlayerCurrentMana >= damageAbilities.First().ManaCost)
            {
                return new CombatAction
                {
                    ActionType = CombatActionType.UseAbility,
                    AbilityIndex = damageAbilities.First().Index
                };
            }

            // Basic attack
            return new CombatAction { ActionType = CombatActionType.Attack };
        }
    }
}
