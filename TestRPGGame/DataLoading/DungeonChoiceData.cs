namespace TestRPGGame.DataLoading
{
    public class DungeonChoiceData
    {
        public string Text { get; set; } = "";
        public string Effect { get; set; } = ""; // GainGold, TakeDamage, Heal, etc.
        public int Value { get; set; }
    }
}
