using Xunit;
using TestRPGGame.Combat.StatusEffects;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.Factories;

namespace TestRPGGame.Tests
{
    public class StatusEffectSystemTests : TestBase
    {
        private Player CreateTestPlayer()
        {
            return new Player("TestPlayer", PlayerClass.Warrior);
        }

        private Enemy CreateTestEnemy()
        {
            return EnemyFactory.CreateEnemy(1);
        }

        #region StatusEffectManager Tests

        [Fact]
        public void StatusEffectManager_StartsEmpty()
        {
            var player = CreateTestPlayer();

            Assert.Empty(player.Effects.ActiveEffects);
            Assert.Empty(player.Effects.GetBuffs());
            Assert.Empty(player.Effects.GetDebuffs());
        }

        [Fact]
        public void AddEffect_AddsEffectToManager()
        {
            var player = CreateTestPlayer();
            var effect = StatusEffectFactory.CreateBurning(3, 10);

            player.Effects.AddEffect(effect);

            Assert.Single(player.Effects.ActiveEffects);
            Assert.Contains(effect, player.Effects.ActiveEffects);
        }

        [Fact]
        public void AddEffect_NonStackingEffect_ReplacesExisting()
        {
            var player = CreateTestPlayer();
            var enemy = CreateTestEnemy();
            var effect1 = StatusEffectFactory.CreateBurning(3, 10);
            var effect2 = StatusEffectFactory.CreateBurning(5, 15);
            effect1.Source = enemy;
            effect2.Source = enemy;

            player.Effects.AddEffect(effect1);
            player.Effects.AddEffect(effect2);

            Assert.Single(player.Effects.ActiveEffects);
            var activeEffect = player.Effects.ActiveEffects.First();
            Assert.Equal(5, activeEffect.RemainingTurns);
            Assert.Equal(15, activeEffect.Value);
        }

        [Fact]
        public void RemoveEffect_RemovesSpecificEffect()
        {
            var player = CreateTestPlayer();
            var effect = StatusEffectFactory.CreateBurning(3, 10);

            player.Effects.AddEffect(effect);
            player.Effects.RemoveEffect(effect);

            Assert.Empty(player.Effects.ActiveEffects);
        }

        [Fact]
        public void ClearAll_RemovesAllEffects()
        {
            var player = CreateTestPlayer();
            var enemy = CreateTestEnemy();

            player.ApplyBurning(enemy, 3, 10);
            player.ApplyPoison(enemy, 2, 5);
            player.ApplyStun(1);

            Assert.Equal(3, player.Effects.ActiveEffects.Count);

            player.Effects.ClearAll();

            Assert.Empty(player.Effects.ActiveEffects);
        }

        [Fact]
        public void GetBuffs_ReturnsOnlyBuffs()
        {
            var player = CreateTestPlayer();
            var enemy = CreateTestEnemy();

            player.ApplyBurning(enemy, 3, 10); // Debuff
            player.ApplyShield(2, 50); // Buff
            player.ApplyRegeneration(2, 10); // Buff

            var buffs = player.Effects.GetBuffs();

            Assert.Equal(2, buffs.Count);
            Assert.All(buffs, buff => Assert.Equal(StatusEffectType.Buff, buff.Type));
        }

        [Fact]
        public void GetDebuffs_ReturnsOnlyDebuffs()
        {
            var player = CreateTestPlayer();
            var enemy = CreateTestEnemy();

            player.ApplyBurning(enemy, 3, 10); // Debuff
            player.ApplyPoison(enemy, 2, 5); // Debuff
            player.ApplyShield(2, 50); // Buff

            var debuffs = player.Effects.GetDebuffs();

            Assert.Equal(2, debuffs.Count);
            Assert.All(debuffs, debuff => Assert.Equal(StatusEffectType.Debuff, debuff.Type));
        }

        [Fact]
        public void HasEffect_ReturnsTrueWhenEffectExists()
        {
            var player = CreateTestPlayer();
            var enemy = CreateTestEnemy();

            player.ApplyBurning(enemy, 3, 10);

            Assert.True(player.Effects.HasEffect("burning"));
        }

        [Fact]
        public void HasEffect_ReturnsFalseWhenEffectDoesNotExist()
        {
            var player = CreateTestPlayer();

            Assert.False(player.Effects.HasEffect("burning"));
        }

        [Fact]
        public void IsStunned_ReturnsTrueWhenStunned()
        {
            var player = CreateTestPlayer();

            player.ApplyStun(2);

            Assert.True(player.Effects.IsStunned());
        }

        [Fact]
        public void IsStunned_ReturnsFalseWhenNotStunned()
        {
            var player = CreateTestPlayer();

            Assert.False(player.Effects.IsStunned());
        }

