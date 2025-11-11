using System;
using TestRPGGame.Entities;

namespace TestRPGGame.Combat.StatusEffects
{
    /// <summary>
    /// Base class for all status effects (buffs, debuffs, DOTs, etc.).
    /// Represents a single effect applied to a combatant.
    /// Similar to MMO buff/debuff systems where each effect is a separate instance.
    /// </summary>
    public abstract class StatusEffect
    {
        /// <summary>
        /// Unique identifier for this effect type (e.g., "burning", "shield", "battle_rage")
        /// </summary>
        public string EffectId { get; set; } = "";

        /// <summary>
        /// Display name shown in UI
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Icon/emoji for UI display
        /// </summary>
        public string Icon { get; set; } = "";

        /// <summary>
        /// Category of this effect (Buff, Debuff, Control)
        /// </summary>
        public StatusEffectType Type { get; set; }

        /// <summary>
        /// Number of turns remaining before effect expires
        /// </summary>
        public int RemainingTurns { get; set; }

        /// <summary>
        /// Primary numeric value of the effect (damage, healing, shield amount, etc.)
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Optional multiplier value for percentage-based effects
        /// </summary>
        public double Multiplier { get; set; } = 1.0;

        /// <summary>
        /// Whether this effect can stack with itself (multiple instances)
        /// </summary>
        public bool CanStack { get; set; } = false;

        /// <summary>
        /// The combatant this effect is applied to
        /// </summary>
        public Combatant? Target { get; set; }

        /// <summary>
        /// The combatant who applied this effect (for targeting purposes)
        /// </summary>
        public Combatant? Source { get; set; }

        protected StatusEffect(string effectId, string name, string icon, StatusEffectType type, int duration, int value = 0)
        {
            EffectId = effectId;
            Name = name;
            Icon = icon;
            Type = type;
            RemainingTurns = duration;
            Value = value;
        }

        /// <summary>
        /// Called when this effect is first applied to a combatant
        /// </summary>
        public virtual void OnApply() { }

        /// <summary>
        /// Called at the start of the target's turn
        /// </summary>
        public virtual void OnTurnStart() { }

        /// <summary>
        /// Called when the target deals damage
        /// </summary>
        public virtual void OnDealDamage(Combatant target, ref int damage) { }

        /// <summary>
        /// Called when the target takes damage
        /// </summary>
        public virtual void OnTakeDamage(Combatant attacker, ref int damage) { }

        /// <summary>
        /// Called when this effect expires
        /// </summary>
        public virtual void OnExpire() { }

        /// <summary>
        /// Decrements the duration and returns true if the effect should be removed
        /// </summary>
        public bool DecrementDuration()
        {
            if (RemainingTurns > 0)
            {
                RemainingTurns--;
            }
            return RemainingTurns <= 0;
        }

        /// <summary>
        /// Gets a formatted description for UI display
        /// </summary>
        public virtual string GetDescription()
        {
            return $"{Icon} {Name} ({RemainingTurns} turn{(RemainingTurns != 1 ? "s" : "")})";
        }
    }
}
