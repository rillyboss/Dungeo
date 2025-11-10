namespace TestRPGGame.Equipment
{
    public class SpecialEffect
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double ProcChance { get; set; } // 0.0 to 1.0
        public EffectType Type { get; set; }
        public int Value { get; set; }

        public SpecialEffect(string name, string description, double procChance, EffectType type, int value)
        {
            Name = name;
            Description = description;
            ProcChance = procChance;
            Type = type;
            Value = value;
        }
    }
}
