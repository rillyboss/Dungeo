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
            report.AppendLine("║                 AUTOMATED GAMEPLAY ANALYSIS REPORT                            ║");
            report.AppendLine($"║                   {allRuns.Count} Complete Playthroughs to Level 15                         ║");
            report.AppendLine("╚═══════════════════════════════════════════════════════════════════════════════╝");
            report.AppendLine();

            // Executive Summary
            report.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
            report.AppendLine("  EXECUTIVE SUMMARY");
            report.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
            report.AppendLine();

            var totalCombats = allRuns.Sum(r => r.TotalCombats);
            var avgCombats = allRuns.Average(r => r.TotalCombats);
            var avgWinRate = allRuns.Average(r => r.GetWinRate());

            report.AppendLine($"Total Runs Completed:          {allRuns.Count}");
            report.AppendLine($"Total Combats Across Runs:     {totalCombats}");
            report.AppendLine($"Average Combats to Level 15:   {avgCombats:F1}");
            report.AppendLine($"Average Win Rate:              {avgWinRate:F1}%");
            report.AppendLine($"Total Dungeon Attempts:        {allRuns.Sum(r => r.DungeonAttempts)}");
            report.AppendLine($"Dungeon Success Rate:          {allRuns.Average(r => r.GetDungeonSuccessRate()):F1}%");
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

            var avgWinRate = runs.Average(r => r.GetWinRate());
            var avgDungeonSuccess = runs.Average(r => r.GetDungeonSuccessRate());
            var avgCombats = (int)runs.Average(r => r.TotalCombats);

            report.AppendLine($"  Average Combats to Level 15: {avgCombats}");
            report.AppendLine($"  Combat Win Rate:             {avgWinRate:F1}%");
            report.AppendLine($"  Dungeon Success Rate:        {avgDungeonSuccess:F1}%");
            report.AppendLine($"  Average Gold Earned:         {runs.Average(r => r.TotalGoldEarned):F0}");
            report.AppendLine($"  Average Gold Spent:          {runs.Average(r => r.TotalGoldSpent):F0}");
            report.AppendLine($"  Average Potions Used:        {runs.Average(r => r.PotionsUsed):F1}");
            report.AppendLine($"  Average Times Rested:        {runs.Average(r => r.TimesRested):F1}");
            report.AppendLine($"  Average Near-Death Moments:  {runs.Average(r => r.TimesNearDeath):F1}");
            report.AppendLine($"  Average Abilities Unlocked:  {runs.Average(r => r.AbilitiesUnlocked):F1}");
            report.AppendLine();

            // Combats per level analysis
            var allLevels = runs.SelectMany(r => r.CombatsPerLevel.Keys).Distinct().OrderBy(l => l).ToList();
            if (allLevels.Any())
            {
                report.AppendLine($"  Average Combats Per Level:");
                foreach (var level in allLevels.Take(15))
                {
                    var avgCombatsAtLevel = runs
                        .Where(r => r.CombatsPerLevel.ContainsKey(level))
                        .Average(r => r.CombatsPerLevel[level]);
                    report.AppendLine($"    • Level {level}: {avgCombatsAtLevel:F1} combats");
                }
                report.AppendLine();
            }

            // Dungeons per level analysis
            var dungeonLevels = runs.SelectMany(r => r.DungeonsPerLevel.Keys).Distinct().OrderBy(l => l).ToList();
            if (dungeonLevels.Any())
            {
                report.AppendLine($"  Average Dungeon Attempts Per Level:");
                foreach (var level in dungeonLevels.Take(15))
                {
                    var avgDungeonsAtLevel = runs
                        .Where(r => r.DungeonsPerLevel.ContainsKey(level))
                        .Average(r => r.DungeonsPerLevel[level]);
                    report.AppendLine($"    • Level {level}: {avgDungeonsAtLevel:F1} attempts");
                }
                report.AppendLine();
            }

            // Damage taken per level analysis
            var damageLevels = runs.SelectMany(r => r.DamageTakenPerLevel.Keys).Distinct().OrderBy(l => l).ToList();
            if (damageLevels.Any())
            {
                report.AppendLine($"  Average Damage Taken Per Combat By Level:");
                foreach (var level in damageLevels.Take(15))
                {
                    var allDamageAtLevel = runs
                        .Where(r => r.DamageTakenPerLevel.ContainsKey(level))
                        .SelectMany(r => r.DamageTakenPerLevel[level])
                        .ToList();

                    if (allDamageAtLevel.Any())
                    {
                        var avgDamage = allDamageAtLevel.Average();
                        report.AppendLine($"    • Level {level}: {avgDamage:F1} damage/combat");
                    }
                }
                report.AppendLine();
            }

            // Equipment analysis at key levels
            var equipLevels = new[] { 5, 10, 15 };
            var hasEquipmentData = runs.Any(r => r.EquippedGearByLevel.Any());

            if (hasEquipmentData)
            {
                report.AppendLine($"  Equipment at Key Levels:");
                foreach (var level in equipLevels)
                {
                    var equipmentAtLevel = runs
                        .Where(r => r.EquippedGearByLevel.ContainsKey(level))
                        .SelectMany(r => r.EquippedGearByLevel[level].Values)
                        .Where(item => item != null)
                        .ToList();

                    if (equipmentAtLevel.Any())
                    {
                        var avgLevel = equipmentAtLevel.Average(i => i!.Level);
                        var avgAttack = equipmentAtLevel.Average(i => i!.AttackBonus);
                        var avgDefense = equipmentAtLevel.Average(i => i!.DefenseBonus);
                        var avgMagic = equipmentAtLevel.Average(i => i!.MagicBonus);

                        report.AppendLine($"    • Level {level}: Avg gear level {avgLevel:F1} (Atk+{avgAttack:F0}, Def+{avgDefense:F0}, Mag+{avgMagic:F0})");
                    }
                }
                report.AppendLine();
            }

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

            report.AppendLine($"  Total Damage Dealt:           {totalDamageDealt:N0}");
            report.AppendLine($"  Total Damage Taken:           {totalDamageTaken:N0}");
            report.AppendLine($"  Average Damage Dealt/Combat:  {avgDamagePerCombat:F1}");
            report.AppendLine($"  Average Damage Taken/Combat:  {allRuns.Average(r => r.GetAverageDamageTakenPerCombat()):F1}");
            report.AppendLine($"  Total Critical Hits:          {totalCrits}");
            report.AppendLine($"  Total Missed Attacks:         {totalMisses}");
            report.AppendLine();

            report.AppendLine($"  Combat Outcomes:");
            report.AppendLine($"    • Win Rate:    {allRuns.Average(r => r.GetWinRate()):F1}%");
            report.AppendLine($"    • Flee Rate:   {allRuns.Average(r => r.GetFleeRate()):F1}%");
            report.AppendLine($"    • Death Rate:  {allRuns.Average(r => r.GetDeathRate()):F1}%");
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

            // Calculate average combats needed to reach key levels
            var avgCombatsToLevel5 = allRuns
                .Select(r => r.CombatsPerLevel.Where(kvp => kvp.Key <= 5).Sum(kvp => kvp.Value))
                .Where(c => c > 0)
                .DefaultIfEmpty(0)
                .Average();

            var avgCombatsToLevel10 = allRuns
                .Select(r => r.CombatsPerLevel.Where(kvp => kvp.Key <= 10).Sum(kvp => kvp.Value))
                .Where(c => c > 0)
                .DefaultIfEmpty(0)
                .Average();

            var avgCombatsToLevel15 = allRuns
                .Select(r => r.CombatsPerLevel.Where(kvp => kvp.Key <= 15).Sum(kvp => kvp.Value))
                .Where(c => c > 0)
                .DefaultIfEmpty(0)
                .Average();

            report.AppendLine($"  Average Combats to Level 5:  {avgCombatsToLevel5:F1}");
            report.AppendLine($"  Average Combats to Level 10: {avgCombatsToLevel10:F1}");
            report.AppendLine($"  Average Combats to Level 15: {avgCombatsToLevel15:F1}");
            report.AppendLine();

            report.AppendLine($"  Average Combats Per Level Range:");
            var combats1to5 = avgCombatsToLevel5 / 5;
            var combats5to10 = (avgCombatsToLevel10 - avgCombatsToLevel5) / 5;
            var combats10to15 = (avgCombatsToLevel15 - avgCombatsToLevel10) / 5;

            report.AppendLine($"    • Levels 1-5:   {combats1to5:F1} combats/level");
            report.AppendLine($"    • Levels 5-10:  {combats5to10:F1} combats/level");
            report.AppendLine($"    • Levels 10-15: {combats10to15:F1} combats/level");
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

            report.AppendLine($"  Total Gold Earned (All Sources):  {avgGoldEarned:F0}");
            report.AppendLine();

            report.AppendLine($"  Gold Sources Breakdown:");
            report.AppendLine($"    • Starting Gold:       {allRuns.Average(r => r.GoldFromStarting):F0} ({allRuns.Average(r => r.GoldFromStarting / (double)r.TotalGoldEarned * 100):F1}%)");
            report.AppendLine($"    • From Combat:         {allRuns.Average(r => r.GoldFromCombat):F0} ({allRuns.Average(r => r.GoldFromCombat / (double)r.TotalGoldEarned * 100):F1}%)");
            report.AppendLine($"    • From Dungeons:       {allRuns.Average(r => r.GoldFromDungeons):F0} ({allRuns.Average(r => r.GoldFromDungeons / (double)r.TotalGoldEarned * 100):F1}%)");
            report.AppendLine($"    • From Achievements:   {allRuns.Average(r => r.GoldFromAchievements):F0} ({allRuns.Average(r => r.GoldFromAchievements / (double)r.TotalGoldEarned * 100):F1}%)");
            report.AppendLine($"    • From Shop Sales:     {allRuns.Average(r => r.GoldFromShopSales):F0} ({allRuns.Average(r => r.GoldFromShopSales / (double)r.TotalGoldEarned * 100):F1}%)");
            report.AppendLine();

            report.AppendLine($"  Total Gold Spent:              {avgGoldSpent:F0}");
            report.AppendLine();

            report.AppendLine($"  Gold Expenditures Breakdown:");
            report.AppendLine($"    • Weapons:             {allRuns.Average(r => r.GoldSpentOnWeapons):F0} ({allRuns.Average(r => r.GoldSpentOnWeapons / (double)r.TotalGoldSpent * 100):F1}%)");
            report.AppendLine($"    • Armor:               {allRuns.Average(r => r.GoldSpentOnArmor):F0} ({allRuns.Average(r => r.GoldSpentOnArmor / (double)r.TotalGoldSpent * 100):F1}%)");
            report.AppendLine($"    • Accessories:         {allRuns.Average(r => r.GoldSpentOnAccessories):F0} ({allRuns.Average(r => r.GoldSpentOnAccessories / (double)r.TotalGoldSpent * 100):F1}%)");
            report.AppendLine($"    • Potions:             {allRuns.Average(r => r.GoldSpentOnPotions):F0} ({allRuns.Average(r => r.GoldSpentOnPotions / (double)r.TotalGoldSpent * 100):F1}%)");
            report.AppendLine($"    • Rest/Inn:            {allRuns.Average(r => r.GoldSpentOnRest):F0} ({allRuns.Average(r => r.GoldSpentOnRest / (double)r.TotalGoldSpent * 100):F1}%)");
            report.AppendLine($"    • Abilities:           {allRuns.Average(r => r.GoldSpentOnAbilities):F0} ({allRuns.Average(r => r.GoldSpentOnAbilities / (double)r.TotalGoldSpent * 100):F1}%)");
            report.AppendLine();

            report.AppendLine($"  Final Gold Remaining:          {avgFinalGold:F0}");
            report.AppendLine($"  Shop Visits:                   {avgShopVisits:F1}");
            report.AppendLine($"  Items Purchased:               {avgItemsPurchased:F1}");
            report.AppendLine($"  Gold Efficiency (Spent/Earned): {allRuns.Average(r => r.GetGoldEfficiency()):F1}%");
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

            // Ability Store Visit Analysis
            var totalStoreVisits = allRuns.Sum(r => r.AbilityStoreVisits.Count);
            if (totalStoreVisits > 0)
            {
                var avgStoreVisits = allRuns.Average(r => r.AbilityStoreVisits.Count);
                var successfulVisits = allRuns.Sum(r => r.AbilityStoreVisits.Count(v => v.PurchasedAbility));
                var successRate = (double)successfulVisits / totalStoreVisits * 100;

                report.AppendLine($"  Ability Store Visits:");
                report.AppendLine($"    • Total Visits:             {totalStoreVisits}");
                report.AppendLine($"    • Average Visits Per Run:   {avgStoreVisits:F1}");
                report.AppendLine($"    • Successful Purchases:     {successfulVisits} ({successRate:F1}%)");
                report.AppendLine();

                // Analyze reasons for not purchasing
                var allReasons = allRuns.SelectMany(r => r.AbilityStoreVisits)
                    .Where(v => !v.PurchasedAbility)
                    .SelectMany(v => v.ReasonsNotPurchased)
                    .GroupBy(r => r.Contains("afford") ? "Insufficient Gold" :
                                  r.Contains("Level too low") ? "Level Too Low" :
                                  r.Contains("All abilities") ? "All Unlocked" :
                                  "Other")
                    .Select(g => new { Reason = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                if (allReasons.Any())
                {
                    report.AppendLine($"  Reasons for Not Purchasing:");
                    foreach (var reason in allReasons)
                    {
                        var percentage = (double)reason.Count / (totalStoreVisits - successfulVisits) * 100;
                        report.AppendLine($"    • {reason.Reason}: {reason.Count} times ({percentage:F1}%)");
                    }
                    report.AppendLine();
                }
            }

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

            // Equipment-Granted Abilities Analysis
            var totalItemsWithAbilities = allRuns.Sum(r => r.ItemsWithAbilitiesReceived);
            if (totalItemsWithAbilities > 0)
            {
                var avgItemsWithAbilities = allRuns.Average(r => r.ItemsWithAbilitiesReceived);
                var totalEquipped = allRuns.Sum(r => r.ItemsWithAbilitiesEquipped);
                var avgEquipped = allRuns.Average(r => r.ItemsWithAbilitiesEquipped);
                var equipRate = totalItemsWithAbilities > 0 ? (double)totalEquipped / totalItemsWithAbilities * 100 : 0;

                report.AppendLine($"  Equipment-Granted Abilities:");
                report.AppendLine($"    • Items with Abilities Received:  {totalItemsWithAbilities} ({avgItemsWithAbilities:F1} per run)");
                report.AppendLine($"    • Items with Abilities Equipped:  {totalEquipped} ({avgEquipped:F1} per run)");
                report.AppendLine($"    • Equip Rate:                      {equipRate:F1}%");
                report.AppendLine();

                if (equipRate < 50)
                {
                    report.AppendLine($"    ⚠️  Low equip rate suggests ability-granting items may not be competitive");
                    report.AppendLine();
                }
            }
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
