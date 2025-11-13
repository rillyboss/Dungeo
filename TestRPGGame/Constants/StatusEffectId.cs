namespace TestRPGGame.Constants
{
    /// <summary>
    /// Defines status effect IDs for legacy effect types.
    /// New effects created via EffectApplicator use dynamic string IDs.
    /// These IDs are only used for DOT effects (Burning, Poison, Bleed).
    /// </summary>
    public enum StatusEffectId
    {
        // DOT Effects (used by PoisonEffect)
        Burning,
        Poison,
        Bleed
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
                _ => effectId.ToString().ToLower()
            };
        }

        public static StatusEffectId FromIdentifier(string identifier)
        {
            return identifier.ToLower().Replace(" ", "_") switch
            {
                "burning" => StatusEffectId.Burning,
                "poison" => StatusEffectId.Poison,
                "bleed" => StatusEffectId.Bleed,
                _ => throw new System.ArgumentException($"Unknown status effect identifier: {identifier}")
            };
        }
    }
}
