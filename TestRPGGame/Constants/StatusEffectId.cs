namespace TestRPGGame.Constants
{
    /// <summary>
    /// Defines all status effect IDs in the game.
    /// Used to eliminate magic strings for status effect identification.
    /// </summary>
    public enum StatusEffectId
    {
        Burning,
        Poison,
        Bleed,
        Regeneration,
        Shield,
        Thorns,
        BattleRage,
        Enrage,
        ShieldWall,
        SpeedBuff,
        Stun,
        SmokeScreen
    }

    /// <summary>
    /// Extension methods for StatusEffectId enum to get string identifiers.
    /// </summary>
    public static class StatusEffectIdExtensions
    {
        public static string GetIdentifier(this StatusEffectId effectId)
        {
            return effectId switch
            {
                StatusEffectId.Burning => "burning",
                StatusEffectId.Poison => "poison",
                StatusEffectId.Bleed => "bleed",
                StatusEffectId.Regeneration => "regeneration",
                StatusEffectId.Shield => "shield",
                StatusEffectId.Thorns => "thorns",
                StatusEffectId.BattleRage => "battle_rage",
                StatusEffectId.Enrage => "enrage",
                StatusEffectId.ShieldWall => "shield_wall",
                StatusEffectId.SpeedBuff => "speed_buff",
                StatusEffectId.Stun => "stun",
                StatusEffectId.SmokeScreen => "smoke_screen",
                _ => effectId.ToString().ToLower()
            };
        }

        public static StatusEffectId FromIdentifier(string identifier)
        {
            return identifier.ToLower() switch
            {
                "burning" => StatusEffectId.Burning,
                "poison" => StatusEffectId.Poison,
                "bleed" => StatusEffectId.Bleed,
                "regeneration" => StatusEffectId.Regeneration,
                "shield" => StatusEffectId.Shield,
                "thorns" => StatusEffectId.Thorns,
                "battle_rage" => StatusEffectId.BattleRage,
                "enrage" => StatusEffectId.Enrage,
                "shield_wall" => StatusEffectId.ShieldWall,
                "speed_buff" => StatusEffectId.SpeedBuff,
                "stun" => StatusEffectId.Stun,
                "smoke_screen" => StatusEffectId.SmokeScreen,
                _ => throw new System.ArgumentException($"Unknown status effect identifier: {identifier}")
            };
        }
    }
}
