using System;
using System.IO;
using System.Text.Json;
using TestRPGGame.Interfaces;

namespace TestRPGGame.Systems
{
    public class GameConfiguration
    {
        // Regeneration & Healing
        public double ManaRegenRate { get; set; } = 0.05; // 5% of max mana per turn
        public double HealthRegenRate { get; set; } = 0.0; // No health regen by default
        public double PotionHealPercent { get; set; } = 0.5; // Potions heal 50% of max HP
        public double DefeatHPRestorePercent { get; set; } = 0.5; // Restore 50% HP on defeat

        // Shop Settings
        public int ShopRefreshCost { get; set; } = 50; // Cost to refresh shop inventory
        public int PotionPrice { get; set; } = 50; // Cost per potion
        public double ItemSellPriceMultiplier { get; set; } = 0.6; // Sell items for 60% of buy price
        public int ShopInventoryMinItems { get; set; } = 8; // Minimum items in shop
        public int ShopInventoryMaxItems { get; set; } = 12; // Maximum items in shop

        // Combat Settings
        public int MaxCombatTurns { get; set; } = 100; // Max turns before combat timeout
        public double CombatLootDropChance { get; set; } = 0.4; // 40% chance for loot drop
        public double CombatGoldLossPercent { get; set; } = 0.25; // Lose 25% (1/4) of gold on defeat
        public int CombatGoldLossMax { get; set; } = 100; // Maximum gold lost in combat

        // Dungeon Settings
        public double DungeonGoldLossPercent { get; set; } = 0.25; // Lose 25% (1/4) of gold on dungeon defeat
        public int DungeonGoldLossMax { get; set; } = 200; // Maximum gold lost in dungeon
        public int DungeonLootLevelBonus { get; set; } = 2; // Dungeon loot is +2 levels higher

        // Abilities & Progression
        public int RestingCost { get; set; } = 10; // Cost in gold to rest
        public int AbilityUnlockLevel { get; set; } = 5; // Level required to unlock new abilities

        // Economy Multipliers (for easy balance tuning)
        public double CombatGoldMultiplier { get; set; } = 1.0; // Multiply combat gold rewards
        public double DungeonGoldMultiplier { get; set; } = 1.0; // Multiply dungeon gold rewards
        public double EquipmentPurchasePriceMultiplier { get; set; } = 1.0; // Multiply equipment buy prices
        public double EquipmentSellPriceMultiplier { get; set; } = 1.0; // Multiply equipment sell prices
        public double AbilityPurchasePriceMultiplier { get; set; } = 1.0; // Multiply ability purchase costs

        // Combat Difficulty Multipliers (for balance tuning)
        public double EnemyDamageMultiplier { get; set; } = 1.0; // Multiply enemy damage output
        public double EnemyAttackMultiplier { get; set; } = 1.0; // Multiply enemy attack stat
        public double EnemyDefenseMultiplier { get; set; } = 1.0; // Multiply enemy defense stat
        public double EnemyHPMultiplier { get; set; } = 1.0; // Multiply enemy HP

        // Additional Equipment Ability Chances by Rarity
        // Note: Weapons always grant their signature ability based on type
        // These chances determine if an ADDITIONAL ability is granted on top of the signature
        public double AdditionalAbilityChanceCommon { get; set; } = 0.05; // 5% chance for Common to grant extra ability
        public double AdditionalAbilityChanceUncommon { get; set; } = 0.10; // 10% chance for Uncommon to grant extra ability
        public double AdditionalAbilityChanceRare { get; set; } = 0.25; // 25% chance for Rare to grant extra ability
        public double AdditionalAbilityChanceEpic { get; set; } = 0.50; // 50% chance for Epic to grant extra ability
        public double AdditionalAbilityChanceLegendary { get; set; } = 0.75; // 75% chance for Legendary to grant extra ability

        // Equipment Rarity Drop Thresholds (0-100 roll)
        // Higher threshold = rarer items (need higher roll to get them)
        public int RarityLegendaryThreshold { get; set; } = 98; // 2% chance for Legendary
        public int RarityEpicThreshold { get; set; } = 92; // 6% chance for Epic (92-97)
        public int RarityRareThreshold { get; set; } = 80; // 12% chance for Rare (80-91)
        public int RarityUncommonThreshold { get; set; } = 30; // 50% chance for Uncommon (30-79)
        public int RarityCommonThreshold { get; set; } = 0; // 30% chance for Common (0-29)

        // System Settings
        public bool CombatAutosave { get; set; } = true; // Autosave after combat
    }

    public static class GameConfig
    {
        private static GameConfiguration? _config;
        private static readonly string ConfigFileName = "gameconfig.json";
        private static ILogger _logger = new ConsoleLogger(); // Default to console logging

        /// <summary>
        /// Sets the logger for GameConfig. Use NullLogger for tests to suppress output.
        /// </summary>
        public static void SetLogger(ILogger logger)
        {
            _logger = logger;
        }

        public static GameConfiguration Config
        {
            get
            {
                if (_config == null)
                {
                    LoadConfig();
                }
                return _config!;
            }
        }

        private static void LoadConfig()
        {
            try
            {
                // Look for config in the same directory as the executable
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);

                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    _config = JsonSerializer.Deserialize<GameConfiguration>(json);

                    if (_config != null)
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Could not load config file. Using defaults. Error: {ex.Message}");
            }

            // Fallback to default configuration
            _config = new GameConfiguration();
        }

        public static void SaveConfig()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(_config, options);
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving config: {ex.Message}");
            }
        }
    }
}
