using System;
using System.Collections.Generic;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.Abilities;
using TestRPGGame.Equipment;

namespace TestRPGGame.Interfaces
{
    /// <summary>
    /// All events that the game can publish to interfaces
    /// </summary>
    public static class GameEvents
    {
        // Game lifecycle events
        public class GameStartedEvent { }
        public class GameEndedEvent { }

        // Character events
        public class CharacterCreatedEvent
        {
            public string Name { get; set; }
            public PlayerClass Class { get; set; }
            public int MaxHP { get; set; }
            public int MaxMana { get; set; }
            public int Attack { get; set; }
            public int Defense { get; set; }
        }

        public class PlayerStatsChangedEvent
        {
            public int CurrentHP { get; set; }
            public int MaxHP { get; set; }
            public int CurrentMana { get; set; }
            public int MaxMana { get; set; }
            public int Gold { get; set; }
            public int Level { get; set; }
            public int Experience { get; set; }
        }

        public class PlayerLeveledUpEvent
        {
            public int NewLevel { get; set; }
            public int NewMaxHP { get; set; }
            public int NewMaxMana { get; set; }
            public int NewAttack { get; set; }
            public int NewDefense { get; set; }
            public List<(string Name, string Description, int UnlockLevel)> NewlyUnlockedAbilities { get; set; } = new List<(string, string, int)>();
        }

        // Combat events
        public class CombatStartedEvent
        {
            public string EnemyName { get; set; }
            public int EnemyLevel { get; set; }
            public int EnemyMaxHP { get; set; }
            public int EnemyAttack { get; set; }
            public int EnemyDefense { get; set; }
            public bool CanFlee { get; set; }
        }

        public class CombatTurnStartEvent
        {
            public int TurnNumber { get; set; }
            public int PlayerHP { get; set; }
            public int PlayerMaxHP { get; set; }
            public int PlayerMana { get; set; }
            public int PlayerMaxMana { get; set; }
            public int EnemyHP { get; set; }
            public int EnemyMaxHP { get; set; }
            public List<string> PlayerActiveEffects { get; set; } = new();
            public List<string> EnemyActiveEffects { get; set; } = new();
        }

        public class DamageDealtEvent
        {
            public string Attacker { get; set; }
            public string Target { get; set; }
            public int Damage { get; set; }
            public bool IsCritical { get; set; }
            public string AttackType { get; set; } // "Physical", "Magic", "Ability"
        }

        public class AttackMissedEvent
        {
            public string Attacker { get; set; }
            public string Target { get; set; }
            public string MissType { get; set; } // "Miss" or "Dodge"
            public string AttackType { get; set; } // "Physical", "Magic", "Ability"
        }

        public class AbilityUsedEvent
        {
            public string User { get; set; }
            public string AbilityName { get; set; }
            public string Description { get; set; }
            public int ManaCost { get; set; }
        }

        public class EffectAppliedEvent
        {
            public string Target { get; set; }
            public string EffectName { get; set; }
            public int Duration { get; set; }
            public string Description { get; set; }
        }

        public class CombatEndedEvent
        {
            public bool PlayerVictory { get; set; }
            public int GoldEarned { get; set; }
            public int ExperienceEarned { get; set; }
            public EquipmentItem? LootDropped { get; set; }
            public int GoldLost { get; set; }
        }

        // Item events
        public class ItemReceivedEvent
        {
            public string ItemName { get; set; }
            public string ItemType { get; set; }
            public string Rarity { get; set; }
        }

        public class ItemEquippedEvent
        {
            public string ItemName { get; set; }
            public string Slot { get; set; }
        }

        public class PotionUsedEvent
        {
            public int HPRestored { get; set; }
            public int PotionsRemaining { get; set; }
        }

        // Shop events
        public class ShopEnteredEvent
        {
            public List<ShopItemInfo> AvailableItems { get; set; } = new();
        }

        public class ShopItemInfo
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Rarity { get; set; }
            public int Price { get; set; }
            public int Level { get; set; }
        }

        public class ItemPurchasedEvent
        {
            public string ItemName { get; set; }
            public int Price { get; set; }
            public int GoldRemaining { get; set; }
        }

        public class ItemSoldEvent
        {
            public string ItemName { get; set; }
            public int Price { get; set; }
            public int GoldRemaining { get; set; }
        }

        // Dungeon events
        public class DungeonListEvent
        {
            public List<DungeonInfo> Dungeons { get; set; } = new();
        }

        public class DungeonInfo
        {
            public string Name { get; set; }
            public int MinLevel { get; set; }
            public int GoldCost { get; set; }
            public int Difficulty { get; set; }
            public bool IsCompleted { get; set; }
            public bool CanEnter { get; set; }
            public string? RequirementMessage { get; set; }
        }

        public class DungeonEnteredEvent
        {
            public string DungeonName { get; set; }
            public int Difficulty { get; set; }
        }

        public class DungeonEncounterEvent
        {
            public string EncounterType { get; set; } // "Combat", "Treasure", "Choice", "Boss"
            public string Description { get; set; }
        }

        public class DungeonCompletedEvent
        {
            public string DungeonName { get; set; }
            public bool Success { get; set; }
            public int TotalGoldEarned { get; set; }
            public int TotalExperienceEarned { get; set; }
        }

        // Ability unlock events
        public class AbilityUnlockedEvent
        {
            public string AbilityName { get; set; }
            public string Description { get; set; }
            public int Cost { get; set; }
        }

        // Save/Load events
        public class GameSavedEvent
        {
            public int SlotNumber { get; set; }
            public bool Success { get; set; }
        }

        public class GameLoadedEvent
        {
            public int SlotNumber { get; set; }
            public string PlayerName { get; set; }
            public int Level { get; set; }
            public PlayerClass Class { get; set; }
        }

        // Achievement events
        public class AchievementUnlockedEvent
        {
            public string AchievementId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Category { get; set; }
            public int GoldReward { get; set; }
            public int ExperienceReward { get; set; }
            public string? TitleReward { get; set; }
            public int Points { get; set; }
        }

        // Message events
        public class InfoMessageEvent
        {
            public string Message { get; set; }
            public MessageType Type { get; set; }
            public ConsoleColor? Color { get; set; } // Optional explicit color override
        }

        public enum MessageType
        {
            Info,
            Success,
            Warning,
            Error
        }
    }
}
