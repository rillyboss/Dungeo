using System;
using TestRPGGame.UI;

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
            if (context.Enemy == null || context.Player == null) return;

            // Deal damage
            int damage = (int)(context.Enemy.Attack * DamageMultiplier);
            int actualDamage = Math.Max(1, damage - context.Player.GetTotalDefense());
            context.Player.CurrentHP -= actualDamage;

            // Heal caster
            context.Enemy.CurrentHP = Math.Min(context.Enemy.MaxHP, context.Enemy.CurrentHP + HealAmount);

            UIHelper.PrintColoredLine($"💉 Life Steal! {actualDamage} damage dealt, {HealAmount} HP gained!", ConsoleColor.DarkRed);
        }

        public string GetDescription()
        {
            return $"Drain life: Deal {DamageMultiplier}x damage and heal {HealAmount} HP";
        }
    }
}
