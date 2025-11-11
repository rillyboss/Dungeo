using TestRPGGame.Abilities.Effects;

namespace TestRPGGame.Abilities.Applicators
{
    /// <summary>
    /// Applies a Burning/Poison status effect (damage over time).
    /// This is an applicator that triggers the actual DamageOverTimeEffect status effect.
    /// </summary>
    public class BurningApplicator : IAbilityEffect
    {
        public int DamagePerTurn { get; set; }
        public int Duration { get; set; }
        public int InitialDamage { get; set; }

        public BurningApplicator(int damagePerTurn, int duration, int initialDamage = 0)
        {
            DamagePerTurn = damagePerTurn;
            Duration = duration;
            InitialDamage = initialDamage;
        }

        public void Execute(AbilityContext context)
        {
            if (context.Enemy == null) return;

            if (InitialDamage > 0)
            {
                context.Enemy.CurrentHP -= InitialDamage;
            }

            // Status effect applied by CombatSystem
            // Note: Output is handled by the combat system through events
        }

        public string GetDescription()
        {
            return $"Apply poison: {DamagePerTurn} damage/turn for {Duration} turns";
        }
    }
}
