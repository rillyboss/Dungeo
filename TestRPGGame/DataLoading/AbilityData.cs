using System.Collections.Generic;

namespace TestRPGGame.DataLoading
{
    public class AbilityData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string PlayerClass { get; set; } = "";
        public int ManaCost { get; set; }
        public int Cooldown { get; set; }
        public string Type { get; set; } = "";
        public bool Priority { get; set; } = false;
        public bool IsStarting { get; set; }
        public bool IsUnlockable { get; set; }
        public int UnlockLevel { get; set; }
        public int PurchaseCost { get; set; }
        public List<AbilityEffectData> Effects { get; set; } = new();
    }
}
