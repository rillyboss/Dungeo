using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using TestRPGGame.DataLoading;
using TestRPGGame.Equipment;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Dungeon;
using TestRPGGame.Factories;

namespace TestRPGGame.Tests
{
    /// <summary>
    /// Tests for IDataRepository dependency injection across the codebase.
    /// These tests verify that the DI mechanism works correctly, not the full behavior.
    /// </summary>
    public class IDataRepositoryTests : TestBase
    {
        [Fact]
        public void SetRepository_CanBeCalledOnEquipmentGenerator()
        {
            // Arrange
            var mockRepository = new Mock<IDataRepository>();

            // Act - Should not throw
            EquipmentGenerator.SetRepository(mockRepository.Object);

            // Cleanup
            EquipmentGenerator.SetRepository(new JsonDataRepository());
        }

        [Fact]
        public void SetRepository_CanBeCalledOnEnemyFactory()
        {
            // Arrange
            var mockRepository = new Mock<IDataRepository>();

            // Act - Should not throw
            EnemyFactory.SetRepository(mockRepository.Object);

            // Cleanup
            EnemyFactory.SetRepository(new JsonDataRepository());
        }

        [Fact]
        public void SetRepository_CanBeCalledOnEntityFactory()
        {
            // Arrange
            var mockRepository = new Mock<IDataRepository>();

            // Act - Should not throw
            EntityFactory.SetRepository(mockRepository.Object);

            // Cleanup
            EntityFactory.SetRepository(new JsonDataRepository());
        }

        [Fact]
        public void SetRepository_CanBeCalledOnDungeonFactory()
        {
            // Arrange
            var mockRepository = new Mock<IDataRepository>();

            // Act - Should not throw
            DungeonFactory.SetRepository(mockRepository.Object);

            // Cleanup
            DungeonFactory.SetRepository(new JsonDataRepository());
        }

        [Fact]
        public void Player_AcceptsRepositoryInConstructor()
        {
            // Arrange
            var mockRepository = new Mock<IDataRepository>();

            // Setup minimum required data for Player construction
            mockRepository.Setup(r => r.GetClass(It.IsAny<string>())).Returns(new ClassData
            {
                Name = "Warrior",
                BaseMaxHP = 100,
                BaseMaxMana = 50,
                BaseAttack = 15,
                BaseDefense = 10,
                BaseMagicPower = 5,
                BaseSpeed = 8,
                BaseCritChance = 0.05,
                HPPerLevel = 10,
                ManaPerLevel = 5,
                AttackPerLevel = 2,
                DefensePerLevel = 1,
                MagicPowerPerLevel = 1,
                SpeedPerLevel = 1,
                StartingGold = 100,
                StartingPotions = 3
            });

            mockRepository.Setup(r => r.GetAbilitiesForClass(It.IsAny<string>()))
                .Returns(new List<AbilityData>());

            // Act - Should not throw
            var player = new Player("Test Hero", PlayerClass.Warrior, mockRepository.Object);

            // Assert
            Assert.Equal("Test Hero", player.Name);
            Assert.Equal(PlayerClass.Warrior, player.Class);
            mockRepository.Verify(r => r.GetClass("Warrior"), Times.Once);
            mockRepository.Verify(r => r.GetAbilitiesForClass("Warrior"), Times.Once);
        }

        [Fact]
        public void Player_UsesDefaultRepository_WhenNoneProvided()
        {
            // Act - Create player without providing repository (should use default JsonDataRepository)
            var player = new Player("Default Hero", PlayerClass.Warrior);

            // Assert - Should successfully create with real data
            Assert.Equal("Default Hero", player.Name);
            Assert.Equal(PlayerClass.Warrior, player.Class);
            Assert.True(player.MaxHP > 0);
            Assert.True(player.Attack > 0);
        }

