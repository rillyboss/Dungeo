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
                // Use default JSON repository for production
                var dataRepository = new JsonDataRepository();
                dataRepository.LoadAllData();
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

            if (useAutomated)
            {
                // Run automated playtest with UltraThink analysis: 3 runs per class to level 15
                Console.WriteLine("╔═══════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║           ULTRATHINK GAMEPLAY ANALYSIS - 3 RUNS PER CLASS TO LEVEL 15        ║");
                Console.WriteLine("╚═══════════════════════════════════════════════════════════════════════════════╝\n");

                var classes = new[]
                {
                    (BaseName: "Thorin", Class: Entities.Player.PlayerClass.Warrior),
                    (BaseName: "Gandalf", Class: Entities.Player.PlayerClass.Mage),
                    (BaseName: "Legolas", Class: Entities.Player.PlayerClass.Rogue)
                };

                var allAnalytics = new System.Collections.Generic.List<GameplayAnalytics>();
                var results = new System.Collections.Generic.List<(string Name, Entities.Player.PlayerClass Class, int Run, bool Success, int FinalLevel)>();

                // Run 3 playthroughs per class
                foreach (var (BaseName, Class) in classes)
                {
                    for (int runNumber = 1; runNumber <= 3; runNumber++)
                    {
                        string characterName = $"{BaseName}{runNumber}";

                        Console.WriteLine($"\n╔═══════════════════════════════════════════════════════════════════════════════╗");
                        Console.WriteLine($"║  Run {runNumber}/3: {characterName} the {Class,-10}                                        ║");
                        Console.WriteLine($"╚═══════════════════════════════════════════════════════════════════════════════╝\n");

                        try
                        {
                            var analytics = new GameplayAnalytics { RunNumber = runNumber };
                            var strategy = new UltraThinkStrategy(characterName, Class, analytics);
                            var automatedInterface = new AutomatedInterface(strategy);
                            var gameCore = new GameCore(automatedInterface);

                            Console.WriteLine($"🎮 Starting playthrough: {characterName} ({Class})");
                            Console.WriteLine($"🎯 Goal: Reach Level 15 while analyzing all game systems\n");

                            gameCore.Start();

                            // Finalize analytics
                            strategy.FinalizeAnalysis();
                            allAnalytics.Add(analytics);

                            bool success = analytics.FinalLevel >= 15;
                            results.Add((characterName, Class, runNumber, success, analytics.FinalLevel));

                            Console.WriteLine($"\n✅ {characterName} ({Class}) Run {runNumber} - Completed!");
                            Console.WriteLine($"   Final Level: {analytics.FinalLevel}");
                            Console.WriteLine($"   Time Played: {(analytics.EndTime ?? analytics.StartTime).Subtract(analytics.StartTime).TotalMinutes:F1} minutes");
                            Console.WriteLine($"   Combat Win Rate: {analytics.GetWinRate():F1}%");
                        }
                        catch (Exception ex)
                        {
                            results.Add((characterName, Class, runNumber, false, 0));
                            Console.WriteLine($"\n❌ {characterName} ({Class}) Run {runNumber} - FAILED: {ex.Message}");
                            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                        }

                        // Brief pause between runs
                        System.Threading.Thread.Sleep(500);
                    }

                    Console.WriteLine($"\n{new string('=', 85)}");
                    Console.WriteLine($"Completed all 3 runs for {Class}");
                    Console.WriteLine($"{new string('=', 85)}\n");
                }

                // Generate and display comprehensive analysis report
                Console.WriteLine("\n\n");
                Console.WriteLine("╔═══════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                    GENERATING COMPREHENSIVE REPORT...                         ║");
                Console.WriteLine("╚═══════════════════════════════════════════════════════════════════════════════╝\n");

                string report = AnalysisReportGenerator.GenerateFullReport(allAnalytics);
                Console.WriteLine(report);

                // Print quick results summary
                Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                         PLAYTHROUGH RESULTS                                   ║");
                Console.WriteLine("╚═══════════════════════════════════════════════════════════════════════════════╝\n");

                foreach (var classGroup in results.GroupBy(r => r.Class))
                {
                    Console.WriteLine($"{classGroup.Key}:");
                    foreach (var result in classGroup)
                    {
                        string status = result.Success ? "✅" : "❌";
                        Console.WriteLine($"  {status} Run {result.Run}: {result.Name} - Level {result.FinalLevel}");
                    }
                    Console.WriteLine();
                }

                int successCount = results.Count(r => r.Success);
                int totalRuns = results.Count;
                Console.WriteLine($"\n📊 Overall Success: {successCount}/{totalRuns} runs completed successfully");

                if (successCount == totalRuns)
                {
                    Console.WriteLine("🎉 ALL PLAYTHROUGHS SUCCESSFUL! Review the detailed analysis above.");
                }
                else
                {
                    Console.WriteLine("⚠️  Some playthroughs failed. Review the logs above for details.");
                }

                // Save report to file
                try
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string filename = $"UltraThink_Analysis_{timestamp}.txt";
                    System.IO.File.WriteAllText(filename, report);
                    Console.WriteLine($"\n📄 Full report saved to: {filename}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n⚠️  Could not save report to file: {ex.Message}");
                }
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
