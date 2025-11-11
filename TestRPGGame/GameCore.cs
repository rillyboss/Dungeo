using System;
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
        private readonly InterfacedCombatSystem _combat;
        private readonly Shop _shop;
        private readonly SaveManager _saveManager;
        private readonly DungeonManager _dungeonManager;
        private readonly ProgressionManager _progressionManager;

        private Player? _player;
        private bool _isRunning;

        public GameCore(IGameInterface gameInterface)
        {
            _gameInterface = gameInterface;
            _combat = new InterfacedCombatSystem(gameInterface);
            _shop = new Shop(gameInterface);
            _saveManager = new SaveManager(gameInterface);
            _dungeonManager = new DungeonManager(gameInterface, _combat);
            _progressionManager = new ProgressionManager(gameInterface);
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
                var (loadedPlayer, loadedProgress) = _saveManager.LoadCharacter(slotNumber);
                _player = loadedPlayer;

                if (loadedProgress != null)
                {
                    _dungeonManager.SetProgress(loadedProgress);
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

            // Auto-save after successful combat
            if (victory)
            {
                _saveManager.AutoSave(_player, _dungeonManager.Progress);
            }

            _gameInterface.WaitForAcknowledgment();
        }

        private void EnterDungeon()
        {
            if (_player == null) return;

            bool success = _dungeonManager.EnterDungeon(_player);

            if (success)
            {
                _saveManager.AutoSave(_player, _dungeonManager.Progress);
            }
        }

        private void VisitShop()
        {
            if (_player == null) return;

            _shop.Enter(_player);

            // Auto-save after shop visit
            _saveManager.AutoSave(_player, _dungeonManager.Progress);
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

        private void Rest()
        {
            if (_player == null) return;

            bool success = _progressionManager.Rest(_player);

            if (success)
            {
                _saveManager.AutoSave(_player, _dungeonManager.Progress);
            }
        }

        private void SaveGame()
        {
            if (_player == null) return;

            _saveManager.SaveGame(_player, _dungeonManager.Progress);
        }

        private void ShowHelp()
        {
            _gameInterface.OnEvent(new GameEvents.InfoMessageEvent
            {
                Message = @"
=== GAME HELP ===

⚔️  COMBAT: Battle enemies to gain gold and experience
🏰 DUNGEONS: Complete dungeons for greater rewards
🏪 SHOP: Buy better equipment and items
📊 PROGRESSION: Level up to unlock new abilities
💡 TIP: Save your game often!
",
                Type = GameEvents.MessageType.Info
            });
            _gameInterface.WaitForAcknowledgment();
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
