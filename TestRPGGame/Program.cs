using System;
using TestRPGGame.UI;
using TestRPGGame.DataLoading;
using TestRPGGame.Systems;

namespace TestRPGGame
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Load all game data at startup - this is the ONLY place data is loaded
            Console.WriteLine("Loading game data...");
            try
            {
                DataLoader.LoadAllData();
                Console.WriteLine("Game data loaded successfully!\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FATAL ERROR: Failed to load game data!");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
                return;
            }

            Game game = new Game();
            game.Start();
        }
    }
}