        [Fact]
        public void Player_CallsRepositoryOnLevelUp()
        {
            // Arrange
            var mockRepository = new Mock<IDataRepository>();

            var classData = new ClassData
            {
                Name = "Warrior",
                BaseMaxHP = 100,
                BaseMaxMana = 50,
                BaseAttack = 15,
                BaseDefense = 10,
                BaseMagicPower = 5,
                BaseSpeed = 8,
                BaseCritChance = 0.05,
                HPPerLevel = 10,
                ManaPerLevel = 5,
                AttackPerLevel = 2,
                DefensePerLevel = 1,
                MagicPowerPerLevel = 1,
                SpeedPerLevel = 1,
                StartingGold = 100,
                StartingPotions = 3
            };

            mockRepository.Setup(r => r.GetClass(It.IsAny<string>())).Returns(classData);
            mockRepository.Setup(r => r.GetAbilitiesForClass(It.IsAny<string>()))
                .Returns(new List<AbilityData>());

            var player = new Player("Test Hero", PlayerClass.Warrior, mockRepository.Object);
            int initialMaxHP = player.MaxHP;

            // Act - Level up the player
            player.GainExperience(100);

            // Assert - GetClass should be called twice: once in constructor, once in LevelUp
            mockRepository.Verify(r => r.GetClass("Warrior"), Times.Exactly(2));
            Assert.Equal(2, player.Level);
            Assert.True(player.MaxHP > initialMaxHP, "MaxHP should increase after level up");
        }

        [Fact]
        public void JsonDataRepository_ImplementsIDataRepository()
        {
            // Arrange & Act
            IDataRepository repository = new JsonDataRepository();

            // Assert - Should be able to assign to interface type
            Assert.NotNull(repository);
            Assert.IsAssignableFrom<IDataRepository>(repository);
        }

        [Fact]
        public void JsonDataRepository_CanLoadData()
        {
            // Arrange
            var repository = new JsonDataRepository();

            // Act - Should not throw (data already loaded in TestBase)
            repository.LoadAllData();

            // Assert - Should be able to retrieve data
            var warrior = repository.GetClass("Warrior");
            Assert.NotNull(warrior);
            Assert.Equal("Warrior", warrior.Name);
        }

        [Fact]
        public void JsonDataRepository_CanGetAbility()
        {
            // Arrange
            var repository = new JsonDataRepository();

            // Act
            var ability = repository.GetAbility("warrior_power_strike");

            // Assert
            Assert.NotNull(ability);
            Assert.Equal("Power Strike", ability.Name);
        }

        [Fact]
        public void JsonDataRepository_CanGetEnemy()
        {
            // Arrange
            var repository = new JsonDataRepository();

            // Act
            var enemy = repository.GetEnemy("goblin");

            // Assert
            Assert.NotNull(enemy);
            Assert.Equal("Goblin", enemy.Name);
        }

        [Fact]
        public void JsonDataRepository_CanGetDungeon()
        {
            // Arrange
            var repository = new JsonDataRepository();

            // Act
            var dungeon = repository.GetDungeon("goblin_caves");

            // Assert
            Assert.NotNull(dungeon);
            Assert.Equal("goblin_caves", dungeon.Id);
        }

        [Fact]
        public void JsonDataRepository_CanGetAllDungeons()
        {
            // Arrange
            var repository = new JsonDataRepository();

            // Act
            var dungeons = repository.GetAllDungeons();

            // Assert
            Assert.NotNull(dungeons);
            Assert.NotEmpty(dungeons);
        }

        [Fact]
        public void JsonDataRepository_CanGetItemGenerationData()
        {
            // Arrange
            var repository = new JsonDataRepository();

            // Act
            var itemData = repository.GetItemGenerationData();

            // Assert
            Assert.NotNull(itemData);
            Assert.NotEmpty(itemData.WeaponTypes);
        }

        [Fact]
        public void AllFactories_ResetToDefaultRepository_AfterTest()
        {
            // This test verifies that our cleanup in Dispose() works
            // by checking that we can use factories with default repository

            // Act - Use factories with default repositories
            var player = new Player("Test", PlayerClass.Warrior);
            var dungeons = DungeonFactory.CreateAllDungeons();

            // Assert
            Assert.NotNull(player);
            Assert.NotEmpty(dungeons);
        }
    }
}
