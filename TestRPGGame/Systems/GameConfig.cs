using System;
using System.IO;
using System.Text.Json;

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
        public int AbilityPurchaseCostMultiplier { get; set; } = 100; // Base cost multiplier for abilities
        public int AbilityUnlockLevel { get; set; } = 5; // Level required to unlock new abilities

        // System Settings
        public bool CombatAutosave { get; set; } = true; // Autosave after combat
    }

    public static class GameConfig
    {
        private static GameConfiguration? _config;
        private static readonly string ConfigFileName = "gameconfig.json";

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
                Console.WriteLine($"Warning: Could not load config file. Using defaults. Error: {ex.Message}");
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
                Console.WriteLine($"Error saving config: {ex.Message}");
            }
        }
    }
}
