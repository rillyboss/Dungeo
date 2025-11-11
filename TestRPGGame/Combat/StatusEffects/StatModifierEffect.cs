namespace TestRPGGame.Combat.StatusEffects
{
    /// <summary>
    /// Modifies a stat (attack, defense, speed) through a multiplier or flat bonus.
    /// Examples: Battle Rage (+50% attack), Speed Buff (+20 speed), Enrage (+damage)
    /// </summary>
    public class StatModifierEffect : StatusEffect
    {
        public enum StatType
        {
            Attack,
            Defense,
            Speed,
            Damage // General damage multiplier
        }

        public StatType Stat { get; set; }
        public bool IsMultiplier { get; set; } // true = multiply, false = flat addition

        public StatModifierEffect(string effectId, string name, string icon, StatusEffectType type,
            int duration, StatType stat, double value, bool isMultiplier = true)
            : base(effectId, name, icon, type, duration)
        {
            Stat = stat;
            IsMultiplier = isMultiplier;

            if (isMultiplier)
            {
                Multiplier = value;
            }
            else
            {
                Value = (int)value;
            }
        }

        public override void OnApply()
        {
            // Stat modifications are applied when calculating stats, not stored directly
            // This is handled by CombatSystem when checking for active buffs
        }

        public override void OnExpire()
        {
            // Note: Output is handled by the combat system through events
        }

        public override string GetDescription()
        {
            string valueStr = IsMultiplier
                ? $"{(Multiplier - 1.0) * 100:+0;-0}%"
                : $"{Value:+0;-0}";
            return $"{Icon} {Name}: {valueStr} {Stat} ({RemainingTurns} turn{(RemainingTurns != 1 ? "s" : "")})";
        }

        /// <summary>
        /// Gets the effective modifier value for damage calculations
        /// </summary>
        public double GetDamageMultiplier()
        {
            return Stat == StatType.Damage || Stat == StatType.Attack ? Multiplier : 1.0;
        }
    }
}
