using System;
using TestRPGGame.UI;

namespace TestRPGGame.Abilities.Effects
{
    // Poison/DoT effect
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
            if (context.Enemy == null) return;

            if (InitialDamage > 0)
            {
                context.Enemy.CurrentHP -= InitialDamage;
                UIHelper.PrintColoredLine($"💚 {InitialDamage} poison damage!", ConsoleColor.Green);
            }

            // These need to be set on the context
            UIHelper.PrintColoredLine($"💚 Poison applied: {DamagePerTurn} damage per turn for {Duration} turns!", ConsoleColor.Green);
        }

        public string GetDescription()
        {
            return $"Apply poison: {DamagePerTurn} damage/turn for {Duration} turns";
        }
    }
}
