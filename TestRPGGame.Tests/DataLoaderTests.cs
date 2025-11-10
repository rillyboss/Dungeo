using System;
using System.Linq;
using Xunit;
using TestRPGGame.DataLoading;

namespace TestRPGGame.Tests
{
    public class DataLoaderTests : TestBase
    {
        public DataLoaderTests()
        {
            // Load all data before running tests
            try
            {
                DataLoader.LoadAllData();
            }
            catch
            {
                // Data loading will be tested, so exceptions are expected in some scenarios
            }
        }

        [Fact]
        public void DataLoader_LoadAllData_ShouldLoadWithoutErrors()
        {
            // Act & Assert - should not throw
            DataLoader.LoadAllData();
        }

        [Fact]
        public void DataLoader_GetAbility_ShouldReturnValidAbility()
        {
            // Arrange
            DataLoader.LoadAllData();

            // Act
            var ability = DataLoader.GetAbility("warrior_power_strike");

            // Assert
            Assert.NotNull(ability);
            Assert.Equal("warrior_power_strike", ability.Id);
            Assert.Equal("Power Strike", ability.Name);
            Assert.Equal("Warrior", ability.PlayerClass);
            Assert.Equal(20, ability.ManaCost);
            Assert.Equal(3, ability.Cooldown);
        }

        [Fact]
        public void DataLoader_GetAbilitiesForClass_ShouldReturnCorrectAbilities()
        {
            // Arrange
            DataLoader.LoadAllData();

            // Act
            var warriorAbilities = DataLoader.GetAbilitiesForClass("Warrior").ToList();

            // Assert
            Assert.NotEmpty(warriorAbilities);
            Assert.Contains(warriorAbilities, a => a.Name == "Power Strike");
            Assert.Contains(warriorAbilities, a => a.Name == "Shield Wall");
            Assert.Contains(warriorAbilities, a => a.Name == "Battle Rage");
            Assert.Contains(warriorAbilities, a => a.Name == "Whirlwind");
        }

        [Fact]
        public void DataLoader_GetEnemy_ShouldReturnValidEnemy()
        {
            // Arrange
            DataLoader.LoadAllData();

            // Act
            var enemy = DataLoader.GetEnemy("goblin");

            // Assert
            Assert.NotNull(enemy);
            Assert.Equal("goblin", enemy.Id);
            Assert.Equal("Goblin", enemy.Name);
            Assert.Equal("Humanoid", enemy.Type);
        }

        [Fact]
        public void DataLoader_GetEnemy_ShouldReturnValidBoss()
        {
            // Arrange
            DataLoader.LoadAllData();

            // Act - Bosses are now loaded as regular enemies
            var boss = DataLoader.GetEnemy("goblin_king");

            // Assert
            Assert.NotNull(boss);
            Assert.Equal("goblin_king", boss.Id);
            Assert.Equal("Goblin King", boss.Name);
            Assert.Equal(1, boss.Level);
            Assert.Equal("Beast", boss.Type);
            Assert.Equal(350, boss.MaxHP);
            Assert.Equal(2, boss.Abilities.Count); // Now uses Abilities instead of BossAbilities
        }

        [Fact]
        public void DataLoader_GetEnemy_BossShouldHaveCorrectAbilities()
        {
            // Arrange
            DataLoader.LoadAllData();

            // Act - Bosses are now loaded as regular enemies
            var boss = DataLoader.GetEnemy("void_lord");

            // Assert
            Assert.NotNull(boss);
            Assert.Equal(6, boss.Abilities.Count); // Now uses Abilities instead of BossAbilities
            Assert.Contains(boss.Abilities, a => a.AbilityId == "boss_void_annihilation");
            Assert.Contains(boss.Abilities, a => a.AbilityId == "boss_eternal_darkness");
            Assert.Contains(boss.Abilities, a => a.AbilityId == "boss_void_regeneration");
        }

        [Fact]
        public void DataLoader_GetDungeon_ShouldReturnValidDungeon()
        {
            // Arrange
            DataLoader.LoadAllData();

            // Act
            var dungeon = DataLoader.GetDungeon("goblin_caves");

            // Assert
            Assert.NotNull(dungeon);
            Assert.Equal("goblin_caves", dungeon.Id);
            Assert.Equal("Goblin Caves", dungeon.Name);
            Assert.Equal(1, dungeon.RecommendedLevel);
            Assert.Equal("goblin_champion", dungeon.MinibossId);
            Assert.Equal("goblin_king", dungeon.BossId);
        }

        [Fact]
        public void DataLoader_GetAllDungeons_ShouldReturnInOrder()
        {
            // Arrange
            DataLoader.LoadAllData();

            // Act
            var dungeons = DataLoader.GetAllDungeons().ToList();

            // Assert
            Assert.Equal(5, dungeons.Count);
            Assert.Equal("goblin_caves", dungeons[0].Id);
            Assert.Equal("haunted_crypt", dungeons[1].Id);
            Assert.Equal("dragons_lair", dungeons[2].Id);
            Assert.Equal("ancient_ruins", dungeons[3].Id);
            Assert.Equal("void_temple", dungeons[4].Id);
        }

        [Fact]
        public void DataLoader_GetDungeon_ShouldHaveCorrectEncounters()
        {
            // Arrange
            DataLoader.LoadAllData();

            // Act
            var dungeon = DataLoader.GetDungeon("dragons_lair");

            // Assert
            Assert.NotNull(dungeon);
            Assert.Equal(2, dungeon.Encounters.Count);
            Assert.All(dungeon.Encounters, e => Assert.NotNull(e.Choices));
            Assert.All(dungeon.Encounters, e => Assert.Equal(2, e.Choices!.Count));
        }

