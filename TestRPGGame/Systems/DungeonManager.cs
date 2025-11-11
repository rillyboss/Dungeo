using System;
using System.Collections.Generic;
using System.Linq;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Dungeon;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.Combat;
using TestRPGGame.Factories;
using TestRPGGame.Interfaces;

namespace TestRPGGame.Systems
{
    /// <summary>
    /// Manages dungeon progression, requirements checking, and dungeon runs.
    /// Extracted from GameCore to follow Single Responsibility Principle.
    /// </summary>
    public class DungeonManager
    {
        private readonly IGameInterface _gameInterface;
        private readonly InterfacedCombatSystem _combat;
        private readonly List<Dungeon> _dungeons;
        private DungeonProgress _dungeonProgress;

        public DungeonManager(IGameInterface gameInterface, InterfacedCombatSystem combat)
        {
            _gameInterface = gameInterface;
            _combat = combat;
            _dungeons = DungeonFactory.CreateAllDungeons();
            _dungeonProgress = new DungeonProgress();
        }

        /// <summary>
        /// Gets the current dungeon progress.
        /// </summary>
        public DungeonProgress Progress => _dungeonProgress;

        /// <summary>
        /// Sets the dungeon progress (used when loading a game).
        /// </summary>
        public void SetProgress(DungeonProgress progress)
        {
            _dungeonProgress = progress ?? new DungeonProgress();
        }

        /// <summary>
        /// Handles the dungeon selection and entry flow.
        /// Returns true if a dungeon was successfully completed.
        /// </summary>
        public bool EnterDungeon(Player player)
        {
            var dungeonInfoList = _dungeons.Select((d, i) => new DungeonSelectionInfo
            {
                Index = i,
                Name = d.Name,
                MinLevel = d.Requirements.MinLevel,
                GoldCost = d.Requirements.GoldCost,
                Difficulty = d.Difficulty,
                IsCompleted = _dungeonProgress.CompletedDungeons.GetValueOrDefault(d.Name, false),
                CanEnter = d.Requirements.MeetsRequirements(player, _dungeonProgress.CompletedDungeons),
                BlockingReason = GetDungeonBlockingReason(d, player)
            }).ToList();

            int dungeonIndex = _gameInterface.RequestDungeonSelection(dungeonInfoList);

            if (dungeonIndex >= 0 && dungeonIndex < _dungeons.Count)
            {
                var dungeon = _dungeons[dungeonIndex];

                if (!dungeon.Requirements.MeetsRequirements(player, _dungeonProgress.CompletedDungeons))
                {
                    _gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                    {
                        Message = "You don't meet the requirements for this dungeon!",
                        Type = GameEvents.MessageType.Error
                    });
                    return false;
                }

                bool confirm = _gameInterface.RequestConfirmation(
                    $"Enter {dungeon.Name}? (Cost: {dungeon.Requirements.GoldCost} gold)"
                );

                if (confirm)
                {
                    // Pay entry cost
                    player.Gold -= dungeon.Requirements.GoldCost;

                    _gameInterface.OnEvent(new GameEvents.DungeonEnteredEvent
                    {
                        DungeonName = dungeon.Name,
                        Difficulty = dungeon.Difficulty
                    });

                    // Run the dungeon
                    bool success = RunSimplifiedDungeon(player, dungeon);

                    if (success)
                    {
                        _dungeonProgress.CompletedDungeons[dungeon.Name] = true;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Runs a simplified dungeon with combat encounters and a boss fight.
        /// </summary>
        private bool RunSimplifiedDungeon(Player player, Dungeon dungeon)
        {
            // Run 3 random encounters
            int combats = 3;
            int goldEarned = 0;
            int expEarned = 0;

            for (int i = 0; i < combats; i++)
            {
                var enemy = EnemyFactory.CreateEnemy(player.Level + dungeon.Difficulty);
                bool victory = _combat.StartBattle(player, enemy, canFlee: false);

                if (!victory)
                {
                    _gameInterface.OnEvent(new GameEvents.DungeonCompletedEvent
                    {
                        DungeonName = dungeon.Name,
                        Success = false,
                        TotalGoldEarned = goldEarned,
                        TotalExperienceEarned = expEarned
                    });
                    return false;
                }
            }

            // Boss fight!
            _gameInterface.OnEvent(new GameEvents.InfoMessageEvent
            {
                Message = $"\n💀 The {dungeon.Boss.Name} awaits!",
                Type = GameEvents.MessageType.Info
            });

            bool bossVictory = _combat.StartBattle(player, dungeon.Boss, canFlee: false);

            if (!bossVictory)
            {
                _gameInterface.OnEvent(new GameEvents.DungeonCompletedEvent
                {
                    DungeonName = dungeon.Name,
                    Success = false,
                    TotalGoldEarned = goldEarned,
                    TotalExperienceEarned = expEarned
                });
                return false;
            }

            _gameInterface.OnEvent(new GameEvents.DungeonCompletedEvent
            {
                DungeonName = dungeon.Name,
                Success = true,
                TotalGoldEarned = goldEarned,
                TotalExperienceEarned = expEarned
            });

            return true;
        }

        /// <summary>
        /// Gets the reason why a player cannot enter a dungeon, if any.
        /// </summary>
        private string GetDungeonBlockingReason(Dungeon dungeon, Player player)
        {
            if (player.Level < dungeon.Requirements.MinLevel)
                return $"Requires Level {dungeon.Requirements.MinLevel}";

            if (player.Gold < dungeon.Requirements.GoldCost)
                return $"Need {dungeon.Requirements.GoldCost - player.Gold} more gold";

            if (!string.IsNullOrEmpty(dungeon.Requirements.PreviousDungeonRequired) &&
                !_dungeonProgress.CompletedDungeons.GetValueOrDefault(dungeon.Requirements.PreviousDungeonRequired, false))
                return $"Must complete {dungeon.Requirements.PreviousDungeonRequired} first";

            return "";
        }
    }
}
