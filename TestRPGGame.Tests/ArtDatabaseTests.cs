using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using TestRPGGame.DataLoading;

namespace TestRPGGame.Tests
{
    /// <summary>
    /// Tests for the data-driven ASCII art systems (ClassArtDatabase and EnemyArtDatabase)
    /// </summary>
    public class ArtDatabaseTests : TestBase
    {
        #region ClassArtDatabase Tests

        [Fact]
        public void ClassArtDatabase_LoadsAllThreeClasses()
        {
            // Arrange & Act - Data already loaded by TestBase
            var warriorArt = ClassArtDatabase.GetClassArt("Warrior");
            var mageArt = ClassArtDatabase.GetClassArt("Mage");
            var rogueArt = ClassArtDatabase.GetClassArt("Rogue");

            // Assert
            Assert.NotEmpty(warriorArt.art);
            Assert.NotEmpty(mageArt.art);
            Assert.NotEmpty(rogueArt.art);
        }

        [Fact]
        public void ClassArtDatabase_WarriorArt_HasCorrectColor()
        {
            // Arrange & Act
            var (art, color) = ClassArtDatabase.GetClassArt("Warrior");

            // Assert
            Assert.Equal(ConsoleColor.Red, color);
            Assert.NotEmpty(art);
        }

        [Fact]
        public void ClassArtDatabase_MageArt_HasCorrectColor()
        {
            // Arrange & Act
            var (art, color) = ClassArtDatabase.GetClassArt("Mage");

            // Assert
            Assert.Equal(ConsoleColor.Magenta, color);
            Assert.NotEmpty(art);
        }

        [Fact]
        public void ClassArtDatabase_RogueArt_HasCorrectColor()
        {
            // Arrange & Act
            var (art, color) = ClassArtDatabase.GetClassArt("Rogue");

            // Assert
            Assert.Equal(ConsoleColor.Green, color);
            Assert.NotEmpty(art);
        }

        [Fact]
        public void ClassArtDatabase_UnknownClass_ReturnsEmptyArt()
        {
            // Arrange & Act
            var (art, color) = ClassArtDatabase.GetClassArt("InvalidClass");

            // Assert
            Assert.Empty(art);
            Assert.Equal(ConsoleColor.White, color); // Default color
        }

        [Fact]
        public void ClassArtDatabase_WarriorArt_ContainsMultipleLines()
        {
            // Arrange & Act
            var (art, color) = ClassArtDatabase.GetClassArt("Warrior");

            // Assert - Warrior art should have multiple lines (at least 5)
            Assert.True(art.Count >= 5, $"Expected at least 5 lines, got {art.Count}");
        }

        [Theory]
        [InlineData("Warrior")]
        [InlineData("Mage")]
        [InlineData("Rogue")]
        public void ClassArtDatabase_AllClasses_HaveNonEmptyArt(string className)
        {
            // Arrange & Act
            var (art, color) = ClassArtDatabase.GetClassArt(className);

            // Assert
            Assert.NotEmpty(art);
            Assert.All(art, line => Assert.NotNull(line)); // All lines should be non-null
        }

        #endregion

        #region EnemyArtDatabase Tests

        [Fact]
        public void EnemyArtDatabase_GoblinPattern_MatchesGoblin()
        {
            // Arrange & Act
            var (art, color) = EnemyArtDatabase.GetEnemyArt("Goblin");

            // Assert
            Assert.NotEmpty(art);
            Assert.Equal(ConsoleColor.DarkGreen, color);
        }

        [Fact]
        public void EnemyArtDatabase_GoblinPattern_MatchesArmoredGoblin()
        {
            // Arrange & Act
            var (art, color) = EnemyArtDatabase.GetEnemyArt("Armored Goblin");

            // Assert - Should match Goblin pattern
            Assert.NotEmpty(art);
            Assert.Equal(ConsoleColor.DarkGreen, color);
        }

