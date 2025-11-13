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
                                ultraThink4.AnalyzeLootDrop(e.LootDropped.Name, e.LootDropped.Rarity.ToString());
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
                            analytics.TotalGoldEarned += e.GoldEarned;
                        }
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

                    if (strategy is UltraThinkStrategy ultraThink6)
                    {
                        // DungeonName will need to be tracked from DungeonEnteredEvent
                        ultraThink6.AnalyzeDungeonResult("Dungeon", e.Success);
                        if (e.Success)
                            ultraThink6.GetAnalytics().TotalGoldEarned += e.TotalGoldEarned;
                    }
                    break;

                case GameEvents.GameSavedEvent e:
                    if (e.Success)
                        Log($"  💾 Game saved to slot {e.SlotNumber}");
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
            // Simple strategy: buy if affordable and good value
            var affordableItems = forSale.Where(i => i.Price <= playerGold).ToList();
            if (affordableItems.Any())
            {
                var bestItem = affordableItems.OrderByDescending(i => i.Level).First();
                Log($"  Shop: Buy {bestItem.Name} for {bestItem.Price} gold");

                if (strategy is UltraThinkStrategy ultraThink)
                    ultraThink.AnalyzeShopPurchase(bestItem.Name, bestItem.Price);

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

        public void DisplayCharacterSheet(CharacterSheetInfo info)
        {
            Log($"  Character Sheet: {info.Name} (Lv {info.Level} {info.Class}) - {info.CurrentHP}/{info.MaxHP} HP, {info.Gold} gold");
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

                if (strategy is UltraThinkStrategy ultraThink)
                    ultraThink.AnalyzeAbilityUnlock(ability.Name, ability.PurchaseCost);

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

            // TODO: Ability unlocking system not yet implemented - skip for now
            // if (level >= 3 && (level % 3 == 0 || gold > 500))
            // {
            //     return MainMenuChoice.UnlockAbilities;
            // }

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
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

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
        public TimeSpan TimeToLevel5 { get; set; }
        public TimeSpan TimeToLevel10 { get; set; }
        public TimeSpan TimeToLevel15 { get; set; }
        public int TotalGoldEarned { get; set; }
        public int TotalGoldSpent { get; set; }
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

        // Abilities
        public int AbilitiesUnlocked { get; set; }
        public List<string> UnlockedAbilityNames { get; } = new();

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

        public double GetWinRate() => TotalCombats > 0 ? (double)CombatsWon / TotalCombats * 100 : 0;
        public double GetCritRate() => (TotalDamageDealt > 0) ? (double)CriticalHits / (CombatsWon * 10) * 100 : 0; // Rough estimate
        public double GetDungeonSuccessRate() => DungeonAttempts > 0 ? (double)DungeonsCompleted / DungeonAttempts * 100 : 0;
        public double GetAverageDamagePerCombat() => CombatsWon > 0 ? (double)TotalDamageDealt / CombatsWon : 0;
        public double GetGoldEfficiency() => TotalGoldEarned > 0 ? (double)TotalGoldSpent / TotalGoldEarned * 100 : 0;
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
        private bool levelMilestone5Logged = false;
        private bool levelMilestone10Logged = false;
        private bool levelMilestone15Logged = false;

        // Contextual state for analysis
        private int consecutiveCombatWins = 0;
        private int consecutiveCombatLosses = 0;
        private string? currentDungeon = null;
        private int currentDungeonCombats = 0;
        private int lowHealthCombats = 0;
        private int lowManaCombats = 0;

        public UltraThinkStrategy(string characterName, PlayerClass playerClass, GameplayAnalytics analytics)
        {
            this.characterName = characterName;
            this.playerClass = playerClass;
            this.analytics = analytics;
            this.analytics.Class = playerClass;
            this.analytics.StartTime = DateTime.Now;
        }

        public GameplayAnalytics GetAnalytics() => analytics;

        public override (string name, PlayerClass playerClass) ChooseCharacterClass()
        {
            return (characterName, playerClass);
        }

        public override MainMenuChoice ChooseMainMenuAction(int combatCount, int level, int gold, int hp, int maxHp)
        {
            currentLevel = level;
            analytics.FinalLevel = level;
            analytics.FinalGold = gold;

            // Track level progression time
            var elapsed = DateTime.Now - analytics.StartTime;
            if (level >= 5 && !levelMilestone5Logged)
            {
                analytics.TimeToLevel5 = elapsed;
                levelMilestone5Logged = true;
                LogUltraThink($"📊 Level 5 Analysis: Reached in {elapsed.TotalMinutes:F1} minutes");
                AnalyzeLevelingSpeed(5, elapsed);
            }
            if (level >= 10 && !levelMilestone10Logged)
            {
                analytics.TimeToLevel10 = elapsed;
                levelMilestone10Logged = true;
                LogUltraThink($"📊 Level 10 Analysis: Reached in {elapsed.TotalMinutes:F1} minutes");
                AnalyzeLevelingSpeed(10, elapsed);
            }
            if (level >= 15 && !levelMilestone15Logged)
            {
                analytics.TimeToLevel15 = elapsed;
                analytics.EndTime = DateTime.Now;
                levelMilestone15Logged = true;
                LogUltraThink($"📊 Level 15 Analysis: Reached in {elapsed.TotalMinutes:F1} minutes");
                AnalyzeLevelingSpeed(15, elapsed);
            }

            // Log progress milestones
            if (level > lastLoggedLevel)
            {
                Console.WriteLine($"\n🎯 MILESTONE: {characterName} reached Level {level}! [Gold: {gold}, HP: {hp}/{maxHp}]");
                LogUltraThink($"Level up to {level}: Currently at {hp}/{maxHp} HP, {gold} gold");
                AnalyzeHealthState(hp, maxHp, level);
                lastLoggedLevel = level;
            }

            // Stop at level 15 - test complete!
            if (level >= 15)
            {
                Console.WriteLine($"\n✅ TEST COMPLETE: {characterName} reached level 15!");
                return MainMenuChoice.Exit;
            }

            // Rest if HP is below 60% and we have enough gold (costs 10)
            double hpPercent = (double)hp / maxHp;
            if (hpPercent < 0.6 && gold >= 10)
            {
                analytics.TimesRested++;
                if (hpPercent < 0.3)
                {
                    LogUltraThink($"Emergency rest needed at {hpPercent:P0} HP - combat may be too dangerous");
                    analytics.AddObservation("negative", $"Frequently at critically low HP ({hpPercent:P0})");
                }
                return MainMenuChoice.Rest;
            }

            // If HP is low but can't afford rest, note this as a problem
            if (hpPercent < 0.6 && gold < 10)
            {
                LogUltraThink($"Need to rest at {hpPercent:P0} HP but only have {gold} gold (need 10) - forced to continue");
                analytics.AddObservation("balance", $"Stuck at low HP with insufficient gold for rest - economy too tight");
            }

            // TODO: Ability unlocking system not yet implemented - skip for now
            // if (level >= 3 && (level % 3 == 0 || gold > 500))
            // {
            //     return MainMenuChoice.UnlockAbilities;
            // }

            // Visit shop every 8-10 combats if we have gold
            combatsSinceShop++;
            if (combatsSinceShop >= 8 && gold >= 100)
            {
                analytics.ShopVisits++;
                combatsSinceShop = 0;
                LogUltraThink($"Visiting shop with {gold} gold - equipment upgrade opportunity");
                return MainMenuChoice.Shop;
            }

            // Try dungeons every 6-8 combats starting at level 3
            combatsSinceDungeon++;
            if (level >= 3 && combatsSinceDungeon >= 6 && hpPercent >= 0.7)
            {
                analytics.DungeonAttempts++;
                combatsSinceDungeon = 0;
                LogUltraThink($"Attempting dungeon at level {level} with {hpPercent:P0} HP");
                return MainMenuChoice.Dungeon;
            }

            // Manage inventory occasionally
            if (combatCount % 15 == 0 && combatCount > 0)
            {
                LogUltraThink("Checking inventory - equipment management");
                return MainMenuChoice.Inventory;
            }

            // Main activity: combat to gain XP
            analytics.TotalCombats++;
            return MainMenuChoice.Combat;
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
                LogUltraThink($"Emergency potion use at {hpPercent:P0} HP");
                return new CombatAction { ActionType = CombatActionType.UsePotion };
            }

            // Flee if critically low HP and no potions
            if (hpPercent < 0.15 && state.PlayerPotions == 0)
            {
                analytics.CombatsFled++;
                LogUltraThink($"Fleeing combat - critically low HP ({hpPercent:P0}) and no potions");
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
                LogUltraThink($"Low mana ({manaPercent:P0}) - forced to basic attack despite abilities available");
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

        private void LogUltraThink(string thought)
        {
            Console.WriteLine($"  💭 [UltraThink] {thought}");
        }

        private void AnalyzeLevelingSpeed(int level, TimeSpan elapsed)
        {
            double minutesPerLevel = elapsed.TotalMinutes / level;

            if (minutesPerLevel < 1.0)
            {
                analytics.AddObservation("positive", $"Fast leveling pace ({minutesPerLevel:F1} min/level) - good XP balance");
            }
            else if (minutesPerLevel > 3.0)
            {
                analytics.AddObservation("negative", $"Slow leveling pace ({minutesPerLevel:F1} min/level) - may feel grindy");
                analytics.AddObservation("balance", $"Consider increasing XP rewards or reducing level requirements");
            }
            else
            {
                analytics.AddObservation("positive", $"Balanced leveling pace ({minutesPerLevel:F1} min/level)");
            }
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
            analytics.TotalGoldSpent += cost;

            LogUltraThink($"Unlocked ability: {abilityName} for {cost} gold");
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
                LogUltraThink($"Rare loot drop: {rarity} {itemName} - exciting moment!");
                analytics.AddObservation("positive", $"Received {rarity} item - loot system creates exciting moments");
            }
        }

        public void AnalyzeShopPurchase(string itemName, int cost)
        {
            analytics.ItemsPurchased++;
            analytics.TotalGoldSpent += cost;

            LogUltraThink($"Purchased {itemName} for {cost} gold");
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

        public void FinalizeAnalysis()
        {
            analytics.EndTime = DateTime.Now;

            // Final summary observations
            LogUltraThink("=== Final Analysis ===");
            LogUltraThink($"Win Rate: {analytics.GetWinRate():F1}%");
            LogUltraThink($"Dungeon Success: {analytics.GetDungeonSuccessRate():F1}%");
            LogUltraThink($"Gold Efficiency: {analytics.GetGoldEfficiency():F1}%");
            LogUltraThink($"Near-Death Situations: {analytics.TimesNearDeath}");
            LogUltraThink($"Potions Used: {analytics.PotionsUsed}");
            LogUltraThink($"Abilities Unlocked: {analytics.AbilitiesUnlocked}");

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
