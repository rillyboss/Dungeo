using System.Collections.Generic;
using EnemyEntity = TestRPGGame.Entities.Enemy.Enemy;
using TestRPGGame.DataLoading;

namespace TestRPGGame.Entities.Dungeon
{
    public class Dungeon
    {
        public string Name { get; set; }
        public DungeonType Type { get; set; }
        public string Story { get; set; }
        public DungeonRequirements Requirements { get; set; }
        public DungeonReward MinibossReward { get; set; }
        public DungeonReward BossReward { get; set; }
        public List<DungeonEncounter> Encounters { get; set; }
        public EnemyEntity Miniboss { get; set; }
        public EnemyEntity Boss { get; set; }
        public int Difficulty { get; set; }

        // New: Random encounter system
        public DungeonEncounterConfig? EncounterConfig { get; set; }
        public List<DungeonEncounter> EncounterPool { get; set; }

        public Dungeon(string name, DungeonType type, string story, int difficulty)
        {
            Name = name;
            Type = type;
            Story = story;
            Difficulty = difficulty;
            Requirements = new DungeonRequirements();
            MinibossReward = new DungeonReward();
            BossReward = new DungeonReward();
            Encounters = new List<DungeonEncounter>();
            EncounterPool = new List<DungeonEncounter>();
        }
    }
}
