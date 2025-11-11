using System;
using TestRPGGame.Combat.StatusEffects;

namespace TestRPGGame.Abilities.Effects
{
    /// <summary>
    /// Poison/DoT effect - applies burning status effect to target
    /// </summary>
    public class PoisonEffect : IAbilityEffect
    {
        public int DamagePerTurn { get; set; }
        public int Duration { get; set; }
        public int InitialDamage { get; set; }

        public PoisonEffect(int damagePerTurn, int duration, int initialDamage = 0)
        {
            DamagePerTurn = damagePerTurn;
            Duration = duration;
            InitialDamage = initialDamage;
        }

        public void Execute(AbilityContext context)
        {
            if (context.Target == null) return;

            if (InitialDamage > 0)
            {
                context.Target.ApplyDamage(InitialDamage, applyShieldAbsorption: true, attacker: context.Source);
            }

            // Apply burning/poison status effect
            context.Target.ApplyBurning(context.Source, Duration, DamagePerTurn);

            // Note: Output is handled by the combat system through events
        }

        public string GetDescription()
        {
            return $"Apply poison: {DamagePerTurn} damage/turn for {Duration} turns";
        }
    }
}
