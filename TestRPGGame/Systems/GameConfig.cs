using System;
using System.IO;
using System.Text.Json;

namespace TestRPGGame.Systems
{
    public class GameConfiguration
    {
        public double ManaRegenRate { get; set; } = 0.05; // 5% of max mana per turn
        public double HealthRegenRate { get; set; } = 0.0; // No health regen by default
        public int RestingCost { get; set; } = 10; // Cost in gold to rest
        public int AbilityPurchaseCostMultiplier { get; set; } = 100; // Base cost multiplier for abilities
        public int AbilityUnlockLevel { get; set; } = 5; // Level required to unlock new abilities
        public double PotionHealPercent { get; set; } = 0.5; // Potions heal 50% of max HP
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
