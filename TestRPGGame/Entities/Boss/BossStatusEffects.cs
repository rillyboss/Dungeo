using System;
using TestRPGGame.UI;
using PlayerEntity = TestRPGGame.Entities.Player.Player;
using EnemyEntity = TestRPGGame.Entities.Enemy.Enemy;

namespace TestRPGGame.Entities.Boss
{
    // Boss status effects manager
    public class BossStatusEffects
    {
        public int HealOverTimeTurns { get; set; }
        public int HealOverTimeAmount { get; set; }
        public int DamageOverTimeTurns { get; set; }
        public int DamageOverTimeAmount { get; set; }
        public int BleedTurns { get; set; }
        public int BleedAmount { get; set; }
        public int ThornsValue { get; set; }
        public int ThornsTurns { get; set; }
        public int ShieldValue { get; set; }
        public int ShieldTurns { get; set; }
        public bool IsEnraged { get; set; }
        public int EnrageTurns { get; set; }
        public double EnrageDamageMultiplier { get; set; }
        public int StunTurnsRemaining { get; set; }

        public BossStatusEffects()
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

        public void ApplyTurnEffects(EnemyEntity boss, PlayerEntity player)
        {
            // Heal over time
            if (HealOverTimeTurns > 0)
            {
                boss.CurrentHP = Math.Min(boss.MaxHP, boss.CurrentHP + HealOverTimeAmount);
                UIHelper.PrintColoredLine($"💚 {boss.Name} regenerates {HealOverTimeAmount} HP!", ConsoleColor.Green);
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
                    UIHelper.PrintColoredLine($"🌵 {boss.Name}'s thorns fade away!", ConsoleColor.Gray);
                }
            }

            // Reduce shield duration
            if (ShieldTurns > 0)
            {
                ShieldTurns--;
                if (ShieldTurns == 0)
                {
                    ShieldValue = 0;
                    UIHelper.PrintColoredLine($"🛡️  {boss.Name}'s shield shatters!", ConsoleColor.Gray);
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
                    UIHelper.PrintColoredLine($"😤 {boss.Name}'s rage subsides!", ConsoleColor.Gray);
                }
            }

            // Reduce stun
            if (StunTurnsRemaining > 0)
            {
                StunTurnsRemaining--;
            }
        }
    }
}
