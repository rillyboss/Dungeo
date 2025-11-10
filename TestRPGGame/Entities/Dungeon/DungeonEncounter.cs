using System.Collections.Generic;
using EnemyEntity = TestRPGGame.Entities.Enemy.Enemy;

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

        public DungeonEncounter(string description)
        {
            Description = description;
            Choices = new List<string>();
            ChoiceResults = new List<string>();
            IsCombat = false;
        }
    }
}
