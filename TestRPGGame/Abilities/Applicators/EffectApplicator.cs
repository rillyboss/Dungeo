using TestRPGGame.Abilities.Effects;
using TestRPGGame.Combat.StatusEffects;
using TestRPGGame.Constants;
using TestRPGGame.Interfaces;
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

        // For random damage ranges (e.g., 2.2x-2.8x damage)
        public double MinMultiplier { get; set; }
        public double MaxMultiplier { get; set; }

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
                    ApplyGenericEffect(context, kind, DisplayName, Duration, multiplier, flatValue, Icon);
                }
            }
            else
            {
                // Single effect - apply directly
                ApplyGenericEffect(context, EffectKind, DisplayName, Duration, Multiplier, FlatValue, Icon);
            }

            // Note: Output is handled by the combat system through events
        }

        private void ApplyGenericEffect(AbilityContext context, EffectKind kind, string name, int duration, double multiplier, int flatValue, string icon)
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
                        context.Source.Effects.AddEffect(effect);
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
                        context.Source.Effects.AddEffect(effect);
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
                        context.Source.Effects.AddEffect(effect);
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
                        context.Source.Effects.AddEffect(effect);
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
                        context.Source.Effects.AddEffect(effect);
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
                        context.Source.Effects.AddEffect(effect);
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
                        context.Source.Effects.AddEffect(effect);
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
                        context.Source.Effects.AddEffect(effect);
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
                        context.Source.Effects.AddEffect(effect);
                        break;
                    }

                case EffectKind.DamageOverTime:
                    {
                        var effect = new DamageOverTimeEffect(
                            name.ToLower().Replace(" ", "_"),
                            name,
                            icon,
                            duration,
                            flatValue > 0 ? flatValue : 10  // Default 10 dmg/turn
                        );
                        context.Source.Effects.AddEffect(effect);
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
                        context.Source.Effects.AddEffect(effect);
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
                        context.Source.Effects.AddEffect(effect);
                        break;
                    }

                // Immediate Actions
                case EffectKind.InstantDamage:
                    {
                        if (context.Target != null)
                        {
                            // Support random damage ranges (e.g., 2.2x-2.8x)
                            double actualMultiplier = multiplier;
                            if (MinMultiplier > 0 && MaxMultiplier > 0)
                            {
                                // Random range between min and max
                                actualMultiplier = MinMultiplier + (Utils.RandomProvider.NextDouble() * (MaxMultiplier - MinMultiplier));
                            }

                            // Calculate effective attack with buffs
                            double effectiveAttack = context.Source.Attack + context.Source.Effects.GetAttackBonus();
                            effectiveAttack *= context.Source.Effects.GetAttackMultiplier();

                            // Apply ability multiplier
                            int baseDamage = (int)(effectiveAttack * actualMultiplier);

                            // Apply general damage multipliers from source's buffs
                            baseDamage = (int)(baseDamage * context.Source.Effects.GetTotalDamageMultiplier());

                            int actualDamage = context.Target.ApplyDamage(baseDamage, applyShieldAbsorption: true, attacker: context.Source);

                            // Publish damage event for UI feedback
                            if (context.CombatInterface != null)
                            {
                                string attackerName = context.Source is Entities.Player.Player ? "Player" : (context.Source as Entities.Enemy.Enemy)?.Name ?? "Unknown";
                                string targetName = context.Target is Entities.Player.Player ? "Player" : (context.Target as Entities.Enemy.Enemy)?.Name ?? "Unknown";

                                context.CombatInterface.OnEvent(new GameEvents.DamageDealtEvent
                                {
                                    Attacker = attackerName,
                                    Target = targetName,
                                    Damage = actualDamage,
                                    IsCritical = false,
                                    AttackType = "Ability"
                                });
                            }
                        }
                        break;
                    }

                case EffectKind.Heal:
                    {
                        // Check if this is mana restoration (for players only)
                        bool isMana = name.ToLower().Contains("mana") || name.ToLower().Contains("restore");

                        if (isMana && context.Source is Entities.Player.Player player)
                        {
                            player.RestoreMana(flatValue);
                        }
                        else
                        {
                            // HP healing
                            int actualHeal = Math.Min(flatValue, context.Source.MaxHP - context.Source.CurrentHP);
                            context.Source.CurrentHP += actualHeal;

                            // Notify AI if enemy healed
                            if (context.Source is Entities.Enemy.Enemy enemy && enemy.AI != null)
                            {
                                enemy.AI.RecordHealUsed();
                            }
                        }
                        break;
                    }

                case EffectKind.LifeSteal:
                    {
                        if (context.Target != null)
                        {
                            // Calculate effective attack with buffs
                            double effectiveAttack = context.Source.Attack + context.Source.Effects.GetAttackBonus();
                            effectiveAttack *= context.Source.Effects.GetAttackMultiplier();

                            // Apply ability multiplier
                            int baseDamage = (int)(effectiveAttack * multiplier);

                            // Apply general damage multipliers from source's buffs
                            baseDamage = (int)(baseDamage * context.Source.Effects.GetTotalDamageMultiplier());

                            int actualDamage = context.Target.ApplyDamage(baseDamage, applyShieldAbsorption: true, attacker: context.Source);

                            // Publish damage event for UI feedback
                            if (context.CombatInterface != null)
                            {
                                string attackerName = context.Source is Entities.Player.Player ? "Player" : (context.Source as Entities.Enemy.Enemy)?.Name ?? "Unknown";
                                string targetName = context.Target is Entities.Player.Player ? "Player" : (context.Target as Entities.Enemy.Enemy)?.Name ?? "Unknown";

                                context.CombatInterface.OnEvent(new GameEvents.DamageDealtEvent
                                {
                                    Attacker = attackerName,
                                    Target = targetName,
                                    Damage = actualDamage,
                                    IsCritical = false,
                                    AttackType = "Ability"
                                });
                            }

                            // Heal source (caster)
                            int actualHeal = Math.Min(flatValue, context.Source.MaxHP - context.Source.CurrentHP);
                            context.Source.CurrentHP += actualHeal;
                        }
                        break;
                    }

                case EffectKind.Dodge:
                    {
                        // Dodge is handled specially by the combat system
                        // This is a marker effect that tells the combat system to dodge the next attack
                        break;
                    }

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
                EffectKind.InstantDamage => "💥",
                EffectKind.Heal => "💚",
                EffectKind.LifeSteal => "🩸",
                EffectKind.Regeneration => "💚",
                EffectKind.DamageOverTime => "☠️",
                EffectKind.Shield => "🛡️",
                EffectKind.Stun => "⚡",
                EffectKind.Dodge => "💨",
                _ => "✨"
            };
        }
    }
}
