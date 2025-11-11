using System;

namespace TestRPGGame.Abilities.Effects
{
    // Life steal effect - damages enemy and heals caster
    public class LifeStealEffect : IAbilityEffect
    {
        public double DamageMultiplier { get; set; }
        public int HealAmount { get; set; }

        public LifeStealEffect(double damageMultiplier, int healAmount)
        {
            DamageMultiplier = damageMultiplier;
            HealAmount = healAmount;
        }

        public void Execute(AbilityContext context)
        {
            if (context.Target == null) return;

            // Deal damage to target
            int baseDamage = (int)(context.Source.Attack * DamageMultiplier);
            int actualDamage = context.Target.ApplyDamage(baseDamage, applyShieldAbsorption: true, attacker: context.Source);

            // Heal source (caster)
            int actualHeal = Math.Min(HealAmount, context.Source.MaxHP - context.Source.CurrentHP);
            context.Source.CurrentHP += actualHeal;

            // Note: Output is handled by the combat system through events
        }

        public string GetDescription()
        {
            return $"Drain life: Deal {DamageMultiplier}x damage and heal {HealAmount} HP";
        }
    }
}
