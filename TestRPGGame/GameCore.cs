using System;
using System.Collections.Generic;
using System.Linq;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Dungeon;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.DataLoading;
using TestRPGGame.Factories;
using TestRPGGame.Systems;
using TestRPGGame.Combat;
using TestRPGGame.Interfaces;

namespace TestRPGGame
{
    /// <summary>
    /// Core game engine - pure game logic with no UI dependencies
    /// Uses IGameInterface for all input/output operations
    /// </summary>
    public class GameCore
    {
        private readonly IGameInterface gameInterface;
        private Player? player;
        private InterfacedCombatSystem combat;
        private Shop shop;
        private bool isRunning;
        private int? lastSaveSlot;
        private List<Dungeon> dungeons;
        private DungeonProgress dungeonProgress;

        public GameCore(IGameInterface gameInterface)
        {
            this.gameInterface = gameInterface;
            this.combat = new InterfacedCombatSystem(gameInterface);
            this.shop = new Shop(gameInterface);
            this.isRunning = true;
            this.lastSaveSlot = null;
            this.dungeons = DungeonFactory.CreateAllDungeons();
            this.dungeonProgress = new DungeonProgress();
        }

        public void Start()
        {
            gameInterface.OnEvent(new GameEvents.GameStartedEvent());

            // Load or create character
            var slots = GetSaveSlotInfoList();
            var (slotNumber, isNewCharacter) = gameInterface.RequestSaveSlotSelection(slots);

            lastSaveSlot = slotNumber;

            if (isNewCharacter)
            {
                CreateNewCharacter();
            }
            else
            {
                LoadCharacter(slotNumber);
            }

            // Main game loop
            MainGameLoop();

            gameInterface.OnEvent(new GameEvents.GameEndedEvent());
        }

        private void CreateNewCharacter()
        {
            var (name, playerClass) = gameInterface.RequestCharacterCreation();
            player = new Player(name, playerClass);

            gameInterface.OnEvent(new GameEvents.CharacterCreatedEvent
            {
                Name = player.Name,
                Class = player.Class,
                MaxHP = player.MaxHP,
                MaxMana = player.MaxMana,
                Attack = player.Attack,
                Defense = player.Defense
            });
        }

