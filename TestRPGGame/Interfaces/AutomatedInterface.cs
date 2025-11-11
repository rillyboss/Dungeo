using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestRPGGame.Entities.Player;
using TestRPGGame.Equipment;

namespace TestRPGGame.Interfaces
{
    /// <summary>
    /// Automated game interface that makes decisions programmatically.
    /// Can be configured with strategies or used for AI playthroughs.
    /// Logs all events and decisions to a string output.
    /// </summary>
    public class AutomatedInterface : IGameInterface
    {
        private readonly StringBuilder log;
        private readonly AutomatedStrategy strategy;
        private int combatCount = 0;
        private int inventoryActionCount = 0;

        public string GetLog() => log.ToString();

        public AutomatedInterface(AutomatedStrategy? strategy = null)
        {
            log = new StringBuilder();
            this.strategy = strategy ?? new DefaultStrategy();
        }

        public void OnEvent<T>(T gameEvent) where T : class
        {
            // Log all events
            switch (gameEvent)
            {
                case GameEvents.GameStartedEvent:
                    Log("=== GAME STARTED ===\n");
                    break;

                case GameEvents.CharacterCreatedEvent e:
                    Log($"Character Created: {e.Name} the {e.Class}");
                    Log($"  HP: {e.MaxHP}, Mana: {e.MaxMana}");
                    Log($"  Attack: {e.Attack}, Defense: {e.Defense}\n");
                    break;

                case GameEvents.CombatStartedEvent e:
                    Log($"\n=== COMBAT {++combatCount}: {e.EnemyName} (Lvl {e.EnemyLevel}) ===");
                    Log($"  Enemy: HP={e.EnemyMaxHP}, Atk={e.EnemyAttack}, Def={e.EnemyDefense}");
                    break;

                case GameEvents.CombatTurnStartEvent e:
                    Log($"\n--- Turn {e.TurnNumber} ---");
                    Log($"  Player: {e.PlayerHP}/{e.PlayerMaxHP} HP, {e.PlayerMana}/{e.PlayerMaxMana} Mana");
                    Log($"  Enemy: {e.EnemyHP}/{e.EnemyMaxHP} HP");
                    if (e.PlayerActiveEffects.Any())
                        Log($"  Player Effects: {string.Join(", ", e.PlayerActiveEffects)}");
                    if (e.EnemyActiveEffects.Any())
                        Log($"  Enemy Effects: {string.Join(", ", e.EnemyActiveEffects)}");
                    break;

                case GameEvents.AbilityUsedEvent e:
                    Log($"  {e.User} used {e.AbilityName}! ({e.ManaCost} mana)");
                    break;

                case GameEvents.DamageDealtEvent e:
                    string critMarker = e.IsCritical ? " [CRIT!]" : "";
                    Log($"  {e.Attacker} → {e.Target}: {e.Damage} {e.AttackType} damage{critMarker}");
                    break;

                case GameEvents.AttackMissedEvent e:
                    string missText = e.MissType == "Dodge" ? "dodged" : "missed";
                    Log($"  {e.Attacker}'s attack {missText}! ({e.Target})");
                    break;

                case GameEvents.EffectAppliedEvent e:
                    Log($"  Effect on {e.Target}: {e.EffectName} ({e.Duration} turns)");
                    break;

                case GameEvents.CombatEndedEvent e:
                    if (e.PlayerVictory)
                    {
                        Log($"\n*** VICTORY! ***");
                        Log($"  Gained: {e.GoldEarned} gold, {e.ExperienceEarned} XP");
                        if (e.LootDropped != null)
                            Log($"  Loot: [{e.LootDropped.Rarity}] {e.LootDropped.Name}");
                    }
                    else
                    {
                        Log($"\n*** DEFEAT ***");
                        Log($"  Lost: {e.GoldLost} gold");
                    }
                    break;

                case GameEvents.PlayerLeveledUpEvent e:
                    Log($"\n🎉 LEVEL UP! → Level {e.NewLevel}");
                    Log($"  New Stats: HP={e.NewMaxHP}, Mana={e.NewMaxMana}, Atk={e.NewAttack}, Def={e.NewDefense}");
                    break;

                case GameEvents.PlayerStatsChangedEvent e:
                    // Only log significant changes, not every tick
                    break;

                case GameEvents.ItemReceivedEvent e:
                    Log($"  Received: [{e.Rarity}] {e.ItemName} ({e.ItemType})");
                    break;

                case GameEvents.InfoMessageEvent e:
                    string prefix = e.Type switch
                    {
                        GameEvents.MessageType.Success => "✓",
                        GameEvents.MessageType.Warning => "⚠",
                        GameEvents.MessageType.Error => "✗",
                        _ => "ℹ"
                    };
                    Log($"  {prefix} {e.Message}");
                    break;

                case GameEvents.DungeonEnteredEvent e:
                    Log($"\n=== DUNGEON: {e.DungeonName} (Difficulty: {e.Difficulty}) ===");
                    break;

                case GameEvents.DungeonCompletedEvent e:
                    string result = e.Success ? "COMPLETED" : "FAILED";
                    Log($"\n*** DUNGEON {result} ***");
                    Log($"  Total: {e.TotalGoldEarned} gold, {e.TotalExperienceEarned} XP");
                    break;

                case GameEvents.GameSavedEvent e:
                    if (e.Success)
                        Log($"  💾 Game saved to slot {e.SlotNumber}");
                    break;
            }
        }

