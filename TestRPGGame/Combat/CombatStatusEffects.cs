using System;
using TestRPGGame.UI;
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
                UIHelper.PrintColoredLine($"💚 {enemy.Name} regenerates {HealOverTimeAmount} HP!", ConsoleColor.Green);
                HealOverTimeTurns--;
            }

            // Damage over time on player
            if (DamageOverTimeTurns > 0)
            {
                player.CurrentHP -= DamageOverTimeAmount;
                UIHelper.PrintColoredLine($"🔥 You take {DamageOverTimeAmount} damage from burning!", ConsoleColor.Red);
                DamageOverTimeTurns--;
            }

            // Bleed damage on player
            if (BleedTurns > 0)
            {
                player.CurrentHP -= BleedAmount;
                UIHelper.PrintColoredLine($"🩸 You take {BleedAmount} bleed damage!", ConsoleColor.DarkRed);
                BleedTurns--;
                if (BleedTurns == 0)
                {
                    UIHelper.PrintColoredLine($"🩹 The bleeding stops.", ConsoleColor.Gray);
                }
            }

            // Reduce thorns duration
            if (ThornsTurns > 0)
            {
                ThornsTurns--;
                if (ThornsTurns == 0)
                {
                    ThornsValue = 0;
                    UIHelper.PrintColoredLine($"🌵 {enemy.Name}'s thorns fade away!", ConsoleColor.Gray);
                }
            }

            // Reduce shield duration
            if (ShieldTurns > 0)
            {
                ShieldTurns--;
                if (ShieldTurns == 0)
                {
                    ShieldValue = 0;
                    UIHelper.PrintColoredLine($"🛡️  {enemy.Name}'s shield shatters!", ConsoleColor.Gray);
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
                    UIHelper.PrintColoredLine($"😤 {enemy.Name}'s rage subsides!", ConsoleColor.Gray);
                }
            }

            // Reduce stun
            if (StunTurnsRemaining > 0)
            {
                StunTurnsRemaining--;
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
                UIHelper.PrintColoredLine($"💚 You regenerate {HealOverTimeAmount} HP!", ConsoleColor.Green);
                HealOverTimeTurns--;
            }

            // Damage over time on enemy
            if (DamageOverTimeTurns > 0)
            {
                enemy.CurrentHP -= DamageOverTimeAmount;
                UIHelper.PrintColoredLine($"🔥 {enemy.Name} takes {DamageOverTimeAmount} damage from burning!", ConsoleColor.Red);
                DamageOverTimeTurns--;
            }

            // Bleed damage on enemy
            if (BleedTurns > 0)
            {
                enemy.CurrentHP -= BleedAmount;
                UIHelper.PrintColoredLine($"🩸 {enemy.Name} takes {BleedAmount} bleed damage!", ConsoleColor.DarkRed);
                BleedTurns--;
                if (BleedTurns == 0)
                {
                    UIHelper.PrintColoredLine($"🩹 The bleeding stops.", ConsoleColor.Gray);
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
        }
    }
}
