using System;
using System.Collections.Generic;
using System.Linq;
using TestRPGGame.Entities;

namespace TestRPGGame.Combat.StatusEffects
{
    /// <summary>
    /// Manages a collection of status effects for a combatant.
    /// Handles application, expiration, and triggering of effects.
    /// Similar to MMO buff/debuff managers.
    /// </summary>
    public class StatusEffectManager
    {
        private List<StatusEffect> activeEffects;
        private Combatant owner;

        public StatusEffectManager(Combatant owner)
        {
            this.owner = owner;
            activeEffects = new List<StatusEffect>();
        }

        /// <summary>
        /// Gets all currently active effects
        /// </summary>
        public IReadOnlyList<StatusEffect> ActiveEffects => activeEffects.AsReadOnly();

        /// <summary>
        /// Adds a new status effect to the combatant.
        ///
        /// Stacking Rules:
        /// - Non-stacking effects (CanStack = false): Refreshes duration of existing effect
        /// - Stacking effects (CanStack = true): Adds new stack up to MaxStacks limit
        /// - At max stacks: Refreshes duration but doesn't add more stacks
        /// </summary>
        public void AddEffect(StatusEffect effect)
        {
            effect.Target = owner;

            var existing = activeEffects.FirstOrDefault(e => e.EffectId == effect.EffectId);

            if (!effect.CanStack)
            {
                // NON-STACKING: Replace existing and refresh duration
                if (existing != null)
                {
                    // Refresh the duration instead of replacing the entire effect
                    existing.RemainingTurns = effect.RemainingTurns;
                    existing.JustApplied = true; // Reset the JustApplied flag

                    // Update value/multiplier in case they changed
                    existing.Value = effect.Value;
                    existing.Multiplier = effect.Multiplier;

                    return; // Don't add a new effect
                }
            }
            else
            {
                // STACKING: Increment stacks or refresh if at max
                if (existing != null)
                {
                    // Check if we can add more stacks
                    bool atMaxStacks = effect.MaxStacks > 0 && existing.CurrentStacks >= effect.MaxStacks;

                    if (atMaxStacks)
                    {
                        // At max stacks: Refresh duration but don't add more stacks
                        existing.RemainingTurns = effect.RemainingTurns;
                        existing.JustApplied = true;
                        return;
                    }
                    else
                    {
                        // Can add more stacks: Increment and refresh
                        existing.CurrentStacks++;
                        existing.RemainingTurns = effect.RemainingTurns;
                        existing.JustApplied = true;
                        return;
                    }
                }
            }

            // New effect: Add to list
            activeEffects.Add(effect);
            effect.OnApply();
        }

        /// <summary>
        /// Removes a specific effect
        /// </summary>
        public void RemoveEffect(StatusEffect effect)
        {
            if (activeEffects.Remove(effect))
            {
                effect.OnExpire();
            }
        }

        /// <summary>
        /// Removes all effects of a specific type
        /// </summary>
        public void RemoveEffectById(string effectId)
        {
            var effects = activeEffects.Where(e => e.EffectId == effectId).ToList();
            foreach (var effect in effects)
            {
                RemoveEffect(effect);
            }
        }

        /// <summary>
        /// Removes all effects
        /// </summary>
        public void ClearAll()
        {
            foreach (var effect in activeEffects.ToList())
            {
                effect.OnExpire();
            }
            activeEffects.Clear();
        }

        /// <summary>
        /// Processes all effects at the start of the combatant's turn
        /// </summary>
        public void ProcessTurnStart()
        {
            // Process effects in a copy to avoid modification during iteration
            var effectsCopy = activeEffects.ToList();

            foreach (var effect in effectsCopy)
            {
                effect.OnTurnStart();
            }

            // Remove expired effects
            var expired = activeEffects.Where(e => e.DecrementDuration()).ToList();
            foreach (var effect in expired)
            {
                effect.OnExpire();
                activeEffects.Remove(effect);
            }
        }

        /// <summary>
        /// Triggers OnDealDamage for all active effects
        /// </summary>
        public void ProcessDealDamage(Combatant target, ref int damage)
        {
            foreach (var effect in activeEffects.ToList())
            {
                effect.OnDealDamage(target, ref damage);
            }
        }

        /// <summary>
        /// Triggers OnTakeDamage for all active effects
        /// </summary>
        public void ProcessTakeDamage(Combatant attacker, ref int damage)
        {
            foreach (var effect in activeEffects.ToList())
            {
                effect.OnTakeDamage(attacker, ref damage);
            }
        }

        /// <summary>
        /// Gets all active buffs (positive effects)
        /// </summary>
        public List<StatusEffect> GetBuffs()
        {
            return activeEffects.Where(e => e.Type == StatusEffectType.Buff).ToList();
        }

