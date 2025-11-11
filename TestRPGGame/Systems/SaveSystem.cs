using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Dungeon;

namespace TestRPGGame.Systems
{
    public static class SaveSystem
    {
        private static readonly string SaveDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TestRPGGame",
            "Saves"
        );

        private const int MaxSaveSlots = 3;

        private static string GetSaveFilePath(int slot)
        {
            return Path.Combine(SaveDirectory, $"save_slot_{slot}.json");
        }

        public static bool SaveGame(Player player, int slot, DungeonProgress? dungeonProgress = null)
        {
            try
            {
                if (slot < 1 || slot > MaxSaveSlots)
                {
                    // Note: Error handling is done by caller, no direct console output
                    return false;
                }

                // Create directory if it doesn't exist
                if (!Directory.Exists(SaveDirectory))
                {
                    Directory.CreateDirectory(SaveDirectory);
                }

                // Access private base stats via reflection or add public getters
                // For now, we'll recalculate base stats from current stats minus equipment
                var equipStats = player.Inventory.GetTotalStats();

                // Create save data
                var saveData = new SaveData
                {
                    Name = player.Name,
                    Class = player.Class,
                    Level = player.Level,
                    Experience = player.Experience,
                    ExperienceToNextLevel = player.ExperienceToNextLevel,
                    Gold = player.Gold,
                    PotionCount = player.PotionCount,

                    // Calculate base stats by subtracting equipment bonuses
                    BaseMaxHP = player.MaxHP - equipStats.HP,
                    BaseMaxMana = player.MaxMana - equipStats.Mana,
                    BaseAttack = player.Attack - equipStats.Attack,
                    BaseDefense = player.Defense - equipStats.Defense,
                    BaseMagicPower = player.MagicPower - equipStats.Magic,
                    BaseSpeed = player.Speed - equipStats.Speed,
                    BaseCritChance = player.CritChance - equipStats.Crit,

                    CurrentHP = player.CurrentHP,
                    CurrentMana = player.CurrentMana,

                    // Equipment
                    Weapon = player.Inventory.Weapon,
                    Armor = player.Inventory.Armor,
                    Helmet = player.Inventory.Helmet,
                    Boots = player.Inventory.Boots,
                    Gloves = player.Inventory.Gloves,
                    Ring1 = player.Inventory.Ring1,
                    Ring2 = player.Inventory.Ring2,
                    Amulet = player.Inventory.Amulet,
                    Relic = player.Inventory.Relic,

                    BackpackItems = player.Inventory.BackpackItems,

                    // Save unlocked ability names
                    UnlockedAbilityNames = player.Abilities.Where(a => a.IsUnlocked).Select(a => a.Name).ToList(),

                    // Save dungeon progress
                    CompletedDungeons = dungeonProgress?.CompletedDungeons ?? new Dictionary<string, bool>(),

                    SaveTime = DateTime.Now,
                    PlayTime = 0 // Could track this in the future
                };

                // Serialize to JSON
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string json = JsonSerializer.Serialize(saveData, options);

                // Write to file
                File.WriteAllText(GetSaveFilePath(slot), json);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static (Player?, DungeonProgress?) LoadGame(int slot)
        {
            try
            {
                if (slot < 1 || slot > MaxSaveSlots)
                {
                    // Note: Error handling is done by caller, no direct console output
                    return (null, null);
                }

                string saveFilePath = GetSaveFilePath(slot);
                if (!File.Exists(saveFilePath))
                {
                    return (null, null);
                }

                // Read file
                string json = File.ReadAllText(saveFilePath);

                // Deserialize
                var saveData = JsonSerializer.Deserialize<SaveData>(json);

                if (saveData == null)
                {
                    return (null, null);
                }

                // Create player with basic constructor
                var player = new Player(saveData.Name, saveData.Class);

                // Restore basic data
                player.Level = saveData.Level;
                player.Experience = saveData.Experience;
                player.ExperienceToNextLevel = saveData.ExperienceToNextLevel;
                player.Gold = saveData.Gold;
                player.PotionCount = saveData.PotionCount;

                // Restore equipment
                player.Inventory.Weapon = saveData.Weapon;
                player.Inventory.Armor = saveData.Armor;
                player.Inventory.Helmet = saveData.Helmet;
                player.Inventory.Boots = saveData.Boots;
                player.Inventory.Gloves = saveData.Gloves;
                player.Inventory.Ring1 = saveData.Ring1;
                player.Inventory.Ring2 = saveData.Ring2;
                player.Inventory.Amulet = saveData.Amulet;
                player.Inventory.Relic = saveData.Relic;
                player.Inventory.BackpackItems = saveData.BackpackItems;

                // Update stats from equipment (this will recalculate MaxHP, MaxMana, etc.)
                player.UpdateStatsFromEquipment();

                // Restore current HP/Mana (must be after UpdateStatsFromEquipment)
                player.CurrentHP = Math.Min(saveData.CurrentHP, player.MaxHP);
                player.CurrentMana = Math.Min(saveData.CurrentMana, player.MaxMana);

                // Restore ability unlock status
                if (saveData.UnlockedAbilityNames != null)
                {
                    foreach (var ability in player.Abilities)
                    {
                        if (saveData.UnlockedAbilityNames.Contains(ability.Name))
                        {
                            ability.Unlock();
                        }
                    }
                }

                // Restore dungeon progress
                var dungeonProgress = new DungeonProgress();
                if (saveData.CompletedDungeons != null)
                {
                    dungeonProgress.CompletedDungeons = saveData.CompletedDungeons;
                }

                return (player, dungeonProgress);
            }
            catch (Exception)
            {
                return (null, null);
            }
        }

        public static List<SaveSlotInfo> GetAllSaveSlots()
        {
            var slots = new List<SaveSlotInfo>();

            for (int i = 1; i <= MaxSaveSlots; i++)
            {
                string saveFilePath = GetSaveFilePath(i);

                if (File.Exists(saveFilePath))
                {
                    try
                    {
                        string json = File.ReadAllText(saveFilePath);
                        var saveData = JsonSerializer.Deserialize<SaveData>(json);

                        if (saveData != null)
                        {
                            slots.Add(new SaveSlotInfo
                            {
                                SlotNumber = i,
                                Name = saveData.Name,
                                Class = saveData.Class,
                                Level = saveData.Level,
                                SaveTime = saveData.SaveTime,
                                IsEmpty = false
                            });
                            continue;
                        }
                    }
                    catch
                    {
                        // If we can't read the file, treat it as empty
                    }
                }

                // Empty slot
                slots.Add(new SaveSlotInfo
                {
                    SlotNumber = i,
                    IsEmpty = true
                });
            }

            return slots;
        }

        public static bool DeleteSave(int slot)
        {
            try
            {
                if (slot < 1 || slot > MaxSaveSlots)
                {
                    return false;
                }

                string saveFilePath = GetSaveFilePath(slot);
                if (File.Exists(saveFilePath))
                {
                    File.Delete(saveFilePath);
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Note: DisplaySaveSlots() method removed - UI display logic should be in interface layer
        // Use GetAllSaveSlots() to retrieve slot info and display it in your interface implementation
    }
}
