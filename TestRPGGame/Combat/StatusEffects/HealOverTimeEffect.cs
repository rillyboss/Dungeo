using System;

namespace TestRPGGame.Combat.StatusEffects
{
    /// <summary>
    /// Restores health at the start of each turn (regeneration, etc.)
    /// </summary>
    public class HealOverTimeEffect : StatusEffect
    {
        public HealOverTimeEffect(string effectId, string name, string icon, int duration, int healPerTurn)
            : base(effectId, name, icon, StatusEffectType.Buff, duration, healPerTurn)
        {
        }

        public override void OnTurnStart()
        {
            if (Target != null)
            {
                int healAmount = Math.Min(Value, Target.MaxHP - Target.CurrentHP);
                Target.Heal(Value);
                // Note: Output is handled by the combat system through events
            }
        }

        public override string GetDescription()
        {
            return $"{Icon} {Name}: +{Value} HP/turn ({RemainingTurns} turn{(RemainingTurns != 1 ? "s" : "")})";
        }
    }
}
