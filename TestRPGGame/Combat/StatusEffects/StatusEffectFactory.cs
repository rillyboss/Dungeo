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
            return new DamageOverTimeEffect("burning", "Burning", "🔥", duration, damagePerTurn);
        }

        public static DamageOverTimeEffect CreatePoison(int duration, int damagePerTurn)
        {
            return new DamageOverTimeEffect("poison", "Poisoned", "☠️", duration, damagePerTurn);
        }

        public static DamageOverTimeEffect CreateBleed(int duration, int damagePerTurn)
        {
            return new DamageOverTimeEffect("bleed", "Bleeding", "🩸", duration, damagePerTurn);
        }

        // HOT Effects
        public static HealOverTimeEffect CreateRegeneration(int duration, int healPerTurn)
        {
            return new HealOverTimeEffect("regeneration", "Regeneration", "💚", duration, healPerTurn);
        }

        // Defensive Effects
        public static ShieldEffect CreateShield(int duration, int shieldAmount)
        {
            return new ShieldEffect("shield", "Shield", "🛡️", duration, shieldAmount);
        }

        public static ThornsEffect CreateThorns(int duration, int reflectDamage)
        {
            return new ThornsEffect("thorns", "Thorns", "🌵", duration, reflectDamage);
        }

        // Stat Buff Effects
        public static StatModifierEffect CreateBattleRage(int duration)
        {
            return new StatModifierEffect(
                "battle_rage",
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
                "enrage",
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
                "speed_buff",
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
                "shield_wall",
                "Shield Wall",
                "🛡️",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Defense,
                0.5, // 50% damage reduction (simulated as defense buff)
                isMultiplier: true
            );
        }

        // Control Effects
        public static StunEffect CreateStun(int duration)
        {
            return new StunEffect("stun", "Stunned", "⚡", duration);
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
    }
}
