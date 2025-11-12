using System;
using System.Collections.Generic;
using System.Linq;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Dungeon;
using TestRPGGame.Interfaces;

namespace TestRPGGame.Systems
{
    /// <summary>
    /// Manages all save/load operations for the game.
    /// Extracted from GameCore to follow Single Responsibility Principle.
    /// </summary>
    public class SaveManager
    {
        private readonly IGameInterface _gameInterface;
        private int? _lastSaveSlot;

        public SaveManager(IGameInterface gameInterface)
        {
            _gameInterface = gameInterface;
            _lastSaveSlot = null;
        }

        /// <summary>
        /// Gets the last save slot number used, if any.
        /// </summary>
        public int? LastSaveSlot => _lastSaveSlot;

        /// <summary>
        /// Creates a new character based on user input.
        /// </summary>
        public Player CreateNewCharacter(int slotNumber)
        {
            _lastSaveSlot = slotNumber;

            var (name, playerClass) = _gameInterface.RequestCharacterCreation();
            var player = new Player(name, playerClass);

            _gameInterface.OnEvent(new GameEvents.CharacterCreatedEvent
            {
                Name = player.Name,
                Class = player.Class,
                MaxHP = player.MaxHP,
                MaxMana = player.MaxMana,
                Attack = player.Attack,
                Defense = player.Defense
            });

            return player;
        }

        /// <summary>
        /// Loads a character from the specified save slot.
        /// </summary>
        /// <returns>Tuple of (Player, DungeonProgress, PlayerStatistics, UnlockedAchievements) or nulls if load failed</returns>
        public (Player?, DungeonProgress?, PlayerStatistics?, HashSet<string>?) LoadCharacter(int slotNumber)
        {
            var (loadedPlayer, loadedProgress, loadedStatistics, unlockedAchievements) = SaveSystem.LoadGame(slotNumber);

            if (loadedPlayer != null)
            {
                _lastSaveSlot = slotNumber;

                _gameInterface.OnEvent(new GameEvents.GameLoadedEvent
                {
                    SlotNumber = slotNumber,
                    PlayerName = loadedPlayer.Name,
                    Level = loadedPlayer.Level,
                    Class = loadedPlayer.Class
                });

                _gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = "Game loaded successfully!",
                    Type = GameEvents.MessageType.Success
                });

                return (loadedPlayer, loadedProgress ?? new DungeonProgress(), loadedStatistics ?? new PlayerStatistics(), unlockedAchievements ?? new HashSet<string>());
            }
            else
            {
                _gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = "Failed to load save!",
                    Type = GameEvents.MessageType.Error
                });

                return (null, null, null, null);
            }
        }

        /// <summary>
        /// Saves the game to a specific slot with user confirmation.
        /// </summary>
        public void SaveGame(Player player, DungeonProgress dungeonProgress, PlayerStatistics? statistics = null, HashSet<string>? unlockedAchievements = null)
        {
            var slots = GetSaveSlotInfoList();
            int slotNumber = _gameInterface.RequestSaveSlot(slots);

            if (slotNumber >= 1 && slotNumber <= 3)
            {
                var selectedSlot = slots.FirstOrDefault(s => s.SlotNumber == slotNumber);

                if (selectedSlot != null && !selectedSlot.IsEmpty)
                {
                    bool confirm = _gameInterface.RequestConfirmation(
                        $"Overwrite save in slot {slotNumber}?"
                    );
                    if (!confirm) return;
                }

                bool success = SaveSystem.SaveGame(player, slotNumber, dungeonProgress, statistics, unlockedAchievements);

                if (success)
                {
                    _lastSaveSlot = slotNumber;
                }

                _gameInterface.OnEvent(new GameEvents.GameSavedEvent
                {
                    SlotNumber = slotNumber,
                    Success = success
                });

                if (success)
                {
                    _gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                    {
                        Message = $"Game saved to slot {slotNumber}!",
                        Type = GameEvents.MessageType.Success
                    });
                }
                else
                {
                    _gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                    {
                        Message = "Failed to save game!",
                        Type = GameEvents.MessageType.Error
                    });
                }
            }
        }

        /// <summary>
        /// Automatically saves the game to the last used save slot.
        /// </summary>
        public void AutoSave(Player player, DungeonProgress dungeonProgress, PlayerStatistics? statistics = null, HashSet<string>? unlockedAchievements = null)
        {
            if (_lastSaveSlot.HasValue)
            {
                bool success = SaveSystem.SaveGame(player, _lastSaveSlot.Value, dungeonProgress, statistics, unlockedAchievements);
                _gameInterface.OnEvent(new GameEvents.GameSavedEvent
                {
                    SlotNumber = _lastSaveSlot.Value,
                    Success = success
                });
            }
        }

        /// <summary>
        /// Requests the user to select a save slot (for loading or new game).
        /// </summary>
        public (int slotNumber, bool isNewCharacter) RequestSaveSlotSelection()
        {
            var slots = GetSaveSlotInfoList();
            return _gameInterface.RequestSaveSlotSelection(slots);
        }

        /// <summary>
        /// Gets information about all save slots.
        /// </summary>
        private List<InterfaceSaveSlotInfo> GetSaveSlotInfoList()
        {
            var systemSlots = SaveSystem.GetAllSaveSlots();
            return systemSlots.Select(s => new InterfaceSaveSlotInfo
            {
                SlotNumber = s.SlotNumber,
                IsEmpty = s.IsEmpty,
                Name = s.Name,
                Level = s.Level,
                Class = s.Class,
                SaveTime = s.SaveTime
            }).ToList();
        }
    }
}
