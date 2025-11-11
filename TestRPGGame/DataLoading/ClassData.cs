using System.Collections.Generic;

namespace TestRPGGame.DataLoading
{
    public class ClassData
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        // Base stats at level 1
        public int BaseMaxHP { get; set; }
        public int BaseMaxMana { get; set; }
        public int BaseAttack { get; set; }
        public int BaseDefense { get; set; }
        public int BaseMagicPower { get; set; }
        public int BaseSpeed { get; set; }
        public double BaseCritChance { get; set; }

        // Stat growth per level
        public int HPPerLevel { get; set; }
        public int ManaPerLevel { get; set; }
        public int AttackPerLevel { get; set; }
        public int DefensePerLevel { get; set; }
        public int MagicPowerPerLevel { get; set; }
        public int SpeedPerLevel { get; set; }

        // Starting resources
        public int StartingGold { get; set; }
        public int StartingPotions { get; set; }
        public List<string> StartingEquipment { get; set; } = new List<string>();

        // Visual representation
        public List<string> Art { get; set; } = new List<string>();
        public string ArtColor { get; set; } = "White";
    }
}