        private void LoadCharacter(int slotNumber)
        {
            var (loadedPlayer, loadedProgress) = SaveSystem.LoadGame(slotNumber);
            if (loadedPlayer != null)
            {
                player = loadedPlayer;
                if (loadedProgress != null)
                {
                    dungeonProgress = loadedProgress;
                }

                gameInterface.OnEvent(new GameEvents.GameLoadedEvent
                {
                    SlotNumber = slotNumber,
                    PlayerName = player.Name,
                    Level = player.Level,
                    Class = player.Class
                });

                gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = "Game loaded successfully!",
                    Type = GameEvents.MessageType.Success
                });
            }
            else
            {
                gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = "Failed to load save!",
                    Type = GameEvents.MessageType.Error
                });
            }
        }

        private void MainGameLoop()
        {
            while (isRunning && player != null)
            {
                PublishPlayerStats();

                var choice = gameInterface.RequestMainMenuChoice(
                    player.Name,
                    player.Level,
                    player.Class,
                    player.CurrentHP,
                    player.MaxHP,
                    player.Gold
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
                        isRunning = false;
                        gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                        {
                            Message = "Thanks for playing! May your legend live on...",
                            Type = GameEvents.MessageType.Info
                        });
                        break;
                }
            }
        }

        private void EnterCombat()
        {
            if (player == null) return;

            var enemy = EnemyFactory.CreateEnemy(player.Level);
            bool victory = combat.StartBattle(player, enemy, canFlee: true);

            // Auto-save after combat
            if (victory && lastSaveSlot.HasValue)
            {
                SaveSystem.SaveGame(player, lastSaveSlot.Value, dungeonProgress);
                gameInterface.OnEvent(new GameEvents.GameSavedEvent
                {
                    SlotNumber = lastSaveSlot.Value,
                    Success = true
                });
            }

            gameInterface.WaitForAcknowledgment();
        }

        private void EnterDungeon()
        {
            if (player == null) return;

            var dungeonInfoList = dungeons.Select((d, i) => new DungeonSelectionInfo
            {
                Index = i,
                Name = d.Name,
                MinLevel = d.Requirements.MinLevel,
                GoldCost = d.Requirements.GoldCost,
                Difficulty = d.Difficulty,
                IsCompleted = dungeonProgress.CompletedDungeons.GetValueOrDefault(d.Name, false),
                CanEnter = d.Requirements.MeetsRequirements(player, dungeonProgress.CompletedDungeons),
                BlockingReason = GetDungeonBlockingReason(d)
            }).ToList();

            int dungeonIndex = gameInterface.RequestDungeonSelection(dungeonInfoList);

            if (dungeonIndex >= 0 && dungeonIndex < dungeons.Count)
            {
                var dungeon = dungeons[dungeonIndex];

                if (!dungeon.Requirements.MeetsRequirements(player, dungeonProgress.CompletedDungeons))
                {
                    gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                    {
                        Message = "You don't meet the requirements for this dungeon!",
                        Type = GameEvents.MessageType.Error
                    });
                    return;
                }

                bool confirm = gameInterface.RequestConfirmation(
                    $"Enter {dungeon.Name}? (Cost: {dungeon.Requirements.GoldCost} gold)"
                );

                if (confirm)
                {
                    // Pay entry cost
                    player.Gold -= dungeon.Requirements.GoldCost;

                    gameInterface.OnEvent(new GameEvents.DungeonEnteredEvent
                    {
                        DungeonName = dungeon.Name,
                        Difficulty = dungeon.Difficulty
                    });

                    // Simplified dungeon run - just do combat encounters for now
                    bool success = RunSimplifiedDungeon(player, dungeon);

                    if (success)
                    {
                        dungeonProgress.CompletedDungeons[dungeon.Name] = true;
                        AutoSave();
                    }
                }
            }
        }

        private bool RunSimplifiedDungeon(Player player, Dungeon dungeon)
        {
            // Run 3 random encounters
            int combats = 3;
            int goldEarned = 0;
            int expEarned = 0;

            for (int i = 0; i < combats; i++)
            {
                var enemy = EnemyFactory.CreateEnemy(player.Level + dungeon.Difficulty);
                bool victory = combat.StartBattle(player, enemy, canFlee: false);

                if (!victory)
                {
                    gameInterface.OnEvent(new GameEvents.DungeonCompletedEvent
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
            gameInterface.OnEvent(new GameEvents.InfoMessageEvent
            {
                Message = $"\n💀 The {dungeon.Boss.Name} awaits!",
                Type = GameEvents.MessageType.Info
            });

            bool bossVictory = combat.StartBattle(player, dungeon.Boss, canFlee: false);

            if (!bossVictory)
            {
                gameInterface.OnEvent(new GameEvents.DungeonCompletedEvent
                {
                    DungeonName = dungeon.Name,
                    Success = false,
                    TotalGoldEarned = goldEarned,
                    TotalExperienceEarned = expEarned
                });
                return false;
            }

            gameInterface.OnEvent(new GameEvents.DungeonCompletedEvent
            {
                DungeonName = dungeon.Name,
                Success = true,
                TotalGoldEarned = goldEarned,
                TotalExperienceEarned = expEarned
            });

            return true;
        }

        private string GetDungeonBlockingReason(Dungeon dungeon)
        {
            if (player == null) return "No player";

            if (player.Level < dungeon.Requirements.MinLevel)
                return $"Requires Level {dungeon.Requirements.MinLevel}";

            if (player.Gold < dungeon.Requirements.GoldCost)
                return $"Need {dungeon.Requirements.GoldCost - player.Gold} more gold";

            if (!string.IsNullOrEmpty(dungeon.Requirements.PreviousDungeonRequired) &&
                !dungeonProgress.CompletedDungeons.GetValueOrDefault(dungeon.Requirements.PreviousDungeonRequired, false))
                return $"Must complete {dungeon.Requirements.PreviousDungeonRequired} first";

            return "";
        }

        private void VisitShop()
        {
            if (player == null) return;

            // Use existing Shop system (has embedded Console UI)
            shop.Enter(player);

            // Auto-save after shop visit
            AutoSave();
        }

        private void ManageInventory()
        {
            if (player == null) return;

            // Use interface-driven inventory system
            player.Inventory.ManageInventory(player, gameInterface);
        }

        private void ViewCharacterSheet()
        {
            if (player == null) return;

            // Use existing character sheet display
            player.DisplayCharacterSheet();
            gameInterface.WaitForAcknowledgment();
        }

        private void UnlockAbilities()
        {
            if (player == null) return;

            var lockedAbilities = player.Abilities
                .Where(a => !a.IsUnlocked)
                .Select((a, i) => new AbilityInfo
                {
                    Index = i,
                    Name = a.Name,
                    Description = a.Description,
                    ManaCost = a.ManaCost,
                    UnlockLevel = a.UnlockLevel,
                    PurchaseCost = a.PurchaseCost,
                    IsUnlocked = false
                }).ToList();

            if (!lockedAbilities.Any())
            {
                gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = "All abilities unlocked!",
                    Type = GameEvents.MessageType.Success
                });
                return;
            }

            int abilityIndex = gameInterface.RequestAbilityUnlock(lockedAbilities, player.Gold, player.Level);

            if (abilityIndex >= 0 && abilityIndex < lockedAbilities.Count)
            {
                var selectedAbility = player.Abilities.Where(a => !a.IsUnlocked).ElementAt(abilityIndex);

                if (selectedAbility.CanUnlock(player.Level, player.Gold))
                {
                    bool confirm = gameInterface.RequestConfirmation(
                        $"Unlock {selectedAbility.Name} for {selectedAbility.PurchaseCost} gold?"
                    );

                    if (confirm)
                    {
                        player.Gold -= selectedAbility.PurchaseCost;
                        selectedAbility.Unlock();

                        gameInterface.OnEvent(new GameEvents.AbilityUnlockedEvent
                        {
                            AbilityName = selectedAbility.Name,
                            Description = selectedAbility.Description,
                            Cost = selectedAbility.PurchaseCost
                        });

                        gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                        {
                            Message = $"{selectedAbility.Name} unlocked!",
                            Type = GameEvents.MessageType.Success
                        });
                    }
                }
                else
                {
                    gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                    {
                        Message = "Cannot unlock this ability yet!",
                        Type = GameEvents.MessageType.Error
                    });
                }
            }
        }

        private void Rest()
        {
            if (player == null) return;

            int restCost = GameConfig.Config.RestingCost;

            if (player.Gold < restCost)
            {
                gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = $"Not enough gold! Resting costs {restCost} gold. You have {player.Gold} gold.",
                    Type = GameEvents.MessageType.Error
                });
                return;
            }

            int hpBefore = player.CurrentHP;
            int manaBefore = player.CurrentMana;

            player.CurrentHP = player.MaxHP;
            player.CurrentMana = player.MaxMana;
            player.Gold -= restCost;

            int hpRestored = player.CurrentHP - hpBefore;
            int manaRestored = player.CurrentMana - manaBefore;

            gameInterface.OnEvent(new GameEvents.InfoMessageEvent
            {
                Message = $"Rested at the inn. HP +{hpRestored}, Mana +{manaRestored} (Cost: {restCost} gold)",
                Type = GameEvents.MessageType.Success
            });

            AutoSave();
        }

        private void SaveGame()
        {
            if (player == null) return;

            var slots = GetSaveSlotInfoList();
            int slotNumber = gameInterface.RequestSaveSlot(slots);

            if (slotNumber >= 1 && slotNumber <= 3)
            {
                var selectedSlot = slots.FirstOrDefault(s => s.SlotNumber == slotNumber);

                if (selectedSlot != null && !selectedSlot.IsEmpty)
                {
                    bool confirm = gameInterface.RequestConfirmation(
                        $"Overwrite save in slot {slotNumber}?"
                    );
                    if (!confirm) return;
                }

                bool success = SaveSystem.SaveGame(player, slotNumber, dungeonProgress);
                if (success)
                {
                    lastSaveSlot = slotNumber;
                }

                gameInterface.OnEvent(new GameEvents.GameSavedEvent
                {
                    SlotNumber = slotNumber,
                    Success = success
                });

                if (success)
                {
                    gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                    {
                        Message = $"Game saved to slot {slotNumber}!",
                        Type = GameEvents.MessageType.Success
                    });
                }
                else
                {
                    gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                    {
                        Message = "Failed to save game!",
                        Type = GameEvents.MessageType.Error
                    });
                }
            }
        }

        private void ShowHelp()
        {
            gameInterface.OnEvent(new GameEvents.InfoMessageEvent
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
            gameInterface.WaitForAcknowledgment();
        }

        private void AutoSave()
        {
            if (lastSaveSlot.HasValue && player != null)
            {
                bool success = SaveSystem.SaveGame(player, lastSaveSlot.Value, dungeonProgress);
                gameInterface.OnEvent(new GameEvents.GameSavedEvent
                {
                    SlotNumber = lastSaveSlot.Value,
                    Success = success
                });
            }
        }

        private void PublishPlayerStats()
        {
            if (player == null) return;

            gameInterface.OnEvent(new GameEvents.PlayerStatsChangedEvent
            {
                CurrentHP = player.CurrentHP,
                MaxHP = player.MaxHP,
                CurrentMana = player.CurrentMana,
                MaxMana = player.MaxMana,
                Gold = player.Gold,
                Level = player.Level,
                Experience = player.Experience
            });
        }

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
