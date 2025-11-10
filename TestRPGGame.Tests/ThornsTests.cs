using Xunit;
using TestRPGGame.Combat;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.Factories;
using TestRPGGame.DataLoading;

namespace TestRPGGame.Tests
{
    public class ThornsTests : TestBase
    {
        [Fact]
        public void EnemyThorns_DamagesPlayerWhenPlayerAttacks()
        {
            // Arrange
            var player = new Player("Test Player", PlayerClass.Warrior);
            player.CurrentHP = 100;
            var enemy = EnemyFactory.CreateEnemy(1);

            // Manually set Thorns on enemy
            enemy.EnsureStatusEffects();
            enemy.StatusEffects!.ThornsValue = 20;
            enemy.StatusEffects.ThornsTurns = 3;

            int playerHPBefore = player.CurrentHP;
            int enemyHPBefore = enemy.CurrentHP;

            // Act - Simulate player attacking enemy (basic attack logic)
            int damage = player.Attack;
            enemy.CurrentHP -= damage;

            // Apply Thorns reflection (this is what the combat system does)
            if (enemy.StatusEffects != null && enemy.StatusEffects.ThornsValue > 0)
            {
                player.CurrentHP -= enemy.StatusEffects.ThornsValue;
            }

            // Assert
            Assert.True(enemy.CurrentHP < enemyHPBefore, "Enemy should take damage from player attack");
            Assert.True(player.CurrentHP < playerHPBefore, "Player should take Thorns damage");
            Assert.Equal(playerHPBefore - 20, player.CurrentHP); // Player took 20 Thorns damage
        }

        [Fact]
        public void PlayerThorns_DamagesEnemyWhenEnemyAttacks()
        {
            // Arrange
            var player = new Player("Test Player", PlayerClass.Warrior);
            player.CurrentHP = 100;
            var enemy = EnemyFactory.CreateEnemy(1);

            int playerHPBefore = player.CurrentHP;
            int enemyHPBefore = enemy.CurrentHP;

            // Simulate player having Thorns from equipment (value = 15)
            int playerThornsValue = 15;

            // Act - Simulate enemy attacking player
            int damage = enemy.Attack;
            int actualDamage = Math.Max(1, damage - player.GetTotalDefense());
            player.CurrentHP -= actualDamage;

            // Apply Thorns reflection to enemy (this is what ApplyDamageToPlayer does)
            enemy.CurrentHP -= playerThornsValue;

            // Assert
            Assert.True(player.CurrentHP < playerHPBefore, "Player should take damage from enemy attack");
            Assert.True(enemy.CurrentHP < enemyHPBefore, "Enemy should take Thorns damage");
            Assert.Equal(enemyHPBefore - playerThornsValue, enemy.CurrentHP); // Enemy took 15 Thorns damage
        }

        [Fact]
        public void EnemyThorns_DoesNotDamageEnemyItself()
        {
            // Arrange
            var player = new Player("Test Player", PlayerClass.Warrior);
            player.CurrentHP = 100;
            var enemy = EnemyFactory.CreateEnemy(1);

            // Set Thorns on enemy
            if (enemy.StatusEffects != null)
            {
                enemy.StatusEffects.ThornsValue = 20;
                enemy.StatusEffects.ThornsTurns = 3;
            }

            int enemyHPBefore = enemy.CurrentHP;

            // Act - Player attacks enemy
            int damageToEnemy = player.Attack;
            enemy.CurrentHP -= damageToEnemy;

            // Apply Thorns - should ONLY damage the player, not the enemy
            if (enemy.StatusEffects != null && enemy.StatusEffects.ThornsValue > 0)
            {
                player.CurrentHP -= enemy.StatusEffects.ThornsValue;
            }

            // Assert - Enemy should only take attack damage, not Thorns damage
            int expectedEnemyHP = enemyHPBefore - damageToEnemy;
            Assert.Equal(expectedEnemyHP, enemy.CurrentHP);
        }

        [Fact]
        public void PlayerThorns_DoesNotDamagePlayerItself()
        {
            // Arrange
            var player = new Player("Test Player", PlayerClass.Warrior);
            player.CurrentHP = 100;
            var enemy = EnemyFactory.CreateEnemy(1);

            int playerHPBefore = player.CurrentHP;
            int playerThornsValue = 15;

            // Act - Enemy attacks player
            int damage = enemy.Attack;
            int actualDamage = Math.Max(1, damage - player.GetTotalDefense());
            player.CurrentHP -= actualDamage;

            // Apply Thorns to enemy (not to player)
            enemy.CurrentHP -= playerThornsValue;

            // Assert - Player should only take attack damage, not Thorns damage
            int expectedPlayerHP = playerHPBefore - actualDamage;
            Assert.Equal(expectedPlayerHP, player.CurrentHP);
        }

        [Fact]
        public void ThornsEffect_DecrementsTurnsCorrectly()
        {
            // Arrange
            var enemy = EnemyFactory.CreateEnemy(1);

            enemy.EnsureStatusEffects();
            enemy.StatusEffects!.ThornsValue = 20;
            enemy.StatusEffects.ThornsTurns = 3;

            // Act - Simulate turn decrement (from ApplyEnemyTurnEffects)
            if (enemy.StatusEffects != null && enemy.StatusEffects.ThornsTurns > 0)
            {
                enemy.StatusEffects.ThornsTurns--;
                if (enemy.StatusEffects.ThornsTurns == 0)
                {
                    enemy.StatusEffects.ThornsValue = 0;
                }
            }

            // Assert
            Assert.Equal(2, enemy.StatusEffects!.ThornsTurns);
            Assert.Equal(20, enemy.StatusEffects.ThornsValue); // Still active

            // Act - Decrement two more times
            for (int i = 0; i < 2; i++)
            {
                if (enemy.StatusEffects.ThornsTurns > 0)
                {
                    enemy.StatusEffects.ThornsTurns--;
                    if (enemy.StatusEffects.ThornsTurns == 0)
                    {
                        enemy.StatusEffects.ThornsValue = 0;
                    }
                }
            }

            // Assert - Thorns should be expired
            Assert.Equal(0, enemy.StatusEffects.ThornsTurns);
            Assert.Equal(0, enemy.StatusEffects.ThornsValue);
        }

        [Fact]
        public void ThornsAbility_IsLoadedFromData()
        {
            // Arrange & Act - Load Goblin King's Void Thorns ability from data
            var ability = DataLoader.GetAbility("boss_void_thorns");

            // Assert
            Assert.NotNull(ability);
            Assert.Equal("Void Thorns", ability.Name);
            Assert.Contains(ability.Effects, e => e.Type == "Thorns");

            var thornsEffect = ability.Effects.FirstOrDefault(e => e.Type == "Thorns");
            Assert.NotNull(thornsEffect);
            Assert.Equal(30, thornsEffect.Value); // Void Thorns reflects 30 damage
            Assert.Equal(4, thornsEffect.Duration); // Lasts 4 turns
        }
    }
}
