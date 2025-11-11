using System;
using TestRPGGame.UI;

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
                UIHelper.PrintColoredLine($"{Icon} {Target.Name} regenerates {healAmount} HP!", ConsoleColor.Green);
            }
        }

        public override string GetDescription()
        {
            return $"{Icon} {Name}: +{Value} HP/turn ({RemainingTurns} turn{(RemainingTurns != 1 ? "s" : "")})";
        }
    }
}
