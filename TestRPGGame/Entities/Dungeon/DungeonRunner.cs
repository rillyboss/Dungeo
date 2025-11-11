using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TestRPGGame.Combat;
using TestRPGGame.Equipment;
using TestRPGGame.Systems;
using TestRPGGame.DataLoading;
using PlayerEntity = TestRPGGame.Entities.Player.Player;
using EnemyEntity = TestRPGGame.Entities.Enemy.Enemy;

namespace TestRPGGame.Entities.Dungeon
{
    public class DungeonProgress
    {
        public Dictionary<string, bool> CompletedDungeons { get; set; }

        public DungeonProgress()
        {
            CompletedDungeons = new Dictionary<string, bool>();
        }
    }

    public class DungeonRunner
    {
        private CombatSystem combatSystem;
        private Random random;
        private List<EquipmentItem> dungeonLoot;

        public DungeonRunner()
        {
            combatSystem = new CombatSystem();
            random = new Random();
            dungeonLoot = new List<EquipmentItem>();
        }

        public bool RunDungeon(PlayerEntity player, Dungeon dungeon, DungeonProgress progress)
        {
            dungeonLoot.Clear();

            // Display dungeon intro
            ShowDungeonIntro(dungeon);

            // Pay entry cost
            if (dungeon.Requirements.GoldCost > 0)
            {
                player.Gold -= dungeon.Requirements.GoldCost;
                Console.WriteLine($"💰 You pay {dungeon.Requirements.GoldCost} gold to enter the dungeon.");
                Thread.Sleep(1500);
            }

            // Generate and run random encounters (or use fixed encounters for legacy dungeons)
            var encounters = GenerateEncounters(dungeon, player.Level);

            foreach (var encounter in encounters)
            {
                if (!RunEncounter(player, encounter))
                {
                    // Player chose to flee or died
                    return false;
                }

                if (player.CurrentHP <= 0)
                {
                    HandleDeath(player);
                    return false;
                }
            }

            // Miniboss fight
            Console.WriteLine("\n\n⚠️  You've reached the inner sanctum...");
            Thread.Sleep(1500);

            bool minibossVictory = combatSystem.StartBossBattle(player, dungeon.Miniboss, true);

            if (!minibossVictory)
            {
                HandleDeath(player);
                return false;
            }

            // Miniboss rewards
            ShowMinibossVictory(player, dungeon);

            // Offer choice: leave or continue
            if (!OfferContinueChoice())
            {
                // Player chooses to leave with miniboss rewards
                GiveRewards(player, dungeon.MinibossReward, false);
                return true;
            }

            // Boss fight
            Console.WriteLine("\n\n💀 You venture deeper into the heart of darkness...");
            Thread.Sleep(2000);

            bool bossVictory = combatSystem.StartBossBattle(player, dungeon.Boss, false);

            if (!bossVictory)
            {
                HandleDeath(player);
                // Player loses ALL loot on final boss death
                dungeonLoot.Clear();
                return false;
            }

            // Full dungeon completion
            ShowBossVictory(player, dungeon);
            GiveRewards(player, dungeon.BossReward, true);

            // Mark dungeon as completed
            progress.CompletedDungeons[dungeon.Name] = true;

            return true;
        }

