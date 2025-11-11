using System;
using TestRPGGame.Combat.StatusEffects;
using TestRPGGame.UI;

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
                UIHelper.PrintColoredLine($"💚 {InitialDamage} poison damage!", ConsoleColor.Green);
            }

            // Apply burning/poison status effect
            context.Target.ApplyBurning(context.Source, Duration, DamagePerTurn);
            UIHelper.PrintColoredLine($"💚 Poison applied: {DamagePerTurn} damage per turn for {Duration} turns!", ConsoleColor.Green);
        }

        public string GetDescription()
        {
            return $"Apply poison: {DamagePerTurn} damage/turn for {Duration} turns";
        }
    }
}
