namespace TestRPGGame.Entities.Boss
{
    public class BossAbilityEffect
    {
        public BossAbilityEffectType Type { get; set; }
        public int Value { get; set; }
        public int Duration { get; set; }
        public double Multiplier { get; set; }

        public BossAbilityEffect(BossAbilityEffectType type, int value = 0, int duration = 0, double multiplier = 1.0)
        {
            Type = type;
            Value = value;
            Duration = duration;
            Multiplier = multiplier;
        }
    }
}