        /// <summary>
        /// Generate random encounters for this dungeon run, providing variety and replayability.
        /// Uses EncounterPool and EncounterConfig if available, otherwise uses fixed Encounters list.
        /// </summary>
        private List<DungeonEncounter> GenerateEncounters(Dungeon dungeon, int playerLevel)
        {
            var generatedEncounters = new List<DungeonEncounter>();

            // Legacy mode: Use fixed encounters if no encounter pool defined
            if (dungeon.EncounterPool == null || dungeon.EncounterPool.Count == 0)
            {
                return dungeon.Encounters;
            }

            // New mode: Generate random encounters from pool
            var config = dungeon.EncounterConfig ?? new DungeonEncounterConfig();

            // Determine number of encounters
            int numEncounters = random.Next(config.MinEncounters, config.MaxEncounters + 1);
            int numCombatEncounters = random.Next(config.MinCombatEncounters, config.MaxCombatEncounters + 1);

            // Create a shuffled copy of the encounter pool
            var availableEncounters = dungeon.EncounterPool.ToList();
            ShuffleList(availableEncounters);

            // Add choice/event encounters
            for (int i = 0; i < numEncounters && availableEncounters.Count > 0; i++)
            {
                var encounterData = availableEncounters[0];
                availableEncounters.RemoveAt(0);
                generatedEncounters.Add(encounterData);

                // Random chance for bonus combat encounter after this
                if (random.NextDouble() < config.RandomCombatChance)
                {
                    generatedEncounters.Add(CreateRandomCombatEncounter(playerLevel));
                }
            }

            // Add guaranteed combat encounters
            for (int i = 0; i < numCombatEncounters; i++)
            {
                generatedEncounters.Add(CreateRandomCombatEncounter(playerLevel));
            }

            // Shuffle the final encounter order for variety
            ShuffleList(generatedEncounters);

            return generatedEncounters;
        }

        /// <summary>
        /// Create a random combat encounter with an enemy appropriate for the player's level.
        /// </summary>
        private DungeonEncounter CreateRandomCombatEncounter(int playerLevel)
        {
            var encounter = new DungeonEncounter("You hear movement ahead...");
            encounter.IsCombat = true;
            encounter.CombatLevel = playerLevel; // Store level for enemy generation
            return encounter;
        }

        /// <summary>
        /// Fisher-Yates shuffle algorithm for randomizing lists.
        /// </summary>
        private void ShuffleList<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Get randomized choice indices for an encounter.
        /// If RandomChoiceCount > 0, randomly selects that many choices.
        /// Otherwise, returns all choices in original order.
        /// </summary>
        private List<int> GetRandomizedChoiceIndices(DungeonEncounter encounter)
        {
            int totalChoices = encounter.Choices.Count;

            // If no random selection needed, return all indices in order
            if (encounter.RandomChoiceCount <= 0 || encounter.RandomChoiceCount >= totalChoices)
            {
                var allIndices = new List<int>();
                for (int i = 0; i < totalChoices; i++)
                {
                    allIndices.Add(i);
                }
                return allIndices;
            }

            // Randomly select N choices
            var availableIndices = new List<int>();
            for (int i = 0; i < totalChoices; i++)
            {
                availableIndices.Add(i);
            }

            ShuffleList(availableIndices);

            // Take only the requested number of choices
            return availableIndices.Take(encounter.RandomChoiceCount).ToList();
        }

        private void ShowDungeonIntro(Dungeon dungeon)
        {
            Console.Clear();
            Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
            Console.WriteLine($"  {dungeon.Name.ToUpper()}");
            Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");

            // Word wrap the story
            var storyLines = dungeon.Story.Split('\n');
            foreach (var line in storyLines)
            {
                Console.WriteLine(line);
            }

            Console.WriteLine("\n");
            Console.WriteLine("Press any key to enter the dungeon...");
            Console.ReadKey(true);
        }

