using System;
using TestRPGGame.UI;

namespace TestRPGGame.Abilities.Effects
{
    // Damage-based effects
    public class DamageEffect : IAbilityEffect
    {
        public double Multiplier { get; set; }
        public bool UsesMagic { get; set; }
        public bool GuaranteedCrit { get; set; }

        public DamageEffect(double multiplier, bool usesMagic = false, bool guaranteedCrit = false)
        {
            Multiplier = multiplier;
            UsesMagic = usesMagic;
            GuaranteedCrit = guaranteedCrit;
        }

        public void Execute(AbilityContext context)
        {
            if (context.Enemy == null) return;

            int baseDamage = UsesMagic ? context.Player.GetTotalMagicPower() : context.Player.GetTotalAttack();
            int damage = (int)(baseDamage * Multiplier);

            // Apply buffs
            if (context.ActiveBuffs != null && context.ActiveBuffs.ContainsKey("Battle Rage") && !UsesMagic)
            {
                damage = (int)(damage * 1.5);
            }

            // Apply critical hit
            bool isCrit = GuaranteedCrit || (context.Random.NextDouble() < context.Player.CritChance);
            if (isCrit)
            {
                damage = (int)(damage * 2);
            }

            int actualDamage = Math.Max(1, damage - context.Enemy.Defense);

            context.Enemy.CurrentHP -= actualDamage;

            if (isCrit)
            {
                UIHelper.PrintColoredLine($"💥 CRITICAL! {actualDamage} damage!", ConsoleColor.Yellow);
            }
            else
            {
                UIHelper.PrintColoredLine($"⚔️  {actualDamage} damage!", ConsoleColor.White);
            }
        }

        public string GetDescription()
        {
            string dmgType = UsesMagic ? "magic" : "physical";
            string crit = GuaranteedCrit ? " (guaranteed crit)" : "";
            return $"Deal {Multiplier}x {dmgType} damage{crit}";
        }
    }
}
