namespace TestRPGGame.DataLoading
{
    public class RarityThresholdData
    {
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; } = 999; // Default max level (no upper limit)
        public int RollThreshold { get; set; } // 0-100, minimum roll needed to get this rarity
    }
}