        private bool RunEncounter(PlayerEntity player, DungeonEncounter encounter)
        {
            // Handle random combat encounters (no choices, just fight)
            if (encounter.IsCombat && encounter.CombatLevel.HasValue)
            {
                Console.Clear();
                Console.WriteLine("═══════════════════════════════════════════");
                Console.WriteLine("          ENEMY ENCOUNTER!");
                Console.WriteLine("═══════════════════════════════════════════\n");

                Console.WriteLine(encounter.Description);
                Thread.Sleep(1200);

                // Generate random enemy at appropriate level
                EnemyEntity enemy = Entities.Enemy.EnemyFactory.CreateEnemy(encounter.CombatLevel.Value);
                CombatSystem normalCombat = new CombatSystem();
                bool victory = normalCombat.StartBattle(player, enemy);

                if (!victory)
                {
                    return false;
                }

                Console.WriteLine("\n✓ Enemy defeated!");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey(true);
                return true;
            }

            // Handle choice-based encounters
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine("          DUNGEON ENCOUNTER");
            Console.WriteLine("═══════════════════════════════════════════\n");

            Console.WriteLine(encounter.Description + "\n");

            // Randomize choices if configured
            List<int> selectedChoiceIndices = GetRandomizedChoiceIndices(encounter);

            for (int i = 0; i < selectedChoiceIndices.Count; i++)
            {
                int actualIndex = selectedChoiceIndices[i];
                Console.WriteLine($"{i + 1}. {encounter.Choices[actualIndex]}");
            }

            Console.Write("\nYour choice: ");
            string choice = Console.ReadLine() ?? "";

            if (int.TryParse(choice, out int choiceIndex) && choiceIndex > 0 && choiceIndex <= selectedChoiceIndices.Count)
            {
                // Map the user's choice to the actual choice index
                int index = selectedChoiceIndices[choiceIndex - 1];
                Console.WriteLine();
                Console.WriteLine(encounter.ChoiceResults[index]);
                Thread.Sleep(1500);

                // Apply encounter effects
                if (index == 0 && encounter.GoldReward.HasValue)
                {
                    player.Gold += encounter.GoldReward.Value;
                    Console.WriteLine($"\n💰 +{encounter.GoldReward.Value} gold!");
                }
                if (index == 0 && encounter.HealthReward.HasValue)
                {
                    player.Heal(encounter.HealthReward.Value);
                    Console.WriteLine($"\n❤️  +{encounter.HealthReward.Value} HP!");
                }
                if (index == 0 && encounter.ManaReward.HasValue)
                {
                    player.RestoreMana(encounter.ManaReward.Value);
                    Console.WriteLine($"\n💙 +{encounter.ManaReward.Value} mana!");
                }

                // Negative effects for certain choices
                if (index == 1 && encounter.ChoiceResults[index].Contains("10 damage"))
                {
                    player.CurrentHP -= 10;
                    Console.WriteLine($"\n💔 You take 10 damage! HP: {player.CurrentHP}/{player.MaxHP}");
                }
                if (index == 1 && encounter.ChoiceResults[index].Contains("15 HP"))
                {
                    player.CurrentHP -= 15;
                    Console.WriteLine($"\n💔 You take 15 damage! HP: {player.CurrentHP}/{player.MaxHP}");
                }

                // Combat encounter
                if (index == 0 && encounter.ChoiceResults[index].Contains("Combat"))
                {
                    Thread.Sleep(1000);
                    EnemyEntity enemy = Entities.Enemy.EnemyFactory.CreateEnemy(player.Level + 1); // Slightly harder
                    CombatSystem normalCombat = new CombatSystem();
                    bool victory = normalCombat.StartBattle(player, enemy);

                    if (!victory)
                    {
                        return false;
                    }

                    // No gold/exp from dungeon random encounters - that's for bosses
                    Console.WriteLine("\n✓ Enemy defeated!");
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey(true);
                return true;
            }

            return true;
        }

        private void ShowMinibossVictory(PlayerEntity player, Dungeon dungeon)
        {
            Console.Clear();
            Console.WriteLine("\n╔══════════════════════════════════════════╗");
            Console.WriteLine("║      MINIBOSS DEFEATED!               ║");
            Console.WriteLine("╚══════════════════════════════════════════╝\n");

            Console.WriteLine($"You have defeated the {dungeon.Miniboss.Name}!");
            Console.WriteLine($"\nMiniboss rewards available:");
            Console.WriteLine($"  💰 {dungeon.MinibossReward.GoldMin}-{dungeon.MinibossReward.GoldMax} gold");
            Console.WriteLine($"  ⭐ {dungeon.MinibossReward.Experience} experience");
            Console.WriteLine($"  ✨ {dungeon.MinibossReward.GuaranteedLootCount} guaranteed items (min rarity: {GetRarityName(dungeon.MinibossReward.MinLootRarity)})");

            Thread.Sleep(2000);
        }

        private void ShowBossVictory(PlayerEntity player, Dungeon dungeon)
        {
            Console.Clear();
            Console.WriteLine("\n╔══════════════════════════════════════════╗");
            Console.WriteLine("║      DUNGEON CONQUERED!               ║");
            Console.WriteLine("╚══════════════════════════════════════════╝\n");

            Console.WriteLine($"You have defeated {dungeon.Boss.Name} and conquered the {dungeon.Name}!");
            Thread.Sleep(1500);
        }

        private bool OfferContinueChoice()
        {
            Console.WriteLine("\n");
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine("      CHOICE: LEAVE OR CONTINUE?");
            Console.WriteLine("═══════════════════════════════════════════\n");

            Console.WriteLine("You can leave now with the miniboss rewards...");
            Console.WriteLine("Or press on to face the final boss for greater treasures!");
            Console.WriteLine("\n⚠️  WARNING: If you die to the final boss, you lose ALL loot!");

            Console.WriteLine("\n1. Leave the dungeon (safe, keep miniboss rewards)");
            Console.WriteLine("2. Continue to the final boss (risky, greater rewards)");

            Console.Write("\nYour choice: ");
            string choice = Console.ReadLine() ?? "";

            return choice == "2";
        }

        private void GiveRewards(PlayerEntity player, DungeonReward reward, bool isBossReward)
        {
            Console.WriteLine();
            Console.WriteLine("╔══════════════ REWARDS ══════════════╗");

            // Gold
            int goldReward = random.Next(reward.GoldMin, reward.GoldMax + 1);
            player.Gold += goldReward;
            Console.WriteLine($"  💰 Gold: +{goldReward} (Total: {player.Gold})");

            // Experience
            bool leveledUp = player.GainExperience(reward.Experience);
            Console.WriteLine($"  ⭐ Experience: +{reward.Experience}");

            if (leveledUp)
            {
                Console.WriteLine($"\n  🎉 LEVEL UP! You are now level {player.Level}!");
            }

            // Loot
            Console.WriteLine();
            Console.WriteLine($"  ✨ Legendary Loot:");

            for (int i = 0; i < reward.GuaranteedLootCount; i++)
            {
                EquipmentItem item = GenerateDungeonLoot(player.Level, reward.MinLootRarity);
                player.Inventory.BackpackItems.Add(item);

                Console.WriteLine($"    • [{item.Rarity}] {item.Name}");
            }

            Console.WriteLine("╚═════════════════════════════════════╝");

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
        }

        private EquipmentItem GenerateDungeonLoot(int playerLevel, int minRarity)
        {
            // Force minimum rarity
            EquipmentItem item;
            int attempts = 0;

            do
            {
                item = EquipmentGenerator.GenerateItem(playerLevel + 2); // Better loot than regular
                attempts++;
            } while (GetRarityValue(item.Rarity) < minRarity && attempts < 50);

            return item;
        }

        private int GetRarityValue(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Common => 0,
                ItemRarity.Uncommon => 1,
                ItemRarity.Rare => 2,
                ItemRarity.Epic => 3,
                ItemRarity.Legendary => 4,
                _ => 0
            };
        }

        private string GetRarityName(int rarityValue)
        {
            return rarityValue switch
            {
                0 => "Common",
                1 => "Uncommon",
                2 => "Rare",
                3 => "Epic",
                4 => "Legendary",
                _ => "Common"
            };
        }

        private void HandleDeath(PlayerEntity player)
        {
            Console.Clear();
            Console.WriteLine("\n╔══════════════════════════════════════════╗");
            Console.WriteLine("║           DEFEAT!                     ║");
            Console.WriteLine("╚══════════════════════════════════════════╝\n");

            Console.WriteLine("You have been defeated in the dungeon...");
            Console.WriteLine("You crawl back to safety, but lose all dungeon loot and some gold.");

            int goldLost = Math.Min(player.Gold / 4, 200);
            player.Gold -= goldLost;
            player.CurrentHP = player.MaxHP / 2;

            Console.WriteLine($"\n💰 Lost {goldLost} gold");
            Console.WriteLine($"❤️  Recovered to {player.CurrentHP} HP");

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey(true);
        }
    }
}
