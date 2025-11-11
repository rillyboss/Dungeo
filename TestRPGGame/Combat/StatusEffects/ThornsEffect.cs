using TestRPGGame.UI;

namespace TestRPGGame.Combat.StatusEffects
{
    /// <summary>
    /// Reflects damage back to attackers
    /// </summary>
    public class ThornsEffect : StatusEffect
    {
        public ThornsEffect(string effectId, string name, string icon, int duration, int reflectDamage)
            : base(effectId, name, icon, StatusEffectType.Buff, duration, reflectDamage)
        {
        }

        public override void OnTakeDamage(Entities.Combatant attacker, ref int damage)
        {
            if (attacker != null && Value > 0)
            {
                int reflected = attacker.ApplyDamage(Value, applyShieldAbsorption: false);
                UIHelper.PrintColoredLine($"   🌵 THORNS! {attacker.Name} takes {reflected} reflected damage!", ConsoleColor.Yellow);
            }
        }

        public override void OnExpire()
        {
            if (Target != null)
            {
                UIHelper.PrintColoredLine($"🌵 {Target.Name}'s thorns fade away!", ConsoleColor.Gray);
            }
        }

        public override string GetDescription()
        {
            return $"{Icon} {Name}: {Value} reflect dmg ({RemainingTurns} turn{(RemainingTurns != 1 ? "s" : "")})";
        }
    }
}
