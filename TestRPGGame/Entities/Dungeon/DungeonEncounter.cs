using System.Collections.Generic;
using EnemyEntity = TestRPGGame.Entities.Enemy.Enemy;
using TestRPGGame.DataLoading;

namespace TestRPGGame.Entities.Dungeon
{
    public class DungeonEncounter
    {
        public string Description { get; set; }
        public List<string> Choices { get; set; }
        public List<string> ChoiceResults { get; set; }
        public bool IsCombat { get; set; }
        public EnemyEntity? Enemy { get; set; }
        public int? HealthReward { get; set; }
        public int? ManaReward { get; set; }
        public int? GoldReward { get; set; }

        /// <summary>
        /// For random combat encounters - the level to use for enemy generation
        /// </summary>
        public int? CombatLevel { get; set; }

        /// <summary>
        /// All possible choices for this encounter (for random selection)
        /// </summary>
        public List<DungeonChoiceData>? AllChoices { get; set; }

        /// <summary>
        /// Number of choices to randomly select from AllChoices (0 = show all)
        /// </summary>
        public int RandomChoiceCount { get; set; }

        public DungeonEncounter(string description)
        {
            Description = description;
            Choices = new List<string>();
            ChoiceResults = new List<string>();
            IsCombat = false;
        }
    }
}
