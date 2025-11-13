namespace TestRPGGame.DataLoading
{
    /// <summary>
    /// Data model for ability effects - supports both old and new formats.
    ///
    /// OLD FORMAT (still supported):
    ///   Type: "Damage", "Buff", "Poison", etc.
    ///   BuffName: "Battle Rage" (specific name)
    ///
    /// NEW FORMAT (composable, data-driven):
    ///   Type: "Effect"
    ///   EffectKind: "AttackBoost", "DefenseBoost", etc. (generic type)
    ///   DisplayName: "Enraged", "Iron Skin", etc. (custom name)
    ///   Multiplier: 1.8 (effect strength)
    ///
    /// This allows JSON to compose generic effects with custom names and parameters.
    /// Example: EffectKind="AttackBoost" + DisplayName="Enraged" + Multiplier=1.8
    /// </summary>
    public class AbilityEffectData
    {
        // Core effect specification
        public string Type { get; set; } = "";              // "Damage", "Buff", "Effect", "Poison", etc.

        // NEW: Composable effect system
        public string? EffectKind { get; set; }             // Generic effect type: "AttackBoost", "DefenseBoost", etc.
        public string? DisplayName { get; set; }            // Custom name shown to player: "Enraged", "Iron Skin", etc.

        // Effect parameters
        public int Value { get; set; }                      // Flat value (damage, healing, shield amount, etc.)
        public int Duration { get; set; }                   // How many turns effect lasts
        public double Multiplier { get; set; } = 1.0;       // Multiplier for damage/stats (1.8 = +80%)
        public double MinMultiplier { get; set; }           // For random range effects
        public double MaxMultiplier { get; set; }           // For random range effects
        public double Accuracy { get; set; } = 1.0;         // Hit chance (0.9 = 90%)

        // OLD: Specific effect data (backward compatibility)
        public string? BuffName { get; set; }               // OLD FORMAT: Specific buff name
        public int DamagePerTurn { get; set; }              // For DOT effects

        // Special flags
        public bool GuaranteedCrit { get; set; } = false;   // Force critical hit

        // For composite effects (multiple stat changes at once)
        // Example: Banner = [{EffectKind: AttackBoost, Multiplier: 1.2}, {EffectKind: DefenseBoost, Multiplier: 1.2}]
        public List<CompositeEffectData>? CompositeEffects { get; set; }
    }

    /// <summary>
    /// Represents a single effect within a composite effect.
    /// Allows abilities to apply multiple generic effects at once.
    /// Example: "Gladiator's Resolve" = AttackBoost + DefenseBoost + SpeedBoost
    /// </summary>
    public class CompositeEffectData
    {
        public string EffectKind { get; set; } = "";        // Generic effect type
        public double Multiplier { get; set; } = 1.0;       // Effect strength
        public int FlatValue { get; set; }                  // For flat bonuses (speed, etc.)
    }
}
