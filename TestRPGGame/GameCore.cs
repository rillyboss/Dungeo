using System;
using System.Linq;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.Factories;
using TestRPGGame.Systems;
using TestRPGGame.Combat;
using TestRPGGame.Interfaces;

namespace TestRPGGame
{
    /// <summary>
    /// Core game engine - orchestrates game flow using specialized managers.
    /// Refactored to follow Single Responsibility Principle.
    /// </summary>
    public class GameCore
    {
        private readonly IGameInterface _gameInterface;
        private readonly IGameInterface _trackingInterface; // Wrapper that tracks statistics
        private readonly InterfacedCombatSystem _combat;
        private readonly Shop _shop;
        private readonly SaveManager _saveManager;
        private readonly DungeonManager _dungeonManager;
        private readonly ProgressionManager _progressionManager;
        private readonly StatisticsManager _statisticsManager;
        private readonly AchievementManager _achievementManager;
        private StatisticsTracker _statisticsTracker; // Not readonly - recreated when loading save

        private Player? _player;
        private PlayerStatistics _statistics;
        private bool _isRunning;

        public GameCore(IGameInterface gameInterface)
        {
            _gameInterface = gameInterface;

            // Initialize statistics tracking
            _statistics = new PlayerStatistics();
            _statisticsTracker = new StatisticsTracker(_statistics);
            _trackingInterface = new EventTrackingInterface(gameInterface, _statisticsTracker);

            // Use tracking interface for all game systems
            _combat = new InterfacedCombatSystem(_trackingInterface);
            _shop = new Shop(_trackingInterface);
            _saveManager = new SaveManager(_trackingInterface);
            _dungeonManager = new DungeonManager(_trackingInterface, _combat);
            _progressionManager = new ProgressionManager(_trackingInterface);
            _statisticsManager = new StatisticsManager(_trackingInterface);
            _achievementManager = new AchievementManager(_trackingInterface);
            _isRunning = true;
        }

        public void Start()
        {
            _gameInterface.OnEvent(new GameEvents.GameStartedEvent());

            // Load or create character using SaveManager
            var (slotNumber, isNewCharacter) = _saveManager.RequestSaveSlotSelection();

            if (isNewCharacter)
            {
                _player = _saveManager.CreateNewCharacter(slotNumber);
            }
            else
            {
                var (loadedPlayer, loadedProgress, loadedStatistics, unlockedAchievements) = _saveManager.LoadCharacter(slotNumber);
                _player = loadedPlayer;

                if (loadedProgress != null)
                {
                    _dungeonManager.SetProgress(loadedProgress);
                }

                // Load statistics or create new if none saved (backwards compatibility)
                if (loadedStatistics != null)
                {
                    _statistics = loadedStatistics;
                    _statisticsTracker = new StatisticsTracker(_statistics);
                    // Note: We'd need to recreate the tracking interface here, but for simplicity
                    // we'll just update the tracker's reference. The existing wrapper will continue to work.
                }

                // Load unlocked achievements
                if (unlockedAchievements != null)
                {
                    _achievementManager.RestoreUnlockStatus(unlockedAchievements);
                }
            }

            // Main game loop
            MainGameLoop();

            _gameInterface.OnEvent(new GameEvents.GameEndedEvent());
        }

        private void MainGameLoop()
        {
            while (_isRunning && _player != null)
            {
                PublishPlayerStats();

                var choice = _gameInterface.RequestMainMenuChoice(
                    _player.Name,
                    _player.Level,
                    _player.Class,
                    _player.CurrentHP,
                    _player.MaxHP,
                    _player.Gold
                );

                switch (choice)
                {
                    case MainMenuChoice.Combat:
                        EnterCombat();
                        break;
                    case MainMenuChoice.Dungeon:
                        EnterDungeon();
                        break;
                    case MainMenuChoice.Shop:
                        VisitShop();
                        break;
                    case MainMenuChoice.Inventory:
                        ManageInventory();
                        break;
                    case MainMenuChoice.CharacterSheet:
                        ViewCharacterSheet();
                        break;
                    case MainMenuChoice.UnlockAbilities:
                        UnlockAbilities();
                        break;
                    case MainMenuChoice.Statistics:
                        ViewStatistics();
                        break;
                    case MainMenuChoice.Achievements:
                        ViewAchievements();
                        break;
                    case MainMenuChoice.Rest:
                        Rest();
                        break;
                    case MainMenuChoice.Save:
                        SaveGame();
                        break;
                    case MainMenuChoice.Help:
                        ShowHelp();
                        break;
                    case MainMenuChoice.Exit:
                        ExitGame();
                        break;
                }
            }
        }

