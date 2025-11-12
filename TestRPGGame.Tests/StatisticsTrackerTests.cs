using System;
using Xunit;
using TestRPGGame.Entities.Player;
using TestRPGGame.Systems;
using TestRPGGame.Interfaces;
using TestRPGGame.Equipment;

namespace TestRPGGame.Tests
{
    public class StatisticsTrackerTests : TestBase
    {
        [Fact]
        public void StatisticsTracker_InitializesWithEmptyStatistics()
        {
            // Arrange & Act
            var stats = new PlayerStatistics();
            var tracker = new StatisticsTracker(stats);

            // Assert
            Assert.NotNull(tracker.Statistics);
            Assert.Equal(0, stats.TotalKills);
            Assert.Equal(0, stats.TotalDeaths);
            Assert.Equal(0, stats.TotalGoldEarned);
        }

        [Fact]
        public void TrackEvent_CombatStarted_InitializesCombatTracking()
        {
            // Arrange
            var stats = new PlayerStatistics();
            var tracker = new StatisticsTracker(stats);
            var evt = new GameEvents.CombatStartedEvent { EnemyName = "Goblin" };

            // Act
            tracker.TrackEvent(evt);

            // Assert - No immediate stat changes, but combat tracking initialized
            Assert.Equal(0, stats.TotalKills); // Not killed yet
        }

        [Fact]
        public void TrackEvent_CombatVictory_RecordsKill()
        {
            // Arrange
            var stats = new PlayerStatistics();
            var tracker = new StatisticsTracker(stats);

            tracker.TrackEvent(new GameEvents.CharacterCreatedEvent { Name = "Hero" });
            tracker.TrackEvent(new GameEvents.CombatStartedEvent { EnemyName = "Goblin" });

            // Act
            tracker.TrackEvent(new GameEvents.CombatEndedEvent
            {
                PlayerVictory = true,
                GoldEarned = 50,
                ExperienceEarned = 100
            });

            // Assert
            Assert.Equal(1, stats.TotalKills);
            Assert.Equal(1, stats.CombatsWon);
            Assert.Equal(50, stats.TotalGoldEarned);
            Assert.Equal(100, stats.TotalExperienceGained);
            Assert.Equal(1, stats.KillsByEnemyType["Goblin"]);
        }

        [Fact]
        public void TrackEvent_CombatDefeat_RecordsDeath()
        {
            // Arrange
            var stats = new PlayerStatistics();
            var tracker = new StatisticsTracker(stats);

            tracker.TrackEvent(new GameEvents.CombatStartedEvent { EnemyName = "Dragon" });

            // Act
            tracker.TrackEvent(new GameEvents.CombatEndedEvent
            {
                PlayerVictory = false,
                GoldLost = 100
            });

            // Assert
            Assert.Equal(1, stats.TotalDeaths);
            Assert.Equal(0, stats.CombatsWon);
            Assert.Equal(100, stats.TotalGoldSpent); // Lost gold counts as spent
        }

        [Fact]
        public void TrackEvent_DamageDealt_RecordsDamageAndCrits()
        {
            // Arrange
            var stats = new PlayerStatistics();
            var tracker = new StatisticsTracker(stats);

            tracker.TrackEvent(new GameEvents.CharacterCreatedEvent { Name = "Hero" });
            tracker.TrackEvent(new GameEvents.CombatStartedEvent { EnemyName = "Orc" });

            // Act
            tracker.TrackEvent(new GameEvents.DamageDealtEvent
            {
                Attacker = "Hero",
                Target = "Orc",
                Damage = 50,
                IsCritical = true
            });

            tracker.TrackEvent(new GameEvents.DamageDealtEvent
            {
                Attacker = "Hero",
                Target = "Orc",
                Damage = 30,
                IsCritical = false
            });

            // Assert
            Assert.Equal(80, stats.TotalDamageDealt);
            Assert.Equal(1, stats.CriticalHitsDealt);
            Assert.Equal(50, stats.HighestDamageInOneTurn);
        }

        [Fact]
        public void TrackEvent_DamageTaken_RecordsDamage()
        {
            // Arrange
            var stats = new PlayerStatistics();
            var tracker = new StatisticsTracker(stats);

            tracker.TrackEvent(new GameEvents.CharacterCreatedEvent { Name = "Hero" });
            tracker.TrackEvent(new GameEvents.CombatStartedEvent { EnemyName = "Orc" });

            // Act
            tracker.TrackEvent(new GameEvents.DamageDealtEvent
            {
                Attacker = "Orc",
                Target = "Hero",
                Damage = 25,
                IsCritical = false
            });

            // Assert
            Assert.Equal(25, stats.TotalDamageTaken);
        }

