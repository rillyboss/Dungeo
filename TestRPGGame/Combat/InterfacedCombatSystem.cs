using System;
using System.Collections.Generic;
using System.Linq;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.Abilities;
using TestRPGGame.Interfaces;
using TestRPGGame.Equipment;
using TestRPGGame.Utils;

namespace TestRPGGame.Combat
{
    /// <summary>
    /// Combat system that uses IGameInterface for all I/O
    /// Pure combat logic with no direct Console dependencies
    /// </summary>
    public class InterfacedCombatSystem
    {
        private readonly IGameInterface gameInterface;

        public InterfacedCombatSystem(IGameInterface gameInterface)
        {
            this.gameInterface = gameInterface;
        }

        public bool StartBattle(Player player, Enemy enemy, bool canFlee = true)
        {
            // Publish combat started event
            gameInterface.OnEvent(new GameEvents.CombatStartedEvent
            {
                EnemyName = enemy.Name,
                EnemyLevel = 1, // Enemies don't have level, use placeholder
                EnemyMaxHP = enemy.MaxHP,
                EnemyAttack = enemy.Attack,
                EnemyDefense = enemy.Defense,
                CanFlee = canFlee
            });

            int turnNumber = 0;
            int maxTurns = Systems.GameConfig.Config.MaxCombatTurns;

            while (player.CurrentHP > 0 && enemy.CurrentHP > 0 && turnNumber < maxTurns)
            {
                turnNumber++;

                // Turn start event
                gameInterface.OnEvent(new GameEvents.CombatTurnStartEvent
                {
                    TurnNumber = turnNumber,
                    PlayerHP = player.CurrentHP,
                    PlayerMaxHP = player.MaxHP,
                    PlayerMana = player.CurrentMana,
                    PlayerMaxMana = player.MaxMana,
                    EnemyHP = enemy.CurrentHP,
                    EnemyMaxHP = enemy.MaxHP,
                    PlayerActiveEffects = GetActiveEffectNames(player),
                    EnemyActiveEffects = GetActiveEffectNames(enemy)
                });

                // Request player action
                var combatState = BuildCombatState(turnNumber, player, enemy, canFlee);
                var playerAction = gameInterface.RequestCombatAction(combatState);

                // Process player action
                if (playerAction.ActionType == CombatActionType.Flee && canFlee)
                {
                    gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                    {
                        Message = "You fled from battle!",
                        Type = GameEvents.MessageType.Warning
                    });
                    return false; // Combat ended, player fled
                }

                if (playerAction.ActionType == CombatActionType.UsePotion)
                {
                    if (player.UsePotion())
                    {
                        int restored = (int)(player.MaxHP * Systems.GameConfig.Config.PotionHealPercent);
                        gameInterface.OnEvent(new GameEvents.PotionUsedEvent
                        {
                            HPRestored = restored,
                            PotionsRemaining = player.PotionCount
                        });
                    }
                    else
                    {
                        gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                        {
                            Message = "No potions left!",
                            Type = GameEvents.MessageType.Error
                        });
                    }
                }
                else if (playerAction.ActionType == CombatActionType.UseAbility && playerAction.AbilityIndex.HasValue)
                {
                    var abilities = player.Abilities.Where(a => a.IsUnlocked).ToList();
                    if (playerAction.AbilityIndex.Value >= 0 && playerAction.AbilityIndex.Value < abilities.Count)
                    {
                        var ability = abilities[playerAction.AbilityIndex.Value];
                        if (ability.CanUse(player.CurrentMana) && ability.CurrentCooldown == 0)
                        {
                            ExecutePlayerAbility(player, enemy, ability);
                        }
                    }
                }
                else // Attack
                {
                    ExecutePlayerAttack(player, enemy);
                }

                // Check if enemy died
                if (enemy.CurrentHP <= 0)
                {
                    return EndCombat(player, enemy, true);
                }

                // Enemy turn - simple AI
                ExecuteEnemyTurn(player, enemy);

                // Check if player died
                if (player.CurrentHP <= 0)
                {
                    return EndCombat(player, enemy, false);
                }

                // Tick effects and regenerate
                // player.RegenerateMana(); // This method exists in Player
                player.CurrentMana = Math.Min(player.MaxMana,
                    player.CurrentMana + (int)(player.MaxMana * Systems.GameConfig.Config.ManaRegenRate));
                TickCooldowns(player);
            }

