using System;
using System.Collections.Generic;
using System.Linq;

namespace TestRPGGame.DataLoading
{
    public class EnemyArtData
    {
        public List<string> Pattern { get; set; } = new();
        public List<string> Art { get; set; } = new();
        public string Color { get; set; } = "White";
    }

    public class EnemyArtDatabase
    {
        private static Dictionary<string, EnemyArtData>? _artData;

        public static void LoadEnemyArt(Dictionary<string, EnemyArtData> artData)
        {
            _artData = artData;
        }

        public static (List<string> art, ConsoleColor color) GetEnemyArt(string enemyName)
        {
            if (_artData == null)
            {
                // Fallback if art not loaded
                return (new List<string>(), ConsoleColor.White);
            }

            // Try to find matching art by pattern
            foreach (var kvp in _artData)
            {
                var artEntry = kvp.Value;

                // Check if enemy name matches any pattern
                if (artEntry.Pattern.Any(pattern => enemyName.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
                {
                    var color = ParseColor(artEntry.Color);
                    return (artEntry.Art, color);
                }
            }

            // Return default art if no match found
            if (_artData.TryGetValue("Default", out var defaultArt))
            {
                return (defaultArt.Art, ParseColor(defaultArt.Color));
            }

            return (new List<string>(), ConsoleColor.White);
        }

        private static ConsoleColor ParseColor(string colorName)
        {
            return colorName switch
            {
                "Red" => ConsoleColor.Red,
                "DarkRed" => ConsoleColor.DarkRed,
                "Green" => ConsoleColor.Green,
                "DarkGreen" => ConsoleColor.DarkGreen,
                "Blue" => ConsoleColor.Blue,
                "DarkBlue" => ConsoleColor.DarkBlue,
                "Yellow" => ConsoleColor.Yellow,
                "DarkYellow" => ConsoleColor.DarkYellow,
                "Cyan" => ConsoleColor.Cyan,
                "DarkCyan" => ConsoleColor.DarkCyan,
                "Magenta" => ConsoleColor.Magenta,
                "DarkMagenta" => ConsoleColor.DarkMagenta,
                "Gray" => ConsoleColor.Gray,
                "DarkGray" => ConsoleColor.DarkGray,
                "White" => ConsoleColor.White,
                _ => ConsoleColor.White
            };
        }
    }
}
