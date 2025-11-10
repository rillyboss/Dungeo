using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace TestRPGGame.DataLoading
{
    /// <summary>
    /// Central data loading system for all game entities.
    /// All game data is loaded from JSON files in the Data/ directory.
    /// </summary>
    public static class DataLoader
    {
        private static string DataPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

        // Cached data collections
        private static Dictionary<string, AbilityData>? _abilities;
        private static Dictionary<string, EnemyData>? _enemies;
        private static Dictionary<string, BossData>? _bosses;
        private static Dictionary<string, DungeonData>? _dungeons;
        private static ItemGenerationData? _itemGeneration;

        /// <summary>
        /// Load all game data from JSON files.
        /// </summary>
        public static void LoadAllData()
        {
            try
            {
                // Load player abilities
                _abilities = LoadJsonFile<Dictionary<string, AbilityData>>("abilities.json");

                // Load enemy abilities and merge with player abilities
                var enemyAbilities = LoadJsonFile<Dictionary<string, AbilityData>>("enemy-abilities.json");
                foreach (var ability in enemyAbilities)
                {
                    _abilities[ability.Key] = ability.Value;
                }

                _enemies = LoadJsonFile<Dictionary<string, EnemyData>>("enemies.json");
                _bosses = LoadJsonFile<Dictionary<string, BossData>>("bosses.json");
                _dungeons = LoadJsonFile<Dictionary<string, DungeonData>>("dungeons.json");

                // Load item generation data from separate files
                _itemGeneration = new ItemGenerationData
                {
                    weapon_prefixes = LoadJsonFile<Dictionary<string, WeaponPrefixData>>(Path.Combine("Items", "weapon-prefixes.json")),
                    weapon_types = LoadJsonFile<Dictionary<string, WeaponTypeData>>(Path.Combine("Items", "weapon-types.json")),
                    weapon_suffixes = LoadJsonFile<Dictionary<string, WeaponSuffixData>>(Path.Combine("Items", "weapon-suffixes.json")),
                    armor_prefixes = LoadJsonFile<Dictionary<string, ArmorPrefixData>>(Path.Combine("Items", "armor-prefixes.json")),
                    armor_suffixes = LoadJsonFile<Dictionary<string, ArmorSuffixData>>(Path.Combine("Items", "armor-suffixes.json")),
                    rarity_multipliers = LoadJsonFile<Dictionary<string, double>>(Path.Combine("Items", "rarity-multipliers.json"))
                };

                Console.WriteLine($"✓ Loaded {_abilities.Count} abilities ({enemyAbilities.Count} enemy abilities)");
                Console.WriteLine($"✓ Loaded {_enemies.Count} enemies");
                Console.WriteLine($"✓ Loaded {_bosses.Count} bosses");
                Console.WriteLine($"✓ Loaded {_dungeons.Count} dungeons");
                Console.WriteLine($"✓ Loaded item generation data ({_itemGeneration.WeaponPrefixes.Count} weapon prefixes, {_itemGeneration.WeaponTypes.Count} weapon types)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR loading game data: {ex.Message}");
                throw;
            }
        }

        private static T LoadJsonFile<T>(string filename)
        {
            string path = Path.Combine(DataPath, filename);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Data file not found: {path}");
            }

            string json = File.ReadAllText(path);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            return JsonSerializer.Deserialize<T>(json, options)
                ?? throw new InvalidOperationException($"Failed to deserialize {filename}");
        }

        // Ability queries
        public static AbilityData GetAbility(string id)
        {
            if (_abilities == null) throw new InvalidOperationException("Data not loaded. Call LoadAllData() first.");
            if (!_abilities.TryGetValue(id, out var ability))
                throw new KeyNotFoundException($"Ability not found: {id}");
            return ability;
        }

        public static IEnumerable<AbilityData> GetAbilitiesForClass(string playerClass)
        {
            if (_abilities == null) throw new InvalidOperationException("Data not loaded. Call LoadAllData() first.");
            return _abilities.Values.Where(a => a.PlayerClass == playerClass);
        }

        // Enemy queries
        public static EnemyData GetEnemy(string id)
        {
            if (_enemies == null) throw new InvalidOperationException("Data not loaded. Call LoadAllData() first.");
            if (!_enemies.TryGetValue(id, out var enemy))
                throw new KeyNotFoundException($"Enemy not found: {id}");
            return enemy;
        }

        public static IEnumerable<EnemyData> GetEnemiesByLevel(int minLevel, int maxLevel)
        {
            if (_enemies == null) throw new InvalidOperationException("Data not loaded. Call LoadAllData() first.");
            return _enemies.Values.Where(e => e.Level >= minLevel && e.Level <= maxLevel);
        }

        // Boss queries
        public static BossData GetBoss(string id)
        {
            if (_bosses == null) throw new InvalidOperationException("Data not loaded. Call LoadAllData() first.");
            if (!_bosses.TryGetValue(id, out var boss))
                throw new KeyNotFoundException($"Boss not found: {id}");
            return boss;
        }

        // Dungeon queries
        public static DungeonData GetDungeon(string id)
        {
            if (_dungeons == null) throw new InvalidOperationException("Data not loaded. Call LoadAllData() first.");
            if (!_dungeons.TryGetValue(id, out var dungeon))
                throw new KeyNotFoundException($"Dungeon not found: {id}");
            return dungeon;
        }

        public static IEnumerable<DungeonData> GetAllDungeons()
        {
            if (_dungeons == null) throw new InvalidOperationException("Data not loaded. Call LoadAllData() first.");
            return _dungeons.Values.OrderBy(d => d.RecommendedLevel);
        }

        // Item generation queries
        public static ItemGenerationData GetItemGenerationData()
        {
            if (_itemGeneration == null) throw new InvalidOperationException("Data not loaded. Call LoadAllData() first.");
            return _itemGeneration;
        }
    }
}
