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
            if (context.Target == null) return;

            // Calculate base damage from source's stats
            int baseDamage;
            if (UsesMagic && context.Source is Entities.Player.Player playerSource)
            {
                baseDamage = playerSource.MagicPower;
            }
            else
            {
                baseDamage = context.Source.Attack;
            }
            int damage = (int)(baseDamage * Multiplier);

            // Apply damage multiplier from status effects (Battle Rage, etc.)
            double damageMultiplier = context.Source.Effects.GetTotalDamageMultiplier();
            damage = (int)(damage * damageMultiplier);

            // Apply critical hit (only for players for now)
            bool isCrit = false;
            if (context.Source is Entities.Player.Player player)
            {
                isCrit = GuaranteedCrit || (context.Random.NextDouble() < player.CritChance);
                if (isCrit)
                {
                    damage = (int)(damage * 2);
                }
            }

            // Apply damage to target (handles defense, shields, thorns reflection)
            int actualDamage = context.Target.ApplyDamage(damage, applyShieldAbsorption: true, attacker: context.Source);

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
