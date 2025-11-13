namespace TestRPGGame.Constants
{
    /// <summary>
    /// Generic effect types that can be composed in data-driven JSON.
    /// These describe WHAT an effect does, not specific ability names.
    ///
    /// Example: AttackBoost with 1.5x multiplier = "Battle Rage"
    ///          AttackBoost with 1.8x multiplier = "Enraged"
    ///
    /// Data provides custom display names, durations, and parameters.
    /// Code provides generic, reusable mechanics.
    /// </summary>
    public enum EffectKind
    {
        // Stat Boosts (Buffs)
        AttackBoost,      // Increase attack power (multiplier-based)
        DefenseBoost,     // Increase defense/damage reduction
        SpeedBoost,       // Increase speed
        DamageBoost,      // General damage amplification
        EvasionBoost,     // Dodge/evasion increase

        // Stat Reductions (Debuffs)
        AttackReduction,  // Reduce attack power
        DefenseReduction, // Reduce defense (increase damage taken)
        SpeedReduction,   // Slow/cripple
        AccuracyReduction, // Miss chance increase

        // Special Mechanics
        LifeSteal,        // Heal on damage dealt
        Thorns,           // Reflect damage
        Regeneration,     // Heal over time
        Shield,           // Absorb damage
        DamageOverTime,   // Poison/Burn/Bleed
        Stun,             // Disable
        Dodge,            // Complete evasion

        // Composite (Multiple effects at once)
        Composite         // For effects that apply multiple buff types
    }

    /// <summary>
    /// Extension methods for EffectKind enum.
    /// </summary>
    public static class EffectKindExtensions
    {
        /// <summary>
        /// Gets a generic description of what this effect kind does.
        /// Specific display names come from the data, not from code.
        /// </summary>
        public static string GetGenericDescription(this EffectKind effectKind)
        {
            return effectKind switch
            {
                EffectKind.AttackBoost => "Attack Boost",
                EffectKind.DefenseBoost => "Defense Boost",
                EffectKind.SpeedBoost => "Speed Boost",
                EffectKind.DamageBoost => "Damage Boost",
                EffectKind.EvasionBoost => "Evasion Boost",
                EffectKind.AttackReduction => "Attack Reduction",
                EffectKind.DefenseReduction => "Defense Reduction",
                EffectKind.SpeedReduction => "Speed Reduction",
                EffectKind.AccuracyReduction => "Accuracy Reduction",
                EffectKind.LifeSteal => "Life Steal",
                EffectKind.Thorns => "Thorns",
                EffectKind.Regeneration => "Regeneration",
                EffectKind.Shield => "Shield",
                EffectKind.DamageOverTime => "Damage Over Time",
                EffectKind.Stun => "Stun",
                EffectKind.Dodge => "Dodge",
                EffectKind.Composite => "Composite Effect",
                _ => effectKind.ToString()
            };
        }
    }
}
