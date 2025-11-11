using System;
using System.Collections.Generic;
using TestRPGGame.Abilities;
using TestRPGGame.Equipment;
using TestRPGGame.Entities.Player;

namespace TestRPGGame.Interfaces
{
    /// <summary>
    /// Contract for game interfaces. The game core uses this to request input
    /// and publish output. Different implementations can handle this differently
    /// (console UI, automated AI, web API, Unity UI, etc.)
    /// </summary>
    public interface IGameInterface
    {
        // Event publishing - game notifies interface of state changes
        void OnEvent<T>(T gameEvent) where T : class;

        // Input requests - game asks interface for player decisions
        // These return the player's choice

        /// <summary>
        /// Ask player to select from save slots or create new character
        /// Returns: (slotNumber, isNewCharacter)
        /// </summary>
        (int slotNumber, bool isNewCharacter) RequestSaveSlotSelection(List<InterfaceSaveSlotInfo> slots);

        /// <summary>
        /// Ask player for character name and class
        /// </summary>
        (string name, PlayerClass playerClass) RequestCharacterCreation();

        /// <summary>
        /// Display main menu and get player's choice
        /// </summary>
        MainMenuChoice RequestMainMenuChoice(string playerName, int level, PlayerClass playerClass, int currentHP, int maxHP, int gold);

        /// <summary>
        /// Request combat action from player
        /// Returns the action and optional ability/item choice
        /// </summary>
        CombatAction RequestCombatAction(CombatState state);

        /// <summary>
        /// Request ability selection
        /// Returns selected ability index or -1 for cancel
        /// </summary>
        int RequestAbilitySelection(List<AbilityInfo> abilities);

        /// <summary>
        /// Request shop action
        /// </summary>
        ShopAction RequestShopAction(List<ShopItemInfo> forSale, List<EquipmentItem> inventory, int playerGold);

        /// <summary>
        /// Request inventory action (equip, sell, etc)
        /// </summary>
        InventoryAction RequestInventoryAction(List<EquipmentItem> backpack, Dictionary<EquipmentSlot, EquipmentItem?> equipped);

        /// <summary>
        /// Request dungeon selection
        /// Returns dungeon index or -1 for cancel
        /// </summary>
        int RequestDungeonSelection(List<DungeonSelectionInfo> dungeons);

        /// <summary>
        /// Request which ability to unlock
        /// Returns ability index or -1 for cancel
        /// </summary>
        int RequestAbilityUnlock(List<AbilityInfo> lockedAbilities, int playerGold, int playerLevel);

        /// <summary>
        /// Request confirmation (yes/no)
        /// </summary>
        bool RequestConfirmation(string message);

        /// <summary>
        /// Request save slot for saving
        /// </summary>
        int RequestSaveSlot(List<InterfaceSaveSlotInfo> slots);

        /// <summary>
        /// Wait for user to acknowledge a message (like "press any key")
        /// </summary>
        void WaitForAcknowledgment();

        /// <summary>
        /// Request player to choose from dungeon encounter options
        /// Returns the index of the chosen option (0-based)
        /// </summary>
        int RequestEncounterChoice(string description, List<string> choices);
    }

    // Data structures for interface communication

    public class InterfaceSaveSlotInfo
    {
        public int SlotNumber { get; set; }
        public bool IsEmpty { get; set; }
        public string? Name { get; set; }
        public int Level { get; set; }
        public PlayerClass Class { get; set; }
        public DateTime SaveTime { get; set; }
    }

    public enum MainMenuChoice
    {
        Combat,
        Dungeon,
        Shop,
        Inventory,
        CharacterSheet,
        UnlockAbilities,
        Rest,
        Save,
        Help,
        Exit
    }

    public class CombatState
    {
        public int TurnNumber { get; set; }
        public string EnemyName { get; set; }
        public int EnemyCurrentHP { get; set; }
        public int EnemyMaxHP { get; set; }
        public int PlayerCurrentHP { get; set; }
        public int PlayerMaxHP { get; set; }
        public int PlayerCurrentMana { get; set; }
        public int PlayerMaxMana { get; set; }
        public int PlayerPotions { get; set; }
        public bool CanFlee { get; set; }
        public List<AbilityInfo> AvailableAbilities { get; set; } = new();
        public List<string> PlayerActiveEffects { get; set; } = new();
        public List<string> EnemyActiveEffects { get; set; } = new();
    }

    public enum CombatActionType
    {
        Attack,
        UseAbility,
        UsePotion,
        Flee
    }

    public class CombatAction
    {
        public CombatActionType ActionType { get; set; }
        public int? AbilityIndex { get; set; } // If UseAbility, which ability
    }

    public class AbilityInfo
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ManaCost { get; set; }
        public int CurrentCooldown { get; set; }
        public bool CanUse { get; set; }
        public bool IsUnlocked { get; set; }
        public int UnlockLevel { get; set; }
        public int PurchaseCost { get; set; }
        public bool Priority { get; set; }
    }

    public class ShopItemInfo
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Rarity { get; set; }
        public int Price { get; set; }
        public int Level { get; set; }
        public EquipmentItem? Item { get; set; }
    }

    public enum ShopActionType
    {
        BuyItem,
        SellItem,
        RefreshShop,
        BuyPotion,
        Exit
    }

    public class ShopAction
    {
        public ShopActionType ActionType { get; set; }
        public int? ItemIndex { get; set; } // For BuyItem or SellItem
        public int? Quantity { get; set; } // For BuyPotion
    }

    public enum InventoryActionType
    {
        EquipItem,
        UnequipItem,
        ViewDetails,
        Exit
    }

    public class InventoryAction
    {
        public InventoryActionType ActionType { get; set; }
        public int? ItemIndex { get; set; }
        public EquipmentSlot? Slot { get; set; } // For UnequipItem
    }

    public class CharacterSheetInfo
    {
        public string Name { get; set; } = "";
        public int Level { get; set; }
        public int Experience { get; set; }
        public int ExperienceToNextLevel { get; set; }
        public PlayerClass Class { get; set; }
        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }
        public int CurrentMana { get; set; }
        public int MaxMana { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int MagicPower { get; set; }
        public int Speed { get; set; }
        public double CritChance { get; set; }
        public int Gold { get; set; }
        public int Potions { get; set; }
        public Dictionary<EquipmentSlot, EquipmentItem?> Equipment { get; set; } = new();
        public List<AbilityInfo> Abilities { get; set; } = new();
    }

    public class DungeonSelectionInfo
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public int MinLevel { get; set; }
        public int GoldCost { get; set; }
        public int Difficulty { get; set; }
        public bool IsCompleted { get; set; }
        public bool CanEnter { get; set; }
        public string? BlockingReason { get; set; }
    }
}
