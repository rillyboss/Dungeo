using System;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Enemy;

namespace TestRPGGame.Abilities.Effects
{
    // Context for executing ability effects
    public class AbilityContext
    {
        public Player Player { get; set; }
        public Enemy? Enemy { get; set; }
        public System.Collections.Generic.Dictionary<string, int>? ActiveBuffs { get; set; }
        public bool PlayerDodgeNext { get; set; }
        public Random Random { get; set; }
        /// <summary>
        /// Tracks if this is a player-used ability (true) or enemy-used ability (false)
        /// </summary>
        public bool IsPlayerAbility { get; set; }

        public AbilityContext(Player player, Enemy? enemy = null)
        {
            Player = player;
            Enemy = enemy;
            Random = new Random();
        }
    }
}
