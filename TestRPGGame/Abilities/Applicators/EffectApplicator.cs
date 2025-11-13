using TestRPGGame.Abilities.Effects;
using TestRPGGame.Combat.StatusEffects;
using TestRPGGame.Constants;
using System;

namespace TestRPGGame.Abilities.Applicators
{
    /// <summary>
    /// NEW: Generic, composable effect applicator.
    ///
    /// Instead of hardcoding specific buffs like "Battle Rage" or "Enraged",
    /// this applies generic effect types (AttackBoost, DefenseBoost, etc.)
    /// with custom display names and parameters from data.
    ///
    /// Examples:
    ///   - EffectKind.AttackBoost + DisplayName="Battle Rage" + Multiplier=1.5
    ///   - EffectKind.AttackBoost + DisplayName="Enraged" + Multiplier=1.8
    ///   - EffectKind.DefenseBoost + DisplayName="Iron Skin" + Multiplier=2.0
    ///
    /// This allows content creators to compose unique effects in JSON without code changes.
    /// </summary>
    public class EffectApplicator : IAbilityEffect
    {
        public EffectKind EffectKind { get; set; }
        public string DisplayName { get; set; } = "";
        public string Icon { get; set; } = "";
        public int Duration { get; set; }
        public double Multiplier { get; set; } = 1.0;
        public int FlatValue { get; set; }

        // For composite effects (e.g., Banner = AttackBoost + DefenseBoost + SpeedBoost)
        public List<(EffectKind kind, double multiplier, int flatValue)>? CompositeEffects { get; set; }

        public EffectApplicator(EffectKind effectKind, string displayName, int duration, double multiplier = 1.0, int flatValue = 0, string icon = "")
        {
            EffectKind = effectKind;
            DisplayName = displayName;
            Duration = duration;
            Multiplier = multiplier;
            FlatValue = flatValue;
            Icon = string.IsNullOrEmpty(icon) ? GetDefaultIcon(effectKind) : icon;
        }

        public void Execute(AbilityContext context)
        {
            // If this is a composite effect, apply all sub-effects
            if (CompositeEffects != null && CompositeEffects.Count > 0)
            {
                foreach (var (kind, multiplier, flatValue) in CompositeEffects)
                {
                    ApplyGenericEffect(context.Source, kind, DisplayName, Duration, multiplier, flatValue, Icon);
                }
            }
            else
            {
                // Single effect - apply directly
                ApplyGenericEffect(context.Source, EffectKind, DisplayName, Duration, Multiplier, FlatValue, Icon);
            }

            // Note: Output is handled by the combat system through events
        }

