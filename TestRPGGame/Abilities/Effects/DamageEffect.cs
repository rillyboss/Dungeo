using System;

namespace TestRPGGame.Abilities.Effects
{
    // Damage-based effects
    public class DamageEffect : IAbilityEffect
    {
        public double Multiplier { get; set; }
        public bool UsesMagic { get; set; }
        public bool GuaranteedCrit { get; set; }

        // New: Min/Max multiplier system for damage variance
        public double MinMultiplier { get; set; }
        public double MaxMultiplier { get; set; }
        public double Accuracy { get; set; } = 1.0;  // 1.0 = 100% accuracy

        // Original constructor for backwards compatibility
        public DamageEffect(double multiplier, bool usesMagic = false, bool guaranteedCrit = false)
        {
            Multiplier = multiplier;
            UsesMagic = usesMagic;
            GuaranteedCrit = guaranteedCrit;

            // Default: no variance (min = max = multiplier)
            MinMultiplier = multiplier;
            MaxMultiplier = multiplier;
            Accuracy = 1.0;
        }

        // New constructor with variance
        public DamageEffect(double minMultiplier, double maxMultiplier, bool usesMagic = false, bool guaranteedCrit = false, double accuracy = 1.0)
        {
            MinMultiplier = minMultiplier;
            MaxMultiplier = maxMultiplier;
            Multiplier = (minMultiplier + maxMultiplier) / 2.0;  // Average for backwards compat
            UsesMagic = usesMagic;
            GuaranteedCrit = guaranteedCrit;
            Accuracy = accuracy;
        }

        public void Execute(AbilityContext context)
        {
            if (context.Target == null) return;

            // Step 1: Check for hit/miss based on accuracy and target's dodge chance
            double dodgeChance = context.Target.GetDodgeChance();
            double hitChance = Accuracy * (1.0 - dodgeChance);

            if (context.Random.NextDouble() >= hitChance)
            {
                // MISS! Determine if it was a dodge or miss
                bool wasDodged = context.Random.NextDouble() < dodgeChance / (1.0 - hitChance);

                // Publish miss event (combat system will handle display)
                if (context.CombatInterface != null)
                {
                    context.CombatInterface.OnEvent(new TestRPGGame.Interfaces.GameEvents.AttackMissedEvent
                    {
                        Attacker = context.Source.Name,
                        Target = context.Target.Name,
                        MissType = wasDodged ? "Dodge" : "Miss",
                        AttackType = UsesMagic ? "Magic" : "Physical"
                    });
                }
                return; // No damage dealt
            }

            // Step 2: Roll damage multiplier within min/max range
            double rolledMultiplier = MinMultiplier +
                (context.Random.NextDouble() * (MaxMultiplier - MinMultiplier));

            // Step 3: Calculate base damage from source's stats
            int baseDamage;
            if (UsesMagic && context.Source is Entities.Player.Player playerSource)
            {
                baseDamage = playerSource.MagicPower;
            }
            else
            {
                baseDamage = context.Source.Attack;
            }
            int damage = (int)(baseDamage * rolledMultiplier);

            // Step 4: Apply damage multiplier from status effects (Battle Rage, etc.)
            double statusMultiplier = context.Source.Effects.GetTotalDamageMultiplier();
            damage = (int)(damage * statusMultiplier);

            // Step 5: Apply critical hit (only for players for now)
            // Agility provides bonus crit chance
            bool isCrit = false;
            if (context.Source is Entities.Player.Player player)
            {
                double totalCritChance = player.CritChance + (player.Agility / 500.0);
                isCrit = GuaranteedCrit || (context.Random.NextDouble() < totalCritChance);
                if (isCrit)
                {
                    damage = (int)(damage * 2);
                }
            }

            // Step 6: Apply damage to target (handles defense, shields, thorns reflection)
            int actualDamage = context.Target.ApplyDamage(damage, applyShieldAbsorption: true, attacker: context.Source);

            // Note: Output is handled by the combat system through events
        }

        public string GetDescription()
        {
            string dmgType = UsesMagic ? "magic" : "physical";
            string crit = GuaranteedCrit ? " (guaranteed crit)" : "";
            return $"Deal {Multiplier}x {dmgType} damage{crit}";
        }
    }
}
