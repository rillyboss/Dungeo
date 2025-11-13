namespace TestRPGGame.Constants
{
    /// <summary>
    /// Defines status effect IDs used by the legacy effect system.
    /// New effects created via EffectApplicator use string IDs directly.
    /// These are kept for backward compatibility with old abilities.
    /// </summary>
    public enum StatusEffectId
    {
        // DOT Effects (used by PoisonEffect and old applicators)
        Burning,
        Poison,
        Bleed,

        // HOT Effects
        Regeneration,

        // Defensive Effects
        Shield,
        Thorns,

        // Legacy Buffs (backward compatibility)
        BattleRage,      // Used by old BuffApplicator
        Enrage,          // Used by old EnrageEffect
        ShieldWall,      // Used by old BuffApplicator
        SmokeScreen,     // Used by old BuffApplicator
        SpeedBuff,       // Used by SpeedBuffApplicator

        // Control Effects
        Stun             // Used by StunApplicator
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
                // DOT
                StatusEffectId.Burning => "burning",
                StatusEffectId.Poison => "poison",
                StatusEffectId.Bleed => "bleed",
                // HOT
                StatusEffectId.Regeneration => "regeneration",
                // Defensive
                StatusEffectId.Shield => "shield",
                StatusEffectId.Thorns => "thorns",
                // Legacy Buffs
                StatusEffectId.BattleRage => "battle_rage",
                StatusEffectId.Enrage => "enrage",
                StatusEffectId.ShieldWall => "shield_wall",
                StatusEffectId.SmokeScreen => "smoke_screen",
                StatusEffectId.SpeedBuff => "speed_buff",
                // Control
                StatusEffectId.Stun => "stun",
                _ => effectId.ToString().ToLower()
            };
        }

        public static StatusEffectId FromIdentifier(string identifier)
        {
            return identifier.ToLower().Replace(" ", "_") switch
            {
                // DOT
                "burning" => StatusEffectId.Burning,
                "poison" => StatusEffectId.Poison,
                "bleed" => StatusEffectId.Bleed,
                // HOT
                "regeneration" => StatusEffectId.Regeneration,
                // Defensive
                "shield" => StatusEffectId.Shield,
                "thorns" => StatusEffectId.Thorns,
                // Legacy Buffs
                "battle_rage" => StatusEffectId.BattleRage,
                "enrage" => StatusEffectId.Enrage,
                "shield_wall" => StatusEffectId.ShieldWall,
                "smoke_screen" => StatusEffectId.SmokeScreen,
                "speed_buff" => StatusEffectId.SpeedBuff,
                // Control
                "stun" => StatusEffectId.Stun,
                _ => throw new System.ArgumentException($"Unknown status effect identifier: {identifier}")
            };
        }
    }
}