        public (int slotNumber, bool isNewCharacter) RequestSaveSlotSelection(List<InterfaceSaveSlotInfo> slots)
        {
            var emptySlot = slots.FirstOrDefault(s => s.IsEmpty);
            if (emptySlot != null)
            {
                Log($"Decision: Create new character in slot {emptySlot.SlotNumber}");
                return (emptySlot.SlotNumber, true);
            }

            // Load first available save
            var firstSave = slots.First();
            Log($"Decision: Load existing save from slot {firstSave.SlotNumber} ({firstSave.Name}, Lvl {firstSave.Level})");
            return (firstSave.SlotNumber, false);
        }

        public (string name, PlayerClass playerClass) RequestCharacterCreation()
        {
            var (name, playerClass) = strategy.ChooseCharacterClass();
            Log($"Decision: Create '{name}' as {playerClass}");
            return (name, playerClass);
        }

        public MainMenuChoice RequestMainMenuChoice(string playerName, int level, PlayerClass playerClass, int currentHP, int maxHP, int gold)
        {
            var choice = strategy.ChooseMainMenuAction(combatCount, level, gold, currentHP, maxHP);
            Log($"\nMain Menu: {playerName} (Lvl {level}) - HP: {currentHP}/{maxHP}, Gold: {gold}");
            Log($"Decision: {choice}");
            return choice;
        }

        public CombatAction RequestCombatAction(CombatState state)
        {
            var action = strategy.ChooseCombatAction(state);

            string actionDesc = action.ActionType switch
            {
                CombatActionType.Attack => "Attack",
                CombatActionType.UseAbility => $"Use Ability: {state.AvailableAbilities[action.AbilityIndex!.Value].Name}",
                CombatActionType.UsePotion => "Use Potion",
                CombatActionType.Flee => "Flee",
                _ => "Unknown"
            };
            Log($"  Player Action: {actionDesc}");

            return action;
        }

        public int RequestAbilitySelection(List<AbilityInfo> abilities)
        {
            var usable = abilities.Where(a => a.CanUse).ToList();
            if (usable.Any())
            {
                var selected = usable.First();
                Log($"  Selected Ability: {selected.Name}");
                return selected.Index;
            }
            return -1;
        }

        public ShopAction RequestShopAction(List<ShopItemInfo> forSale, List<EquipmentItem> inventory, int playerGold)
        {
            // Simple strategy: buy if affordable and good value
            var affordableItems = forSale.Where(i => i.Price <= playerGold).ToList();
            if (affordableItems.Any())
            {
                var bestItem = affordableItems.OrderByDescending(i => i.Level).First();
                Log($"  Shop: Buy {bestItem.Name} for {bestItem.Price} gold");
                return new ShopAction { ActionType = ShopActionType.BuyItem, ItemIndex = bestItem.Index };
            }

            Log($"  Shop: Exit (nothing affordable)");
            return new ShopAction { ActionType = ShopActionType.Exit };
        }

