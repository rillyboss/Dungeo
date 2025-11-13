using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestRPGGame.Entities.Player;

namespace TestRPGGame.Interfaces
{
    /// <summary>
    /// Generates comprehensive analysis reports from gameplay analytics data
    /// </summary>
    public static class AnalysisReportGenerator
    {
        public static string GenerateFullReport(List<GameplayAnalytics> allRuns)
        {
            var report = new StringBuilder();

            report.AppendLine("╔═══════════════════════════════════════════════════════════════════════════════╗");
            report.AppendLine("║                    ULTRATHINK GAMEPLAY ANALYSIS REPORT                        ║");
            report.AppendLine("║                     9 Complete Playthroughs to Level 15                       ║");
            report.AppendLine("╚═══════════════════════════════════════════════════════════════════════════════╝");
            report.AppendLine();

            // Executive Summary
            report.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("  EXECUTIVE SUMMARY");
            report.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
            report.AppendLine();

            var avgPlaytime = TimeSpan.FromTicks((long)allRuns.Average(r => (r.EndTime ?? r.StartTime).Subtract(r.StartTime).Ticks));
            var totalCombats = allRuns.Sum(r => r.TotalCombats);
            var avgWinRate = allRuns.Average(r => r.GetWinRate());

            report.AppendLine($"Total Runs Completed:       {allRuns.Count}");
            report.AppendLine($"Average Time to Level 15:   {avgPlaytime.TotalMinutes:F1} minutes");
            report.AppendLine($"Total Combats Across Runs:  {totalCombats}");
            report.AppendLine($"Average Win Rate:           {avgWinRate:F1}%");
            report.AppendLine($"Total Dungeon Attempts:     {allRuns.Sum(r => r.DungeonAttempts)}");
            report.AppendLine($"Dungeon Success Rate:       {allRuns.Average(r => r.GetDungeonSuccessRate()):F1}%");
            report.AppendLine();

            // Per-Class Analysis
            report.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("  CLASS-SPECIFIC ANALYSIS");
            report.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
            report.AppendLine();

            foreach (var playerClass in new[] { PlayerClass.Warrior, PlayerClass.Mage, PlayerClass.Rogue })
            {
                var classRuns = allRuns.Where(r => r.Class == playerClass).ToList();
                GenerateClassAnalysis(report, playerClass, classRuns);
            }

            // System Analysis
            report.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("  GAME SYSTEMS ANALYSIS");
            report.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
            report.AppendLine();

            GenerateCombatSystemAnalysis(report, allRuns);
            GenerateProgressionAnalysis(report, allRuns);
            GenerateEconomyAnalysis(report, allRuns);
            GenerateDungeonAnalysis(report, allRuns);
            GenerateAbilityAnalysis(report, allRuns);
            GenerateLootAnalysis(report, allRuns);
            GenerateResourceManagementAnalysis(report, allRuns);

            // Key Observations
            report.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("  KEY OBSERVATIONS & INSIGHTS");
            report.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
            report.AppendLine();

            GenerateObservationsSummary(report, allRuns);

            // Recommendations
            report.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("  RECOMMENDATIONS FOR IMPROVEMENT");
            report.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
            report.AppendLine();

            GenerateRecommendations(report, allRuns);

            return report.ToString();
        }

        private static void GenerateClassAnalysis(StringBuilder report, PlayerClass playerClass, List<GameplayAnalytics> runs)
        {
            if (!runs.Any()) return;

            report.AppendLine($"--- {playerClass} Class ({runs.Count} runs) ---");
            report.AppendLine();

            var avgTime = TimeSpan.FromTicks((long)runs.Average(r => (r.EndTime ?? r.StartTime).Subtract(r.StartTime).Ticks));
            var avgWinRate = runs.Average(r => r.GetWinRate());
            var avgDungeonSuccess = runs.Average(r => r.GetDungeonSuccessRate());
            var avgCombats = (int)runs.Average(r => r.TotalCombats);

            report.AppendLine($"  Average Time to Level 15:    {avgTime.TotalMinutes:F1} minutes");
            report.AppendLine($"  Average Combats:             {avgCombats}");
            report.AppendLine($"  Combat Win Rate:             {avgWinRate:F1}%");
            report.AppendLine($"  Dungeon Success Rate:        {avgDungeonSuccess:F1}%");
            report.AppendLine($"  Average Gold Earned:         {runs.Average(r => r.TotalGoldEarned):F0}");
            report.AppendLine($"  Average Gold Spent:          {runs.Average(r => r.TotalGoldSpent):F0}");
            report.AppendLine($"  Average Potions Used:        {runs.Average(r => r.PotionsUsed):F1}");
            report.AppendLine($"  Average Times Rested:        {runs.Average(r => r.TimesRested):F1}");
            report.AppendLine($"  Average Near-Death Moments:  {runs.Average(r => r.TimesNearDeath):F1}");
            report.AppendLine($"  Average Abilities Unlocked:  {runs.Average(r => r.AbilitiesUnlocked):F1}");
            report.AppendLine();

            // Ability usage
            var allAbilities = runs.SelectMany(r => r.AbilityUsageCount).GroupBy(kvp => kvp.Key)
                .OrderByDescending(g => g.Sum(kvp => kvp.Value))
                .Take(5)
                .ToList();

            if (allAbilities.Any())
            {
                report.AppendLine($"  Most Used Abilities:");
                foreach (var ability in allAbilities)
                {
                    var avgUses = ability.Sum(kvp => kvp.Value) / (double)runs.Count;
                    report.AppendLine($"    • {ability.Key}: {avgUses:F1} uses per run");
                }
                report.AppendLine();
            }

            // Class-specific insights
            var classObservations = runs.SelectMany(r => r.PositiveObservations.Concat(r.NegativeObservations).Concat(r.BalanceIssues))
                .Distinct()
                .Take(5);

            if (classObservations.Any())
            {
                report.AppendLine($"  Key Insights:");
                foreach (var obs in classObservations)
                {
                    report.AppendLine($"    • {obs}");
                }
                report.AppendLine();
            }

            report.AppendLine();
        }

