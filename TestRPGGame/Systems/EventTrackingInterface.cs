using System;
using System.Collections.Generic;
using TestRPGGame.Entities.Player;
using TestRPGGame.Equipment;
using TestRPGGame.Interfaces;

namespace TestRPGGame.Systems
{
    /// <summary>
    /// Decorator for IGameInterface that tracks statistics from events.
    /// Implements the Decorator pattern - wraps another IGameInterface and forwards
    /// all calls while collecting statistics from events.
    /// </summary>
    public class EventTrackingInterface : IGameInterface
    {
        private readonly IGameInterface _inner;
        private readonly StatisticsTracker _tracker;

        public EventTrackingInterface(IGameInterface inner, StatisticsTracker tracker)
        {
            _inner = inner;
            _tracker = tracker;
        }

        // Intercept OnEvent to track statistics, then forward to inner interface
        public void OnEvent<T>(T gameEvent) where T : class
        {
            // Track the event
            _tracker.TrackEvent(gameEvent);

            // Forward to inner interface
            _inner.OnEvent(gameEvent);
        }

        // Forward all other methods directly to inner interface
        public (string name, PlayerClass playerClass) RequestCharacterCreation()
            => _inner.RequestCharacterCreation();

        public (int slotNumber, bool isNewCharacter) RequestSaveSlotSelection(List<InterfaceSaveSlotInfo> slots)
            => _inner.RequestSaveSlotSelection(slots);

        public int RequestSaveSlot(List<InterfaceSaveSlotInfo> slots)
            => _inner.RequestSaveSlot(slots);

        public MainMenuChoice RequestMainMenuChoice(string playerName, int level, PlayerClass playerClass, int currentHP, int maxHP, int gold)
            => _inner.RequestMainMenuChoice(playerName, level, playerClass, currentHP, maxHP, gold);

        public int RequestDungeonSelection(List<DungeonSelectionInfo> dungeons)
            => _inner.RequestDungeonSelection(dungeons);

        public bool RequestConfirmation(string message)
            => _inner.RequestConfirmation(message);

        public CombatAction RequestCombatAction(CombatState state)
            => _inner.RequestCombatAction(state);

        public int RequestAbilitySelection(List<AbilityInfo> abilities)
            => _inner.RequestAbilitySelection(abilities);

        public ShopAction RequestShopAction(List<ShopItemInfo> forSale, List<EquipmentItem> inventory, int playerGold)
            => _inner.RequestShopAction(forSale, inventory, playerGold);

        public InventoryAction RequestInventoryAction(List<EquipmentItem> backpack, Dictionary<EquipmentSlot, EquipmentItem?> equipped)
            => _inner.RequestInventoryAction(backpack, equipped);

        public int RequestAbilityUnlock(List<AbilityInfo> lockedAbilities, int playerGold, int playerLevel)
            => _inner.RequestAbilityUnlock(lockedAbilities, playerGold, playerLevel);

        public void DisplayCharacterSheet(CharacterSheetInfo info)
            => _inner.DisplayCharacterSheet(info);

        public void DisplayStatistics(StatisticsInfo info)
            => _inner.DisplayStatistics(info);

        public void DisplayAchievements(AchievementDisplayInfo info)
            => _inner.DisplayAchievements(info);

        public void WaitForAcknowledgment()
            => _inner.WaitForAcknowledgment();

        public int RequestEncounterChoice(string description, List<string> choices)
            => _inner.RequestEncounterChoice(description, choices);
    }
}