            // Timeout
            return false;
        }

        private void ExecutePlayerAttack(Player player, Enemy enemy)
        {
            int baseDamage = player.Attack;
            bool isCrit = RandomProvider.NextDouble() < player.CritChance;
            if (isCrit) baseDamage = (int)(baseDamage * 2.0);

            int finalDamage = Math.Max(1, baseDamage - enemy.Defense / 2);
            enemy.CurrentHP = Math.Max(0, enemy.CurrentHP - finalDamage);

            gameInterface.OnEvent(new GameEvents.DamageDealtEvent
            {
                Attacker = "Player",
                Target = enemy.Name,
                Damage = finalDamage,
                IsCritical = isCrit,
                AttackType = player.GetWeaponAttackType().ToString()
            });
        }

        private void ExecutePlayerAbility(Player player, Enemy enemy, Ability ability)
        {
            player.CurrentMana -= ability.ManaCost;
            ability.CurrentCooldown = ability.Cooldown; // Start cooldown

            gameInterface.OnEvent(new GameEvents.AbilityUsedEvent
            {
                User = "Player",
                AbilityName = ability.Name,
                Description = ability.Description,
                ManaCost = ability.ManaCost
            });

            // Execute ability effects through the proper system
            var context = new Abilities.Effects.AbilityContext(player, enemy, gameInterface);
            ability.Execute(context);
        }

        private void ExecuteEnemyTurn(Player player, Enemy enemy)
        {
            // Simple enemy AI - just attack
            int damage = Math.Max(1, enemy.Attack - player.Defense / 2);
            player.CurrentHP = Math.Max(0, player.CurrentHP - damage);

            gameInterface.OnEvent(new GameEvents.DamageDealtEvent
            {
                Attacker = enemy.Name,
                Target = "Player",
                Damage = damage,
                IsCritical = false,
                AttackType = "Physical"
            });
        }

        private bool EndCombat(Player player, Enemy enemy, bool playerVictory)
        {
            EquipmentItem? loot = null;
            int goldEarned = 0;
            int expEarned = 0;
            int goldLost = 0;

            if (playerVictory)
            {
                goldEarned = (int)(enemy.GoldReward * Systems.GameConfig.Config.CombatGoldMultiplier);
                expEarned = enemy.ExpReward;
                player.Gold += goldEarned;

                // Check for loot drop
                if (RandomProvider.NextDouble() < Systems.GameConfig.Config.CombatLootDropChance)
                {
                    loot = EquipmentGenerator.GenerateItem(player.Level);
                    player.Inventory.BackpackItems.Add(loot);
                }

                // Check for level up
                bool leveledUp = player.GainExperience(expEarned);
                if (leveledUp)
                {
                    gameInterface.OnEvent(new GameEvents.PlayerLeveledUpEvent
                    {
                        NewLevel = player.Level,
                        NewMaxHP = player.MaxHP,
                        NewMaxMana = player.MaxMana,
                        NewAttack = player.Attack,
                        NewDefense = player.Defense
                    });
                }
            }
            else
            {
                goldLost = Math.Min((int)(player.Gold * Systems.GameConfig.Config.CombatGoldLossPercent),
                                   Systems.GameConfig.Config.CombatGoldLossMax);
                player.Gold -= goldLost;
                player.CurrentHP = (int)(player.MaxHP * Systems.GameConfig.Config.DefeatHPRestorePercent);
            }

            gameInterface.OnEvent(new GameEvents.CombatEndedEvent
            {
                PlayerVictory = playerVictory,
                GoldEarned = goldEarned,
                ExperienceEarned = expEarned,
                LootDropped = loot,
                GoldLost = goldLost
            });

            return playerVictory;
        }

        private CombatState BuildCombatState(int turnNumber, Player player, Enemy enemy, bool canFlee)
        {
            var abilities = player.Abilities.Where(a => a.IsUnlocked).ToList();

            return new CombatState
            {
                TurnNumber = turnNumber,
                EnemyName = enemy.Name,
                EnemyCurrentHP = enemy.CurrentHP,
                EnemyMaxHP = enemy.MaxHP,
                PlayerCurrentHP = player.CurrentHP,
                PlayerMaxHP = player.MaxHP,
                PlayerCurrentMana = player.CurrentMana,
                PlayerMaxMana = player.MaxMana,
                PlayerPotions = player.PotionCount,
                CanFlee = canFlee,
                AvailableAbilities = abilities.Select((a, i) => new AbilityInfo
                {
                    Index = i,
                    Name = a.Name,
                    Description = a.Description,
                    ManaCost = a.ManaCost,
                    CurrentCooldown = a.CurrentCooldown,
                    CanUse = a.CanUse(player.CurrentMana) && a.CurrentCooldown == 0,
                    IsUnlocked = a.IsUnlocked,
                    Priority = a.Priority
                }).ToList(),
                PlayerActiveEffects = GetActiveEffectNames(player),
                EnemyActiveEffects = GetActiveEffectNames(enemy)
            };
        }

        private List<string> GetActiveEffectNames(Player player)
        {
            var effects = new List<string>();

            // Get all active status effects from the player's StatusEffectManager
            if (player.Effects != null && player.Effects.ActiveEffects.Count > 0)
            {
                foreach (var effect in player.Effects.ActiveEffects)
                {
                    // Get effect display name with icon if available
                    string effectDisplay = !string.IsNullOrEmpty(effect.Icon) ? $"{effect.Icon} {effect.Name}" : effect.Name;
                    if (effect.RemainingTurns > 0)
                    {
                        effectDisplay += $" ({effect.RemainingTurns})";
                    }
                    effects.Add(effectDisplay);
                }
            }

            return effects;
        }

        private List<string> GetActiveEffectNames(Enemy enemy)
        {
            var effects = new List<string>();

            // Get all active status effects from the enemy's StatusEffectManager
            if (enemy.Effects != null && enemy.Effects.ActiveEffects.Count > 0)
            {
                foreach (var effect in enemy.Effects.ActiveEffects)
                {
                    // Get effect display name with icon if available
                    string effectDisplay = !string.IsNullOrEmpty(effect.Icon) ? $"{effect.Icon} {effect.Name}" : effect.Name;
                    if (effect.RemainingTurns > 0)
                    {
                        effectDisplay += $" ({effect.RemainingTurns})";
                    }
                    effects.Add(effectDisplay);
                }
            }

            return effects;
        }

        private void TickCooldowns(Player player)
        {
            foreach (var ability in player.Abilities)
            {
                if (ability.CurrentCooldown > 0)
                {
                    ability.CurrentCooldown--;
                }
            }
        }
    }
}