        private static void GenerateCombatSystemAnalysis(StringBuilder report, List<GameplayAnalytics> allRuns)
        {
            report.AppendLine("🗡️  COMBAT SYSTEM");
            report.AppendLine();

            var totalDamageDealt = allRuns.Sum(r => r.TotalDamageDealt);
            var totalDamageTaken = allRuns.Sum(r => r.TotalDamageTaken);
            var avgDamagePerCombat = allRuns.Average(r => r.GetAverageDamagePerCombat());
            var totalCrits = allRuns.Sum(r => r.CriticalHits);
            var totalMisses = allRuns.Sum(r => r.MissedAttacks);

            report.AppendLine($"  Total Damage Dealt:          {totalDamageDealt:N0}");
            report.AppendLine($"  Total Damage Taken:          {totalDamageTaken:N0}");
            report.AppendLine($"  Average Damage Per Combat:   {avgDamagePerCombat:F1}");
            report.AppendLine($"  Total Critical Hits:         {totalCrits}");
            report.AppendLine($"  Total Missed Attacks:        {totalMisses}");
            report.AppendLine($"  Average Combat Win Rate:     {allRuns.Average(r => r.GetWinRate()):F1}%");
            report.AppendLine();

            // Status effects analysis
            var topEffects = allRuns.SelectMany(r => r.StatusEffectsApplied)
                .GroupBy(kvp => kvp.Key)
                .OrderByDescending(g => g.Sum(kvp => kvp.Value))
                .Take(5);

            if (topEffects.Any())
            {
                report.AppendLine($"  Most Common Status Effects:");
                foreach (var effect in topEffects)
                {
                    var totalCount = effect.Sum(kvp => kvp.Value);
                    report.AppendLine($"    • {effect.Key}: {totalCount} applications");
                }
                report.AppendLine();
            }
        }

        private static void GenerateProgressionAnalysis(StringBuilder report, List<GameplayAnalytics> allRuns)
        {
            report.AppendLine("📈 PROGRESSION SYSTEM");
            report.AppendLine();

            var avgLevel5Time = allRuns.Average(r => r.TimeToLevel5.TotalMinutes);
            var avgLevel10Time = allRuns.Average(r => r.TimeToLevel10.TotalMinutes);
            var avgLevel15Time = allRuns.Average(r => r.TimeToLevel15.TotalMinutes);

            report.AppendLine($"  Average Time to Level 5:     {avgLevel5Time:F1} minutes");
            report.AppendLine($"  Average Time to Level 10:    {avgLevel10Time:F1} minutes");
            report.AppendLine($"  Average Time to Level 15:    {avgLevel15Time:F1} minutes");
            report.AppendLine();

            report.AppendLine($"  Average Pace:");
            report.AppendLine($"    • Levels 1-5:   {avgLevel5Time / 5:F1} min/level");
            report.AppendLine($"    • Levels 5-10:  {(avgLevel10Time - avgLevel5Time) / 5:F1} min/level");
            report.AppendLine($"    • Levels 10-15: {(avgLevel15Time - avgLevel10Time) / 5:F1} min/level");
            report.AppendLine();
        }

