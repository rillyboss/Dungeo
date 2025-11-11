namespace TestRPGGame.Combat.StatusEffects
{
    /// <summary>
    /// Deals damage at the start of each turn (burning, poison, etc.)
    /// </summary>
    public class DamageOverTimeEffect : StatusEffect
    {
        public DamageOverTimeEffect(string effectId, string name, string icon, int duration, int damagePerTurn)
            : base(effectId, name, icon, StatusEffectType.Debuff, duration, damagePerTurn)
        {
        }

        public override void OnTurnStart()
        {
            if (Target != null && Source != null)
            {
                int damage = Target.ApplyDamage(Value, applyShieldAbsorption: false);
                // Note: Output is handled by the combat system through events
            }
        }

        public override string GetDescription()
        {
            return $"{Icon} {Name}: {Value} dmg/turn ({RemainingTurns} turn{(RemainingTurns != 1 ? "s" : "")})";
        }
    }
}
