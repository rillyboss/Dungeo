using System.Collections.Generic;
using TestRPGGame.DataLoading;
using TestRPGGame.Factories;

namespace TestRPGGame.Entities.Dungeon
{
    public static class DungeonFactory
    {
        public static List<Dungeon> CreateAllDungeons()
        {
            // Load all dungeons from data - code has zero knowledge of specific dungeons
            var dungeonDataList = DataLoader.GetAllDungeons();
            var dungeons = new List<Dungeon>();

            foreach (var dungeonData in dungeonDataList)
            {
                var dungeon = EntityFactory.CreateDungeon(dungeonData);
                dungeons.Add(dungeon);
            }

            return dungeons;
        }

        // All dungeon creation methods removed - dungeons are now fully data-driven
        // See Data/dungeons.json to add or modify dungeons
    }
}
