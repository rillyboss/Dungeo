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
        private static Dictionary<string, DungeonData>? _dungeons;
        private static ItemGenerationData? _itemGeneration;
        private static Dictionary<string, EnemyPrefixData>? _enemyPrefixes;
        private static Dictionary<string, EnemySuffixData>? _enemySuffixes;

        /// <summary>
        /// Load all game data from JSON files.
        /// </summary>
        public static void LoadAllData()
        {
            try
            {
                // Load player abilities
                var playerAbilities = LoadJsonFile<Dictionary<string, AbilityData>>("abilities.json");

                // Load enemy abilities and merge with player abilities
                var enemyAbilities = LoadJsonFile<Dictionary<string, AbilityData>>("enemy-abilities.json");

                // Merge into a single dictionary before assigning to _abilities
                _abilities = new Dictionary<string, AbilityData>(playerAbilities);
                foreach (var ability in enemyAbilities)
                {
                    _abilities[ability.Key] = ability.Value;
                }

                // Load enemies and bosses (bosses now use the same format as enemies)
                _enemies = LoadJsonFile<Dictionary<string, EnemyData>>("enemies.json");
                var bossEnemies = LoadJsonFile<Dictionary<string, EnemyData>>("bosses.json");

                // Merge bosses into enemies dictionary
                foreach (var boss in bossEnemies)
                {
                    _enemies[boss.Key] = boss.Value;
                }

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

                // Load enemy modifier data
                _enemyPrefixes = LoadJsonFile<Dictionary<string, EnemyPrefixData>>(Path.Combine("Enemies", "enemy-prefixes.json"));
                _enemySuffixes = LoadJsonFile<Dictionary<string, EnemySuffixData>>(Path.Combine("Enemies", "enemy-suffixes.json"));

                Console.WriteLine($"✓ Loaded {_abilities.Count} abilities ({enemyAbilities.Count} enemy abilities)");
                Console.WriteLine($"✓ Loaded {_enemies.Count} enemies (including {bossEnemies.Count} bosses)");
                Console.WriteLine($"✓ Loaded {_dungeons.Count} dungeons");
                Console.WriteLine($"✓ Loaded item generation data ({_itemGeneration.WeaponPrefixes.Count} weapon prefixes, {_itemGeneration.WeaponTypes.Count} weapon types)");
                Console.WriteLine($"✓ Loaded enemy modifiers ({_enemyPrefixes.Count} prefixes, {_enemySuffixes.Count} suffixes)");
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

        // Bosses are now in the enemies dictionary, use GetEnemy() to retrieve them

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

        // Enemy modifier queries
        public static IEnumerable<EnemyPrefixData> GetEnemyPrefixesByLevel(int level)
        {
            if (_enemyPrefixes == null) throw new InvalidOperationException("Data not loaded. Call LoadAllData() first.");
            return _enemyPrefixes.Values.Where(p => p.MinLevel <= level);
        }

        public static IEnumerable<EnemySuffixData> GetEnemySuffixesByLevel(int level)
        {
            if (_enemySuffixes == null) throw new InvalidOperationException("Data not loaded. Call LoadAllData() first.");
            return _enemySuffixes.Values.Where(s => s.MinLevel <= level);
        }

        public static EnemyPrefixData GetEnemyPrefix(string id)
        {
            if (_enemyPrefixes == null) throw new InvalidOperationException("Data not loaded. Call LoadAllData() first.");
            if (!_enemyPrefixes.TryGetValue(id, out var prefix))
                throw new KeyNotFoundException($"Enemy prefix not found: {id}");
            return prefix;
        }

        public static EnemySuffixData GetEnemySuffix(string id)
        {
            if (_enemySuffixes == null) throw new InvalidOperationException("Data not loaded. Call LoadAllData() first.");
            if (!_enemySuffixes.TryGetValue(id, out var suffix))
                throw new KeyNotFoundException($"Enemy suffix not found: {id}");
            return suffix;
        }
    }
}