        private static void GenerateEconomyAnalysis(StringBuilder report, List<GameplayAnalytics> allRuns)
        {
            report.AppendLine("💰 ECONOMY SYSTEM");
            report.AppendLine();

            var avgGoldEarned = allRuns.Average(r => r.TotalGoldEarned);
            var avgGoldSpent = allRuns.Average(r => r.TotalGoldSpent);
            var avgFinalGold = allRuns.Average(r => r.FinalGold);
            var avgShopVisits = allRuns.Average(r => r.ShopVisits);
            var avgItemsPurchased = allRuns.Average(r => r.ItemsPurchased);

            report.AppendLine($"  Average Gold Earned:         {avgGoldEarned:F0}");
            report.AppendLine($"  Average Gold Spent:          {avgGoldSpent:F0}");
            report.AppendLine($"  Average Final Gold:          {avgFinalGold:F0}");
            report.AppendLine($"  Average Shop Visits:         {avgShopVisits:F1}");
            report.AppendLine($"  Average Items Purchased:     {avgItemsPurchased:F1}");
            report.AppendLine($"  Gold Efficiency:             {allRuns.Average(r => r.GetGoldEfficiency()):F1}%");
            report.AppendLine();
        }

        private static void GenerateDungeonAnalysis(StringBuilder report, List<GameplayAnalytics> allRuns)
        {
            report.AppendLine("🏰 DUNGEON SYSTEM");
            report.AppendLine();

            var totalAttempts = allRuns.Sum(r => r.DungeonAttempts);
            var totalCompleted = allRuns.Sum(r => r.DungeonsCompleted);
            var totalFailed = allRuns.Sum(r => r.DungeonsFailed);
            var successRate = totalAttempts > 0 ? (double)totalCompleted / totalAttempts * 100 : 0;

            report.AppendLine($"  Total Attempts:              {totalAttempts}");
            report.AppendLine($"  Completed:                   {totalCompleted}");
            report.AppendLine($"  Failed:                      {totalFailed}");
            report.AppendLine($"  Success Rate:                {successRate:F1}%");
            report.AppendLine($"  Average Attempts Per Run:    {allRuns.Average(r => r.DungeonAttempts):F1}");
            report.AppendLine();
        }

        private static void GenerateAbilityAnalysis(StringBuilder report, List<GameplayAnalytics> allRuns)
        {
            report.AppendLine("⚡ ABILITY SYSTEM");
            report.AppendLine();

            var avgUnlocked = allRuns.Average(r => r.AbilitiesUnlocked);
            var totalUsages = allRuns.Sum(r => r.AbilityUsageCount.Values.Sum());

            report.AppendLine($"  Average Abilities Unlocked:  {avgUnlocked:F1} (out of 3 available)");
            report.AppendLine($"  Total Ability Usages:        {totalUsages}");
            report.AppendLine($"  Average Usages Per Run:      {totalUsages / (double)allRuns.Count:F1}");
            report.AppendLine();

            var topAbilities = allRuns.SelectMany(r => r.AbilityUsageCount)
                .GroupBy(kvp => kvp.Key)
                .OrderByDescending(g => g.Sum(kvp => kvp.Value))
                .Take(10);

            report.AppendLine($"  Most Used Abilities (All Classes):");
            foreach (var ability in topAbilities)
            {
                var totalUses = ability.Sum(kvp => kvp.Value);
                var avgPerRun = totalUses / (double)allRuns.Count;
                report.AppendLine($"    • {ability.Key}: {totalUses} total ({avgPerRun:F1} per run)");
            }
            report.AppendLine();
        }

        private static void GenerateLootAnalysis(StringBuilder report, List<GameplayAnalytics> allRuns)
        {
            report.AppendLine("🎁 LOOT & EQUIPMENT SYSTEM");
            report.AppendLine();

            var totalLoot = allRuns.Sum(r => r.LootDropsReceived);
            var avgLootPerRun = allRuns.Average(r => r.LootDropsReceived);

            report.AppendLine($"  Total Loot Drops:            {totalLoot}");
            report.AppendLine($"  Average Drops Per Run:       {avgLootPerRun:F1}");
            report.AppendLine();

            var lootByRarity = allRuns.SelectMany(r => r.LootByRarity)
                .GroupBy(kvp => kvp.Key)
                .Select(g => new { Rarity = g.Key, Count = g.Sum(kvp => kvp.Value) })
                .OrderByDescending(x => x.Count);

            report.AppendLine($"  Loot Distribution by Rarity:");
            foreach (var rarity in lootByRarity)
            {
                var percentage = (double)rarity.Count / totalLoot * 100;
                report.AppendLine($"    • {rarity.Rarity}: {rarity.Count} ({percentage:F1}%)");
            }
            report.AppendLine();
        }

