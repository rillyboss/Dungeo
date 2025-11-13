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
        private int currentCombatDamageDealt = 0;
        private int currentCombatDamageTaken = 0;

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
                    currentCombatDamageDealt = 0;
                    currentCombatDamageTaken = 0;

                    if (strategy is UltraThinkStrategy ultraThinkStart)
                        ultraThinkStart.TrackCombatStart();
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

                    // Track damage for analytics
                    if (e.Attacker == "Player")
                        currentCombatDamageDealt += e.Damage;
                    if (e.Target == "Player")
                        currentCombatDamageTaken += e.Damage;

                    if (e.IsCritical && strategy is UltraThinkStrategy ultraThink1)
                        ultraThink1.AnalyzeCriticalHit();
                    break;

                case GameEvents.AttackMissedEvent e:
                    string missText = e.MissType == "Dodge" ? "dodged" : "missed";
                    Log($"  {e.Attacker}'s attack {missText}! ({e.Target})");

                    if (strategy is UltraThinkStrategy ultraThink2)
                        ultraThink2.AnalyzeMissedAttack();
                    break;

                case GameEvents.EffectAppliedEvent e:
                    Log($"  Effect on {e.Target}: {e.EffectName} ({e.Duration} turns)");

                    if (strategy is UltraThinkStrategy ultraThink3)
                        ultraThink3.AnalyzeStatusEffect(e.EffectName, e.Target);
                    break;

                case GameEvents.CombatEndedEvent e:
                    if (e.PlayerVictory)
                    {
                        Log($"\n*** VICTORY! ***");
                        Log($"  Gained: {e.GoldEarned} gold, {e.ExperienceEarned} XP");
                        if (e.LootDropped != null)
                        {
                            Log($"  Loot: [{e.LootDropped.Rarity}] {e.LootDropped.Name}");
                            if (strategy is UltraThinkStrategy ultraThink4)
                            {
                                ultraThink4.AnalyzeLootDrop(e.LootDropped.Name, e.LootDropped.Rarity.ToString());

                                // Track if item grants abilities
                                if (e.LootDropped.GrantedAbilityIds.Any())
                                {
                                    ultraThink4.TrackItemWithAbilityReceived(e.LootDropped.Name, e.LootDropped.GrantedAbilityIds.Count);
                                }
                            }
                        }
                    }
                    else
                    {
                        Log($"\n*** DEFEAT ***");
                        Log($"  Lost: {e.GoldLost} gold");
                    }

                    // Analyze combat result
                    if (strategy is UltraThinkStrategy ultraThink5)
                    {
                        ultraThink5.AnalyzeCombatResult(e.PlayerVictory, currentCombatDamageDealt, currentCombatDamageTaken);
                        if (e.PlayerVictory)
                        {
                            var analytics = ultraThink5.GetAnalytics();
                            analytics.GoldFromCombat += e.GoldEarned;
                        }
                    }
                    break;

                case GameEvents.PlayerLeveledUpEvent e:
                    Log($"\n🎉 LEVEL UP! → Level {e.NewLevel}");
                    Log($"  New Stats: HP={e.NewMaxHP}, Mana={e.NewMaxMana}, Atk={e.NewAttack}, Def={e.NewDefense}");

                    // Trigger inventory check after leveling up (items might be better now)
                    if (strategy is UltraThinkStrategy ultraThinkLevelUp)
                    {
                        ultraThinkLevelUp.TriggerInventoryCheck("leveled up");
                    }
                    break;

                case GameEvents.PlayerStatsChangedEvent e:
                    // Only log significant changes, not every tick
                    break;

                case GameEvents.ItemReceivedEvent e:
                    Log($"  Received: [{e.Rarity}] {e.ItemName} ({e.ItemType})");
                    // Trigger inventory check after receiving loot
                    if (strategy is UltraThinkStrategy ultraThink9)
                    {
                        ultraThink9.TriggerInventoryCheck("received loot");
                    }
                    break;

                case GameEvents.ItemEquippedEvent e:
                    // Equipment changed - tracking happens in inventory action
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

                    if (strategy is UltraThinkStrategy ultraThink6)
                    {
                        // DungeonName will need to be tracked from DungeonEnteredEvent
                        ultraThink6.AnalyzeDungeonResult("Dungeon", e.Success);
                        if (e.Success)
                            ultraThink6.GetAnalytics().GoldFromDungeons += e.TotalGoldEarned;
                    }
                    break;

                case GameEvents.GameSavedEvent e:
                    if (e.Success)
                        Log($"  💾 Game saved to slot {e.SlotNumber}");
                    break;

                case GameEvents.AchievementUnlockedEvent e:
                    if (strategy is UltraThinkStrategy ultraThink7)
                    {
                        ultraThink7.GetAnalytics().GoldFromAchievements += e.GoldReward;
                    }
                    break;

                case GameEvents.ItemSoldEvent e:
                    if (strategy is UltraThinkStrategy ultraThink8)
                    {
                        ultraThink8.GetAnalytics().GoldFromShopSales += e.Price;
                    }
                    break;

                case GameEvents.ItemPurchasedEvent e:
                    // Trigger inventory check after purchasing items
                    if (strategy is UltraThinkStrategy ultraThink10)
                    {
                        ultraThink10.TriggerInventoryCheck("purchased item");
                    }
                    break;
            }
        }

        public (int slotNumber, bool isNewCharacter) RequestSaveSlotSelection(List<InterfaceSaveSlotInfo> slots)
        {
            // Check if strategy has a preferred slot
            var preferredSlot = strategy.GetPreferredSaveSlot();
            if (preferredSlot.HasValue)
            {
                var slot = slots.FirstOrDefault(s => s.SlotNumber == preferredSlot.Value);
                if (slot != null)
                {
                    if (slot.IsEmpty)
                    {
                        Log($"Decision: Create new character in preferred slot {slot.SlotNumber}");
                        return (slot.SlotNumber, true);
                    }
                    else
                    {
                        Log($"Decision: Overwrite slot {slot.SlotNumber} (was {slot.Name}, Lvl {slot.Level})");
                        return (slot.SlotNumber, true); // Overwrite for automated testing
                    }
                }
            }

            // Fallback: use first empty slot
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
            // Get currently equipped items from strategy if available
            Dictionary<EquipmentSlot, EquipmentItem?>? equippedItems = null;
            if (strategy is UltraThinkStrategy ultraThinkStrat)
            {
                equippedItems = ultraThinkStrat.GetCurrentlyEquipped();
            }

            Log($"  💼 Backpack: {inventory.Count} items, Gold: {playerGold}");

            // First, try to sell items that are worse than what we have equipped
            if (equippedItems != null && inventory.Any())
            {
                foreach (var item in inventory)
                {
                    var equipped = equippedItems.GetValueOrDefault(item.Slot);

                    // Sell if we have something better equipped
                    if (equipped != null && !IsItemBetter(item, equipped))
                    {
                        int itemIndex = inventory.IndexOf(item);
                        Log($"  Shop: Sell {item.Name} (Lvl {item.Level}) - have better {equipped.Name}");
                        return new ShopAction { ActionType = ShopActionType.SellItem, ItemIndex = itemIndex };
                    }
                }
            }

            // Try to buy upgrades if affordable
            var affordableItems = forSale.Where(i => i.Price <= playerGold).ToList();
            if (affordableItems.Any())
            {
                // Prioritize items that would be upgrades
                var bestItem = affordableItems.OrderByDescending(i => i.Level).First();
                Log($"  Shop: Buy {bestItem.Name} for {bestItem.Price} gold");

                if (strategy is UltraThinkStrategy ultraThink)
                    ultraThink.AnalyzeShopPurchase(bestItem.Name, bestItem.Price);

                return new ShopAction { ActionType = ShopActionType.BuyItem, ItemIndex = bestItem.Index };
            }

            Log($"  Shop: Exit (nothing to do)");
            return new ShopAction { ActionType = ShopActionType.Exit };
        }

        public InventoryAction RequestInventoryAction(List<EquipmentItem> backpack, Dictionary<EquipmentSlot, EquipmentItem?> equipped)
        {
            inventoryActionCount++;

            // Smart equipping: Evaluate ALL items in backpack for upgrades
            if (backpack.Any())
            {
                // Find the best upgrade in our backpack
                for (int i = 0; i < backpack.Count; i++)
                {
                    var item = backpack[i];
                    var currentlyEquipped = equipped.GetValueOrDefault(item.Slot);

                    // If slot is empty or new item is better, equip it
                    if (currentlyEquipped == null || IsItemBetter(item, currentlyEquipped))
                    {
                        Log($"  Inventory: Equip {item.Name} (Lvl {item.Level}, {item.Slot}) - upgrade from {currentlyEquipped?.Name ?? "empty"}");

                        // Track if item grants abilities
                        if (strategy is UltraThinkStrategy ultraThink && item.GrantedAbilityIds.Any())
                        {
                            string abilityList = string.Join(", ", item.GrantedAbilityIds);
                            ultraThink.TrackItemWithAbilityEquipped(item.Name, item.GrantedAbilityIds.Count);
                            Log($"  ⚡ Item grants {item.GrantedAbilityIds.Count} ability(ies): {abilityList}");
                        }

                        inventoryActionCount = 0; // Reset for next session
                        return new InventoryAction { ActionType = InventoryActionType.EquipItem, ItemIndex = i };
                    }
                }
            }

            inventoryActionCount = 0; // Reset for next time

            // Track final equipped state before exiting inventory
            if (strategy is UltraThinkStrategy ultraThink2)
            {
                ultraThink2.UpdateEquippedGear(equipped);
            }

            Log($"  Inventory: Exit - {backpack.Count} items remaining in backpack");
            return new InventoryAction { ActionType = InventoryActionType.Exit };
        }

        /// <summary>
        /// Determines if newItem is better than currentItem by comparing total stat bonuses
        /// </summary>
        private bool IsItemBetter(EquipmentItem newItem, EquipmentItem currentItem)
        {
            // Calculate total value of each item
            int newValue = newItem.AttackBonus + newItem.DefenseBonus + newItem.MagicBonus +
                          newItem.HPBonus / 10 + newItem.ManaBonus / 10 + newItem.AgilityBonus +
                          (int)(newItem.CritBonus * 100) + (newItem.MinDamage + newItem.MaxDamage) / 2;

            int currentValue = currentItem.AttackBonus + currentItem.DefenseBonus + currentItem.MagicBonus +
                              currentItem.HPBonus / 10 + currentItem.ManaBonus / 10 + currentItem.AgilityBonus +
                              (int)(currentItem.CritBonus * 100) + (currentItem.MinDamage + currentItem.MaxDamage) / 2;

            return newValue > currentValue;
        }

        public void DisplayCharacterSheet(CharacterSheetInfo info)
        {
            Log($"  Character Sheet: {info.Name} (Lv {info.Level} {info.Class}) - {info.CurrentHP}/{info.MaxHP} HP, {info.Gold} gold");

            // Track equipped gear for smart selling/equipping
            if (strategy is UltraThinkStrategy ultraThink)
            {
                ultraThink.UpdateEquippedGear(info.Equipment);
            }
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
            // Track this ability store visit
            var storeVisit = new AbilityStoreVisit
            {
                PlayerLevel = playerLevel,
                PlayerGold = playerGold,
                AvailableAbilitiesCount = lockedAbilities.Count
            };

            var canUnlock = lockedAbilities.Where(a =>
                playerLevel >= a.UnlockLevel && playerGold >= a.PurchaseCost).ToList();

            if (canUnlock.Any())
            {
                // Smart selection: Prioritize lowest unlock level, then lowest cost
                // This ensures we unlock abilities ASAP and save gold for other things
                var ability = canUnlock
                    .OrderBy(a => a.UnlockLevel)
                    .ThenBy(a => a.PurchaseCost)
                    .First();

                Log($"  Unlock: {ability.Name} (Lvl {ability.UnlockLevel}, {ability.PurchaseCost}g) - {ability.Description}");

                // Track successful purchase
                storeVisit.PurchasedAbility = true;
                storeVisit.PurchasedAbilityName = ability.Name;
                storeVisit.PurchasedAbilityCost = ability.PurchaseCost;

                if (strategy is UltraThinkStrategy ultraThink)
                {
                    ultraThink.AnalyzeAbilityUnlock(ability.Name, ability.PurchaseCost);
                    ultraThink.GetAnalytics().AbilityStoreVisits.Add(storeVisit);
                }

                return ability.Index;
            }

            // Track why we didn't purchase
            storeVisit.PurchasedAbility = false;

            if (lockedAbilities.Count == 0)
            {
                storeVisit.ReasonsNotPurchased.Add("All abilities already unlocked");
                Log($"  Ability Store: All abilities unlocked!");
            }
            else
            {
                // Analyze why we couldn't purchase
                var affordableAbilities = lockedAbilities.Where(a => playerGold >= a.PurchaseCost).ToList();
                var levelRequiredAbilities = lockedAbilities.Where(a => playerLevel >= a.UnlockLevel).ToList();

                if (affordableAbilities.Count == 0)
                {
                    var cheapestAbility = lockedAbilities.OrderBy(a => a.PurchaseCost).First();
                    storeVisit.ReasonsNotPurchased.Add($"Can't afford cheapest ability (need {cheapestAbility.PurchaseCost}g, have {playerGold}g)");
                    Log($"  Ability Store: Can't afford any abilities (need {cheapestAbility.PurchaseCost}g, have {playerGold}g)");
                }

                if (levelRequiredAbilities.Count == 0)
                {
                    var lowestLevelAbility = lockedAbilities.OrderBy(a => a.UnlockLevel).First();
                    storeVisit.ReasonsNotPurchased.Add($"Level too low (need level {lowestLevelAbility.UnlockLevel}, currently {playerLevel})");
                    Log($"  Ability Store: Level too low (need level {lowestLevelAbility.UnlockLevel}, currently {playerLevel})");
                }

                // Specifically check if level and gold are both issues
                if (affordableAbilities.Any() && levelRequiredAbilities.Any())
                {
                    // Have money and level, but not for the same ability
                    var cheapestAtLevel = lockedAbilities
                        .Where(a => playerLevel >= a.UnlockLevel)
                        .OrderBy(a => a.PurchaseCost)
                        .FirstOrDefault();

                    if (cheapestAtLevel != null)
                    {
                        storeVisit.ReasonsNotPurchased.Add($"Can't afford level-appropriate abilities (need {cheapestAtLevel.PurchaseCost}g, have {playerGold}g)");
                        Log($"  Ability Store: Can't afford level-appropriate abilities (need {cheapestAtLevel.PurchaseCost}g for {cheapestAtLevel.Name})");
                    }
                }
            }

            if (strategy is UltraThinkStrategy ultraThink2)
            {
                ultraThink2.GetAnalytics().AbilityStoreVisits.Add(storeVisit);
            }

            return -1; // No affordable abilities
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

        public void DisplayStatistics(Systems.StatisticsInfo info)
        {
            // Automated interface logs statistics but doesn't display them interactively
            Log($"STATISTICS: {info.TotalKills} kills, {info.TotalDeaths} deaths, {info.CombatsWon} combats won");
        }

        public void DisplayAchievements(AchievementDisplayInfo info)
        {
            // Automated interface logs achievements but doesn't display them interactively
            Log($"ACHIEVEMENTS: {info.UnlockedAchievements}/{info.TotalAchievements} unlocked ({info.CompletionPercentage:F1}%), {info.EarnedPoints}/{info.TotalPoints} points");
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

    /// <summary>
    /// Comprehensive strategy for automated playtesting to level 15.
    /// Tests ALL game systems: combat, equipment, abilities, leveling, dungeons, shop, inventory.
    /// </summary>
    public class Level15Strategy : AutomatedStrategy
    {
        private readonly string characterName;
        private readonly PlayerClass playerClass;
        private int lastLoggedLevel = 0;
        private int combatsSinceShop = 0;
        private int combatsSinceDungeon = 0;
        private int dungeonAttempts = 0;
        private int shopVisits = 0;
        private int abilitiesUnlocked = 0;
        private int totalCombats = 0;
        private int totalDeaths = 0;
        private int lastAbilityCheckLevel = 0; // Track last level we checked abilities to prevent infinite loops

        public Level15Strategy(string characterName, PlayerClass playerClass)
        {
            this.characterName = characterName;
            this.playerClass = playerClass;
        }

        public override (string name, PlayerClass playerClass) ChooseCharacterClass()
        {
            return (characterName, playerClass);
        }

        public override MainMenuChoice ChooseMainMenuAction(int combatCount, int level, int gold, int hp, int maxHp)
        {
            // Log progress milestones
            if (level > lastLoggedLevel)
            {
                Console.WriteLine($"\n🎯 MILESTONE: Reached Level {level}! [Gold: {gold}, HP: {hp}/{maxHp}]");
                Console.WriteLine($"   Stats: {totalCombats} combats, {dungeonAttempts} dungeons, {shopVisits} shop visits, {abilitiesUnlocked} abilities unlocked");
                lastLoggedLevel = level;
            }

            // Stop at level 15 - test complete!
            if (level >= 15)
            {
                Console.WriteLine($"\n✅ TEST COMPLETE: {characterName} reached level 15!");
                Console.WriteLine($"📊 Final Stats:");
                Console.WriteLine($"   • Total Combats: {totalCombats}");
                Console.WriteLine($"   • Dungeon Attempts: {dungeonAttempts}");
                Console.WriteLine($"   • Shop Visits: {shopVisits}");
                Console.WriteLine($"   • Abilities Unlocked: {abilitiesUnlocked}");
                Console.WriteLine($"   • Final Gold: {gold}");
                return MainMenuChoice.Exit;
            }

            // Rest if HP is below 60% and we have enough gold (costs 10)
            if (hp < maxHp * 0.6 && gold >= 10)
            {
                return MainMenuChoice.Rest;
            }

            // Check for ability unlocks every 2 levels starting at level 3
            // Only check ONCE per level to prevent infinite loops
            // Only check if we have reasonable gold (150+ for cheapest abilities)
            if (level >= 3 && level % 2 == 0 && gold >= 150 && lastAbilityCheckLevel != level)
            {
                lastAbilityCheckLevel = level; // Mark that we've checked this level
                return MainMenuChoice.UnlockAbilities;
            }

            // Visit shop every 8-10 combats if we have gold
            combatsSinceShop++;
            if (combatsSinceShop >= 8 && gold >= 100)
            {
                shopVisits++;
                combatsSinceShop = 0;
                return MainMenuChoice.Shop;
            }

            // Try dungeons every 6-8 combats starting at level 3
            combatsSinceDungeon++;
            if (level >= 3 && combatsSinceDungeon >= 6 && hp >= maxHp * 0.7)
            {
                dungeonAttempts++;
                combatsSinceDungeon = 0;
                return MainMenuChoice.Dungeon;
            }

            // Manage inventory occasionally
            if (combatCount % 15 == 0)
            {
                return MainMenuChoice.Inventory;
            }

            // Main activity: combat to gain XP
            totalCombats++;
            return MainMenuChoice.Combat;
        }

        public override CombatAction ChooseCombatAction(CombatState state)
        {
            // Emergency potion if very low HP
            if (state.PlayerCurrentHP < state.PlayerMaxHP * 0.25 && state.PlayerPotions > 0)
            {
                return new CombatAction { ActionType = CombatActionType.UsePotion };
            }

            // Flee if critically low HP and no potions
            if (state.PlayerCurrentHP < state.PlayerMaxHP * 0.15 && state.PlayerPotions == 0)
            {
                return new CombatAction { ActionType = CombatActionType.Flee };
            }

            // Smart ability usage: prioritize high-damage abilities
            var usableAbilities = state.AvailableAbilities
                .Where(a => a.CanUse && a.CurrentCooldown == 0)
                .OrderByDescending(a => a.ManaCost) // Higher mana cost usually means more powerful
                .ToList();

            // Use ability if we have mana and abilities available
            if (usableAbilities.Any() && state.PlayerCurrentMana >= 15)
            {
                // Prefer abilities when enemy HP is high or we have plenty of mana
                if (state.EnemyCurrentHP > state.EnemyMaxHP * 0.5 || usableAbilities.First().ManaCost <= state.PlayerCurrentMana / 2)
                {
                    return new CombatAction
                    {
                        ActionType = CombatActionType.UseAbility,
                        AbilityIndex = usableAbilities.First().Index
                    };
                }
            }

            // Default: basic attack (conserve mana)
            return new CombatAction { ActionType = CombatActionType.Attack };
        }

        public override int? GetPreferredSaveSlot()
        {
            // Use slot 1 for Warrior, 2 for Mage, 3 for Rogue
            return playerClass switch
            {
                PlayerClass.Warrior => 1,
                PlayerClass.Mage => 2,
                PlayerClass.Rogue => 3,
                _ => 1
            };
        }
    }

    /// <summary>
    /// Legacy alias for Level15Strategy - for backward compatibility
    /// </summary>
    public class Level10Strategy : Level15Strategy
    {
        public Level10Strategy(string characterName, PlayerClass playerClass)
            : base(characterName, playerClass)
        {
        }
    }

    /// <summary>
    /// Analytics tracker for deep gameplay analysis
    /// </summary>
    public class GameplayAnalytics
    {
        public PlayerClass Class { get; set; }
        public int RunNumber { get; set; }

        // Combat Metrics
        public int TotalCombats { get; set; }
        public int CombatsWon { get; set; }
        public int CombatsLost { get; set; }
        public int CombatsFled { get; set; }
        public int TotalDamageDealt { get; set; }
        public int TotalDamageTaken { get; set; }
        public int CriticalHits { get; set; }
        public int MissedAttacks { get; set; }
        public Dictionary<string, int> AbilityUsageCount { get; } = new();
        public Dictionary<string, int> StatusEffectsApplied { get; } = new();
        public List<string> DeathAnalysis { get; } = new();

        // Progression Metrics
        public int FinalLevel { get; set; }
        public Dictionary<int, int> CombatsPerLevel { get; } = new(); // Level -> Combat count
        public Dictionary<int, int> DungeonsPerLevel { get; } = new(); // Level -> Dungeon attempt count

        // Gold Tracking - Sources
        public int GoldFromStarting { get; set; } = 100;
        public int GoldFromCombat { get; set; }
        public int GoldFromDungeons { get; set; }
        public int GoldFromAchievements { get; set; }
        public int GoldFromShopSales { get; set; }
        public int TotalGoldEarned => GoldFromStarting + GoldFromCombat + GoldFromDungeons + GoldFromAchievements + GoldFromShopSales;

        // Gold Tracking - Expenditures
        public int GoldSpentOnPotions { get; set; }
        public int GoldSpentOnRest { get; set; }
        public int GoldSpentOnWeapons { get; set; }
        public int GoldSpentOnArmor { get; set; }
        public int GoldSpentOnAccessories { get; set; }
        public int GoldSpentOnAbilities { get; set; }
        public int TotalGoldSpent => GoldSpentOnPotions + GoldSpentOnRest + GoldSpentOnWeapons + GoldSpentOnArmor + GoldSpentOnAccessories + GoldSpentOnAbilities;

        public int FinalGold { get; set; }

        // Resource Management
        public int PotionsUsed { get; set; }
        public int TimesRested { get; set; }
        public int TimesNearDeath { get; set; } // HP < 20%
        public int TimesOutOfMana { get; set; } // Mana = 0

        // Equipment
        public int LootDropsReceived { get; set; }
        public Dictionary<string, int> LootByRarity { get; } = new();
        public int ItemsPurchased { get; set; }
        public int ShopVisits { get; set; }

        // Dungeons
        public int DungeonAttempts { get; set; }
        public int DungeonsCompleted { get; set; }
        public int DungeonsFailed { get; set; }
        public Dictionary<string, bool> DungeonResults { get; } = new();

        // Dungeon Failure Analysis
        public List<DungeonFailureInfo> DungeonFailures { get; } = new();

        // Abilities
        public int AbilitiesUnlocked { get; set; }
        public List<string> UnlockedAbilityNames { get; } = new();
        public List<AbilityStoreVisit> AbilityStoreVisits { get; } = new();

        // Equipment-Granted Abilities
        public int ItemsWithAbilitiesReceived { get; set; }
        public int ItemsWithAbilitiesEquipped { get; set; }
        public List<string> EquipmentAbilitiesUsed { get; } = new();

        // Observations & Insights
        public List<string> PositiveObservations { get; } = new();
        public List<string> NegativeObservations { get; } = new();
        public List<string> MissingInformation { get; } = new();
        public List<string> ImprovementSuggestions { get; } = new();
        public List<string> BalanceIssues { get; } = new();

        public void AddObservation(string category, string observation)
        {
            switch (category.ToLower())
            {
                case "positive":
                    PositiveObservations.Add($"[Lvl {FinalLevel}] {observation}");
                    break;
                case "negative":
                    NegativeObservations.Add($"[Lvl {FinalLevel}] {observation}");
                    break;
                case "missing":
                    MissingInformation.Add($"[Lvl {FinalLevel}] {observation}");
                    break;
                case "improvement":
                    ImprovementSuggestions.Add($"[Lvl {FinalLevel}] {observation}");
                    break;
                case "balance":
                    BalanceIssues.Add($"[Lvl {FinalLevel}] {observation}");
                    break;
            }
        }

        // Combat Damage Per Level
        public Dictionary<int, List<int>> DamageTakenPerLevel { get; } = new(); // Level -> List of damage taken per combat

        // Equipment Tracking
        public Dictionary<int, Dictionary<EquipmentSlot, EquipmentItem?>> EquippedGearByLevel { get; } = new(); // Level -> Equipped items

        public double GetWinRate() => TotalCombats > 0 ? (double)CombatsWon / TotalCombats * 100 : 0;
        public double GetFleeRate() => TotalCombats > 0 ? (double)CombatsFled / TotalCombats * 100 : 0;
        public double GetDeathRate() => TotalCombats > 0 ? (double)CombatsLost / TotalCombats * 100 : 0;
        public double GetCritRate() => (TotalDamageDealt > 0) ? (double)CriticalHits / (CombatsWon * 10) * 100 : 0; // Rough estimate
        public double GetDungeonSuccessRate() => DungeonAttempts > 0 ? (double)DungeonsCompleted / DungeonAttempts * 100 : 0;
        public double GetAverageDamagePerCombat() => CombatsWon > 0 ? (double)TotalDamageDealt / CombatsWon : 0;
        public double GetAverageDamageTakenPerCombat() => TotalCombats > 0 ? (double)TotalDamageTaken / TotalCombats : 0;
        public double GetGoldEfficiency() => TotalGoldEarned > 0 ? (double)TotalGoldSpent / TotalGoldEarned * 100 : 0;
    }

    /// <summary>
    /// Details about a dungeon failure for analysis
    /// </summary>
    public class DungeonFailureInfo
    {
        public string DungeonName { get; set; } = "";
        public int PlayerLevel { get; set; }
        public int PlayerHP { get; set; }
        public int PlayerMaxHP { get; set; }
        public int PlayerMana { get; set; }
        public int PlayerMaxMana { get; set; }
        public int PotionsAvailable { get; set; }
        public int EncounterNumber { get; set; } // Which encounter in the dungeon
        public bool WasBossFight { get; set; }
        public bool WasFlee { get; set; } // True if fled, false if died
        public int EnemyHPRemaining { get; set; }
        public int EnemyMaxHP { get; set; }
        public string FailureReason { get; set; } = "";
    }

    /// <summary>
    /// Details about an ability store visit for analysis
    /// </summary>
    public class AbilityStoreVisit
    {
        public int PlayerLevel { get; set; }
        public int PlayerGold { get; set; }
        public int AvailableAbilitiesCount { get; set; }
        public bool PurchasedAbility { get; set; }
        public string? PurchasedAbilityName { get; set; }
        public int? PurchasedAbilityCost { get; set; }
        public List<string> ReasonsNotPurchased { get; } = new();
        // Reasons: "No gold", "Level too low", "All already unlocked", "Can't afford cheapest"
    }

    /// <summary>
    /// Ultra-think strategy with comprehensive gameplay analysis.
    /// Tracks detailed metrics and generates insights about game systems.
    /// </summary>
    public class UltraThinkStrategy : AutomatedStrategy
    {
        private readonly string characterName;
        private readonly PlayerClass playerClass;
        private readonly GameplayAnalytics analytics;
        private int currentLevel = 1;
        private int lastLoggedLevel = 0;
        private int combatsSinceShop = 0;
        private int combatsSinceDungeon = 0;

        // Contextual state for analysis
        private int consecutiveCombatWins = 0;
        private int consecutiveCombatLosses = 0;
        private string? currentDungeon = null;
        private int currentDungeonEncounter = 0;
        private bool inDungeon = false;
        private int dungeonEntryHP = 0;
        private int dungeonEntryMaxHP = 0;
        private int dungeonEntryMana = 0;
        private int dungeonEntryMaxMana = 0;
        private int dungeonEntryPotions = 0;
        private int lowHealthCombats = 0;
        private int lowManaCombats = 0;

        // Current equipment state
        private Dictionary<EquipmentSlot, EquipmentItem?> currentlyEquipped = new();

        // Flags to trigger inventory management
        private bool shouldManageInventory = false;
        private int lastAbilityCheckLevel = 0; // Track last level we checked abilities to prevent infinite loops

        public UltraThinkStrategy(string characterName, PlayerClass playerClass, GameplayAnalytics analytics)
        {
            this.characterName = characterName;
            this.playerClass = playerClass;
            this.analytics = analytics;
            this.analytics.Class = playerClass;
        }

        public GameplayAnalytics GetAnalytics() => analytics;

        public Dictionary<EquipmentSlot, EquipmentItem?> GetCurrentlyEquipped() => currentlyEquipped;

        public void UpdateEquippedGear(Dictionary<EquipmentSlot, EquipmentItem?> equipped)
        {
            currentlyEquipped = new Dictionary<EquipmentSlot, EquipmentItem?>(equipped);

            // Track equipped gear by level
            if (!analytics.EquippedGearByLevel.ContainsKey(currentLevel))
            {
                analytics.EquippedGearByLevel[currentLevel] = new Dictionary<EquipmentSlot, EquipmentItem?>(equipped);
            }
        }

        public override (string name, PlayerClass playerClass) ChooseCharacterClass()
        {
            return (characterName, playerClass);
        }

        public override MainMenuChoice ChooseMainMenuAction(int combatCount, int level, int gold, int hp, int maxHp)
        {
            // Track level changes
            if (level > currentLevel)
            {
                // Initialize tracking for new level
                if (!analytics.CombatsPerLevel.ContainsKey(level))
                    analytics.CombatsPerLevel[level] = 0;
                if (!analytics.DungeonsPerLevel.ContainsKey(level))
                    analytics.DungeonsPerLevel[level] = 0;
            }

            currentLevel = level;
            analytics.FinalLevel = level;
            analytics.FinalGold = gold;

            // Log progress milestones
            if (level > lastLoggedLevel)
            {
                Console.WriteLine($"\n🎯 MILESTONE: {characterName} reached Level {level}! [Gold: {gold}, HP: {hp}/{maxHp}]");
                LogAnalysis($"Level up to {level}: Currently at {hp}/{maxHp} HP, {gold} gold");
                AnalyzeHealthState(hp, maxHp, level);
                lastLoggedLevel = level;
            }

            // Stop at level 15 - test complete!
            if (level >= 15)
            {
                Console.WriteLine($"\n✅ TEST COMPLETE: {characterName} reached level 15!");
                return MainMenuChoice.Exit;
            }

            // PRIORITY 1: Manage inventory FIRST - equip loot before doing anything else
            // This ensures we're properly geared before shop/dungeon/combat
            if (shouldManageInventory)
            {
                shouldManageInventory = false; // Reset flag
                LogAnalysis("Managing inventory - checking for equipment upgrades");
                return MainMenuChoice.Inventory;
            }

            // PRIORITY 2: Rest if HP is below 60% and we have enough gold (costs 10)
            double hpPercent = (double)hp / maxHp;
            if (hpPercent < 0.6 && gold >= 10)
            {
                analytics.TimesRested++;
                analytics.GoldSpentOnRest += 10; // Rest costs 10 gold
                if (hpPercent < 0.3)
                {
                    LogAnalysis($"Emergency rest needed at {hpPercent:P0} HP - combat may be too dangerous");
                    analytics.AddObservation("negative", $"Frequently at critically low HP ({hpPercent:P0})");
                }
                return MainMenuChoice.Rest;
            }

            // If HP is low but can't afford rest, note this as a problem
            if (hpPercent < 0.6 && gold < 10)
            {
                LogAnalysis($"Need to rest at {hpPercent:P0} HP but only have {gold} gold (need 10) - forced to continue");
                analytics.AddObservation("balance", $"Stuck at low HP with insufficient gold for rest - economy too tight");
            }

            // PRIORITY 2.5: Check for ability unlocks every 2 levels starting at level 3
            // Only check ONCE per level to prevent infinite loops if no affordable abilities
            // Only check if we have reasonable gold (150+ for cheapest abilities)
            // This gives players meaningful progression rewards
            if (level >= 3 && level % 2 == 0 && gold >= 150 && lastAbilityCheckLevel != level)
            {
                lastAbilityCheckLevel = level; // Mark that we've checked this level
                LogAnalysis($"Level {level} reached - checking for ability unlock opportunities with {gold} gold");
                return MainMenuChoice.UnlockAbilities;
            }

            // PRIORITY 3: Visit shop every 8-10 combats if we have gold
            combatsSinceShop++;
            if (combatsSinceShop >= 8 && gold >= 100)
            {
                analytics.ShopVisits++;
                combatsSinceShop = 0;
                LogAnalysis($"Visiting shop with {gold} gold - equipment upgrade opportunity");
                return MainMenuChoice.Shop;
            }

            // PRIORITY 4: Try dungeons every 6-8 combats starting at level 3
            combatsSinceDungeon++;
            if (level >= 3 && combatsSinceDungeon >= 6 && hpPercent >= 0.7)
            {
                analytics.DungeonAttempts++;
                combatsSinceDungeon = 0;

                // Track dungeons per level
                if (!analytics.DungeonsPerLevel.ContainsKey(currentLevel))
                    analytics.DungeonsPerLevel[currentLevel] = 0;
                analytics.DungeonsPerLevel[currentLevel]++;

                // Track dungeon entry state (we'll get the dungeon name from the event later)
                // For now, just mark that we're attempting a dungeon
                TrackDungeonEntry("Dungeon", hp, maxHp, 0, 0, 0); // We don't have mana/potions here

                LogAnalysis($"Attempting dungeon at level {level} with {hpPercent:P0} HP");
                return MainMenuChoice.Dungeon;
            }

            // Main activity: combat to gain XP
            return MainMenuChoice.Combat;
        }

        public void TriggerInventoryCheck(string reason)
        {
            shouldManageInventory = true;
            LogAnalysis($"Inventory check scheduled: {reason}");
        }

        public void TrackCombatStart()
        {
            analytics.TotalCombats++;

            // Track combats per level
            if (!analytics.CombatsPerLevel.ContainsKey(currentLevel))
                analytics.CombatsPerLevel[currentLevel] = 0;
            analytics.CombatsPerLevel[currentLevel]++;
        }

        public override CombatAction ChooseCombatAction(CombatState state)
        {
            double hpPercent = (double)state.PlayerCurrentHP / state.PlayerMaxHP;
            double manaPercent = (double)state.PlayerCurrentMana / state.PlayerMaxMana;
            double enemyHpPercent = (double)state.EnemyCurrentHP / state.EnemyMaxHP;

            // Track critical states
            if (hpPercent < 0.2)
            {
                analytics.TimesNearDeath++;
                lowHealthCombats++;
                if (lowHealthCombats > 3)
                {
                    analytics.AddObservation("balance", $"Frequently in near-death state (< 20% HP) in combat - may indicate difficulty spike");
                }
            }

            if (manaPercent == 0)
            {
                analytics.TimesOutOfMana++;
                lowManaCombats++;
            }

            // Emergency potion if very low HP
            if (hpPercent < 0.25 && state.PlayerPotions > 0)
            {
                analytics.PotionsUsed++;
                LogAnalysis($"Emergency potion use at {hpPercent:P0} HP");
                return new CombatAction { ActionType = CombatActionType.UsePotion };
            }

            // Flee if critically low HP and no potions
            if (hpPercent < 0.15 && state.PlayerPotions == 0)
            {
                analytics.CombatsFled++;
                LogAnalysis($"Fleeing combat - critically low HP ({hpPercent:P0}) and no potions");
                analytics.AddObservation("missing", "No way to know if fleeing will succeed - success rate info would be helpful");
                return new CombatAction { ActionType = CombatActionType.Flee };
            }

            // Smart ability usage
            var usableAbilities = state.AvailableAbilities
                .Where(a => a.CanUse && a.CurrentCooldown == 0)
                .OrderByDescending(a => a.ManaCost)
                .ToList();

            // Use ability if we have mana and abilities available
            if (usableAbilities.Any() && state.PlayerCurrentMana >= 15)
            {
                // Prefer abilities when enemy HP is high or we have plenty of mana
                if (enemyHpPercent > 0.5 || usableAbilities.First().ManaCost <= state.PlayerCurrentMana / 2)
                {
                    var ability = usableAbilities.First();

                    // Track ability usage
                    if (!analytics.AbilityUsageCount.ContainsKey(ability.Name))
                        analytics.AbilityUsageCount[ability.Name] = 0;
                    analytics.AbilityUsageCount[ability.Name]++;

                    return new CombatAction
                    {
                        ActionType = CombatActionType.UseAbility,
                        AbilityIndex = ability.Index
                    };
                }
            }
            else if (manaPercent < 0.2 && usableAbilities.Any())
            {
                LogAnalysis($"Low mana ({manaPercent:P0}) - forced to basic attack despite abilities available");
                analytics.AddObservation("negative", "Mana constraints limit ability usage - may need mana regeneration improvements");
            }

            // Default: basic attack
            return new CombatAction { ActionType = CombatActionType.Attack };
        }

        public override int? GetPreferredSaveSlot()
        {
            return playerClass switch
            {
                PlayerClass.Warrior => 1,
                PlayerClass.Mage => 2,
                PlayerClass.Rogue => 3,
                _ => 1
            };
        }

        private void LogAnalysis(string thought)
        {
            Console.WriteLine($"  💭 [Analysis] {thought}");
        }

        private void AnalyzeHealthState(int hp, int maxHp, int level)
        {
            double hpPercent = (double)hp / maxHp;

            if (hpPercent < 0.3)
            {
                analytics.AddObservation("negative", $"Leveled up while at low HP ({hpPercent:P0}) - indicates dangerous progression");
            }
            else if (hpPercent > 0.9)
            {
                analytics.AddObservation("positive", $"Leveled up at high HP ({hpPercent:P0}) - safe progression");
            }
        }

        public void AnalyzeCombatResult(bool victory, int damageDealt, int damageTaken)
        {
            // Track damage taken per level
            if (!analytics.DamageTakenPerLevel.ContainsKey(currentLevel))
                analytics.DamageTakenPerLevel[currentLevel] = new List<int>();
            analytics.DamageTakenPerLevel[currentLevel].Add(damageTaken);

            if (victory)
            {
                analytics.CombatsWon++;
                consecutiveCombatWins++;
                consecutiveCombatLosses = 0;

                if (consecutiveCombatWins >= 10)
                {
                    analytics.AddObservation("balance", $"Won {consecutiveCombatWins} consecutive combats - may be too easy");
                }
            }
            else
            {
                analytics.CombatsLost++;
                consecutiveCombatLosses++;
                consecutiveCombatWins = 0;

                if (consecutiveCombatLosses >= 3)
                {
                    analytics.AddObservation("balance", $"Lost {consecutiveCombatLosses} consecutive combats - may be too difficult");
                }

                analytics.DeathAnalysis.Add($"Level {currentLevel}: Lost combat after {damageDealt} damage dealt, {damageTaken} damage taken");
            }

            analytics.TotalDamageDealt += damageDealt;
            analytics.TotalDamageTaken += damageTaken;
        }

        public void AnalyzeAbilityUnlock(string abilityName, int cost)
        {
            analytics.AbilitiesUnlocked++;
            analytics.UnlockedAbilityNames.Add(abilityName);
            analytics.GoldSpentOnAbilities += cost;

            LogAnalysis($"Unlocked ability: {abilityName} for {cost} gold");
            analytics.AddObservation("positive", $"Unlocked {abilityName} - ability progression feels rewarding");
        }

        public void AnalyzeLootDrop(string itemName, string rarity)
        {
            analytics.LootDropsReceived++;

            if (!analytics.LootByRarity.ContainsKey(rarity))
                analytics.LootByRarity[rarity] = 0;
            analytics.LootByRarity[rarity]++;

            if (rarity == "Legendary" || rarity == "Epic")
            {
                LogAnalysis($"Rare loot drop: {rarity} {itemName} - exciting moment!");
                analytics.AddObservation("positive", $"Received {rarity} item - loot system creates exciting moments");
            }
        }

        public void AnalyzeShopPurchase(string itemName, int cost)
        {
            analytics.ItemsPurchased++;

            // Categorize expenditure by item type
            string itemLower = itemName.ToLower();
            if (itemLower.Contains("sword") || itemLower.Contains("axe") || itemLower.Contains("mace") ||
                itemLower.Contains("staff") || itemLower.Contains("dagger") || itemLower.Contains("bow"))
            {
                analytics.GoldSpentOnWeapons += cost;
            }
            else if (itemLower.Contains("armor") || itemLower.Contains("helm") || itemLower.Contains("chest") ||
                     itemLower.Contains("greaves") || itemLower.Contains("boots") || itemLower.Contains("hood") ||
                     itemLower.Contains("slippers") || itemLower.Contains("gauntlets"))
            {
                analytics.GoldSpentOnArmor += cost;
            }
            else if (itemLower.Contains("ring") || itemLower.Contains("amulet") || itemLower.Contains("talisman") ||
                     itemLower.Contains("band") || itemLower.Contains("orb") || itemLower.Contains("circle") ||
                     itemLower.Contains("medallion"))
            {
                analytics.GoldSpentOnAccessories += cost;
            }
            else if (itemLower.Contains("potion"))
            {
                analytics.GoldSpentOnPotions += cost;
            }

            LogAnalysis($"Purchased {itemName} for {cost} gold");
        }

        public void TrackRestExpenditure(int cost)
        {
            analytics.GoldSpentOnRest += cost;
        }

        public void TrackDungeonEntry(string dungeonName, int hp, int maxHp, int mana, int maxMana, int potions)
        {
            currentDungeon = dungeonName;
            currentDungeonEncounter = 0;
            inDungeon = true;
            dungeonEntryHP = hp;
            dungeonEntryMaxHP = maxHp;
            dungeonEntryMana = mana;
            dungeonEntryMaxMana = maxMana;
            dungeonEntryPotions = potions;
        }

        public void TrackDungeonCombatStart(bool isBoss)
        {
            if (inDungeon)
            {
                currentDungeonEncounter++;
            }
        }

        public void TrackDungeonFailure(bool wasFlee, int enemyHP, int enemyMaxHP, string failureReason)
        {
            if (inDungeon && currentDungeon != null)
            {
                var failure = new DungeonFailureInfo
                {
                    DungeonName = currentDungeon,
                    PlayerLevel = currentLevel,
                    PlayerHP = dungeonEntryHP,
                    PlayerMaxHP = dungeonEntryMaxHP,
                    PlayerMana = dungeonEntryMana,
                    PlayerMaxMana = dungeonEntryMaxMana,
                    PotionsAvailable = dungeonEntryPotions,
                    EncounterNumber = currentDungeonEncounter,
                    WasBossFight = currentDungeonEncounter >= 4, // Assuming boss is encounter 4+
                    WasFlee = wasFlee,
                    EnemyHPRemaining = enemyHP,
                    EnemyMaxHP = enemyMaxHP,
                    FailureReason = failureReason
                };

                analytics.DungeonFailures.Add(failure);
            }
        }

        public void AnalyzeDungeonResult(string dungeonName, bool success)
        {
            if (success)
            {
                analytics.DungeonsCompleted++;
                analytics.AddObservation("positive", $"Completed {dungeonName} - dungeon system provides good challenge");
            }
            else
            {
                analytics.DungeonsFailed++;
                analytics.AddObservation("negative", $"Failed {dungeonName} - may need better preparation guidance");
            }

            analytics.DungeonResults[dungeonName] = success;

            // Reset dungeon tracking
            inDungeon = false;
            currentDungeon = null;
            currentDungeonEncounter = 0;
        }

        public void AnalyzeStatusEffect(string effectName, string targetType)
        {
            string key = $"{effectName} on {targetType}";
            if (!analytics.StatusEffectsApplied.ContainsKey(key))
                analytics.StatusEffectsApplied[key] = 0;
            analytics.StatusEffectsApplied[key]++;
        }

        public void AnalyzeCriticalHit()
        {
            analytics.CriticalHits++;
        }

        public void AnalyzeMissedAttack()
        {
            analytics.MissedAttacks++;
        }

        public void TrackItemWithAbilityReceived(string itemName, int abilityCount)
        {
            analytics.ItemsWithAbilitiesReceived++;
            LogAnalysis($"Received item with {abilityCount} ability grant(s): {itemName}");
            analytics.AddObservation("positive", $"Found equipment with {abilityCount} granted ability(ies) - loot system creates excitement");
        }

        public void TrackItemWithAbilityEquipped(string itemName, int abilityCount)
        {
            analytics.ItemsWithAbilitiesEquipped++;
            LogAnalysis($"Equipped item with {abilityCount} ability grant(s): {itemName}");
            analytics.AddObservation("positive", $"Equipped item grants {abilityCount} ability(ies): {itemName} - equipment feels more impactful");
        }

        public void FinalizeAnalysis()
        {
            // Final summary observations
            LogAnalysis("=== Final Analysis ===");
            LogAnalysis($"Total Combats: {analytics.TotalCombats}");
            LogAnalysis($"Win Rate: {analytics.GetWinRate():F1}%");
            LogAnalysis($"Dungeon Success: {analytics.GetDungeonSuccessRate():F1}%");
            LogAnalysis($"Gold Efficiency: {analytics.GetGoldEfficiency():F1}%");
            LogAnalysis($"Near-Death Situations: {analytics.TimesNearDeath}");
            LogAnalysis($"Potions Used: {analytics.PotionsUsed}");
            LogAnalysis($"Abilities Unlocked: {analytics.AbilitiesUnlocked}");

            // Add final observations based on overall performance
            if (analytics.GetWinRate() > 90)
            {
                analytics.AddObservation("balance", "Very high win rate - combat may be too easy");
            }
            else if (analytics.GetWinRate() < 60)
            {
                analytics.AddObservation("balance", "Low win rate - combat may be too difficult");
            }

            if (analytics.TimesNearDeath > 20)
            {
                analytics.AddObservation("balance", "Frequently in near-death situations - health management is challenging");
            }

            if (analytics.PotionsUsed > 30)
            {
                analytics.AddObservation("balance", "Heavy potion usage - may indicate insufficient healing/regeneration");
            }

            if (analytics.AbilitiesUnlocked < 3)
            {
                analytics.AddObservation("negative", "Few abilities unlocked - gold economy or unlock costs may need adjustment");
            }
        }
    }
}
