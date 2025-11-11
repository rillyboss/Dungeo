namespace TestRPGGame.Constants
{
    /// <summary>
    /// Defines all modifiable stats in the game.
    /// Used to eliminate magic strings for stat names.
    /// </summary>
    public enum StatType
    {
        Speed,
        Attack,
        Defense,
        HP,
        MagicPower
    }

    /// <summary>
    /// Extension methods for StatType enum to get string identifiers.
    /// </summary>
    public static class StatTypeExtensions
    {
        public static string GetIdentifier(this StatType statType)
        {
            return statType switch
            {
                StatType.Speed => "speed",
                StatType.Attack => "attack",
                StatType.Defense => "defense",
                StatType.HP => "hp",
                StatType.MagicPower => "magic_power",
                _ => statType.ToString().ToLower()
            };
        }

        public static StatType FromIdentifier(string identifier)
        {
            return identifier.ToLower() switch
            {
                "speed" => StatType.Speed,
                "attack" => StatType.Attack,
                "defense" => StatType.Defense,
                "hp" => StatType.HP,
                "magic_power" => StatType.MagicPower,
                _ => throw new System.ArgumentException($"Unknown stat identifier: {identifier}")
            };
        }
    }
}
