using System;
using TestRPGGame.Entities.Player;
using TestRPGGame.Interfaces;
using TestRPGGame.Equipment;

namespace TestRPGGame.Systems
{
    /// <summary>
    /// Tracks player statistics from game events.
    /// Subscribe to game events via IGameInterface.OnEvent and update statistics.
    /// Simpler approach: This wraps IGameInterface and intercepts events without being a full decorator.
    /// </summary>
    public class StatisticsTracker
    {
        private readonly PlayerStatistics _statistics;
        private string? _currentPlayerName;
        private string? _currentEnemyName;
        private bool _isInCombat;
        private int _currentCombatTurns;

        public StatisticsTracker(PlayerStatistics statistics)
        {
            _statistics = statistics;
            _isInCombat = false;
            _currentCombatTurns = 0;
        }

        /// <summary>
        /// Gets the statistics being tracked.
        /// </summary>
        public PlayerStatistics Statistics => _statistics;

        /// <summary>
        /// Track statistics from a game event.
        /// Call this whenever an event occurs.
        /// </summary>
        public void TrackEvent<T>(T gameEvent) where T : class
        {
            // Track statistics based on event type
            switch (gameEvent)
            {
                case GameEvents.CombatStartedEvent e:
                    _isInCombat = true;
                    _currentCombatTurns = 0;
                    _currentEnemyName = e.EnemyName;
                    break;

                case GameEvents.CombatTurnStartEvent e:
                    if (_isInCombat)
                    {
                        _currentCombatTurns++;
                        _statistics.TotalTurnsInCombat++;
                    }
                    break;

                case GameEvents.DamageDealtEvent e:
                    // Track damage dealt by player
                    if (e.Attacker == _currentPlayerName || e.Target == _currentEnemyName)
                    {
                        _statistics.RecordDamageDealt(e.Damage, e.IsCritical);
                    }
                    // Track damage taken by player
                    if (e.Target == _currentPlayerName)
                    {
                        _statistics.TotalDamageTaken += e.Damage;
                    }
                    break;

                case GameEvents.AttackMissedEvent e:
                    if (e.Attacker == _currentPlayerName)
                    {
                        _statistics.AttacksMissed++;
                    }
                    if (e.Target == _currentPlayerName && e.MissType == "Dodge")
                    {
                        _statistics.AttacksDodged++;
                    }
                    break;

                case GameEvents.AbilityUsedEvent e:
                    if (e.User == _currentPlayerName)
                    {
                        _statistics.RecordAbilityUse(e.AbilityName, e.ManaCost);
                    }
                    break;

                case GameEvents.EffectAppliedEvent e:
                    if (e.Target == _currentPlayerName)
                    {
                        _statistics.StatusEffectsReceived++;
                    }
                    else if (e.Target == _currentEnemyName)
                    {
                        _statistics.StatusEffectsApplied++;
                    }
                    break;

                case GameEvents.CombatEndedEvent e:
                    if (_isInCombat)
                    {
                        if (e.PlayerVictory)
                        {
                            _statistics.CombatsWon++;
                            if (_currentEnemyName != null)
                            {
                                _statistics.RecordKill(_currentEnemyName);
                            }
                            if (e.GoldEarned > 0)
                            {
                                _statistics.TotalGoldEarned += e.GoldEarned;
                            }
                            if (e.ExperienceEarned > 0)
                            {
                                _statistics.TotalExperienceGained += e.ExperienceEarned;
                            }

                            // Track legendary/epic items
                            if (e.LootDropped != null)
                            {
                                if (e.LootDropped.Rarity == ItemRarity.Legendary)
                                {
                                    _statistics.LegendaryItemsFound++;
                                }
                                else if (e.LootDropped.Rarity == ItemRarity.Epic)
                                {
                                    _statistics.EpicItemsFound++;
                                }
                            }
                        }
                        else
                        {
                            _statistics.TotalDeaths++;
                            if (e.GoldLost > 0)
                            {
                                _statistics.TotalGoldSpent += e.GoldLost; // Lost gold counts as "spent"
                            }
                        }

                        // Track longest combat
                        if (_currentCombatTurns > _statistics.LongestCombat)
                        {
                            _statistics.LongestCombat = _currentCombatTurns;
                        }

                        _isInCombat = false;
                        _currentCombatTurns = 0;
                        _currentEnemyName = null;
                    }
                    break;

                case GameEvents.PotionUsedEvent e:
                    _statistics.PotionsUsed++;
                    _statistics.TotalHealingDone += e.HPRestored;
                    break;

                case GameEvents.ItemPurchasedEvent e:
                    _statistics.ItemsBought++;
                    _statistics.TotalGoldSpent += e.Price;
                    break;

                case GameEvents.ItemSoldEvent e:
                    _statistics.ItemsSold++;
                    _statistics.TotalGoldEarned += e.Price;
                    break;

                case GameEvents.ItemEquippedEvent e:
                    _statistics.EquipmentUpgrades++;
                    break;

                case GameEvents.DungeonEnteredEvent e:
                    _statistics.DungeonAttempts++;
                    break;

                case GameEvents.DungeonCompletedEvent e:
                    if (e.Success)
                    {
                        _statistics.RecordDungeonCompletion(e.DungeonName);
                        _statistics.TotalGoldEarned += e.TotalGoldEarned;
                        _statistics.TotalExperienceGained += e.TotalExperienceEarned;
                    }
                    else
                    {
                        _statistics.DungeonFailures++;
                    }
                    break;

                case GameEvents.PlayerLeveledUpEvent e:
                    _statistics.TotalLevelsGained++;
                    if (e.NewLevel > _statistics.HighestLevelReached)
                    {
                        _statistics.HighestLevelReached = e.NewLevel;
                    }
                    break;

                case GameEvents.AbilityUnlockedEvent e:
                    _statistics.AbilitiesUnlocked++;
                    _statistics.TotalGoldSpent += e.Cost;
                    break;

                case GameEvents.GameSavedEvent e:
                    if (e.Success)
                    {
                        _statistics.GameSaves++;
                    }
                    break;

                case GameEvents.CharacterCreatedEvent e:
                    _currentPlayerName = e.Name;
                    break;

                case GameEvents.GameLoadedEvent e:
                    _currentPlayerName = e.PlayerName;
                    break;

                case GameEvents.PlayerStatsChangedEvent e:
                    // Track highest gold
                    if (e.Gold > _statistics.MostGoldAtOnce)
                    {
                        _statistics.MostGoldAtOnce = e.Gold;
                    }
                    break;
            }
        }

        /// <summary>
        /// Records that the player rested at an inn.
        /// Called manually since there's no RestEvent.
        /// </summary>
        public void RecordRest(int goldSpent)
        {
            _statistics.TimesRested++;
            _statistics.TotalGoldSpent += goldSpent;
        }

        /// <summary>
        /// Records a shop refresh.
        /// Called manually since shop refresh doesn't have a dedicated event.
        /// </summary>
        public void RecordShopRefresh(int goldSpent)
        {
            _statistics.ShopRefreshes++;
            _statistics.TotalGoldSpent += goldSpent;
        }

        /// <summary>
        /// Records healing done (for healing abilities that don't go through potion system).
        /// </summary>
        public void RecordHealing(int amount)
        {
            _statistics.TotalHealingDone += amount;
        }

        /// <summary>
        /// Records a boss defeat specifically.
        /// </summary>
        public void RecordBossDefeat(string bossName)
        {
            _statistics.BossesDefeated++;
            _statistics.RecordKill(bossName);
        }

        /// <summary>
        /// Records fleeing from combat.
        /// </summary>
        public void RecordFlee()
        {
            _statistics.CombatsFled++;
        }
    }
}
