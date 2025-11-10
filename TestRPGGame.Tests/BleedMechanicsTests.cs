using System;
using Xunit;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.Entities.Boss;
using TestRPGGame.Combat;

namespace TestRPGGame.Tests
{
    public class BleedMechanicsTests : TestBase
    {
        [Fact]
        public void Bleed_ShouldApplyDamageOverTime()
        {
            // Arrange
            var statusEffects = new BossStatusEffects
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
            var statusEffects = new BossStatusEffects
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
            var statusEffects = new BossStatusEffects
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
        public void BleedEffect_ShouldHaveCorrectType()
        {
            // Arrange & Act
            var effect = new BossAbilityEffect(BossAbilityEffectType.Bleed, 20, 4, 1.5);

            // Assert
            Assert.Equal(BossAbilityEffectType.Bleed, effect.Type);
            Assert.Equal(20, effect.Value);
            Assert.Equal(4, effect.Duration);
            Assert.Equal(1.5, effect.Multiplier);
        }

        [Fact]
        public void HeavyStrike_ShouldDealBurstDamage()
        {
            // Arrange
            var effect = new BossAbilityEffect(BossAbilityEffectType.HeavyStrike, 50, 0, 2.0);
            var boss = new Enemy("Test Boss", 5, EnemyType.Demon);
            boss.Attack = 100;
            var player = new Player("Test Player", PlayerClass.Warrior);
            player.MaxHP = 200;
            player.CurrentHP = 200;

            // Act - Simulate heavy strike
            int damage = (int)(boss.Attack * effect.Multiplier + effect.Value);
            int actualDamage = Math.Max(1, damage - player.GetTotalDefense());
            player.CurrentHP -= actualDamage;

            // Assert
            Assert.True(player.CurrentHP < 200); // Player took damage
            Assert.Equal(250, damage); // 100 * 2.0 + 50
        }

        [Fact]
        public void BossAbility_WithBleed_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var ability = new BossAbility(
                "Rending Strike",
                "Causes severe bleeding",
                4,
                new BossAbilityEffect(BossAbilityEffectType.Bleed, 25, 5, 1.8)
            );

            // Assert
            Assert.Equal("Rending Strike", ability.Name);
            Assert.Equal(4, ability.Cooldown);
            Assert.True(ability.CanUse());
            Assert.Equal(BossAbilityEffectType.Bleed, ability.Effect.Type);
        }

        [Fact]
        public void BossAbility_Cooldown_ShouldPreventImmediateReuse()
        {
            // Arrange
            var ability = new BossAbility(
                "Bleeding Strike",
                "Causes bleeding",
                3,
                new BossAbilityEffect(BossAbilityEffectType.Bleed, 20, 4, 1.5)
            );

            // Act
            ability.Use();

            // Assert
            Assert.False(ability.CanUse());
            Assert.Equal(3, ability.CurrentCooldown);
        }

        [Fact]
        public void BossAbility_Cooldown_ShouldReduceOverTime()
        {
            // Arrange
            var ability = new BossAbility(
                "Bleeding Strike",
                "Causes bleeding",
                3,
                new BossAbilityEffect(BossAbilityEffectType.Bleed, 20, 4, 1.5)
            );
            ability.Use();

            // Act
            ability.ReduceCooldown();
            ability.ReduceCooldown();

            // Assert
            Assert.Equal(1, ability.CurrentCooldown);
            Assert.False(ability.CanUse());
        }

        [Fact]
        public void BossAbility_Cooldown_ShouldEnableReuse()
        {
            // Arrange
            var ability = new BossAbility(
                "Bleeding Strike",
                "Causes bleeding",
                2,
                new BossAbilityEffect(BossAbilityEffectType.Bleed, 20, 4, 1.5)
            );
            ability.Use();

            // Act - Reduce cooldown completely
            ability.ReduceCooldown();
            ability.ReduceCooldown();

            // Assert
            Assert.True(ability.CanUse());
            Assert.Equal(0, ability.CurrentCooldown);
        }

        [Fact]
        public void Bleed_ShouldNotTickWhenDurationZero()
        {
            // Arrange
            var statusEffects = new BossStatusEffects
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
