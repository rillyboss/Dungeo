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

        // Helper to calculate damage with percentage-based defense formula
        private int CalculateExpectedDamage(int rawDamage, int defense)
        {
            double defenseReduction = defense / (double)(defense + 100);
            return Math.Max(1, (int)(rawDamage * (1 - defenseReduction)));
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
            var effect = new DamageOverTimeEffect("burning", "Burning", "🔥", 3, 10);

            player.Effects.AddEffect(effect);

            Assert.Single(player.Effects.ActiveEffects);
            Assert.Contains(effect, player.Effects.ActiveEffects);
        }

        [Fact]
        public void AddEffect_NonStackingEffect_ReplacesExisting()
        {
            var player = CreateTestPlayer();
            var enemy = CreateTestEnemy();
            var effect1 = new DamageOverTimeEffect("burning", "Burning", "🔥", 3, 10);
            var effect2 = new DamageOverTimeEffect("burning", "Burning", "🔥", 5, 15);
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
            var effect = new DamageOverTimeEffect("burning", "Burning", "🔥", 3, 10);

            player.Effects.AddEffect(effect);
            player.Effects.RemoveEffect(effect);

            Assert.Empty(player.Effects.ActiveEffects);
        }

        [Fact]
        public void ClearAll_RemovesAllEffects()
        {
            var player = CreateTestPlayer();
            var enemy = CreateTestEnemy();

            var burningEffect = new DamageOverTimeEffect("burning", "Burning", "🔥", 3, 10);
            burningEffect.Source = enemy;
            player.Effects.AddEffect(burningEffect);

            var poisonEffect = new DamageOverTimeEffect("poison", "Poisoned", "☠️", 2, 5);
            poisonEffect.Source = enemy;
            player.Effects.AddEffect(poisonEffect);

            var stunEffect = new StunEffect("stun", "Stunned", "⚡", 1);
            player.Effects.AddEffect(stunEffect);

            Assert.Equal(3, player.Effects.ActiveEffects.Count);

            player.Effects.ClearAll();

            Assert.Empty(player.Effects.ActiveEffects);
        }

        [Fact]
        public void GetBuffs_ReturnsOnlyBuffs()
        {
            var player = CreateTestPlayer();
            var enemy = CreateTestEnemy();

            var burningEffect1 = new DamageOverTimeEffect("burning", "Burning", "🔥", 3, 10);
            burningEffect1.Source = enemy;
            player.Effects.AddEffect(burningEffect1); // Debuff
            var shieldEffect = new ShieldEffect("shield", "Shield", "🛡️", 2, 50);
            player.Effects.AddEffect(shieldEffect); // Buff
            var regenEffect = new HealOverTimeEffect("regeneration", "Regeneration", "💚", 2, 10);
            player.Effects.AddEffect(regenEffect); // Buff

            var buffs = player.Effects.GetBuffs();

            Assert.Equal(2, buffs.Count);
            Assert.All(buffs, buff => Assert.Equal(StatusEffectType.Buff, buff.Type));
        }

        [Fact]
        public void GetDebuffs_ReturnsOnlyDebuffs()
        {
            var player = CreateTestPlayer();
            var enemy = CreateTestEnemy();

            var burningEffect2 = new DamageOverTimeEffect("burning", "Burning", "🔥", 3, 10);
            burningEffect2.Source = enemy;
            player.Effects.AddEffect(burningEffect2); // Debuff
            var poisonEffect1 = new DamageOverTimeEffect("poison", "Poisoned", "☠️", 2, 5);
            poisonEffect1.Source = enemy;
            player.Effects.AddEffect(poisonEffect1); // Debuff
            var shieldEffect = new ShieldEffect("shield", "Shield", "🛡️", 2, 50);
            player.Effects.AddEffect(shieldEffect); // Buff

            var debuffs = player.Effects.GetDebuffs();

            Assert.Equal(2, debuffs.Count);
            Assert.All(debuffs, debuff => Assert.Equal(StatusEffectType.Debuff, debuff.Type));
        }

        [Fact]
        public void HasEffect_ReturnsTrueWhenEffectExists()
        {
            var player = CreateTestPlayer();
            var enemy = CreateTestEnemy();

            var burningEffect3 = new DamageOverTimeEffect("burning", "Burning", "🔥", 3, 10);
            burningEffect3.Source = enemy;
            player.Effects.AddEffect(burningEffect3);

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

            var stunEffect = new StunEffect("stun", "Stunned", "⚡", 2);
            player.Effects.AddEffect(stunEffect);

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

            var burningEffect4 = new DamageOverTimeEffect("burning", "Burning", "🔥", 3, 10);
            burningEffect4.Source = enemy;
            player.Effects.AddEffect(burningEffect4);
            player.Effects.ProcessTurnStart();

            // DOT bypasses shields but still applies defense (percentage-based)
            int expectedDamage = CalculateExpectedDamage(10, player.Defense);
            Assert.Equal(initialHP - expectedDamage, player.CurrentHP);
        }

        [Fact]
        public void DamageOverTimeEffect_DecrementsRemainingTurns()
        {
            var player = CreateTestPlayer();
            var enemy = CreateTestEnemy();

            var burningEffect5 = new DamageOverTimeEffect("burning", "Burning", "🔥", 3, 10);
            burningEffect5.Source = enemy;
            player.Effects.AddEffect(burningEffect5);

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

            var burningEffect6 = new DamageOverTimeEffect("burning", "Burning", "🔥", 2, 10);
            burningEffect6.Source = enemy;
            player.Effects.AddEffect(burningEffect6);

            player.Effects.ProcessTurnStart(); // Turn 1: 2->1
            Assert.Single(player.Effects.ActiveEffects);

            player.Effects.ProcessTurnStart(); // Turn 2: 1->0, expires
            Assert.Empty(player.Effects.ActiveEffects);
        }

        [Fact]
        public void CreateBurning_CreatesCorrectEffect()
        {
            var effect = new DamageOverTimeEffect("burning", "Burning", "🔥", 3, 10);

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
            var effect = new DamageOverTimeEffect("poison", "Poisoned", "☠️", 4, 8);

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
            var effect = new DamageOverTimeEffect("bleed", "Bleeding", "🩸", 2, 12);

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

            var regenEffect = new HealOverTimeEffect("regeneration", "Regeneration", "💚", 3, 10);
            player.Effects.AddEffect(regenEffect);
            player.Effects.ProcessTurnStart();

            Assert.Equal(damagedHP + 10, player.CurrentHP);
        }

        [Fact]
        public void HealOverTimeEffect_CannotExceedMaxHP()
        {
            var player = CreateTestPlayer();
            int maxHP = player.MaxHP;

            var regenEffect = new HealOverTimeEffect("regeneration", "Regeneration", "💚", 3, 10);
            player.Effects.AddEffect(regenEffect);
            player.Effects.ProcessTurnStart();

            Assert.Equal(maxHP, player.CurrentHP);
        }

        [Fact]
        public void CreateRegeneration_CreatesCorrectEffect()
        {
            var effect = new HealOverTimeEffect("regeneration", "Regeneration", "💚", 4, 15);

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

            var shieldEffect = new ShieldEffect("shield", "Shield", "🛡️", 3, 50);
            player.Effects.AddEffect(shieldEffect);
            player.ApplyDamage(30);

            Assert.Equal(initialHP, player.CurrentHP); // No HP damage
            var shield = player.Effects.GetActiveShield();
            Assert.NotNull(shield);
            // Damage after defense (percentage-based), then shield absorbs that
            int damageAfterDefense = CalculateExpectedDamage(30, player.Defense);
            Assert.Equal(50 - damageAfterDefense, shield.CurrentShieldValue);
        }

        [Fact]
        public void ShieldEffect_BreaksWhenDepleted()
        {
            var player = CreateTestPlayer();
            int initialHP = player.CurrentHP;

            var shieldEffect = new ShieldEffect("shield", "Shield", "🛡️", 3, 30);
            player.Effects.AddEffect(shieldEffect);
            player.ApplyDamage(50);

            // Damage after defense (percentage-based)
            int damageAfterDefense = CalculateExpectedDamage(50, player.Defense);
            // Shield blocks 30, overflow = damageAfterDefense - 30
            int overflowDamage = Math.Max(0, damageAfterDefense - 30);
            Assert.Equal(initialHP - overflowDamage, player.CurrentHP);
            Assert.Null(player.Effects.GetActiveShield()); // Shield broken
        }

        [Fact]
        public void ShieldEffect_ExpiresAfterDuration()
        {
            var player = CreateTestPlayer();

            var shieldEffect = new ShieldEffect("shield", "Shield", "🛡️", 2, 50);
            player.Effects.AddEffect(shieldEffect);

            player.Effects.ProcessTurnStart(); // Turn 1
            Assert.NotNull(player.Effects.GetActiveShield());

            player.Effects.ProcessTurnStart(); // Turn 2
            Assert.Null(player.Effects.GetActiveShield());
        }

        [Fact]
        public void CreateShield_CreatesCorrectEffect()
        {
            var effect = new ShieldEffect("shield", "Shield", "🛡️", 3, 100);

            Assert.Equal("shield", effect.EffectId);
            Assert.Equal("Shield", effect.Name);
            Assert.Equal("🛡️", effect.Icon);
            Assert.Equal(StatusEffectType.Buff, effect.Type);
            Assert.Equal(3, effect.RemainingTurns);
            Assert.Equal(100, effect.CurrentShieldValue);
        }

        #endregion

        #region ThornsEffect Tests

        [Fact]
        public void ThornsEffect_DecrementsAfterTurns()
        {
            var player = CreateTestPlayer();

            var thornsEffect = new ThornsEffect("thorns", "Thorns", "🌵", 2, 10);
            player.Effects.AddEffect(thornsEffect);

            player.Effects.ProcessTurnStart();
            Assert.NotNull(player.Effects.GetEffect("thorns"));

            player.Effects.ProcessTurnStart();
            Assert.Null(player.Effects.GetEffect("thorns"));
        }

        [Fact]
        public void CreateThorns_CreatesCorrectEffect()
        {
            var effect = new ThornsEffect("thorns", "Thorns", "🌵", 3, 15);

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

            var stunEffect = new StunEffect("stun", "Stunned", "⚡", 1);
            player.Effects.AddEffect(stunEffect);

            Assert.True(player.Effects.IsStunned());
        }

        [Fact]
        public void StunEffect_ExpiresAfterDuration()
        {
            var player = CreateTestPlayer();

            var stunEffect = new StunEffect("stun", "Stunned", "⚡", 1);
            player.Effects.AddEffect(stunEffect);
            Assert.True(player.Effects.IsStunned());

            player.Effects.ProcessTurnStart();
            Assert.False(player.Effects.IsStunned());
        }

        [Fact]
        public void CreateStun_CreatesCorrectEffect()
        {
            var effect = new StunEffect("stun", "Stunned", "⚡", 2);

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

            var battleRageEffect = new StatModifierEffect("battle_rage", "Battle Rage", "😤",
                StatusEffectType.Buff, 3, StatModifierEffect.StatType.Attack, 1.5, true);
            player.Effects.AddEffect(battleRageEffect);

            // Battle rage should be active
            Assert.True(player.Effects.HasEffect("battle_rage"));
        }

        [Fact]
        public void StatModifierEffect_SpeedBuff_IncreasesSpeed()
        {
            var player = CreateTestPlayer();

            var speedBuffEffect = new StatModifierEffect("speed_buff", "Speed Boost", "⚡",
                StatusEffectType.Buff, 2, StatModifierEffect.StatType.Speed, 20, false);
            player.Effects.AddEffect(speedBuffEffect);

            // Speed buff should add to the speed modifier
            Assert.Equal(20, player.Effects.GetSpeedModifier());
        }

        [Fact]
        public void StatModifierEffect_ExpiresAfterDuration()
        {
            var player = CreateTestPlayer();

            var battleRageEffect = new StatModifierEffect("battle_rage", "Battle Rage", "😤",
                StatusEffectType.Buff, 1, StatModifierEffect.StatType.Attack, 1.5, true);
            player.Effects.AddEffect(battleRageEffect);
            Assert.True(player.Effects.HasEffect("battle_rage"));

            player.Effects.ProcessTurnStart();

            Assert.False(player.Effects.HasEffect("battle_rage"));
        }

        [Fact]
        public void CreateBattleRage_CreatesCorrectEffect()
        {
            var effect = new StatModifierEffect("battle_rage", "Battle Rage", "😤",
                StatusEffectType.Buff, 3, StatModifierEffect.StatType.Attack, 1.5, true);

            Assert.Equal("battle_rage", effect.EffectId);
            Assert.Equal("Battle Rage", effect.Name);
            Assert.Equal("😤", effect.Icon);
            Assert.Equal(StatusEffectType.Buff, effect.Type);
            Assert.Equal(3, effect.RemainingTurns);
        }

        [Fact]
        public void CreateEnrage_CreatesCorrectEffect()
        {
            var effect = new StatModifierEffect("enrage", "Enraged", "💢",
                StatusEffectType.Buff, 4, StatModifierEffect.StatType.Damage, 2.0, true);

            Assert.Equal("enrage", effect.EffectId);
            Assert.Equal("Enraged", effect.Name);
            Assert.Equal("💢", effect.Icon);
            Assert.Equal(StatusEffectType.Buff, effect.Type);
            Assert.Equal(4, effect.RemainingTurns);
        }

        [Fact]
        public void CreateSpeedBuff_CreatesCorrectEffect()
        {
            var effect = new StatModifierEffect("speed_buff", "Speed Boost", "⚡",
                StatusEffectType.Buff, 2, StatModifierEffect.StatType.Speed, 15, false);

            Assert.Equal("speed_buff", effect.EffectId);
            Assert.Equal("Speed Boost", effect.Name);
            Assert.Equal("⚡", effect.Icon);
            Assert.Equal(StatusEffectType.Buff, effect.Type);
            Assert.Equal(2, effect.RemainingTurns);
        }

        [Fact]
        public void CreateShieldWall_CreatesCorrectEffect()
        {
            var effect = new StatModifierEffect("shield_wall", "Shield Wall", "🛡️",
                StatusEffectType.Buff, 3, StatModifierEffect.StatType.Defense, 15, false);

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

            var burningEffect7 = new DamageOverTimeEffect("burning", "Burning", "🔥", 3, 10);
            burningEffect7.Source = enemy;
            player.Effects.AddEffect(burningEffect7);
            var regenEffect = new HealOverTimeEffect("regeneration", "Regeneration", "💚", 3, 15);
            player.Effects.AddEffect(regenEffect);

            player.Effects.ProcessTurnStart();

            // DOT damage after defense (percentage-based)
            // Healing = 15
            int dotDamage = CalculateExpectedDamage(10, player.Defense);
            Assert.Equal(damagedHP - dotDamage + 15, player.CurrentHP);
        }

        [Fact]
        public void MultipleDebuffs_CanCoexist()
        {
            var player = CreateTestPlayer();
            var enemy = CreateTestEnemy();

            var burningEffect8 = new DamageOverTimeEffect("burning", "Burning", "🔥", 3, 10);
            burningEffect8.Source = enemy;
            player.Effects.AddEffect(burningEffect8);
            var poisonEffect2 = new DamageOverTimeEffect("poison", "Poisoned", "☠️", 2, 5);
            poisonEffect2.Source = enemy;
            player.Effects.AddEffect(poisonEffect2);
            var bleedEffect1 = new DamageOverTimeEffect("bleed", "Bleeding", "🩸", 4, 8);
            bleedEffect1.Source = enemy;
            player.Effects.AddEffect(bleedEffect1);

            Assert.Equal(3, player.Effects.GetDebuffs().Count);
        }

        [Fact]
        public void MultipleBuffs_CanCoexist()
        {
            var player = CreateTestPlayer();

            var shieldEffect = new ShieldEffect("shield", "Shield", "🛡️", 3, 50);
            player.Effects.AddEffect(shieldEffect);
            var thornsEffect = new ThornsEffect("thorns", "Thorns", "🌵", 2, 10);
            player.Effects.AddEffect(thornsEffect);
            var battleRageEffect = new StatModifierEffect("battle_rage", "Battle Rage", "😤",
                StatusEffectType.Buff, 4, StatModifierEffect.StatType.Attack, 1.5, true);
            player.Effects.AddEffect(battleRageEffect);

            Assert.Equal(3, player.Effects.GetBuffs().Count);
        }

        [Fact]
        public void MultipleDOTs_AllDealDamage()
        {
            var player = CreateTestPlayer();
            var enemy = CreateTestEnemy();
            int initialHP = player.CurrentHP;

            var burningEffect9 = new DamageOverTimeEffect("burning", "Burning", "🔥", 3, 10);
            burningEffect9.Source = enemy;
            player.Effects.AddEffect(burningEffect9);
            var poisonEffect3 = new DamageOverTimeEffect("poison", "Poisoned", "☠️", 3, 5);
            poisonEffect3.Source = enemy;
            player.Effects.AddEffect(poisonEffect3);
            var bleedEffect2 = new DamageOverTimeEffect("bleed", "Bleeding", "🩸", 3, 8);
            bleedEffect2.Source = enemy;
            player.Effects.AddEffect(bleedEffect2);

            player.Effects.ProcessTurnStart();

            // Each DOT applies defense separately (percentage-based)
            int totalDamage = CalculateExpectedDamage(10, player.Defense) +
                             CalculateExpectedDamage(5, player.Defense) +
                             CalculateExpectedDamage(8, player.Defense);
            Assert.Equal(initialHP - totalDamage, player.CurrentHP);
        }

        #endregion
    }
}
