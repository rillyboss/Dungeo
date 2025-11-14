using TestRPGGame.Interfaces;
using TestRPGGame.Equipment;
using TestRPGGame.Entities.Player;
using TestRPGGame.Systems;

namespace TestRPGGame.Blazor.Services;

/// <summary>
/// Blazor implementation of IGameInterface - translates game events to UI updates
/// This is a STUB implementation that will be progressively enhanced as we build the UI components
/// </summary>
public class BlazorInterface : IGameInterface
{
    private readonly GameStateService _state;

    public BlazorInterface(GameStateService state)
    {
        _state = state;
    }

    // Generic event handler - routes all events through state service
    public void OnEvent<T>(T gameEvent) where T : class
    {
        // Log the event
        _state.AddEventLog($"[Event] {gameEvent.GetType().Name}");
        _state.NotifyStateChanged();
    }

    public (int slotNumber, bool isNewCharacter) RequestSaveSlotSelection(List<InterfaceSaveSlotInfo> slots)
    {
        // TODO: Implement save slot selection UI
        // For now, return new character in slot 1
        return (1, true);
    }

    public (string name, PlayerClass playerClass) RequestCharacterCreation()
    {
        // TODO: Implement character creation UI
        // For now, return default
        return ("Hero", PlayerClass.Warrior);
    }

    public MainMenuChoice RequestMainMenuChoice(string playerName, int level, PlayerClass playerClass, int currentHP, int maxHP, int gold)
    {
        // TODO: Implement main menu UI
        // For now, return Combat
        return MainMenuChoice.Combat;
    }

    public CombatAction RequestCombatAction(CombatState state)
    {
        // TODO: Implement combat UI
        // For now, return basic attack
        return new CombatAction
        {
            ActionType = CombatActionType.Attack
        };
    }

    public int RequestAbilitySelection(List<AbilityInfo> abilities)
    {
        // TODO: Implement ability selection UI
        // For now, return first ability
        return abilities.Count > 0 ? 0 : -1;
    }

    public ShopAction RequestShopAction(List<ShopItemInfo> forSale, List<EquipmentItem> inventory, int playerGold)
    {
        // TODO: Implement shop UI
        return new ShopAction
        {
            ActionType = ShopActionType.Exit
        };
    }

    public InventoryAction RequestInventoryAction(List<EquipmentItem> backpack, Dictionary<EquipmentSlot, EquipmentItem?> equipped)
    {
        // TODO: Implement inventory UI
        return new InventoryAction
        {
            ActionType = InventoryActionType.Exit
        };
    }

    public void DisplayCharacterSheet(CharacterSheetInfo info)
    {
        // TODO: Implement character sheet UI
        _state.AddEventLog($"Character Sheet for {info.Name}");
    }

    public int RequestDungeonSelection(List<DungeonSelectionInfo> dungeons)
    {
        // TODO: Implement dungeon selection UI
        // For now, return first available dungeon
        for (int i = 0; i < dungeons.Count; i++)
        {
            if (dungeons[i].CanEnter)
                return i;
        }
        return -1;
    }

    public int RequestAbilityUnlock(List<AbilityInfo> lockedAbilities, int playerGold, int playerLevel)
    {
        // TODO: Implement ability unlock UI
        return -1; // Cancel for now
    }

    public bool RequestConfirmation(string message)
    {
        // TODO: Implement confirmation dialog
        _state.AddEventLog($"Confirmation: {message}");
        return true;
    }

    public int RequestSaveSlot(List<InterfaceSaveSlotInfo> slots)
    {
        // TODO: Implement save slot selection UI
        return 1; // Default to slot 1
    }

    public void WaitForAcknowledgment()
    {
        // In web UI, this is handled by component state
        // No blocking needed
    }

    public void DisplayStatistics(StatisticsInfo info)
    {
        // TODO: Implement statistics UI
        _state.AddEventLog("Statistics displayed");
    }

    public void DisplayAchievements(AchievementDisplayInfo info)
    {
        // TODO: Implement achievements UI
        _state.AddEventLog($"Achievements: {info.UnlockedAchievements}/{info.TotalAchievements}");
    }

    public int RequestEncounterChoice(string description, List<string> choices)
    {
        // TODO: Implement encounter choice UI
        _state.AddEventLog($"Encounter: {description}");
        return 0; // Default to first choice
    }
}
