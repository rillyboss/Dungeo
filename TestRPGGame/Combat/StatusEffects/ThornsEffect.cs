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
                // Note: Output is handled by the combat system through events
            }
        }

        public override void OnExpire()
        {
            // Note: Output is handled by the combat system through events
        }

        public override string GetDescription()
        {
            return $"{Icon} {Name}: {Value} reflect dmg ({RemainingTurns} turn{(RemainingTurns != 1 ? "s" : "")})";
        }
    }
}