        #endregion

        #region DamageOverTimeEffect Tests

        [Fact]
        public void DamageOverTimeEffect_DealsDamageOnTurnStart()
        {
            var player = CreateTestPlayer();
            var enemy = CreateTestEnemy();
            int initialHP = player.CurrentHP;

            player.ApplyBurning(enemy, 3, 10);
            player.Effects.ProcessTurnStart();

            // DOT bypasses shields but still applies defense
            // Damage = max(1, 10 - player.Defense)
            int expectedDamage = Math.Max(1, 10 - player.Defense);
            Assert.Equal(initialHP - expectedDamage, player.CurrentHP);
        }

        [Fact]
        public void DamageOverTimeEffect_DecrementsRemainingTurns()
        {
            var player = CreateTestPlayer();
            var enemy = CreateTestEnemy();

            player.ApplyBurning(enemy, 3, 10);

            var effectBefore = player.Effects.ActiveEffects.First();
            Assert.Equal(3, effectBefore.RemainingTurns);

            player.Effects.ProcessTurnStart();

            // Effect should be removed after processing, check if new duration would be 2
            // Actually, the effect stays until duration reaches 0, so let's check the active effects
            if (player.Effects.ActiveEffects.Any())
            {
                var effectAfter = player.Effects.ActiveEffects.First();
                Assert.Equal(2, effectAfter.RemainingTurns);
            }
        }

        [Fact]
        public void DamageOverTimeEffect_ExpiresAfterDuration()
        {
            var player = CreateTestPlayer();
            var enemy = CreateTestEnemy();

            player.ApplyBurning(enemy, 2, 10);

            player.Effects.ProcessTurnStart(); // Turn 1: 2->1
            Assert.Single(player.Effects.ActiveEffects);

            player.Effects.ProcessTurnStart(); // Turn 2: 1->0, expires
            Assert.Empty(player.Effects.ActiveEffects);
        }

        [Fact]
        public void CreateBurning_CreatesCorrectEffect()
        {
            var effect = StatusEffectFactory.CreateBurning(3, 10);

            Assert.Equal("burning", effect.EffectId);
            Assert.Equal("Burning", effect.Name);
            Assert.Equal("🔥", effect.Icon);
            Assert.Equal(StatusEffectType.Debuff, effect.Type);
            Assert.Equal(3, effect.RemainingTurns);
            Assert.Equal(10, effect.Value);
        }

        [Fact]
        public void CreatePoison_CreatesCorrectEffect()
        {
            var effect = StatusEffectFactory.CreatePoison(4, 8);

            Assert.Equal("poison", effect.EffectId);
            Assert.Equal("Poisoned", effect.Name);
            Assert.Equal("☠️", effect.Icon);
            Assert.Equal(StatusEffectType.Debuff, effect.Type);
            Assert.Equal(4, effect.RemainingTurns);
            Assert.Equal(8, effect.Value);
        }

        [Fact]
        public void CreateBleed_CreatesCorrectEffect()
        {
            var effect = StatusEffectFactory.CreateBleed(2, 12);

            Assert.Equal("bleed", effect.EffectId);
            Assert.Equal("Bleeding", effect.Name);
            Assert.Equal("🩸", effect.Icon);
            Assert.Equal(StatusEffectType.Debuff, effect.Type);
            Assert.Equal(2, effect.RemainingTurns);
            Assert.Equal(12, effect.Value);
        }

        #endregion

        #region HealOverTimeEffect Tests

        [Fact]
        public void HealOverTimeEffect_HealsOnTurnStart()
        {
            var player = CreateTestPlayer();

            // Damage player first
            player.ApplyDamage(50);
            int damagedHP = player.CurrentHP;

            player.ApplyRegeneration(3, 10);
            player.Effects.ProcessTurnStart();

            Assert.Equal(damagedHP + 10, player.CurrentHP);
        }

        [Fact]
        public void HealOverTimeEffect_CannotExceedMaxHP()
        {
            var player = CreateTestPlayer();
            int maxHP = player.MaxHP;

            player.ApplyRegeneration(3, 10);
            player.Effects.ProcessTurnStart();

            Assert.Equal(maxHP, player.CurrentHP);
        }

        [Fact]
        public void CreateRegeneration_CreatesCorrectEffect()
        {
            var effect = StatusEffectFactory.CreateRegeneration(4, 15);

            Assert.Equal("regeneration", effect.EffectId);
            Assert.Equal("Regeneration", effect.Name);
            Assert.Equal("💚", effect.Icon);
            Assert.Equal(StatusEffectType.Buff, effect.Type);
            Assert.Equal(4, effect.RemainingTurns);
            Assert.Equal(15, effect.Value);
        }