        [Fact]
        public void TrackEvent_AbilityUsed_RecordsAbilityAndMana()
        {
            // Arrange
            var stats = new PlayerStatistics();
            var tracker = new StatisticsTracker(stats);

            tracker.TrackEvent(new GameEvents.CharacterCreatedEvent { Name = "Hero" });

            // Act
            tracker.TrackEvent(new GameEvents.AbilityUsedEvent
            {
                User = "Hero",
                AbilityName = "Fireball",
                ManaCost = 20
            });

            tracker.TrackEvent(new GameEvents.AbilityUsedEvent
            {
                User = "Hero",
                AbilityName = "Fireball",
                ManaCost = 20
            });

            // Assert
            Assert.Equal(2, stats.TotalAbilitiesUsed);
            Assert.Equal(40, stats.TotalManaSpent);
            Assert.Equal(2, stats.UsageByAbility["Fireball"]);
        }

        [Fact]
        public void TrackEvent_PotionUsed_RecordsPotionAndHealing()
        {
            // Arrange
            var stats = new PlayerStatistics();
            var tracker = new StatisticsTracker(stats);

            // Act
            tracker.TrackEvent(new GameEvents.PotionUsedEvent
            {
                HPRestored = 50,
                PotionsRemaining = 2
            });

            // Assert
            Assert.Equal(1, stats.PotionsUsed);
            Assert.Equal(50, stats.TotalHealingDone);
        }

        [Fact]
        public void TrackEvent_ItemPurchased_RecordsGoldSpent()
        {
            // Arrange
            var stats = new PlayerStatistics();
            var tracker = new StatisticsTracker(stats);

            // Act
            tracker.TrackEvent(new GameEvents.ItemPurchasedEvent
            {
                ItemName = "Iron Sword",
                Price = 100,
                GoldRemaining = 900
            });

            // Assert
            Assert.Equal(1, stats.ItemsBought);
            Assert.Equal(100, stats.TotalGoldSpent);
        }

        [Fact]
        public void TrackEvent_ItemSold_RecordsGoldEarned()
        {
            // Arrange
            var stats = new PlayerStatistics();
            var tracker = new StatisticsTracker(stats);

            // Act
            tracker.TrackEvent(new GameEvents.ItemSoldEvent
            {
                ItemName = "Old Sword",
                Price = 25,
                GoldRemaining = 1025
            });

            // Assert
            Assert.Equal(1, stats.ItemsSold);
            Assert.Equal(25, stats.TotalGoldEarned);
        }

        [Fact]
        public void TrackEvent_DungeonCompleted_RecordsCompletion()
        {
            // Arrange
            var stats = new PlayerStatistics();
            var tracker = new StatisticsTracker(stats);

            // Act
            tracker.TrackEvent(new GameEvents.DungeonEnteredEvent
            {
                DungeonName = "Goblin Caves",
                Difficulty = 1
            });

            tracker.TrackEvent(new GameEvents.DungeonCompletedEvent
            {
                DungeonName = "Goblin Caves",
                Success = true,
                TotalGoldEarned = 200,
                TotalExperienceEarned = 500
            });

            // Assert
            Assert.Equal(1, stats.TotalDungeonsCompleted);
            Assert.Equal(1, stats.DungeonAttempts);
            Assert.Equal(0, stats.DungeonFailures);
            Assert.Equal(200, stats.TotalGoldEarned);
            Assert.Equal(500, stats.TotalExperienceGained);
            Assert.Equal(1, stats.CompletionsByDungeon["Goblin Caves"]);
        }

        [Fact]
        public void TrackEvent_DungeonFailed_RecordsFailure()
        {
            // Arrange
            var stats = new PlayerStatistics();
            var tracker = new StatisticsTracker(stats);

            tracker.TrackEvent(new GameEvents.DungeonEnteredEvent { DungeonName = "Dragon's Lair" });

            // Act
            tracker.TrackEvent(new GameEvents.DungeonCompletedEvent
            {
                DungeonName = "Dragon's Lair",
                Success = false
            });

            // Assert
            Assert.Equal(0, stats.TotalDungeonsCompleted);
            Assert.Equal(1, stats.DungeonAttempts);
            Assert.Equal(1, stats.DungeonFailures);
        }