        [Fact]
        public void DataLoader_GetAbility_InvalidId_ShouldThrow()
        {
            // Arrange
            DataLoader.LoadAllData();

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => DataLoader.GetAbility("invalid_ability"));
        }

        [Fact]
        public void DataLoader_GetEnemy_InvalidId_ShouldThrow()
        {
            // Arrange
            DataLoader.LoadAllData();

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => DataLoader.GetEnemy("invalid_enemy"));
        }

        [Fact]
        public void DataLoader_GetEnemy_InvalidBossId_ShouldThrow()
        {
            // Arrange
            DataLoader.LoadAllData();

            // Act & Assert - Bosses are now in the enemies dictionary
            Assert.Throws<KeyNotFoundException>(() => DataLoader.GetEnemy("invalid_boss"));
        }

        [Fact]
        public void DataLoader_GetDungeon_InvalidId_ShouldThrow()
        {
            // Arrange
            DataLoader.LoadAllData();

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => DataLoader.GetDungeon("invalid_dungeon"));
        }

        [Fact]
        public void DataLoader_AbilityEffects_ShouldBeValid()
        {
            // Arrange
            DataLoader.LoadAllData();

            // Act
            var ability = DataLoader.GetAbility("mage_fireball");

            // Assert
            Assert.NotEmpty(ability.Effects);
            Assert.Equal("Damage", ability.Effects[0].Type);
            Assert.Equal(3.0, ability.Effects[0].Multiplier);
        }

        [Fact]
        public void DataLoader_BossAbilityEffects_ShouldBeValid()
        {
            // Arrange
            DataLoader.LoadAllData();

            // Act - Bosses are now loaded as regular enemies
            var boss = DataLoader.GetEnemy("death_knight");

            // Assert - Abilities now reference IDs from abilities.json
            Assert.Contains(boss.Abilities, a => a.AbilityId == "boss_soul_drain");

            // Verify the ability can be loaded from abilities.json
            var soulDrainAbility = DataLoader.GetAbility("boss_soul_drain");
            Assert.NotNull(soulDrainAbility);
            Assert.Equal("Soul Drain", soulDrainAbility.Name);
        }

        [Fact]
        public void DataLoader_AllEnemies_ShouldHaveValidData()
        {
            // Arrange
            DataLoader.LoadAllData();

            // Act
            var enemies = new[] { "goblin", "orc", "dark_knight", "dragon_whelp",
                "shadow_assassin", "ice_troll", "fire_elemental", "skeleton_warrior",
                "bandit", "wild_beast", "demon_spawn", "stone_golem", "wraith", "dire_wolf" };

            // Assert
            foreach (var enemyId in enemies)
            {
                var enemy = DataLoader.GetEnemy(enemyId);
                Assert.NotNull(enemy);
                Assert.NotEmpty(enemy.Name);
                Assert.NotEmpty(enemy.Type);
            }
        }

        [Fact]
        public void DataLoader_AllBosses_ShouldHaveValidData()
        {
            // Arrange
            DataLoader.LoadAllData();

            // Act - Bosses are now loaded as regular enemies
            var bosses = new[] { "goblin_champion", "goblin_king", "grave_lich", "death_knight",
                "dragon_wyrm", "infernus", "stone_colossus", "arcane_guardian",
                "void_harbinger", "void_lord" };

            // Assert
            foreach (var bossId in bosses)
            {
                var boss = DataLoader.GetEnemy(bossId); // Now uses GetEnemy
                Assert.NotNull(boss);
                Assert.NotEmpty(boss.Name);
                Assert.NotEmpty(boss.Type);
                Assert.NotEmpty(boss.Abilities); // Now uses Abilities instead of BossAbilities
            }
        }

        [Fact]
        public void DataLoader_DungeonProgression_ShouldBeCorrect()
        {
            // Arrange
            DataLoader.LoadAllData();

            // Act
            var goblinCaves = DataLoader.GetDungeon("goblin_caves");
            var hauntedCrypt = DataLoader.GetDungeon("haunted_crypt");
            var dragonsLair = DataLoader.GetDungeon("dragons_lair");
            var ancientRuins = DataLoader.GetDungeon("ancient_ruins");
            var voidTemple = DataLoader.GetDungeon("void_temple");

            // Assert - Progressive level requirements
            Assert.True(goblinCaves.RecommendedLevel < hauntedCrypt.RecommendedLevel);
            Assert.True(hauntedCrypt.RecommendedLevel < dragonsLair.RecommendedLevel);
            Assert.True(dragonsLair.RecommendedLevel < ancientRuins.RecommendedLevel);
            Assert.True(ancientRuins.RecommendedLevel < voidTemple.RecommendedLevel);

            // Assert - Prerequisite chain
            Assert.Empty(goblinCaves.RequiredDungeonIds);
            Assert.Contains("goblin_caves", hauntedCrypt.RequiredDungeonIds);
            Assert.Contains("haunted_crypt", dragonsLair.RequiredDungeonIds);
            Assert.Contains("dragons_lair", ancientRuins.RequiredDungeonIds);
            Assert.Contains("ancient_ruins", voidTemple.RequiredDungeonIds);
        }
    }
}