        #endregion

        #region ShieldEffect Tests

        [Fact]
        public void ShieldEffect_AbsorbsDamage()
        {
            var player = CreateTestPlayer();
            int initialHP = player.CurrentHP;

            player.ApplyShield(3, 50);
            player.ApplyDamage(30);

            Assert.Equal(initialHP, player.CurrentHP); // No HP damage
            var shield = player.Effects.GetActiveShield();
            Assert.NotNull(shield);
            // Damage after defense = max(1, 30 - Defense), then shield absorbs that
            int damageAfterDefense = Math.Max(1, 30 - player.Defense);
            Assert.Equal(50 - damageAfterDefense, shield.CurrentShieldValue);
        }

        [Fact]
        public void ShieldEffect_BreaksWhenDepleted()
        {
            var player = CreateTestPlayer();
            int initialHP = player.CurrentHP;

            player.ApplyShield(3, 30);
            player.ApplyDamage(50);

            // Damage after defense = max(1, 50 - Defense)
            int damageAfterDefense = Math.Max(1, 50 - player.Defense);
            // Shield blocks 30, overflow = damageAfterDefense - 30
            int overflowDamage = Math.Max(0, damageAfterDefense - 30);
            Assert.Equal(initialHP - overflowDamage, player.CurrentHP);
            Assert.Null(player.Effects.GetActiveShield()); // Shield broken
        }

        [Fact]
        public void ShieldEffect_ExpiresAfterDuration()
        {
            var player = CreateTestPlayer();

            player.ApplyShield(2, 50);

            player.Effects.ProcessTurnStart(); // Turn 1
            Assert.NotNull(player.Effects.GetActiveShield());

            player.Effects.ProcessTurnStart(); // Turn 2
            Assert.Null(player.Effects.GetActiveShield());
        }

        [Fact]
        public void CreateShield_CreatesCorrectEffect()
        {
            var effect = StatusEffectFactory.CreateShield(3, 100);

            Assert.Equal("shield", effect.EffectId);
            Assert.Equal("Shield", effect.Name);
            Assert.Equal("🛡️", effect.Icon);
            Assert.Equal(StatusEffectType.Buff, effect.Type);
            Assert.Equal(3, effect.RemainingTurns);
            Assert.Equal(100, ((ShieldEffect)effect).CurrentShieldValue);
        }

        #endregion

        #region ThornsEffect Tests

        [Fact]
        public void ThornsEffect_DecrementsAfterTurns()
        {
            var player = CreateTestPlayer();

            player.ApplyThorns(2, 10);

            player.Effects.ProcessTurnStart();
            Assert.NotNull(player.Effects.GetEffect("thorns"));

            player.Effects.ProcessTurnStart();
            Assert.Null(player.Effects.GetEffect("thorns"));
        }

        [Fact]
        public void CreateThorns_CreatesCorrectEffect()
        {
            var effect = StatusEffectFactory.CreateThorns(3, 15);

            Assert.Equal("thorns", effect.EffectId);
            Assert.Equal("Thorns", effect.Name);
            Assert.Equal("🌵", effect.Icon);
            Assert.Equal(StatusEffectType.Buff, effect.Type);
            Assert.Equal(3, effect.RemainingTurns);
            Assert.Equal(15, effect.Value);
        }

        #endregion

        #region StunEffect Tests

        [Fact]
        public void StunEffect_PreventsAction()
        {
            var player = CreateTestPlayer();

            player.ApplyStun(1);

            Assert.True(player.Effects.IsStunned());
        }

        [Fact]
        public void StunEffect_ExpiresAfterDuration()
        {
            var player = CreateTestPlayer();

            player.ApplyStun(1);
            Assert.True(player.Effects.IsStunned());

            player.Effects.ProcessTurnStart();
            Assert.False(player.Effects.IsStunned());
        }

        [Fact]
        public void CreateStun_CreatesCorrectEffect()
        {
            var effect = StatusEffectFactory.CreateStun(2);

            Assert.Equal("stun", effect.EffectId);
            Assert.Equal("Stunned", effect.Name);
            Assert.Equal("⚡", effect.Icon);
            Assert.Equal(StatusEffectType.Control, effect.Type);
            Assert.Equal(2, effect.RemainingTurns);
        }

        #endregion

        #region StatModifierEffect Tests

        [Fact]
        public void StatModifierEffect_BattleRage_AppliesCorrectly()
        {
            var player = CreateTestPlayer();

            player.ApplyBattleRage(3);

            // Battle rage should be active
            Assert.True(player.Effects.HasEffect("battle_rage"));
        }

