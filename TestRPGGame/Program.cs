using System;
using System.Linq;
using TestRPGGame.UI;
using TestRPGGame.DataLoading;
using TestRPGGame.Systems;
using TestRPGGame.Interfaces;

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

            // Check command line arguments for interface selection
            bool useAutomated = args.Contains("--automated") || args.Contains("-a");
            bool useOldGame = args.Contains("--old") || args.Contains("-o");

            if (useOldGame)
            {
                // Run original game (DEPRECATED - for backwards compatibility only)
                Console.WriteLine("⚠️  Starting LEGACY game (deprecated)...");
                Console.WriteLine("   Use 'dotnet run' without --old flag for the new architecture\n");
                Game game = new Game();
                game.Start();
            }
            else if (useAutomated)
            {
                // Run with automated AI interface (for testing/analysis)
                Console.WriteLine("Starting game with AUTOMATED interface...\n");
                var automatedInterface = new AutomatedInterface(new DefaultStrategy());
                var gameCore = new GameCore(automatedInterface);
                gameCore.Start();

                // Print the playthrough log
                Console.WriteLine("\n\n=== PLAYTHROUGH COMPLETE ===");
                Console.WriteLine("Full log:");
                Console.WriteLine(automatedInterface.GetLog());
            }
            else
            {
                // Default: Interface-driven architecture with console UI
                var consoleInterface = new ConsoleInterface();
                var gameCore = new GameCore(consoleInterface);
                gameCore.Start();
            }
        }
    }
}
