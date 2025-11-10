using System;
using System.Collections.Generic;
using TestRPGGame.Combat;

namespace TestRPGGame.Entities.Enemy
{
    public class Enemy
    {
        public string BaseName { get; set; } // Original enemy name
        public string Name { get; set; } // Full name with modifiers
        public string Prefix { get; set; } = ""; // Modifier prefix
        public string Suffix { get; set; } = ""; // Modifier suffix
        public EnemyType Type { get; set; }
        public int MaxHP { get; set; }
        public int CurrentHP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Speed { get; set; }
        public int GoldReward { get; set; }
        public int ExpReward { get; set; }
        public List<EnemyAbility> Abilities { get; set; }
        public CombatStatusEffects? StatusEffects { get; set; }

        public Enemy(string name, EnemyType type, int hp, int attack, int defense, int speed, int gold, int exp)
        {
            BaseName = name;
            Name = name;
            Prefix = "";
            Suffix = "";
            Type = type;
            MaxHP = hp;
            CurrentHP = hp;
            Attack = attack;
            Defense = defense;
            Speed = speed;
            GoldReward = gold;
            ExpReward = exp;
            Abilities = new List<EnemyAbility>();
            StatusEffects = null; // Created on-demand when needed
        }

        public void TakeDamage(int damage)
        {
            int actualDamage = Math.Max(1, damage - Defense);
            CurrentHP -= actualDamage;
        }

        public bool IsAlive()
        {
            return CurrentHP > 0;
        }

        /// <summary>
        /// Ensures StatusEffects is initialized. Call this before applying status effects.
        /// </summary>
        public void EnsureStatusEffects()
        {
            if (StatusEffects == null)
            {
                StatusEffects = new CombatStatusEffects();
            }
        }
    }
}
