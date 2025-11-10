namespace TestRPGGame.DataLoading
{
    public class AbilityEffectData
    {
        public string Type { get; set; } = "";
        public int Value { get; set; }
        public int Duration { get; set; }
        public double Multiplier { get; set; } = 1.0;
        public string? BuffName { get; set; }
        public int DamagePerTurn { get; set; }
        public bool GuaranteedCrit { get; set; } = false;
    }
}
