using TestRPGGame.Constants;
using TestRPGGame.Entities;

namespace TestRPGGame.Combat.StatusEffects
{
    /// <summary>
    /// Factory for creating common status effects.
    /// Provides convenient methods for applying standard buffs/debuffs.
    /// </summary>
    public static class StatusEffectFactory
    {
        // DOT Effects
        public static DamageOverTimeEffect CreateBurning(int duration, int damagePerTurn)
        {
            return new DamageOverTimeEffect(StatusEffectId.Burning.GetIdentifier(), "Burning", "🔥", duration, damagePerTurn);
        }

        public static DamageOverTimeEffect CreatePoison(int duration, int damagePerTurn)
        {
            return new DamageOverTimeEffect(StatusEffectId.Poison.GetIdentifier(), "Poisoned", "☠️", duration, damagePerTurn);
        }

        public static DamageOverTimeEffect CreateBleed(int duration, int damagePerTurn)
        {
            return new DamageOverTimeEffect(StatusEffectId.Bleed.GetIdentifier(), "Bleeding", "🩸", duration, damagePerTurn);
        }

        // HOT Effects
        public static HealOverTimeEffect CreateRegeneration(int duration, int healPerTurn)
        {
            return new HealOverTimeEffect(StatusEffectId.Regeneration.GetIdentifier(), "Regeneration", "💚", duration, healPerTurn);
        }

        // Defensive Effects
        public static ShieldEffect CreateShield(int duration, int shieldAmount)
        {
            return new ShieldEffect(StatusEffectId.Shield.GetIdentifier(), "Shield", "🛡️", duration, shieldAmount);
        }

        public static ThornsEffect CreateThorns(int duration, int reflectDamage)
        {
            return new ThornsEffect(StatusEffectId.Thorns.GetIdentifier(), "Thorns", "🌵", duration, reflectDamage);
        }

        // Stat Buff Effects
        public static StatModifierEffect CreateBattleRage(int duration)
        {
            return new StatModifierEffect(
                StatusEffectId.BattleRage.GetIdentifier(),
                "Battle Rage",
                "😤",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Damage,
                1.5, // 50% damage increase
                isMultiplier: true
            );
        }

        public static StatModifierEffect CreateEnrage(int duration, double damageMultiplier)
        {
            return new StatModifierEffect(
                StatusEffectId.Enrage.GetIdentifier(),
                "Enraged",
                "💢",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Damage,
                damageMultiplier,
                isMultiplier: true
            );
        }

        public static StatModifierEffect CreateSpeedBuff(int duration, int speedBonus)
        {
            return new StatModifierEffect(
                StatusEffectId.SpeedBuff.GetIdentifier(),
                "Speed Boost",
                "⚡",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Speed,
                speedBonus,
                isMultiplier: false
            );
        }

        public static StatModifierEffect CreateShieldWall(int duration)
        {
            return new StatModifierEffect(
                StatusEffectId.ShieldWall.GetIdentifier(),
                "Shield Wall",
                "🛡️",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Defense,
                0.5, // 50% damage reduction (simulated as defense buff)
                isMultiplier: true
            );
        }

        public static StatModifierEffect CreateSmokeScreen(int duration)
        {
            return new StatModifierEffect(
                StatusEffectId.SmokeScreen.GetIdentifier(),
                "Smoke Screen",
                "💨",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Defense,
                0.7, // 30% damage reduction (simulated as defense buff)
                isMultiplier: true
            );
        }

        // Control Effects
        public static StunEffect CreateStun(int duration)
        {
            return new StunEffect(StatusEffectId.Stun.GetIdentifier(), "Stunned", "⚡", duration);
        }

        // Extension method for easy application
        public static void ApplyBurning(this Combatant target, Combatant source, int duration, int damagePerTurn)
        {
            var effect = CreateBurning(duration, damagePerTurn);
            effect.Source = source;
            target.Effects.AddEffect(effect);
        }

        public static void ApplyPoison(this Combatant target, Combatant source, int duration, int damagePerTurn)
        {
            var effect = CreatePoison(duration, damagePerTurn);
            effect.Source = source;
            target.Effects.AddEffect(effect);
        }

        public static void ApplyBleed(this Combatant target, Combatant source, int duration, int damagePerTurn)
        {
            var effect = CreateBleed(duration, damagePerTurn);
            effect.Source = source;
            target.Effects.AddEffect(effect);
        }

        public static void ApplyRegeneration(this Combatant target, int duration, int healPerTurn)
        {
            var effect = CreateRegeneration(duration, healPerTurn);
            target.Effects.AddEffect(effect);
        }

        public static void ApplyShield(this Combatant target, int duration, int shieldAmount)
        {
            var effect = CreateShield(duration, shieldAmount);
            target.Effects.AddEffect(effect);
        }

        public static void ApplyThorns(this Combatant target, int duration, int reflectDamage)
        {
            var effect = CreateThorns(duration, reflectDamage);
            target.Effects.AddEffect(effect);
        }

        public static void ApplyBattleRage(this Combatant target, int duration)
        {
            var effect = CreateBattleRage(duration);
            target.Effects.AddEffect(effect);
        }

        public static void ApplyEnrage(this Combatant target, int duration, double damageMultiplier)
        {
            var effect = CreateEnrage(duration, damageMultiplier);
            target.Effects.AddEffect(effect);
        }

        public static void ApplySpeedBuff(this Combatant target, int duration, int speedBonus)
        {
            var effect = CreateSpeedBuff(duration, speedBonus);
            target.Effects.AddEffect(effect);
        }

        public static void ApplyStun(this Combatant target, int duration)
        {
            var effect = CreateStun(duration);
            target.Effects.AddEffect(effect);
        }

        public static void ApplyShieldWall(this Combatant target, int duration)
        {
            var effect = CreateShieldWall(duration);
            target.Effects.AddEffect(effect);
        }

        public static void ApplySmokeScreen(this Combatant target, int duration)
        {
            var effect = CreateSmokeScreen(duration);
            target.Effects.AddEffect(effect);
        }
    }
}
