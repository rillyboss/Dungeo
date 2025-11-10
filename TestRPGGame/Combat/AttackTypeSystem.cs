using System;
using System.Collections.Generic;

namespace TestRPGGame.Combat
{
    public static class AttackTypeSystem
    {
        // Damage multipliers for type effectiveness
        private static Dictionary<(AttackType attack, EnemyType enemy), double> weaknessChart = new Dictionary<(AttackType, EnemyType), double>
        {
            // Fire effectiveness
            {(AttackType.Fire, EnemyType.Beast), 1.5},
            {(AttackType.Fire, EnemyType.Undead), 1.3},
            {(AttackType.Fire, EnemyType.Construct), 0.7},
            {(AttackType.Fire, EnemyType.Dragon), 0.5},
            {(AttackType.Fire, EnemyType.Elemental), 0.5},

            // Ice effectiveness
            {(AttackType.Ice, EnemyType.Dragon), 1.5},
            {(AttackType.Ice, EnemyType.Demon), 1.3},
            {(AttackType.Ice, EnemyType.Elemental), 1.3},
            {(AttackType.Ice, EnemyType.Undead), 0.7},

            // Lightning effectiveness
            {(AttackType.Lightning, EnemyType.Construct), 1.5},
            {(AttackType.Lightning, EnemyType.Elemental), 1.4},
            {(AttackType.Lightning, EnemyType.Beast), 1.2},
            {(AttackType.Lightning, EnemyType.Dragon), 0.7},

            // Poison effectiveness
            {(AttackType.Poison, EnemyType.Beast), 1.4},
            {(AttackType.Poison, EnemyType.Humanoid), 1.3},
            {(AttackType.Poison, EnemyType.Undead), 0.3},
            {(AttackType.Poison, EnemyType.Construct), 0.1},

            // Dark effectiveness
            {(AttackType.Dark, EnemyType.Humanoid), 1.3},
            {(AttackType.Dark, EnemyType.Beast), 1.2},
            {(AttackType.Dark, EnemyType.Undead), 0.5},
            {(AttackType.Dark, EnemyType.Demon), 0.5},

            // Holy effectiveness
            {(AttackType.Holy, EnemyType.Undead), 2.0},
            {(AttackType.Holy, EnemyType.Demon), 1.8},
            {(AttackType.Holy, EnemyType.Beast), 0.8},

            // Arcane effectiveness
            {(AttackType.Arcane, EnemyType.Demon), 1.4},
            {(AttackType.Arcane, EnemyType.Elemental), 1.4},
            {(AttackType.Arcane, EnemyType.Construct), 1.2},

            // Physical effectiveness
            {(AttackType.Physical, EnemyType.Humanoid), 1.2},
            {(AttackType.Physical, EnemyType.Beast), 1.1},
            {(AttackType.Physical, EnemyType.Construct), 0.8},
            {(AttackType.Physical, EnemyType.Elemental), 0.7},
        };

        public static double GetDamageMultiplier(AttackType attackType, EnemyType enemyType)
        {
            if (weaknessChart.TryGetValue((attackType, enemyType), out double multiplier))
            {
                return multiplier;
            }
            return 1.0; // Normal effectiveness
        }

        public static string GetEffectivenessText(double multiplier)
        {
            if (multiplier >= 1.8) return "SUPER EFFECTIVE!";
            if (multiplier >= 1.3) return "It's effective!";
            if (multiplier <= 0.5) return "It's not very effective...";
            if (multiplier <= 0.8) return "Resisted...";
            return "";
        }

        public static ConsoleColor GetEffectivenessColor(double multiplier)
        {
            if (multiplier >= 1.3) return ConsoleColor.Green;
            if (multiplier <= 0.7) return ConsoleColor.Red;
            return ConsoleColor.White;
        }

        public static ConsoleColor GetAttackTypeColor(AttackType type)
        {
            return type switch
            {
                AttackType.Physical => ConsoleColor.Gray,
                AttackType.Fire => ConsoleColor.Red,
                AttackType.Ice => ConsoleColor.Cyan,
                AttackType.Lightning => ConsoleColor.Yellow,
                AttackType.Poison => ConsoleColor.Green,
                AttackType.Dark => ConsoleColor.DarkMagenta,
                AttackType.Holy => ConsoleColor.White,
                AttackType.Arcane => ConsoleColor.Magenta,
                _ => ConsoleColor.White
            };
        }

        public static string GetAttackTypeIcon(AttackType type)
        {
            return type switch
            {
                AttackType.Physical => "⚔️",
                AttackType.Fire => "🔥",
                AttackType.Ice => "❄️",
                AttackType.Lightning => "⚡",
                AttackType.Poison => "☠️",
                AttackType.Dark => "🌑",
                AttackType.Holy => "✨",
                AttackType.Arcane => "🔮",
                _ => "•"
            };
        }
    }
}
