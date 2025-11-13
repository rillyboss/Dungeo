namespace TestRPGGame.DataLoading
{
    public class WeaponTypeData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string AttackType { get; set; } = "Physical";
        public double AttackWeight { get; set; } = 1.0;
        public double MagicWeight { get; set; } = 0.0;
        public int SpeedBonus { get; set; } = 0;
        public string? SignatureAbility { get; set; }
    }
}
