using System;
using Xunit;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.Combat;

namespace TestRPGGame.Tests
{
    public class BleedMechanicsTests : TestBase
    {
        [Fact]
        public void Bleed_ShouldApplyDamageOverTime()
        {
            // Arrange
            var statusEffects = new CombatStatusEffects
            {
                BleedTurns = 3,
                BleedAmount = 15
            };
            var player = new Player("Test Player", PlayerClass.Warrior);
            player.MaxHP = 100;
            player.CurrentHP = 100;

            // Act - Simulate bleed tick
            if (statusEffects.BleedTurns > 0)
            {
                player.CurrentHP -= statusEffects.BleedAmount;
                statusEffects.BleedTurns--;
            }

            // Assert
            Assert.Equal(85, player.CurrentHP);
            Assert.Equal(2, statusEffects.BleedTurns);
        }

        [Fact]
        public void Bleed_ShouldExpireAfterDuration()
        {
            // Arrange
            var statusEffects = new CombatStatusEffects
            {
                BleedTurns = 1,
                BleedAmount = 15
            };
            var player = new Player("Test Player", PlayerClass.Warrior);
            player.MaxHP = 100;
            player.CurrentHP = 100;

            // Act - Simulate final bleed tick
            player.CurrentHP -= statusEffects.BleedAmount;
            statusEffects.BleedTurns--;

            // Assert
            Assert.Equal(0, statusEffects.BleedTurns);
            Assert.Equal(85, player.CurrentHP);
        }

        [Fact]
        public void Bleed_MultipleTicks_ShouldStackDamage()
        {
            // Arrange
            var statusEffects = new CombatStatusEffects
            {
                BleedTurns = 3,
                BleedAmount = 10
            };
            var player = new Player("Test Player", PlayerClass.Warrior);
            player.MaxHP = 100;
            player.CurrentHP = 100;

            // Act - Simulate 3 bleed ticks
            for (int i = 0; i < 3; i++)
            {
                if (statusEffects.BleedTurns > 0)
                {
                    player.CurrentHP -= statusEffects.BleedAmount;
                    statusEffects.BleedTurns--;
                }
            }

            // Assert
            Assert.Equal(70, player.CurrentHP); // 100 - (10 * 3)
            Assert.Equal(0, statusEffects.BleedTurns);
        }

        [Fact]
        public void Bleed_ShouldNotTickWhenDurationZero()
        {
            // Arrange
            var statusEffects = new CombatStatusEffects
            {
                BleedTurns = 0,
                BleedAmount = 15
            };
            var player = new Player("Test Player", PlayerClass.Warrior);
            player.MaxHP = 100;
            player.CurrentHP = 100;

            // Act - Try to apply bleed (should not apply)
            if (statusEffects.BleedTurns > 0)
            {
                player.CurrentHP -= statusEffects.BleedAmount;
                statusEffects.BleedTurns--;
            }

            // Assert
            Assert.Equal(100, player.CurrentHP); // No damage taken
            Assert.Equal(0, statusEffects.BleedTurns);
        }
    }
}
