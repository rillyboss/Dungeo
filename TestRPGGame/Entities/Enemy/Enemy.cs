using System;
using System.Collections.Generic;
using TestRPGGame.Combat;
using TestRPGGame.Entities.Boss;

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
        public List<BossAbility>? BossAbilities { get; set; }
        public BossStatusEffects? StatusEffects { get; set; }
        public bool IsBoss { get; set; }

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
            BossAbilities = null;
            StatusEffects = null;
            IsBoss = false;
        }

        // Constructor for boss enemies
        public Enemy(string name, int level, EnemyType type)
        {
            BaseName = name;
            Name = name;
            Prefix = "";
            Suffix = "";
            Type = type;
            MaxHP = 100;
            CurrentHP = 100;
            Attack = 10;
            Defense = 5;
            Speed = 10;
            GoldReward = 0;
            ExpReward = 0;
            Abilities = new List<EnemyAbility>();
            BossAbilities = new List<BossAbility>();
            StatusEffects = new BossStatusEffects();
            IsBoss = true;
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
    }
}
