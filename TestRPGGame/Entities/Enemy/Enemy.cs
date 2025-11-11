using System;
using System.Collections.Generic;
using TestRPGGame.Combat;

namespace TestRPGGame.Entities.Enemy
{
    public class Enemy : Combatant
    {
        public string BaseName { get; set; } // Original enemy name
        // Name inherited from Combatant
        public string Prefix { get; set; } = ""; // Modifier prefix
        public string Suffix { get; set; } = ""; // Modifier suffix
        public string Behavior { get; set; } = ""; // Behavior modifier (shown in name for regular enemies)
        public EnemyType Type { get; set; }
        // MaxHP, CurrentHP, Attack, Defense, Speed inherited from Combatant
        public int GoldReward { get; set; }
        public int ExpReward { get; set; }
        public List<EnemyAbility> Abilities { get; set; }
        // StatusEffects (old) and Effects (new) inherited from Combatant
        public EnemyAI? AI { get; set; } // AI decision engine

        public Enemy(string name, EnemyType type, int hp, int attack, int defense, int speed, int gold, int exp)
        {
            BaseName = name;
            Name = name;
            Prefix = "";
            Suffix = "";
            Behavior = "";
            Type = type;
            MaxHP = hp;
            CurrentHP = hp;
            Attack = attack;
            Defense = defense;
            Speed = speed;
            GoldReward = gold;
            ExpReward = exp;
            Abilities = new List<EnemyAbility>();
            // StatusEffects (old) and Effects (new) initialized by base Combatant constructor
            AI = null; // Created by EnemyFactory
        }

        // TakeDamage, IsAlive, and EnsureStatusEffects inherited from Combatant base class
    }
}
