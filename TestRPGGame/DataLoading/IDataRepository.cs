using System.Collections.Generic;

namespace TestRPGGame.DataLoading
{
    /// <summary>
    /// Repository interface for accessing game data.
    /// Enables dependency injection and mocking for unit tests.
    /// </summary>
    public interface IDataRepository
    {
        /// <summary>
        /// Load all game data from storage.
        /// </summary>
        void LoadAllData();

        // Ability queries
        AbilityData GetAbility(string id);
        IEnumerable<AbilityData> GetAbilitiesForClass(string playerClass);

        // Enemy queries
        EnemyData GetEnemy(string id);
        IEnumerable<EnemyData> GetEnemiesByLevel(int minLevel, int maxLevel);

        // Dungeon queries
        DungeonData GetDungeon(string id);
        IEnumerable<DungeonData> GetAllDungeons();

        // Item generation queries
        ItemGenerationData GetItemGenerationData();

        // Enemy modifier queries
        IEnumerable<EnemyPrefixData> GetEnemyPrefixesByLevel(int level);
        IEnumerable<EnemySuffixData> GetEnemySuffixesByLevel(int level);
        EnemyPrefixData GetEnemyPrefix(string id);
        EnemySuffixData GetEnemySuffix(string id);

        // Behavior queries
        EnemyBehaviorData? GetBehavior(string id);
        IEnumerable<EnemyBehaviorData> GetBehaviorsWithSpawnChance();

        // Class queries
        ClassData GetClass(string className);
        IEnumerable<ClassData> GetAllClasses();
    }
}
