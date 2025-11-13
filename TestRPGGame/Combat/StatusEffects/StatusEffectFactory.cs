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

        // ===== NEW WARRIOR BUFFS =====

        public static StatModifierEffect CreateEnraged(int duration)
        {
            return new StatModifierEffect(
                StatusEffectId.Enraged.GetIdentifier(),
                "Enraged",
                "💢",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Attack,
                1.8, // 80% attack increase
                isMultiplier: true
            );
        }

        public static void ApplyEnraged(this Combatant target, int duration)
        {
            var effect = CreateEnraged(duration);
            target.Effects.AddEffect(effect);
        }

        public static StatModifierEffect CreateBloodlust(int duration)
        {
            // Note: Lifesteal mechanic would need special implementation
            // For now, implement as attack boost + regen
            return new StatModifierEffect(
                StatusEffectId.Bloodlust.GetIdentifier(),
                "Bloodlust",
                "🩸",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Attack,
                1.3, // 30% attack increase
                isMultiplier: true
            );
        }

        public static void ApplyBloodlust(this Combatant target, int duration)
        {
            var effect = CreateBloodlust(duration);
            target.Effects.AddEffect(effect);
        }

        public static StatModifierEffect CreateIronSkin(int duration)
        {
            return new StatModifierEffect(
                StatusEffectId.IronSkin.GetIdentifier(),
                "Iron Skin",
                "🛡️",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Defense,
                2.0, // Double defense
                isMultiplier: true
            );
        }

        public static void ApplyIronSkin(this Combatant target, int duration)
        {
            var effect = CreateIronSkin(duration);
            target.Effects.AddEffect(effect);
        }

        public static StatModifierEffect CreateFortified(int duration)
        {
            return new StatModifierEffect(
                StatusEffectId.Fortified.GetIdentifier(),
                "Fortified",
                "🛡️",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Defense,
                1.4, // 40% damage reduction (via defense)
                isMultiplier: true
            );
        }

        public static void ApplyFortified(this Combatant target, int duration)
        {
            var effect = CreateFortified(duration);
            target.Effects.AddEffect(effect);
        }

        public static StatModifierEffect CreateRally(int duration)
        {
            return new StatModifierEffect(
                StatusEffectId.Rally.GetIdentifier(),
                "Rally",
                "⚔️",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Attack,
                1.3, // 30% attack increase
                isMultiplier: true
            );
        }

        public static void ApplyRally(this Combatant target, int duration)
        {
            var effect = CreateRally(duration);
            target.Effects.AddEffect(effect);
        }

        public static StatModifierEffect CreateBanner(int duration)
        {
            // Banner provides both attack and defense - apply attack boost
            return new StatModifierEffect(
                StatusEffectId.Banner.GetIdentifier(),
                "Banner",
                "🚩",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Attack,
                1.2, // 20% attack increase
                isMultiplier: true
            );
        }

        public static void ApplyBanner(this Combatant target, int duration)
        {
            var effect = CreateBanner(duration);
            target.Effects.AddEffect(effect);
            // Also apply defensive buff
            var defEffect = new StatModifierEffect(
                "banner_defense",
                "Banner (Defense)",
                "🚩",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Defense,
                1.2, // 20% defense increase
                isMultiplier: true
            );
            target.Effects.AddEffect(defEffect);
        }

        public static StatModifierEffect CreateCommandingPresence(int duration)
        {
            return new StatModifierEffect(
                StatusEffectId.CommandingPresence.GetIdentifier(),
                "Commanding Presence",
                "👑",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Damage,
                1.4, // 40% damage increase
                isMultiplier: true
            );
        }

        public static void ApplyCommandingPresence(this Combatant target, int duration)
        {
            var effect = CreateCommandingPresence(duration);
            target.Effects.AddEffect(effect);
        }

        public static StatModifierEffect CreateGladiatorsResolve(int duration)
        {
            return new StatModifierEffect(
                StatusEffectId.GladiatorsResolve.GetIdentifier(),
                "Gladiator's Resolve",
                "⚔️🛡️",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Damage,
                1.5, // 50% damage increase
                isMultiplier: true
            );
        }

        public static void ApplyGladiatorsResolve(this Combatant target, int duration)
        {
            var effect = CreateGladiatorsResolve(duration);
            target.Effects.AddEffect(effect);
            // Also apply defense and speed
            var defEffect = new StatModifierEffect(
                "gladiators_resolve_def",
                "Gladiator's Resolve (Defense)",
                "⚔️🛡️",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Defense,
                1.3,
                isMultiplier: true
            );
            target.Effects.AddEffect(defEffect);
        }

        // ===== NEW MAGE BUFFS =====

        public static StatModifierEffect CreateCombustion(int duration)
        {
            return new StatModifierEffect(
                StatusEffectId.Combustion.GetIdentifier(),
                "Combustion",
                "🔥",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Damage,
                1.5, // 50% fire damage increase
                isMultiplier: true
            );
        }

        public static void ApplyCombustion(this Combatant target, int duration)
        {
            var effect = CreateCombustion(duration);
            target.Effects.AddEffect(effect);
        }

        public static StatModifierEffect CreateTimeWarp(int duration)
        {
            return new StatModifierEffect(
                StatusEffectId.TimeWarp.GetIdentifier(),
                "Time Warp",
                "⏱️",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Speed,
                20, // +20 speed
                isMultiplier: false
            );
        }

        public static void ApplyTimeWarp(this Combatant target, int duration)
        {
            var effect = CreateTimeWarp(duration);
            target.Effects.AddEffect(effect);
        }

        public static StatModifierEffect CreateSpellPower(int duration)
        {
            return new StatModifierEffect(
                StatusEffectId.SpellPower.GetIdentifier(),
                "Spell Power",
                "✨",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Damage,
                1.6, // 60% spell damage increase
                isMultiplier: true
            );
        }

        public static void ApplySpellPower(this Combatant target, int duration)
        {
            var effect = CreateSpellPower(duration);
            target.Effects.AddEffect(effect);
        }

        public static StatModifierEffect CreateArcanePower(int duration)
        {
            return new StatModifierEffect(
                StatusEffectId.ArcanePower.GetIdentifier(),
                "Arcane Power",
                "🌟",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Damage,
                2.0, // Double spell damage!
                isMultiplier: true
            );
        }

        public static void ApplyArcanePower(this Combatant target, int duration)
        {
            var effect = CreateArcanePower(duration);
            target.Effects.AddEffect(effect);
        }

        // ===== NEW ROGUE BUFFS =====

        public static StatModifierEffect CreateShadowCloak(int duration)
        {
            return new StatModifierEffect(
                StatusEffectId.ShadowCloak.GetIdentifier(),
                "Shadow Cloak",
                "🌑",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Defense,
                0.6, // 40% damage reduction
                isMultiplier: true
            );
        }

        public static void ApplyShadowCloak(this Combatant target, int duration)
        {
            var effect = CreateShadowCloak(duration);
            target.Effects.AddEffect(effect);
        }

        public static StatModifierEffect CreateMirrorImage(int duration)
        {
            return new StatModifierEffect(
                StatusEffectId.MirrorImage.GetIdentifier(),
                "Mirror Image",
                "👥",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Defense,
                0.5, // 50% damage reduction
                isMultiplier: true
            );
        }

        public static void ApplyMirrorImage(this Combatant target, int duration)
        {
            var effect = CreateMirrorImage(duration);
            target.Effects.AddEffect(effect);
        }

        public static StatModifierEffect CreateShadowDance(int duration)
        {
            return new StatModifierEffect(
                StatusEffectId.ShadowDance.GetIdentifier(),
                "Shadow Dance",
                "💃",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Speed,
                15, // +15 speed
                isMultiplier: false
            );
        }

        public static void ApplyShadowDance(this Combatant target, int duration)
        {
            var effect = CreateShadowDance(duration);
            target.Effects.AddEffect(effect);
            // Also apply evasion
            var evasion = new StatModifierEffect(
                "shadow_dance_defense",
                "Shadow Dance (Evasion)",
                "💃",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Defense,
                0.7,
                isMultiplier: true
            );
            target.Effects.AddEffect(evasion);
        }

        public static StatModifierEffect CreateShadowRealm(int duration)
        {
            return new StatModifierEffect(
                StatusEffectId.ShadowRealm.GetIdentifier(),
                "Shadow Realm",
                "🌌",
                StatusEffectType.Buff,
                duration,
                StatModifierEffect.StatType.Defense,
                0.3, // 70% damage reduction!
                isMultiplier: true
            );
        }

        public static void ApplyShadowRealm(this Combatant target, int duration)
        {
            var effect = CreateShadowRealm(duration);
            target.Effects.AddEffect(effect);
        }

        // ===== DEBUFFS (Applied to enemies) =====

        public static StatModifierEffect CreateVulnerability(int duration)
        {
            return new StatModifierEffect(
                StatusEffectId.Vulnerability.GetIdentifier(),
                "Vulnerable",
                "💥",
                StatusEffectType.Debuff,
                duration,
                StatModifierEffect.StatType.Defense,
                1.3, // Takes 30% more damage (simulated as negative defense)
                isMultiplier: true
            );
        }

        public static void ApplyVulnerability(this Combatant target, int duration)
        {
            var effect = CreateVulnerability(duration);
            target.Effects.AddEffect(effect);
        }

        public static StatModifierEffect CreateExposed(int duration)
        {
            return new StatModifierEffect(
                StatusEffectId.Exposed.GetIdentifier(),
                "Exposed",
                "🎯",
                StatusEffectType.Debuff,
                duration,
                StatModifierEffect.StatType.Defense,
                0.7, // 30% less defense
                isMultiplier: true
            );
        }

        public static void ApplyExposed(this Combatant target, int duration)
        {
            var effect = CreateExposed(duration);
            target.Effects.AddEffect(effect);
        }

        public static StatModifierEffect CreateCrippled(int duration)
        {
            return new StatModifierEffect(
                StatusEffectId.Crippled.GetIdentifier(),
                "Crippled",
                "🦶",
                StatusEffectType.Debuff,
                duration,
                StatModifierEffect.StatType.Speed,
                -10, // -10 speed
                isMultiplier: false
            );
        }

        public static void ApplyCrippled(this Combatant target, int duration)
        {
            var effect = CreateCrippled(duration);
            target.Effects.AddEffect(effect);
        }
    }
}