        /// <summary>
        /// Gets all active debuffs (negative effects)
        /// </summary>
        public List<StatusEffect> GetDebuffs()
        {
            return activeEffects.Where(e => e.Type == StatusEffectType.Debuff).ToList();
        }

        /// <summary>
        /// Gets all active control effects (CC)
        /// </summary>
        public List<StatusEffect> GetControlEffects()
        {
            return activeEffects.Where(e => e.Type == StatusEffectType.Control).ToList();
        }

        /// <summary>
        /// Checks if a specific effect is active
        /// </summary>
        public bool HasEffect(string effectId)
        {
            return activeEffects.Any(e => e.EffectId == effectId);
        }

        /// <summary>
        /// Gets the first effect with the specified ID
        /// </summary>
        public StatusEffect? GetEffect(string effectId)
        {
            return activeEffects.FirstOrDefault(e => e.EffectId == effectId);
        }

        /// <summary>
        /// Gets all stat modifier effects that affect a specific stat
        /// </summary>
        public List<StatModifierEffect> GetStatModifiers(StatModifierEffect.StatType stat)
        {
            return activeEffects
                .OfType<StatModifierEffect>()
                .Where(e => e.Stat == stat)
                .ToList();
        }

        /// <summary>
        /// Calculates total damage multiplier from DAMAGE-SPECIFIC buffs only.
        /// Does NOT include Attack stat modifiers (those are handled by GetAttackMultiplier).
        /// This allows Attack buffs and Damage buffs to stack multiplicatively.
        /// </summary>
        public double GetTotalDamageMultiplier()
        {
            double multiplier = 1.0;
            foreach (var effect in activeEffects.OfType<StatModifierEffect>())
            {
                // Only count Damage-type modifiers, NOT Attack modifiers
                // Attack modifiers are applied separately via GetAttackMultiplier()
                if (effect.Stat == StatModifierEffect.StatType.Damage && effect.IsMultiplier)
                {
                    multiplier *= effect.Multiplier;
                }
            }
            return multiplier;
        }

        /// <summary>
        /// Gets the active shield effect if one exists
        /// </summary>
        public ShieldEffect? GetActiveShield()
        {
            return activeEffects.OfType<ShieldEffect>().FirstOrDefault();
        }

        /// <summary>
        /// Gets the total shield value from all active shields
        /// </summary>
        public int GetTotalShieldValue()
        {
            return activeEffects.OfType<ShieldEffect>().Sum(s => s.CurrentShieldValue);
        }

        /// <summary>
        /// Checks if the combatant is stunned
        /// </summary>
        public bool IsStunned()
        {
            return activeEffects.Any(e => e is StunEffect);
        }

        /// <summary>
        /// Gets total speed modifier from active effects
        /// </summary>
        public int GetSpeedModifier()
        {
            return activeEffects.OfType<StatModifierEffect>()
                .Where(e => e.Stat == StatModifierEffect.StatType.Speed && !e.IsMultiplier)
                .Sum(e => e.Value);
        }

        /// <summary>
        /// Gets total defense multiplier from active effects.
        /// Includes both defense-specific and general defensive buffs.
        /// Example: Shield Wall might add 1.5x defense multiplier
        /// </summary>
        public double GetDefenseMultiplier()
        {
            double multiplier = 1.0;
            foreach (var effect in activeEffects.OfType<StatModifierEffect>())
            {
                if (effect.Stat == StatModifierEffect.StatType.Defense && effect.IsMultiplier)
                {
                    multiplier *= effect.Multiplier;
                }
            }
            return multiplier;
        }

        /// <summary>
        /// Gets total attack stat multiplier from active effects.
        /// This is for modifying the Attack stat before damage calculations.
        /// Example: Battle Rage adds 1.5x attack multiplier
        /// </summary>
        public double GetAttackMultiplier()
        {
            double multiplier = 1.0;
            foreach (var effect in activeEffects.OfType<StatModifierEffect>())
            {
                if (effect.Stat == StatModifierEffect.StatType.Attack && effect.IsMultiplier)
                {
                    multiplier *= effect.Multiplier;
                }
            }
            return multiplier;
        }

        /// <summary>
        /// Gets flat attack bonus from active effects
        /// </summary>
        public int GetAttackBonus()
        {
            return activeEffects.OfType<StatModifierEffect>()
                .Where(e => e.Stat == StatModifierEffect.StatType.Attack && !e.IsMultiplier)
                .Sum(e => e.Value);
        }

        /// <summary>
        /// Gets flat defense bonus from active effects
        /// </summary>
        public int GetDefenseBonus()
        {
            return activeEffects.OfType<StatModifierEffect>()
                .Where(e => e.Stat == StatModifierEffect.StatType.Defense && !e.IsMultiplier)
                .Sum(e => e.Value);
        }

        // Note: DisplayAllEffects() removed - UI output is handled by the combat system through events
    }
}