        [Fact]
        public void TrackEvent_LegendaryItem_RecordsFind()
        {
            // Arrange
            var stats = new PlayerStatistics();
            var tracker = new StatisticsTracker(stats);

            tracker.TrackEvent(new GameEvents.CombatStartedEvent { EnemyName = "Boss" });

            // Act
            tracker.TrackEvent(new GameEvents.CombatEndedEvent
            {
                PlayerVictory = true,
                LootDropped = new EquipmentItem
                {
                    Name = "Excalibur",
                    Rarity = ItemRarity.Legendary
                }
            });

            // Assert
            Assert.Equal(1, stats.LegendaryItemsFound);
            Assert.Equal(0, stats.EpicItemsFound);
        }

        [Fact]
        public void TrackEvent_LevelUp_RecordsLevel()
        {
            // Arrange
            var stats = new PlayerStatistics();
            var tracker = new StatisticsTracker(stats);

            // Act
            tracker.TrackEvent(new GameEvents.PlayerLeveledUpEvent
            {
                NewLevel = 2,
                NewMaxHP = 150,
                NewMaxMana = 100,
                NewAttack = 20,
                NewDefense = 15
            });

            // Assert
            Assert.Equal(1, stats.TotalLevelsGained);
            Assert.Equal(2, stats.HighestLevelReached);
        }

        [Fact]
        public void TrackEvent_GameSaved_RecordsSave()
        {
            // Arrange
            var stats = new PlayerStatistics();
            var tracker = new StatisticsTracker(stats);

            // Act
            tracker.TrackEvent(new GameEvents.GameSavedEvent
            {
                SlotNumber = 1,
                Success = true
            });

            // Assert
            Assert.Equal(1, stats.GameSaves);
        }

        [Fact]
        public void TrackEvent_HighestGold_TracksCorrectly()
        {
            // Arrange
            var stats = new PlayerStatistics();
            var tracker = new StatisticsTracker(stats);

            // Act
            tracker.TrackEvent(new GameEvents.PlayerStatsChangedEvent { Gold = 100 });
            tracker.TrackEvent(new GameEvents.PlayerStatsChangedEvent { Gold = 500 });
            tracker.TrackEvent(new GameEvents.PlayerStatsChangedEvent { Gold = 250 }); // Went down

            // Assert
            Assert.Equal(500, stats.MostGoldAtOnce);
        }

        [Fact]
        public void RecordRest_TracksRestAndGold()
        {
            // Arrange
            var stats = new PlayerStatistics();
            var tracker = new StatisticsTracker(stats);

            // Act
            tracker.RecordRest(50);
            tracker.RecordRest(50);

            // Assert
            Assert.Equal(2, stats.TimesRested);
            Assert.Equal(100, stats.TotalGoldSpent);
        }

        [Fact]
        public void RecordBossDefeat_TracksBoss()
        {
            // Arrange
            var stats = new PlayerStatistics();
            var tracker = new StatisticsTracker(stats);

            // Act
            tracker.RecordBossDefeat("Dragon King");

            // Assert
            Assert.Equal(1, stats.BossesDefeated);
            Assert.Equal(1, stats.TotalKills);
            Assert.Equal(1, stats.KillsByEnemyType["Dragon King"]);
        }

        [Fact]
        public void TrackEvent_LongestCombat_TracksCorrectly()
        {
            // Arrange
            var stats = new PlayerStatistics();
            var tracker = new StatisticsTracker(stats);

            tracker.TrackEvent(new GameEvents.CombatStartedEvent { EnemyName = "Enemy1" });
            tracker.TrackEvent(new GameEvents.CombatTurnStartEvent { TurnNumber = 1 });
            tracker.TrackEvent(new GameEvents.CombatTurnStartEvent { TurnNumber = 2 });
            tracker.TrackEvent(new GameEvents.CombatTurnStartEvent { TurnNumber = 3 });
            tracker.TrackEvent(new GameEvents.CombatEndedEvent { PlayerVictory = true });

            tracker.TrackEvent(new GameEvents.CombatStartedEvent { EnemyName = "Enemy2" });
            tracker.TrackEvent(new GameEvents.CombatTurnStartEvent { TurnNumber = 1 });
            tracker.TrackEvent(new GameEvents.CombatEndedEvent { PlayerVictory = true });

            // Assert
            Assert.Equal(4, stats.TotalTurnsInCombat); // 3 + 1
            Assert.Equal(3, stats.LongestCombat);
        }
    }
}