        private void ApplyGenericEffect(Entities.Combatant target, EffectKind kind, string name, int duration, double multiplier, int flatValue, string icon)
        {
            switch (kind)
            {
                case EffectKind.AttackBoost:
                    {
                        var effect = new StatModifierEffect(
                            name.ToLower().Replace(" ", "_"),
                            name,
                            icon,
                            StatusEffectType.Buff,
                            duration,
                            StatModifierEffect.StatType.Attack,
                            multiplier,
                            isMultiplier: true
                        );
                        target.Effects.AddEffect(effect);
                        break;
                    }

                case EffectKind.DefenseBoost:
                    {
                        var effect = new StatModifierEffect(
                            name.ToLower().Replace(" ", "_"),
                            name,
                            icon,
                            StatusEffectType.Buff,
                            duration,
                            StatModifierEffect.StatType.Defense,
                            multiplier,
                            isMultiplier: true
                        );
                        target.Effects.AddEffect(effect);
                        break;
                    }

                case EffectKind.SpeedBoost:
                    {
                        var effect = new StatModifierEffect(
                            name.ToLower().Replace(" ", "_"),
                            name,
                            icon,
                            StatusEffectType.Buff,
                            duration,
                            StatModifierEffect.StatType.Speed,
                            flatValue != 0 ? flatValue : (int)multiplier,
                            isMultiplier: flatValue == 0
                        );
                        target.Effects.AddEffect(effect);
                        break;
                    }

                case EffectKind.DamageBoost:
                    {
                        var effect = new StatModifierEffect(
                            name.ToLower().Replace(" ", "_"),
                            name,
                            icon,
                            StatusEffectType.Buff,
                            duration,
                            StatModifierEffect.StatType.Damage,
                            multiplier,
                            isMultiplier: true
                        );
                        target.Effects.AddEffect(effect);
                        break;
                    }

                case EffectKind.EvasionBoost:
                    {
                        // Evasion implemented as defense multiplier (reduces damage taken)
                        var effect = new StatModifierEffect(
                            name.ToLower().Replace(" ", "_"),
                            name,
                            icon,
                            StatusEffectType.Buff,
                            duration,
                            StatModifierEffect.StatType.Defense,
                            multiplier,
                            isMultiplier: true
                        );
                        target.Effects.AddEffect(effect);
                        break;
                    }

                case EffectKind.AttackReduction:
                    {
                        var effect = new StatModifierEffect(
                            name.ToLower().Replace(" ", "_"),
                            name,
                            icon,
                            StatusEffectType.Debuff,
                            duration,
                            StatModifierEffect.StatType.Attack,
                            multiplier,
                            isMultiplier: true
                        );
                        target.Effects.AddEffect(effect);
                        break;
                    }

                case EffectKind.DefenseReduction:
                    {
                        var effect = new StatModifierEffect(
                            name.ToLower().Replace(" ", "_"),
                            name,
                            icon,
                            StatusEffectType.Debuff,
                            duration,
                            StatModifierEffect.StatType.Defense,
                            multiplier,
                            isMultiplier: true
                        );
                        target.Effects.AddEffect(effect);
                        break;
                    }

                case EffectKind.SpeedReduction:
                    {
                        var effect = new StatModifierEffect(
                            name.ToLower().Replace(" ", "_"),
                            name,
                            icon,
                            StatusEffectType.Debuff,
                            duration,
                            StatModifierEffect.StatType.Speed,
                            flatValue != 0 ? -flatValue : (int)multiplier,
                            isMultiplier: flatValue == 0
                        );
                        target.Effects.AddEffect(effect);
                        break;
                    }

                case EffectKind.Regeneration:
                    {
                        var effect = new HealOverTimeEffect(
                            name.ToLower().Replace(" ", "_"),
                            name,
                            icon,
                            duration,
                            flatValue > 0 ? flatValue : 10  // Default 10 HP/turn
                        );
                        target.Effects.AddEffect(effect);
                        break;
                    }

                case EffectKind.Shield:
                    {
                        var effect = new ShieldEffect(
                            name.ToLower().Replace(" ", "_"),
                            name,
                            icon,
                            duration,
                            flatValue > 0 ? flatValue : 50  // Default 50 HP shield
                        );
                        target.Effects.AddEffect(effect);
                        break;
                    }

                case EffectKind.Stun:
                    {
                        var effect = new StunEffect(
                            name.ToLower().Replace(" ", "_"),
                            name,
                            icon,
                            duration
                        );
                        target.Effects.AddEffect(effect);
                        break;
                    }

                // Add more effect kinds as needed

                default:
                    throw new ArgumentException($"Unsupported EffectKind: {kind}");
            }
        }

        public string GetDescription()
        {
            return $"Apply {DisplayName} for {Duration} turns";
        }

        private static string GetDefaultIcon(EffectKind kind)
        {
            return kind switch
            {
                EffectKind.AttackBoost => "⚔️",
                EffectKind.DefenseBoost => "🛡️",
                EffectKind.SpeedBoost => "⚡",
                EffectKind.DamageBoost => "💥",
                EffectKind.EvasionBoost => "💨",
                EffectKind.AttackReduction => "🔻",
                EffectKind.DefenseReduction => "🎯",
                EffectKind.SpeedReduction => "🦶",
                EffectKind.Regeneration => "💚",
                EffectKind.Shield => "🛡️",
                EffectKind.Stun => "⚡",
                _ => "✨"
            };
        }
    }
}
