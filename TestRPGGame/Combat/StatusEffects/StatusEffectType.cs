namespace TestRPGGame.Combat.StatusEffects
{
    /// <summary>
    /// Categories of status effects for filtering and display purposes.
    /// </summary>
    public enum StatusEffectType
    {
        Buff,       // Positive effects (heals, shields, damage buffs)
        Debuff,     // Negative effects (DOTs, stat reductions)
        Control     // Crowd control effects (stun, slow)
    }
}
