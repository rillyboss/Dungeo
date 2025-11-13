namespace TestRPGGame.Constants
{
    /// <summary>
    /// Defines all status effect IDs in the game.
    /// Used to eliminate magic strings for status effect identification.
    /// </summary>
    public enum StatusEffectId
    {
        // DOT Effects
        Burning,
        Poison,
        Bleed,

        // HOT Effects
        Regeneration,

        // Defensive Effects
        Shield,
        Thorns,

        // Warrior Buffs
        BattleRage,      // Standard attack boost
        Enrage,          // Already exists - high attack, low defense
        Enraged,         // Alternative name for Enrage
        Bloodlust,       // Lifesteal
        IronSkin,        // Massive defense
        LastStand,       // Prevent death
        Fortified,       // Damage reduction
        Unbreakable,     // CC immunity
        Rally,           // Attack + morale
        Banner,          // Attack + defense
        CommandingPresence, // Multi-stat
        GladiatorsResolve,  // Ultimate
        ShieldWall,      // Defense boost

        // Mage Buffs
        Combustion,      // Fire damage amp
        TimeWarp,        // Haste/speed
        SpellPower,      // Magic damage amp
        ArcanePower,     // Ultimate magic

        // Rogue Buffs
        SmokeScreen,     // Evasion
        ShadowCloak,     // Enhanced evasion
        MirrorImage,     // Confusion + evasion
        ShadowDance,     // Dodge + speed
        ShadowRealm,     // Ultimate evasion

        // Debuffs
        HealingReduction,  // Reduce healing
        Vulnerability,     // Increase damage taken
        Exposed,           // Defense reduction
        Distracted,        // Accuracy reduction
        Crippled,          // Speed reduction
        Confused,          // Mixed penalties

        // Control Effects
        SpeedBuff,       // Speed increase
        Stun             // Stun/disable
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
                // Warrior
                StatusEffectId.BattleRage => "battle_rage",
                StatusEffectId.Enrage => "enrage",
                StatusEffectId.Enraged => "enraged",
                StatusEffectId.Bloodlust => "bloodlust",
                StatusEffectId.IronSkin => "iron_skin",
                StatusEffectId.LastStand => "last_stand",
                StatusEffectId.Fortified => "fortified",
                StatusEffectId.Unbreakable => "unbreakable",
                StatusEffectId.Rally => "rally",
                StatusEffectId.Banner => "banner",
                StatusEffectId.CommandingPresence => "commanding_presence",
                StatusEffectId.GladiatorsResolve => "gladiators_resolve",
                StatusEffectId.ShieldWall => "shield_wall",
                // Mage
                StatusEffectId.Combustion => "combustion",
                StatusEffectId.TimeWarp => "time_warp",
                StatusEffectId.SpellPower => "spell_power",
                StatusEffectId.ArcanePower => "arcane_power",
                // Rogue
                StatusEffectId.SmokeScreen => "smoke_screen",
                StatusEffectId.ShadowCloak => "shadow_cloak",
                StatusEffectId.MirrorImage => "mirror_image",
                StatusEffectId.ShadowDance => "shadow_dance",
                StatusEffectId.ShadowRealm => "shadow_realm",
                // Debuffs
                StatusEffectId.HealingReduction => "healing_reduction",
                StatusEffectId.Vulnerability => "vulnerability",
                StatusEffectId.Exposed => "exposed",
                StatusEffectId.Distracted => "distracted",
                StatusEffectId.Crippled => "crippled",
                StatusEffectId.Confused => "confused",
                // Control
                StatusEffectId.SpeedBuff => "speed_buff",
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
                // Warrior
                "battle_rage" => StatusEffectId.BattleRage,
                "enrage" => StatusEffectId.Enrage,
                "enraged" => StatusEffectId.Enraged,
                "bloodlust" => StatusEffectId.Bloodlust,
                "iron_skin" => StatusEffectId.IronSkin,
                "last_stand" => StatusEffectId.LastStand,
                "fortified" => StatusEffectId.Fortified,
                "unbreakable" => StatusEffectId.Unbreakable,
                "rally" => StatusEffectId.Rally,
                "banner" => StatusEffectId.Banner,
                "commanding_presence" => StatusEffectId.CommandingPresence,
                "gladiators_resolve" => StatusEffectId.GladiatorsResolve,
                "shield_wall" => StatusEffectId.ShieldWall,
                // Mage
                "combustion" => StatusEffectId.Combustion,
                "time_warp" => StatusEffectId.TimeWarp,
                "spell_power" => StatusEffectId.SpellPower,
                "arcane_power" => StatusEffectId.ArcanePower,
                // Rogue
                "smoke_screen" => StatusEffectId.SmokeScreen,
                "shadow_cloak" => StatusEffectId.ShadowCloak,
                "mirror_image" => StatusEffectId.MirrorImage,
                "shadow_dance" => StatusEffectId.ShadowDance,
                "shadow_realm" => StatusEffectId.ShadowRealm,
                // Debuffs
                "healing_reduction" => StatusEffectId.HealingReduction,
                "vulnerability" => StatusEffectId.Vulnerability,
                "exposed" => StatusEffectId.Exposed,
                "distracted" => StatusEffectId.Distracted,
                "crippled" => StatusEffectId.Crippled,
                "confused" => StatusEffectId.Confused,
                // Control
                "speed_buff" => StatusEffectId.SpeedBuff,
                "stun" => StatusEffectId.Stun,
                _ => throw new System.ArgumentException($"Unknown status effect identifier: {identifier}")
            };
        }
    }
}
