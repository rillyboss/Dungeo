using System;
using System.Linq;
using TestRPGGame.Entities.Player;
using TestRPGGame.Interfaces;

namespace TestRPGGame.Systems
{
    /// <summary>
    /// Manages player progression: ability unlocking, resting, and character development.
    /// Extracted from GameCore to follow Single Responsibility Principle.
    /// </summary>
    public class ProgressionManager
    {
        private readonly IGameInterface _gameInterface;

        public ProgressionManager(IGameInterface gameInterface)
        {
            _gameInterface = gameInterface;
        }

        /// <summary>
        /// Handles the ability unlocking flow with user interaction.
        /// </summary>
        public void UnlockAbilities(Player player)
        {
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
                })
                .OrderBy(a => a.PurchaseCost) // Sort by cost for better user experience
                .ThenBy(a => a.UnlockLevel)   // Then by level if costs are equal
                .ToList();

            if (!lockedAbilities.Any())
            {
                _gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = "All abilities unlocked!",
                    Type = GameEvents.MessageType.Success
                });
                return;
            }

            int selectedIndex = _gameInterface.RequestAbilityUnlock(lockedAbilities, player.Gold, player.Level);

            if (selectedIndex >= 0 && selectedIndex < lockedAbilities.Count)
            {
                // Use the stored Index from AbilityInfo to get the correct ability from the original unsorted list
                int originalIndex = lockedAbilities[selectedIndex].Index;
                var selectedAbility = player.Abilities.Where(a => !a.IsUnlocked).ElementAt(originalIndex);

                if (selectedAbility.CanUnlock(player.Level, player.Gold))
                {
                    bool confirm = _gameInterface.RequestConfirmation(
                        $"Unlock {selectedAbility.Name} for {selectedAbility.PurchaseCost} gold?"
                    );

                    if (confirm)
                    {
                        player.Gold -= selectedAbility.PurchaseCost;
                        selectedAbility.Unlock();

                        _gameInterface.OnEvent(new GameEvents.AbilityUnlockedEvent
                        {
                            AbilityName = selectedAbility.Name,
                            Description = selectedAbility.Description,
                            Cost = selectedAbility.PurchaseCost
                        });

                        _gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                        {
                            Message = $"{selectedAbility.Name} unlocked!",
                            Type = GameEvents.MessageType.Success
                        });
                    }
                }
                else
                {
                    _gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                    {
                        Message = "Cannot unlock this ability yet!",
                        Type = GameEvents.MessageType.Error
                    });
                }
            }
        }

        /// <summary>
        /// Allows the player to rest at an inn to restore HP and Mana.
        /// </summary>
        /// <returns>True if rest was successful</returns>
        public bool Rest(Player player)
        {
            int restCost = GameConfig.Config.RestingCost;

            if (player.Gold < restCost)
            {
                _gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = $"Not enough gold! Resting costs {restCost} gold. You have {player.Gold} gold.",
                    Type = GameEvents.MessageType.Error
                });
                return false;
            }

            int hpBefore = player.CurrentHP;
            int manaBefore = player.CurrentMana;

            player.CurrentHP = player.MaxHP;
            player.CurrentMana = player.MaxMana;
            player.Gold -= restCost;

            int hpRestored = player.CurrentHP - hpBefore;
            int manaRestored = player.CurrentMana - manaBefore;

            _gameInterface.OnEvent(new GameEvents.InfoMessageEvent
            {
                Message = $"Rested at the inn. HP +{hpRestored}, Mana +{manaRestored} (Cost: {restCost} gold)",
                Type = GameEvents.MessageType.Success
            });

            return true;
        }
    }
}
