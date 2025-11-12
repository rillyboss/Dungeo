using System;
using System.Collections.Generic;
using TestRPGGame.Equipment;
using TestRPGGame.Entities.Player;

namespace TestRPGGame.Systems
{
    public class SaveData
    {
        public string Name { get; set; } = "";
        public PlayerClass Class { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int ExperienceToNextLevel { get; set; }
        public int Gold { get; set; }
        public int PotionCount { get; set; }

        // Base stats (without equipment)
        public int BaseMaxHP { get; set; }
        public int BaseMaxMana { get; set; }
        public int BaseAttack { get; set; }
        public int BaseDefense { get; set; }
        public int BaseMagicPower { get; set; }
        public int BaseSpeed { get; set; }
        public double BaseCritChance { get; set; }

        // Current HP/Mana
        public int CurrentHP { get; set; }
        public int CurrentMana { get; set; }

        // Equipment - 9 slots
        public EquipmentItem? Weapon { get; set; }
        public EquipmentItem? Armor { get; set; }
        public EquipmentItem? Helmet { get; set; }
        public EquipmentItem? Boots { get; set; }
        public EquipmentItem? Gloves { get; set; }
        public EquipmentItem? Ring1 { get; set; }
        public EquipmentItem? Ring2 { get; set; }
        public EquipmentItem? Amulet { get; set; }
        public EquipmentItem? Relic { get; set; }

        // Backpack
        public List<EquipmentItem> BackpackItems { get; set; } = new List<EquipmentItem>();

        // Ability unlock status
        public List<string> UnlockedAbilityNames { get; set; } = new List<string>();

        // Dungeon progress
        public Dictionary<string, bool> CompletedDungeons { get; set; } = new Dictionary<string, bool>();

        // Statistics
        public PlayerStatistics? Statistics { get; set; }

        // Achievements
        public HashSet<string> UnlockedAchievements { get; set; } = new HashSet<string>();

        // Metadata
        public DateTime SaveTime { get; set; }
        public int PlayTime { get; set; } // In seconds
    }
}
