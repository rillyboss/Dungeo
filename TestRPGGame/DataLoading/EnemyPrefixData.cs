namespace TestRPGGame.DataLoading
{
    public class EnemyPrefixData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int MinLevel { get; set; }
        public double HPMultiplier { get; set; } = 1.0;
        public double AttackMultiplier { get; set; } = 1.0;
        public double DefenseMultiplier { get; set; } = 1.0;
        public double SpeedMultiplier { get; set; } = 1.0;
        public double GoldMultiplier { get; set; } = 1.0;
        public double ExpMultiplier { get; set; } = 1.0;
    }
}
