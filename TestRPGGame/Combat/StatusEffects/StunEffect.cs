using TestRPGGame.UI;

namespace TestRPGGame.Combat.StatusEffects
{
    /// <summary>
    /// Prevents the target from taking actions (skip turn)
    /// </summary>
    public class StunEffect : StatusEffect
    {
        public StunEffect(string effectId, string name, string icon, int duration)
            : base(effectId, name, icon, StatusEffectType.Control, duration)
        {
        }

        public override string GetDescription()
        {
            return $"{Icon} {Name} ({RemainingTurns} turn{(RemainingTurns != 1 ? "s" : "")})";
        }
    }
}
