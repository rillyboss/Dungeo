namespace TestRPGGame.Constants
{
    /// <summary>
    /// Defines all available buff types in the game.
    /// Used to eliminate magic strings for buff names.
    /// </summary>
    public enum BuffType
    {
        BattleRage,
        ShieldWall,
        SmokeScreen
    }

    /// <summary>
    /// Extension methods for BuffType enum to get display names.
    /// </summary>
    public static class BuffTypeExtensions
    {
        public static string GetDisplayName(this BuffType buffType)
        {
            return buffType switch
            {
                BuffType.BattleRage => "Battle Rage",
                BuffType.ShieldWall => "Shield Wall",
                BuffType.SmokeScreen => "Smoke Screen",
                _ => buffType.ToString()
            };
        }
    }
}