        public InventoryAction RequestInventoryAction(List<EquipmentItem> backpack, Dictionary<EquipmentSlot, EquipmentItem?> equipped)
        {
            inventoryActionCount++;

            // Equip first unequipped item, but exit after 3 actions to avoid infinite loops
            if (backpack.Any() && inventoryActionCount <= 3)
            {
                Log($"  Inventory: Equip {backpack.First().Name}");
                return new InventoryAction { ActionType = InventoryActionType.EquipItem, ItemIndex = 0 };
            }

            inventoryActionCount = 0; // Reset for next time
            Log($"  Inventory: Exit");
            return new InventoryAction { ActionType = InventoryActionType.Exit };
        }

        public int RequestDungeonSelection(List<DungeonSelectionInfo> dungeons)
        {
            var available = dungeons.Where(d => d.CanEnter).ToList();
            if (available.Any())
            {
                var dungeon = available.First();
                Log($"  Dungeon: Enter {dungeon.Name}");
                return dungeon.Index;
            }

            return -1; // Cancel
        }

        public int RequestAbilityUnlock(List<AbilityInfo> lockedAbilities, int playerGold, int playerLevel)
        {
            var canUnlock = lockedAbilities.Where(a =>
                playerLevel >= a.UnlockLevel && playerGold >= a.PurchaseCost).ToList();

            if (canUnlock.Any())
            {
                var ability = canUnlock.First();
                Log($"  Unlock: {ability.Name} for {ability.PurchaseCost} gold");
                return ability.Index;
            }

            return -1;
        }

        public bool RequestConfirmation(string message)
        {
            Log($"  Confirm: {message} → Yes");
            return true;
        }

        public int RequestSaveSlot(List<InterfaceSaveSlotInfo> slots)
        {
            var slot = slots.First();
            return slot.SlotNumber;
        }

        public void WaitForAcknowledgment()
        {
            // No-op for automated interface
        }

        public int RequestEncounterChoice(string description, List<string> choices)
        {
            // AI strategy: Choose first option (typically the "safe" choice)
            Log($"  Encounter: {description}");
            Log($"  Choice: {choices[0]}");
            return 0;
        }

        private void Log(string message)
        {
            log.AppendLine(message);
            // Also print to console so we can see progress
            Console.WriteLine(message);
        }
    }

    /// <summary>
    /// Strategy pattern for automated decision making
    /// </summary>
    public abstract class AutomatedStrategy
    {
        public abstract (string name, PlayerClass playerClass) ChooseCharacterClass();
        public abstract MainMenuChoice ChooseMainMenuAction(int combatCount, int level, int gold, int hp, int maxHp);
        public abstract CombatAction ChooseCombatAction(CombatState state);

        /// <summary>
        /// Override to specify a preferred save slot. Return null for default behavior.
        /// </summary>
        public virtual int? GetPreferredSaveSlot() => null;
    }

    /// <summary>
    /// Default strategy: cautious and systematic
    /// </summary>
    public class DefaultStrategy : AutomatedStrategy
    {
        public override (string name, PlayerClass playerClass) ChooseCharacterClass()
        {
            return ("AIHero", PlayerClass.Warrior);
        }

        public override MainMenuChoice ChooseMainMenuAction(int combatCount, int level, int gold, int hp, int maxHp)
        {
            // Rest if low HP
            if (hp < maxHp * 0.5)
                return MainMenuChoice.Rest;

            // Do 3 combats then stop to analyze
            if (combatCount < 3)
                return MainMenuChoice.Combat;

            // Exit after 3 combats for analysis
            return MainMenuChoice.Exit;
        }

        public override CombatAction ChooseCombatAction(CombatState state)
        {
            // Use potion if HP is low
            if (state.PlayerCurrentHP < state.PlayerMaxHP * 0.3 && state.PlayerPotions > 0)
            {
                return new CombatAction { ActionType = CombatActionType.UsePotion };
            }

            // Try to use an ability if available
            var usableAbilities = state.AvailableAbilities
                .Where(a => a.CanUse && a.CurrentCooldown == 0)
                .OrderByDescending(a => a.ManaCost) // Prefer high-cost abilities
                .ToList();

            if (usableAbilities.Any() && state.PlayerCurrentMana >= 20)
            {
                return new CombatAction
                {
                    ActionType = CombatActionType.UseAbility,
                    AbilityIndex = usableAbilities.First().Index
                };
            }

            // Default: basic attack
            return new CombatAction { ActionType = CombatActionType.Attack };
        }
    }
}
