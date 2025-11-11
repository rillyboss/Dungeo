using System;
using TestRPGGame.Entities;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Enemy;

namespace TestRPGGame.Abilities.Effects
{
    /// <summary>
    /// Context for executing ability effects.
    /// Uses Source (caster/attacker) and Target (defender) for symmetrical ability execution.
    /// </summary>
    public class AbilityContext
    {
        /// <summary>
        /// The combatant using the ability (caster/attacker)
        /// </summary>
        public Combatant Source { get; set; }

        /// <summary>
        /// The combatant being targeted by the ability (defender)
        /// </summary>
        public Combatant? Target { get; set; }

        public bool PlayerDodgeNext { get; set; }
        public Random Random { get; set; }

        /// <summary>
        /// Tracks if this is a player-used ability (true) or enemy-used ability (false)
        /// </summary>
        public bool IsPlayerAbility { get; set; }

        // Convenience properties for backward compatibility during migration
        public Player? Player => Source as Player ?? Target as Player;
        public Enemy? Enemy => Source as Enemy ?? Target as Enemy;

        public AbilityContext(Combatant source, Combatant? target = null)
        {
            Source = source;
            Target = target;
            Random = new Random();
        }
    }
}
