using System;
using TestRPGGame.Combat;
using TestRPGGame.Combat.StatusEffects;

namespace TestRPGGame.Entities
{
    /// <summary>
    /// Base class for all entities that participate in combat (Player, Enemy).
    /// Provides shared combat properties and methods to eliminate code duplication.
    /// </summary>
    public abstract class Combatant
    {
        // Identity
        public string Name { get; set; } = "";

        // Core Combat Stats
        public int MaxHP { get; set; }
        public int CurrentHP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Agility { get; set; }  // Renamed from Speed

        // Backwards compatibility
        public int Speed
        {
            get => Agility;
            set => Agility = value;
        }

        // Status Effect System (MMO-style buffs/debuffs)
        public StatusEffectManager Effects { get; private set; }

        protected Combatant()
        {
            Effects = new StatusEffectManager(this);
        }

        /// <summary>
        /// Checks if this combatant is still alive.
        /// </summary>
        public bool IsAlive()
        {
            return CurrentHP > 0;
        }

        /// <summary>
        /// Applies damage to this combatant.
        /// This is the legacy method maintained for backward compatibility.
        /// New code should use ApplyDamage() instead.
        /// </summary>
        public virtual void TakeDamage(int damage)
        {
            // Use percentage-based defense reduction
            double defenseReduction = Defense / (double)(Defense + 100);
            int actualDamage = Math.Max(1, (int)(damage * (1 - defenseReduction)));
            CurrentHP -= actualDamage;
        }

        /// <summary>
        /// Unified damage application method for all combat damage.
        /// This method will be the single source of truth for damage calculation.
        /// It handles defense calculation, shield absorption, status effect hooks, and HP reduction.
        /// </summary>
        /// <param name="rawDamage">The incoming damage before defense/shields</param>
        /// <param name="applyShieldAbsorption">Whether to check for shield absorption (default: true)</param>
        /// <param name="attacker">The combatant dealing the damage (for Thorns reflection)</param>
        /// <returns>The actual damage dealt after defense and shields</returns>
        public virtual int ApplyDamage(int rawDamage, bool applyShieldAbsorption = true, Combatant? attacker = null)
        {
            // Step 1: Apply defense reduction using percentage-based formula
            // Formula: defense reduces damage by Defense/(Defense+100) percentage
            // This provides diminishing returns and ensures multipliers scale properly
            double defenseReduction = Defense / (double)(Defense + 100);
            int damageAfterDefense = Math.Max(1, (int)(rawDamage * (1 - defenseReduction)));

            // Step 2: Apply shield absorption using new status effect system
            int damageAfterShield = damageAfterDefense;
            if (applyShieldAbsorption)
            {
                var shield = Effects.GetActiveShield();
                if (shield != null && shield.CurrentShieldValue > 0)
                {
                    int blocked = shield.AbsorbDamage(damageAfterDefense);
                    damageAfterShield = damageAfterDefense - blocked;

                    // Remove shield if depleted
                    if (shield.CurrentShieldValue <= 0)
                    {
                        Effects.RemoveEffect(shield);
                    }
                }
            }

            // Step 3: Trigger status effect hooks (e.g., Thorns)
            if (attacker != null)
            {
                Effects.ProcessTakeDamage(attacker, ref damageAfterShield);
            }

            // Step 4: Apply final damage to HP
            int actualDamage = Math.Max(0, damageAfterShield);
            CurrentHP -= actualDamage;

            return actualDamage;
        }

        /// <summary>
        /// Calculates dodge chance based on Agility stat.
        /// Formula: Agility / 200, capped at 30% to prevent unkillable builds.
        /// Examples: 10 agility = 5% dodge, 20 agility = 10% dodge, 60+ agility = 30% dodge
        /// </summary>
        public double GetDodgeChance()
        {
            double baseDodge = Agility / 200.0;
            return Math.Min(0.30, baseDodge);  // Hard cap at 30%
        }

        /// <summary>
        /// Heals this combatant by the specified amount, capped at MaxHP.
        /// </summary>
        public void Heal(int amount)
        {
            CurrentHP = Math.Min(CurrentHP + amount, MaxHP);
        }

        /// <summary>
        /// Gets the total attack power for this combatant.
        /// Virtual to allow Player to override with equipment bonuses.
        /// </summary>
        public virtual int GetTotalAttack()
        {
            return Attack;
        }

        /// <summary>
        /// Gets the total defense for this combatant.
        /// Virtual to allow Player to override with equipment bonuses.
        /// </summary>
        public virtual int GetTotalDefense()
        {
            return Defense;
        }

    }
}
