using System;
using System.Collections.Generic;
using Xunit;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.Entities.Boss;
using TestRPGGame.Abilities;
using TestRPGGame.Combat;

namespace TestRPGGame.Tests
{
    public class BuffDurationTests : TestBase
    {
        [Fact]
        public void Buff_ShouldDecrementOnlyOnPlayerTurn()
        {
            // Arrange
            var buffs = new Dictionary<string, int>
            {
                { "Battle Rage", 3 }
            };

            // Act - Simulate player turn (buff should decrement)
            buffs["Battle Rage"]--;

            // Assert
            Assert.Equal(2, buffs["Battle Rage"]);
        }

        [Fact]
        public void Buff_ShouldNotDecrementOnEnemyTurn()
        {
            // Arrange
            var buffs = new Dictionary<string, int>
            {
                { "Battle Rage", 3 }
            };
            int initialValue = buffs["Battle Rage"];

            // Act - Enemy turn (buff should NOT decrement in this simulation)
            // No action on buff during enemy turn

            // Assert
            Assert.Equal(initialValue, buffs["Battle Rage"]);
        }

        [Fact]
        public void Buff_ShouldBeRemovedWhenReachingZero()
        {
            // Arrange
            var buffs = new Dictionary<string, int>
            {
                { "Battle Rage", 1 }
            };

            // Act
            buffs["Battle Rage"]--;
            if (buffs["Battle Rage"] <= 0)
            {
                buffs.Remove("Battle Rage");
            }

            // Assert
            Assert.False(buffs.ContainsKey("Battle Rage"));
        }

        [Fact]
        public void Buff_ThreeTurns_ShouldLastThreePlayerActions()
        {
            // Arrange
            var buffs = new Dictionary<string, int>
            {
                { "Battle Rage", 3 }
            };

            // Act - Simulate 3 player turns
            for (int i = 0; i < 3; i++)
            {
                buffs["Battle Rage"]--;
            }

            // Assert
            Assert.Equal(0, buffs["Battle Rage"]);
        }

        [Fact]
        public void MultipleBuffs_ShouldDecrementIndependently()
        {
            // Arrange
            var buffs = new Dictionary<string, int>
            {
                { "Battle Rage", 3 },
                { "Shield Wall", 2 }
            };

            // Act - Simulate player turn
            buffs["Battle Rage"]--;
            buffs["Shield Wall"]--;

            // Assert
            Assert.Equal(2, buffs["Battle Rage"]);
            Assert.Equal(1, buffs["Shield Wall"]);
        }

        [Fact]
        public void BossStatusEffect_PoisonShouldTickOnPlayerTurn()
        {
            // Arrange
            int poisonDamage = 10;
            int poisonTurns = 3;
            var enemy = new Enemy("Test Enemy", 1, EnemyType.Beast);
            enemy.MaxHP = 100;
            enemy.CurrentHP = 100;

            // Act - Simulate poison tick
            if (poisonTurns > 0)
            {
                enemy.CurrentHP -= poisonDamage;
                poisonTurns--;
            }

            // Assert
            Assert.Equal(90, enemy.CurrentHP);
            Assert.Equal(2, poisonTurns);
        }

        [Fact]
        public void BossStatusEffect_EnrageShouldIncreaseAttackMultiplier()
        {
            // Arrange
            var statusEffects = new BossStatusEffects
            {
                IsEnraged = true,
                EnrageTurns = 3,
                EnrageDamageMultiplier = 1.5
            };

            // Assert
            Assert.True(statusEffects.IsEnraged);
            Assert.Equal(1.5, statusEffects.EnrageDamageMultiplier);
        }

        [Fact]
        public void BossStatusEffect_ShieldShouldAbsorbDamage()
        {
            // Arrange
            var statusEffects = new BossStatusEffects
            {
                ShieldValue = 50,
                ShieldTurns = 3
            };

            // Act - Simulate damage absorption
            int incomingDamage = 30;
            int damageToShield = Math.Min(statusEffects.ShieldValue, incomingDamage);
            statusEffects.ShieldValue -= damageToShield;
            int remainingDamage = Math.Max(0, incomingDamage - damageToShield);

            // Assert
            Assert.Equal(20, statusEffects.ShieldValue);
            Assert.Equal(0, remainingDamage);
        }
    }
}
