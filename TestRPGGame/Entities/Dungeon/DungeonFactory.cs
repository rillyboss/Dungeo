using System.Collections.Generic;
using TestRPGGame.DataLoading;
using TestRPGGame.Factories;

namespace TestRPGGame.Entities.Dungeon
{
    public static class DungeonFactory
    {
        private static IDataRepository _repository = new JsonDataRepository(); // Default repository

        /// <summary>
        /// Sets the data repository. Use for dependency injection (e.g., mock repository for tests).
        /// </summary>
        public static void SetRepository(IDataRepository repository)
        {
            _repository = repository;
        }

        public static List<Dungeon> CreateAllDungeons()
        {
            // Load all dungeons from data - code has zero knowledge of specific dungeons
            var dungeonDataList = _repository.GetAllDungeons();
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