        [Fact]
        public void EnemyArtDatabase_SkeletonPattern_MatchesSkeletonWarrior()
        {
            // Arrange & Act
            var (art, color) = EnemyArtDatabase.GetEnemyArt("Skeleton Warrior");

            // Assert
            Assert.NotEmpty(art);
            // Color should match Skeleton art definition
        }

        [Theory]
        [InlineData("Dragon")]
        [InlineData("Ancient Dragon")]
        [InlineData("Fire Dragon")]
        public void EnemyArtDatabase_DragonPattern_MatchesAllDragonVariants(string enemyName)
        {
            // Arrange & Act
            var (art, color) = EnemyArtDatabase.GetEnemyArt(enemyName);

            // Assert - All should match Dragon pattern
            Assert.NotEmpty(art);
            Assert.Equal(ConsoleColor.Red, color);
        }

        [Fact]
        public void EnemyArtDatabase_UnknownEnemy_ReturnsDefaultArt()
        {
            // Arrange & Act
            var (art, color) = EnemyArtDatabase.GetEnemyArt("Totally Unknown Monster");

            // Assert - Should return Default art
            Assert.NotEmpty(art); // Default art should be defined
        }

        [Fact]
        public void EnemyArtDatabase_PatternMatching_IsCaseInsensitive()
        {
            // Arrange & Act
            var art1 = EnemyArtDatabase.GetEnemyArt("GOBLIN");
            var art2 = EnemyArtDatabase.GetEnemyArt("goblin");
            var art3 = EnemyArtDatabase.GetEnemyArt("GoBliN");

            // Assert - All should match the same Goblin pattern
            Assert.NotEmpty(art1.art);
            Assert.NotEmpty(art2.art);
            Assert.NotEmpty(art3.art);
            Assert.Equal(art1.color, art2.color);
            Assert.Equal(art2.color, art3.color);
        }

        [Theory]
        [InlineData("Slime", ConsoleColor.Green)]
        [InlineData("Ghost", ConsoleColor.Cyan)]
        [InlineData("Demon", ConsoleColor.DarkRed)]
        public void EnemyArtDatabase_CommonEnemies_HaveCorrectColors(string enemyName, ConsoleColor expectedColor)
        {
            // Arrange & Act
            var (art, color) = EnemyArtDatabase.GetEnemyArt(enemyName);

            // Assert
            Assert.NotEmpty(art);
            Assert.Equal(expectedColor, color);
        }

        [Fact]
        public void EnemyArtDatabase_BeastPattern_MatchesMultipleTypes()
        {
            // Arrange & Act
            var wolfArt = EnemyArtDatabase.GetEnemyArt("Wolf");
            var bearArt = EnemyArtDatabase.GetEnemyArt("Wild Bear");
            var tigerArt = EnemyArtDatabase.GetEnemyArt("Tiger");

            // Assert - All should use Beast pattern
            Assert.NotEmpty(wolfArt.art);
            Assert.NotEmpty(bearArt.art);
            Assert.NotEmpty(tigerArt.art);
            // All should have same color (DarkYellow for Beast)
            Assert.Equal(ConsoleColor.DarkYellow, wolfArt.color);
            Assert.Equal(ConsoleColor.DarkYellow, bearArt.color);
            Assert.Equal(ConsoleColor.DarkYellow, tigerArt.color);
        }

        #endregion

        #region Color Parsing Tests

