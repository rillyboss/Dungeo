using TestRPGGame.UI;

namespace TestRPGGame.Combat.StatusEffects
{
    /// <summary>
    /// Absorbs damage before HP is reduced.
    /// Shield value is tracked internally and checked by Combatant.ApplyDamage()
    /// </summary>
    public class ShieldEffect : StatusEffect
    {
        public int CurrentShieldValue { get; set; }

        public ShieldEffect(string effectId, string name, string icon, int duration, int shieldAmount)
            : base(effectId, name, icon, StatusEffectType.Buff, duration, shieldAmount)
        {
            CurrentShieldValue = shieldAmount;
        }

        public override void OnApply()
        {
            CurrentShieldValue = Value;
        }

        public override void OnExpire()
        {
            CurrentShieldValue = 0;
            if (Target != null)
            {
                UIHelper.PrintColoredLine($"🛡️  {Target.Name}'s shield shatters!", ConsoleColor.Gray);
            }
        }

        public override string GetDescription()
        {
            return $"{Icon} {Name}: {CurrentShieldValue} HP ({RemainingTurns} turn{(RemainingTurns != 1 ? "s" : "")})";
        }

        /// <summary>
        /// Absorbs damage and returns the amount blocked
        /// </summary>
        public int AbsorbDamage(int incomingDamage)
        {
            int blocked = Math.Min(incomingDamage, CurrentShieldValue);
            CurrentShieldValue -= blocked;
            return blocked;
        }
    }
}