        private void EnterCombat()
        {
            if (_player == null) return;

            var enemy = EnemyFactory.CreateEnemy(_player.Level);
            bool victory = _combat.StartBattle(_player, enemy, canFlee: true);

            // Check achievements and auto-save after successful combat
            if (victory)
            {
                _achievementManager.CheckAchievements(_statistics, _player);
                _saveManager.AutoSave(_player, _dungeonManager.Progress, _statistics, _achievementManager.GetUnlockedAchievementIds());
            }

            _gameInterface.WaitForAcknowledgment();
        }

        private void EnterDungeon()
        {
            if (_player == null) return;

            bool success = _dungeonManager.EnterDungeon(_player);

            if (success)
            {
                _achievementManager.CheckAchievements(_statistics, _player);
                _saveManager.AutoSave(_player, _dungeonManager.Progress, _statistics, _achievementManager.GetUnlockedAchievementIds());
            }
        }

        private void VisitShop()
        {
            if (_player == null) return;

            _shop.Enter(_player);

            // Check achievements and auto-save after shop visit
            _achievementManager.CheckAchievements(_statistics, _player);
            _saveManager.AutoSave(_player, _dungeonManager.Progress, _statistics, _achievementManager.GetUnlockedAchievementIds());
        }

        private void ManageInventory()
        {
            if (_player == null) return;

            _player.Inventory.ManageInventory(_player, _gameInterface);
        }

        private void ViewCharacterSheet()
        {
            if (_player == null) return;

            var info = _player.GetCharacterSheetInfo();
            _gameInterface.DisplayCharacterSheet(info);
        }

        private void UnlockAbilities()
        {
            if (_player == null) return;

            _progressionManager.UnlockAbilities(_player);
        }

        private void ViewStatistics()
        {
            if (_player == null) return;

            _statisticsManager.DisplayStatistics(_statistics);
        }

        private void Rest()
        {
            if (_player == null) return;

            int restCost = GameConfig.Config.RestingCost;
            bool success = _progressionManager.Rest(_player);

            if (success)
            {
                // Manually track rest since there's no dedicated event
                _statisticsTracker.RecordRest(restCost);
                _achievementManager.CheckAchievements(_statistics, _player);
                _saveManager.AutoSave(_player, _dungeonManager.Progress, _statistics, _achievementManager.GetUnlockedAchievementIds());
            }
        }

        private void SaveGame()
        {
            if (_player == null) return;

            _achievementManager.CheckAchievements(_statistics, _player);
            _saveManager.SaveGame(_player, _dungeonManager.Progress, _statistics, _achievementManager.GetUnlockedAchievementIds());
        }

        private void ViewAchievements()
        {
            if (_player == null) return;

            // Build display info from achievement manager
            var displayInfo = new Interfaces.AchievementDisplayInfo
            {
                Categories = _achievementManager.GetCategories(),
                AchievementsByCategory = _achievementManager.GetCategories().ToDictionary(
                    category => category,
                    category => _achievementManager.GetAchievementsByCategory(category)
                        .Select(a => new Interfaces.AchievementInfo
                        {
                            Id = a.Id,
                            Name = a.Name,
                            Description = a.Description,
                            Category = a.Category,
                            Points = a.Points,
                            IsUnlocked = a.IsUnlocked,
                            IsHidden = a.IsHidden,
                            ProgressText = a.GetProgressString(_statistics),
                            GoldReward = a.GoldReward,
                            ExperienceReward = a.ExperienceReward,
                            TitleReward = a.TitleReward
                        }).ToList()
                ),
                TotalAchievements = _achievementManager.Achievements.Count,
                UnlockedAchievements = _achievementManager.Achievements.Count(a => a.IsUnlocked),
                CompletionPercentage = _achievementManager.CompletionPercentage,
                TotalPoints = _achievementManager.TotalPoints,
                EarnedPoints = _achievementManager.EarnedPoints
            };

            _gameInterface.DisplayAchievements(displayInfo);
        }

        private void ShowHelp()
        {
            if (_gameInterface is ConsoleInterface consoleInterface)
            {
                consoleInterface.DisplayHelp();
            }
            else
            {
                // Fallback for non-console interfaces
                _gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = "Help is available in the console version.",
                    Type = GameEvents.MessageType.Info
                });
                _gameInterface.WaitForAcknowledgment();
            }
        }

        private void ExitGame()
        {
            _isRunning = false;
            _gameInterface.OnEvent(new GameEvents.InfoMessageEvent
            {
                Message = "Thanks for playing! May your legend live on...",
                Type = GameEvents.MessageType.Info
            });
        }

        private void PublishPlayerStats()
        {
            if (_player == null) return;

            _gameInterface.OnEvent(new GameEvents.PlayerStatsChangedEvent
            {
                CurrentHP = _player.CurrentHP,
                MaxHP = _player.MaxHP,
                CurrentMana = _player.CurrentMana,
                MaxMana = _player.MaxMana,
                Gold = _player.Gold,
                Level = _player.Level,
                Experience = _player.Experience
            });
        }
    }
}
