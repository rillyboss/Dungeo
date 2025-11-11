using System;

using PlayerEntity = TestRPGGame.Entities.Player.Player;
using EnemyEntity = TestRPGGame.Entities.Enemy.Enemy;

namespace TestRPGGame.Combat
{
    /// <summary>
    /// Status effects that can apply to any combatant (player or enemy).
    /// Tracks buffs, debuffs, shields, DoTs, and other temporary combat effects.
    /// </summary>
    public class CombatStatusEffects
    {
        // Heal over time
        public int HealOverTimeTurns { get; set; }
        public int HealOverTimeAmount { get; set; }

        // Damage over time (burning, etc.)
        public int DamageOverTimeTurns { get; set; }
        public int DamageOverTimeAmount { get; set; }

        // Bleed
        public int BleedTurns { get; set; }
        public int BleedAmount { get; set; }

        // Thorns (reflect damage)
        public int ThornsValue { get; set; }
        public int ThornsTurns { get; set; }

        // Shield (absorb damage)
        public int ShieldValue { get; set; }
        public int ShieldTurns { get; set; }

        // Enrage (increased damage)
        public bool IsEnraged { get; set; }
        public int EnrageTurns { get; set; }
        public double EnrageDamageMultiplier { get; set; }

        // Stun
        public int StunTurnsRemaining { get; set; }

        // Speed buff/debuff
        public int SpeedBuffValue { get; set; }
        public int SpeedBuffTurns { get; set; }

        public CombatStatusEffects()
        {
            HealOverTimeTurns = 0;
            HealOverTimeAmount = 0;
            DamageOverTimeTurns = 0;
            DamageOverTimeAmount = 0;
            BleedTurns = 0;
            BleedAmount = 0;
            ThornsValue = 0;
            ThornsTurns = 0;
            ShieldValue = 0;
            ShieldTurns = 0;
            IsEnraged = false;
            EnrageTurns = 0;
            EnrageDamageMultiplier = 1.0;
            StunTurnsRemaining = 0;
            SpeedBuffValue = 0;
            SpeedBuffTurns = 0;
        }

        /// <summary>
        /// Apply all active status effects for an enemy at the start of their turn
        /// </summary>
        public void ApplyEnemyTurnEffects(EnemyEntity enemy, PlayerEntity player)
        {
            // Heal over time
            if (HealOverTimeTurns > 0)
            {
                enemy.CurrentHP = Math.Min(enemy.MaxHP, enemy.CurrentHP + HealOverTimeAmount);
                Console.WriteLine($"💚 {enemy.Name} regenerates {HealOverTimeAmount} HP!");
                HealOverTimeTurns--;
            }

            // Damage over time on player
            if (DamageOverTimeTurns > 0)
            {
                player.CurrentHP -= DamageOverTimeAmount;
                Console.WriteLine($"🔥 You take {DamageOverTimeAmount} damage from burning!");
                DamageOverTimeTurns--;
            }

            // Bleed damage on player
            if (BleedTurns > 0)
            {
                player.CurrentHP -= BleedAmount;
                Console.WriteLine($"🩸 You take {BleedAmount} bleed damage!");
                BleedTurns--;
                if (BleedTurns == 0)
                {
                    Console.WriteLine($"🩹 The bleeding stops.");
                }
            }

            // Reduce thorns duration
            if (ThornsTurns > 0)
            {
                ThornsTurns--;
                if (ThornsTurns == 0)
                {
                    ThornsValue = 0;
                    Console.WriteLine($"🌵 {enemy.Name}'s thorns fade away!");
                }
            }

            // Reduce shield duration
            if (ShieldTurns > 0)
            {
                ShieldTurns--;
                if (ShieldTurns == 0)
                {
                    ShieldValue = 0;
                    Console.WriteLine($"🛡️  {enemy.Name}'s shield shatters!");
                }
            }

            // Reduce enrage duration
            if (EnrageTurns > 0)
            {
                EnrageTurns--;
                if (EnrageTurns == 0)
                {
                    IsEnraged = false;
                    EnrageDamageMultiplier = 1.0;
                    Console.WriteLine($"😤 {enemy.Name}'s rage subsides!");
                }
            }

            // Reduce stun
            if (StunTurnsRemaining > 0)
            {
                StunTurnsRemaining--;
            }

            // Reduce speed buff duration
            if (SpeedBuffTurns > 0)
            {
                SpeedBuffTurns--;
                if (SpeedBuffTurns == 0)
                {
                    SpeedBuffValue = 0;
                    Console.WriteLine($"⚡ {enemy.Name}'s speed buff fades!");
                }
            }
        }

        /// <summary>
        /// Apply all active status effects for a player at the start of their turn
        /// </summary>
        public void ApplyPlayerTurnEffects(PlayerEntity player, EnemyEntity enemy)
        {
            // Heal over time on player
            if (HealOverTimeTurns > 0)
            {
                player.Heal(HealOverTimeAmount);
                Console.WriteLine($"💚 You regenerate {HealOverTimeAmount} HP!");
                HealOverTimeTurns--;
            }

            // Damage over time on enemy
            if (DamageOverTimeTurns > 0)
            {
                enemy.CurrentHP -= DamageOverTimeAmount;
                Console.WriteLine($"🔥 {enemy.Name} takes {DamageOverTimeAmount} damage from burning!");
                DamageOverTimeTurns--;
            }

            // Bleed damage on enemy
            if (BleedTurns > 0)
            {
                enemy.CurrentHP -= BleedAmount;
                Console.WriteLine($"🩸 {enemy.Name} takes {BleedAmount} bleed damage!");
                BleedTurns--;
                if (BleedTurns == 0)
                {
                    Console.WriteLine($"🩹 The bleeding stops.");
                }
            }

            // Player-side effects durations
            if (ThornsTurns > 0)
            {
                ThornsTurns--;
                if (ThornsTurns == 0)
                {
                    ThornsValue = 0;
                }
            }

            if (ShieldTurns > 0)
            {
                ShieldTurns--;
                if (ShieldTurns == 0)
                {
                    ShieldValue = 0;
                }
            }

            // Reduce speed buff duration
            if (SpeedBuffTurns > 0)
            {
                SpeedBuffTurns--;
                if (SpeedBuffTurns == 0)
                {
                    SpeedBuffValue = 0;
                    Console.WriteLine($"⚡ Your speed buff fades!");
                }
            }
        }
    }
}
