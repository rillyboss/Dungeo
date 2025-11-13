using TestRPGGame.Constants;
using TestRPGGame.Entities;

namespace TestRPGGame.Combat.StatusEffects
{
    /// <summary>
    /// Factory for creating status effects used by legacy effect types.
    /// Most effects are now created via EffectApplicator using the new composable system.
    /// This factory only provides methods for DOT effects still used by PoisonEffect.
    /// </summary>
    public static class StatusEffectFactory
    {
        // DOT Effects (used by PoisonEffect)
        public static DamageOverTimeEffect CreateBurning(int duration, int damagePerTurn)
        {
            return new DamageOverTimeEffect(StatusEffectId.Burning.GetIdentifier(), "Burning", "🔥", duration, damagePerTurn);
        }

        public static DamageOverTimeEffect CreatePoison(int duration, int damagePerTurn)
        {
            return new DamageOverTimeEffect(StatusEffectId.Poison.GetIdentifier(), "Poisoned", "☠️", duration, damagePerTurn);
        }

        public static DamageOverTimeEffect CreateBleed(int duration, int damagePerTurn)
        {
            return new DamageOverTimeEffect(StatusEffectId.Bleed.GetIdentifier(), "Bleeding", "🩸", duration, damagePerTurn);
        }

        // Extension methods for DOT application
        public static void ApplyBurning(this Combatant target, Combatant source, int duration, int damagePerTurn)
        {
            var effect = CreateBurning(duration, damagePerTurn);
            effect.Source = source;
            target.Effects.AddEffect(effect);
        }

        public static void ApplyPoison(this Combatant target, Combatant source, int duration, int damagePerTurn)
        {
            var effect = CreatePoison(duration, damagePerTurn);
            effect.Source = source;
            target.Effects.AddEffect(effect);
        }

        public static void ApplyBleed(this Combatant target, Combatant source, int duration, int damagePerTurn)
        {
            var effect = CreateBleed(duration, damagePerTurn);
            effect.Source = source;
            target.Effects.AddEffect(effect);
        }
    }
}
