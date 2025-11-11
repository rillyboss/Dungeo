using System.Collections.Generic;

namespace TestRPGGame.DataLoading
{
    /// <summary>
    /// JSON-based implementation of IDataRepository.
    /// Delegates to the static DataLoader for actual file I/O.
    /// </summary>
    public class JsonDataRepository : IDataRepository
    {
        public void LoadAllData()
        {
            DataLoader.LoadAllData();
        }

        public AbilityData GetAbility(string id)
        {
            return DataLoader.GetAbility(id);
        }

        public IEnumerable<AbilityData> GetAbilitiesForClass(string playerClass)
        {
            return DataLoader.GetAbilitiesForClass(playerClass);
        }

        public EnemyData GetEnemy(string id)
        {
            return DataLoader.GetEnemy(id);
        }

        public IEnumerable<EnemyData> GetEnemiesByLevel(int minLevel, int maxLevel)
        {
            return DataLoader.GetEnemiesByLevel(minLevel, maxLevel);
        }

        public DungeonData GetDungeon(string id)
        {
            return DataLoader.GetDungeon(id);
        }

        public IEnumerable<DungeonData> GetAllDungeons()
        {
            return DataLoader.GetAllDungeons();
        }

        public ItemGenerationData GetItemGenerationData()
        {
            return DataLoader.GetItemGenerationData();
        }

        public IEnumerable<EnemyPrefixData> GetEnemyPrefixesByLevel(int level)
        {
            return DataLoader.GetEnemyPrefixesByLevel(level);
        }

        public IEnumerable<EnemySuffixData> GetEnemySuffixesByLevel(int level)
        {
            return DataLoader.GetEnemySuffixesByLevel(level);
        }

        public EnemyPrefixData GetEnemyPrefix(string id)
        {
            return DataLoader.GetEnemyPrefix(id);
        }

        public EnemySuffixData GetEnemySuffix(string id)
        {
            return DataLoader.GetEnemySuffix(id);
        }

        public EnemyBehaviorData? GetBehavior(string id)
        {
            return DataLoader.GetBehavior(id);
        }

        public IEnumerable<EnemyBehaviorData> GetBehaviorsWithSpawnChance()
        {
            return DataLoader.GetBehaviorsWithSpawnChance();
        }

        public ClassData GetClass(string className)
        {
            return DataLoader.GetClass(className);
        }

        public IEnumerable<ClassData> GetAllClasses()
        {
            return DataLoader.GetAllClasses();
        }
    }
}