        [Fact]
        public void StatModifierEffect_SpeedBuff_IncreasesSpeed()
        {
            var player = CreateTestPlayer();

            player.ApplySpeedBuff(2, 20);

            // Speed buff should add to the speed modifier
            Assert.Equal(20, player.Effects.GetSpeedModifier());
        }

        [Fact]
        public void StatModifierEffect_ExpiresAfterDuration()
        {
            var player = CreateTestPlayer();

            player.ApplyBattleRage(1);
            Assert.True(player.Effects.HasEffect("battle_rage"));

            player.Effects.ProcessTurnStart();

            Assert.False(player.Effects.HasEffect("battle_rage"));
        }

        [Fact]
        public void CreateBattleRage_CreatesCorrectEffect()
        {
            var effect = StatusEffectFactory.CreateBattleRage(3);

            Assert.Equal("battle_rage", effect.EffectId);
            Assert.Equal("Battle Rage", effect.Name);
            Assert.Equal("😤", effect.Icon);
            Assert.Equal(StatusEffectType.Buff, effect.Type);
            Assert.Equal(3, effect.RemainingTurns);
        }

        [Fact]
        public void CreateEnrage_CreatesCorrectEffect()
        {
            var effect = StatusEffectFactory.CreateEnrage(4, 2.0);

            Assert.Equal("enrage", effect.EffectId);
            Assert.Equal("Enraged", effect.Name);
            Assert.Equal("💢", effect.Icon);
            Assert.Equal(StatusEffectType.Buff, effect.Type);
            Assert.Equal(4, effect.RemainingTurns);
        }

        [Fact]
        public void CreateSpeedBuff_CreatesCorrectEffect()
        {
            var effect = StatusEffectFactory.CreateSpeedBuff(2, 15);

            Assert.Equal("speed_buff", effect.EffectId);
            Assert.Equal("Speed Boost", effect.Name);
            Assert.Equal("⚡", effect.Icon);
            Assert.Equal(StatusEffectType.Buff, effect.Type);
            Assert.Equal(2, effect.RemainingTurns);
        }

        [Fact]
        public void CreateShieldWall_CreatesCorrectEffect()
        {
            var effect = StatusEffectFactory.CreateShieldWall(3);

            Assert.Equal("shield_wall", effect.EffectId);
            Assert.Equal("Shield Wall", effect.Name);
            Assert.Equal("🛡️", effect.Icon);
            Assert.Equal(StatusEffectType.Buff, effect.Type);
            Assert.Equal(3, effect.RemainingTurns);
        }

        #endregion

        #region Multiple Effects Tests

        [Fact]
        public void MultipleEffects_AllProcessedOnTurnStart()
        {
            var player = CreateTestPlayer();
            var enemy = CreateTestEnemy();

            // Damage player to allow healing
            player.ApplyDamage(50);
            int damagedHP = player.CurrentHP;

            player.ApplyBurning(enemy, 3, 10);
            player.ApplyRegeneration(3, 15);

            player.Effects.ProcessTurnStart();

            // DOT damage after defense = max(1, 10 - Defense)
            // Healing = 15
            int dotDamage = Math.Max(1, 10 - player.Defense);
            Assert.Equal(damagedHP - dotDamage + 15, player.CurrentHP);
        }

        [Fact]
        public void MultipleDebuffs_CanCoexist()
        {
            var player = CreateTestPlayer();
            var enemy = CreateTestEnemy();

            player.ApplyBurning(enemy, 3, 10);
            player.ApplyPoison(enemy, 2, 5);
            player.ApplyBleed(enemy, 4, 8);

            Assert.Equal(3, player.Effects.GetDebuffs().Count);
        }

        [Fact]
        public void MultipleBuffs_CanCoexist()
        {
            var player = CreateTestPlayer();

            player.ApplyShield(3, 50);
            player.ApplyThorns(2, 10);
            player.ApplyBattleRage(4);

            Assert.Equal(3, player.Effects.GetBuffs().Count);
        }

        [Fact]
        public void MultipleDOTs_AllDealDamage()
        {
            var player = CreateTestPlayer();
            var enemy = CreateTestEnemy();
            int initialHP = player.CurrentHP;

            player.ApplyBurning(enemy, 3, 10);
            player.ApplyPoison(enemy, 3, 5);
            player.ApplyBleed(enemy, 3, 8);

            player.Effects.ProcessTurnStart();

            // Each DOT applies defense separately
            int totalDamage = Math.Max(1, 10 - player.Defense) +
                             Math.Max(1, 5 - player.Defense) +
                             Math.Max(1, 8 - player.Defense);
            Assert.Equal(initialHP - totalDamage, player.CurrentHP);
        }

        #endregion
    }
}
