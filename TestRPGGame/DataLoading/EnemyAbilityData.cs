namespace TestRPGGame.DataLoading
{
    public class EnemyAbilityData
    {
        public string AbilityId { get; set; } = "";
        public int UseThreshold { get; set; } = 100; // HP% threshold (0-100) to start using this ability
    }
}