        [Theory]
        [InlineData("Red", ConsoleColor.Red)]
        [InlineData("DarkRed", ConsoleColor.DarkRed)]
        [InlineData("Green", ConsoleColor.Green)]
        [InlineData("DarkGreen", ConsoleColor.DarkGreen)]
        [InlineData("Blue", ConsoleColor.Blue)]
        [InlineData("DarkBlue", ConsoleColor.DarkBlue)]
        [InlineData("Yellow", ConsoleColor.Yellow)]
        [InlineData("DarkYellow", ConsoleColor.DarkYellow)]
        [InlineData("Cyan", ConsoleColor.Cyan)]
        [InlineData("DarkCyan", ConsoleColor.DarkCyan)]
        [InlineData("Magenta", ConsoleColor.Magenta)]
        [InlineData("DarkMagenta", ConsoleColor.DarkMagenta)]
        [InlineData("Gray", ConsoleColor.Gray)]
        [InlineData("DarkGray", ConsoleColor.DarkGray)]
        [InlineData("White", ConsoleColor.White)]
        public void ColorParsing_ValidColors_ParsedCorrectly(string colorName, ConsoleColor expectedColor)
        {
            // This test verifies color parsing by checking class art
            // (we can't directly test the private ParseColor method)
            // Create a mock class data with the color
            var classData = new Dictionary<string, ClassData>
            {
                ["TestClass"] = new ClassData
                {
                    Art = new List<string> { "Test Art" },
                    ArtColor = colorName
                }
            };

            // Reload with test data
            ClassArtDatabase.LoadClassArt(classData);
            var (art, color) = ClassArtDatabase.GetClassArt("TestClass");

            // Assert
            Assert.Equal(expectedColor, color);

            // Reload original data for other tests
            var originalClasses = DataLoader.GetAllClasses().ToDictionary(c => c.Name, c => c);
            ClassArtDatabase.LoadClassArt(originalClasses);
        }

        [Fact]
        public void ColorParsing_InvalidColor_DefaultsToWhite()
        {
            // Arrange
            var classData = new Dictionary<string, ClassData>
            {
                ["TestClass"] = new ClassData
                {
                    Art = new List<string> { "Test Art" },
                    ArtColor = "InvalidColor"
                }
            };

            // Act
            ClassArtDatabase.LoadClassArt(classData);
            var (art, color) = ClassArtDatabase.GetClassArt("TestClass");

            // Assert
            Assert.Equal(ConsoleColor.White, color);

            // Cleanup - Reload original data
            var originalClasses = DataLoader.GetAllClasses().ToDictionary(c => c.Name, c => c);
            ClassArtDatabase.LoadClassArt(originalClasses);
        }

        #endregion

        #region Data Integrity Tests

        [Fact]
        public void ArtData_AllClassArt_HasValidStructure()
        {
            // Arrange
            var classes = new[] { "Warrior", "Mage", "Rogue" };

            // Act & Assert
            foreach (var className in classes)
            {
                var (art, color) = ClassArtDatabase.GetClassArt(className);

                // Verify art exists and has content
                Assert.NotEmpty(art);
                Assert.True(art.Count > 0, $"{className} should have at least one art line");

                // Verify no null lines
                Assert.All(art, line => Assert.NotNull(line));

                // Verify color is valid (not default white unless intended)
                Assert.NotEqual(ConsoleColor.Black, color); // Black would be invisible
            }
        }

        [Fact]
        public void ArtData_ClassesJson_MatchesExpectedClasses()
        {
            // Arrange
            var expectedClasses = new[] { "Warrior", "Mage", "Rogue" };

            // Act
            var loadedClasses = DataLoader.GetAllClasses().Select(c => c.Name).ToList();

            // Assert
            foreach (var expectedClass in expectedClasses)
            {
                Assert.Contains(expectedClass, loadedClasses);

                // Verify each has art data
                var (art, color) = ClassArtDatabase.GetClassArt(expectedClass);
                Assert.NotEmpty(art);
            }
        }

        [Fact]
        public void ArtData_EnemyArtTemplates_AllHaveValidData()
        {
            // Arrange - Known enemy types from enemy-art.json
            var knownEnemyTypes = new[]
            {
                "Goblin", "Skeleton", "Dragon", "Beast",
                "Demon", "Ghost", "Slime", "Default"
            };

            // Act & Assert
            foreach (var enemyType in knownEnemyTypes)
            {
                var (art, color) = EnemyArtDatabase.GetEnemyArt(enemyType);

                // Verify art exists
                Assert.NotEmpty(art);

                // Verify no null lines
                Assert.All(art, line => Assert.NotNull(line));
            }
        }

        #endregion
    }
}