        private static void GenerateResourceManagementAnalysis(StringBuilder report, List<GameplayAnalytics> allRuns)
        {
            report.AppendLine("💊 RESOURCE MANAGEMENT");
            report.AppendLine();

            var avgPotions = allRuns.Average(r => r.PotionsUsed);
            var avgRests = allRuns.Average(r => r.TimesRested);
            var avgNearDeath = allRuns.Average(r => r.TimesNearDeath);
            var avgOutOfMana = allRuns.Average(r => r.TimesOutOfMana);

            report.AppendLine($"  Average Potions Used:        {avgPotions:F1}");
            report.AppendLine($"  Average Times Rested:        {avgRests:F1}");
            report.AppendLine($"  Average Near-Death Moments:  {avgNearDeath:F1}");
            report.AppendLine($"  Average Out-of-Mana Moments: {avgOutOfMana:F1}");
            report.AppendLine();
        }

        private static void GenerateObservationsSummary(StringBuilder report, List<GameplayAnalytics> allRuns)
        {
            // Aggregate all observations
            var allPositive = allRuns.SelectMany(r => r.PositiveObservations).Distinct().ToList();
            var allNegative = allRuns.SelectMany(r => r.NegativeObservations).Distinct().ToList();
            var allMissing = allRuns.SelectMany(r => r.MissingInformation).Distinct().ToList();
            var allBalance = allRuns.SelectMany(r => r.BalanceIssues).Distinct().ToList();

            if (allPositive.Any())
            {
                report.AppendLine("✅ WHAT WORKS WELL:");
                report.AppendLine();
                foreach (var obs in allPositive.Take(10))
                {
                    report.AppendLine($"  • {obs}");
                }
                report.AppendLine();
            }

            if (allNegative.Any())
            {
                report.AppendLine("❌ WHAT DOESN'T WORK:");
                report.AppendLine();
                foreach (var obs in allNegative.Take(10))
                {
                    report.AppendLine($"  • {obs}");
                }
                report.AppendLine();
            }

            if (allMissing.Any())
            {
                report.AppendLine("❓ MISSING INFORMATION:");
                report.AppendLine();
                foreach (var obs in allMissing.Distinct().Take(10))
                {
                    report.AppendLine($"  • {obs}");
                }
                report.AppendLine();
            }

            if (allBalance.Any())
            {
                report.AppendLine("⚖️  BALANCE ISSUES:");
                report.AppendLine();
                foreach (var obs in allBalance.Take(10))
                {
                    report.AppendLine($"  • {obs}");
                }
                report.AppendLine();
            }
        }

        private static void GenerateRecommendations(StringBuilder report, List<GameplayAnalytics> allRuns)
        {
            var recommendations = new List<string>();

            // Analyze and generate recommendations based on metrics
            var avgWinRate = allRuns.Average(r => r.GetWinRate());
            if (avgWinRate > 95)
            {
                recommendations.Add("Combat is too easy (95%+ win rate). Consider increasing enemy stats or adding more challenging mechanics.");
            }
            else if (avgWinRate < 70)
            {
                recommendations.Add("Combat may be too difficult (<70% win rate). Consider rebalancing enemy stats or improving player capabilities.");
            }

            var avgNearDeath = allRuns.Average(r => r.TimesNearDeath);
            if (avgNearDeath > 25)
            {
                recommendations.Add($"Players frequently in near-death situations ({avgNearDeath:F0} times per run). Consider improving HP regeneration or reducing damage spikes.");
            }

            var avgAbilitiesUnlocked = allRuns.Average(r => r.AbilitiesUnlocked);
            if (avgAbilitiesUnlocked < 2)
            {
                recommendations.Add("Players unlocking few abilities. Consider reducing ability costs or increasing gold rewards.");
            }

            var dungeonSuccess = allRuns.Average(r => r.GetDungeonSuccessRate());
            if (dungeonSuccess < 50)
            {
                recommendations.Add("Low dungeon success rate. Consider adding guidance on dungeon difficulty or adjusting balance.");
            }

            var avgPotions = allRuns.Average(r => r.PotionsUsed);
            if (avgPotions > 40)
            {
                recommendations.Add("Heavy potion usage indicates insufficient healing sources. Consider improving rest effectiveness or HP regeneration.");
            }

            var goldEfficiency = allRuns.Average(r => r.GetGoldEfficiency());
            if (goldEfficiency > 90)
            {
                recommendations.Add("Players spending most of their gold - economy may be too tight. Consider increasing gold rewards.");
            }
            else if (goldEfficiency < 30)
            {
                recommendations.Add("Players not spending gold - shop items may not be valuable enough or prices too high.");
            }

            // Add unique improvement suggestions from runs
            var uniqueSuggestions = allRuns.SelectMany(r => r.ImprovementSuggestions).Distinct().Take(5);
            recommendations.AddRange(uniqueSuggestions);

            if (recommendations.Any())
            {
                int count = 1;
                foreach (var rec in recommendations)
                {
                    report.AppendLine($"{count}. {rec}");
                    count++;
                }
            }
            else
            {
                report.AppendLine("No specific recommendations at this time. Game balance appears good!");
            }

            report.AppendLine();
        }
    }
}
