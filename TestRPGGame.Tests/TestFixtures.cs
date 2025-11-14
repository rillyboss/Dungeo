using System;
using System.Collections.Generic;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.Equipment;
using TestRPGGame.DataLoading;
using TestRPGGame.Abilities;
using TestRPGGame.Abilities.Effects;
using TestRPGGame.Combat;
using TestRPGGame.Combat.StatusEffects;
using Moq;

namespace TestRPGGame.Tests
{
    /// <summary>
    /// Test fixtures and helper methods for creating controlled test data.
    /// This class provides methods to create test entities with known values,
    /// eliminating dependency on JSON data files in unit tests.
    /// </summary>
    public static class TestFixtures
    {
        #region Enemy Creation

        /// <summary>
        /// Creates a test enemy with controlled stats (no data dependency).
        /// </summary>
        public static Enemy CreateTestEnemy(
            string name = "Test Enemy",
            int maxHP = 100,
            int attack = 20,
            int defense = 10,
            int speed = 10,
            int goldReward = 50,
            int expReward = 100)
        {
            var enemy = new Enemy(
                name,
                EnemyType.Beast,
                maxHP,
                attack,
                defense,
                speed,
                goldReward,
                expReward
            );

            return enemy;
        }

        /// <summary>
        /// Creates a test boss enemy with higher stats.
        /// </summary>
        public static Enemy CreateTestBoss(
            string name = "Test Boss",
            int maxHP = 500,
            int attack = 50,
            int defense = 25)
        {
            return CreateTestEnemy(name, maxHP, attack, defense, 15, 200, 500);
        }

        /// <summary>
        /// Creates a weak enemy for testing victory scenarios.
        /// </summary>
        public static Enemy CreateWeakEnemy(string name = "Weak Enemy")
        {
            return CreateTestEnemy(name, 10, 5, 2, 5, 10, 20);
        }

        #endregion

        #region Player Creation

        /// <summary>
        /// Creates a test player with controlled stats (no ClassData dependency).
        /// </summary>
        public static Player CreateTestPlayer(
            string name = "Test Player",
            int level = 1,
            int maxHP = 150,
            int maxMana = 100,
            int attack = 15,
            int defense = 12,
            int gold = 100)
        {
            // Create minimal mock repository
            var mockRepo = CreateMockRepository();

            // Use Warrior as default class enum value
            var player = new Player(name, PlayerClass.Warrior, mockRepo.Object);

            // Set stats directly
            player.Level = level;
            player.MaxHP = maxHP;
            player.CurrentHP = maxHP;
            player.MaxMana = maxMana;
            player.CurrentMana = maxMana;
            player.Attack = attack;
            player.Defense = defense;
            player.Gold = gold;

            return player;
        }

        /// <summary>
        /// Creates a warrior-like test player with high HP and defense.
        /// </summary>
        public static Player CreateWarriorPlayer(string name = "Test Warrior", int level = 1)
        {
            return CreateTestPlayer(name, level,
                maxHP: 140 + (level - 1) * 12,
                maxMana: 80 + (level - 1) * 5,
                attack: 14 + (level - 1) * 2,
                defense: 12 + (level - 1) * 2);
        }

        /// <summary>
        /// Creates a mage-like test player with high mana and magic power.
        /// </summary>
        public static Player CreateMagePlayer(string name = "Test Mage", int level = 1)
        {
            return CreateTestPlayer(name, level,
                maxHP: 90 + (level - 1) * 7,
                maxMana: 140 + (level - 1) * 12,
                attack: 7 + (level - 1) * 1,
                defense: 6 + (level - 1) * 1);
        }

        #endregion

        #region Equipment Creation

        /// <summary>
        /// Creates a test weapon with controlled stats.
        /// </summary>
        public static EquipmentItem CreateTestWeapon(
            string name = "Test Sword",
            int attack = 10,
            int defense = 0,
            AttackType? weaponAttackType = null,
            ItemRarity rarity = ItemRarity.Common,
            List<string>? grantedAbilityIds = null)
        {
            return new EquipmentItem
            {
                Name = name,
                Slot = EquipmentSlot.Weapon,
                Rarity = rarity,
                AttackBonus = attack,
                DefenseBonus = defense,
                HPBonus = 0,
                ManaBonus = 0,
                MagicBonus = 0,
                AgilityBonus = 0,
                WeaponAttackType = weaponAttackType,
                GrantedAbilityIds = grantedAbilityIds ?? new List<string>()
            };
        }

        /// <summary>
        /// Creates a test armor piece with controlled stats.
        /// </summary>
        public static EquipmentItem CreateTestArmor(
            string name = "Test Armor",
            EquipmentSlot slot = EquipmentSlot.Armor,
            int defense = 5,
            int hp = 10,
            ItemRarity rarity = ItemRarity.Common,
            List<string>? grantedAbilityIds = null)
        {
            return new EquipmentItem
            {
                Name = name,
                Slot = slot,
                Rarity = rarity,
                AttackBonus = 0,
                DefenseBonus = defense,
                HPBonus = hp,
                ManaBonus = 0,
                MagicBonus = 0,
                AgilityBonus = 0,
                GrantedAbilityIds = grantedAbilityIds ?? new List<string>()
            };
        }

        #endregion

        #region Ability Creation

        /// <summary>
        /// Creates a test ability with controlled properties.
        /// </summary>
        public static Ability CreateTestAbility(
            string id = "test_ability",
            string name = "Test Ability",
            string description = "A test ability",
            int manaCost = 20,
            int cooldown = 3,
            int unlockLevel = 1,
            AbilityType type = AbilityType.Physical)
        {
            var ability = new Ability(
                name,
                manaCost,
                cooldown,
                description,
                type,
                unlockLevel,
                purchaseCost: 0,
                id: id
            );

            // Mark as unlocked for testing
            ability.IsUnlocked = true;

            return ability;
        }

        #endregion

        #region Mock Data Repository

        /// <summary>
        /// Creates a mock IDataRepository with test data.
        /// </summary>
        public static Mock<IDataRepository> CreateMockRepository(
            List<EnemyData>? enemies = null,
            List<AbilityData>? abilities = null,
            List<ClassData>? classes = null)
        {
            var mockRepo = new Mock<IDataRepository>();

            // Setup enemy queries
            var enemyList = enemies ?? CreateDefaultTestEnemies();
            mockRepo.Setup(r => r.GetEnemiesByLevel(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(enemyList);
            mockRepo.Setup(r => r.GetEnemy(It.IsAny<string>()))
                .Returns<string>(id => enemyList.Find(e => e.Id == id) ?? CreateTestEnemyData());

            // Setup ability queries
            var abilityList = abilities ?? CreateDefaultTestAbilities();
            mockRepo.Setup(r => r.GetAbilitiesForClass(It.IsAny<string>()))
                .Returns(abilityList);
            mockRepo.Setup(r => r.GetAbility(It.IsAny<string>()))
                .Returns<string>(id => abilityList.Find(a => a.Id == id) ?? CreateDefaultTestAbilityData());

            // Setup class queries
            var classList = classes ?? CreateDefaultTestClasses();
            mockRepo.Setup(r => r.GetClass(It.IsAny<string>()))
                .Returns<string>(name => classList.Find(c => c.Name == name) ?? CreateDefaultTestClassData());

            return mockRepo;
        }

        #endregion

        #region Test Data Creation

        private static List<EnemyData> CreateDefaultTestEnemies()
        {
            return new List<EnemyData>
            {
                CreateTestEnemyData("test_enemy_1", "Test Enemy 1", minLevel: 1),
                CreateTestEnemyData("test_enemy_2", "Test Enemy 2", minLevel: 3),
                CreateTestEnemyData("test_boss", "Test Boss", minLevel: 5, isBoss: true)
            };
        }

        public static EnemyData CreateTestEnemyData(
            string id = "test_enemy",
            string name = "Test Enemy",
            int minLevel = 1,
            int maxHP = 100,
            int attack = 20,
            int defense = 10,
            bool isBoss = false,
            List<EnemyAbilityData>? abilities = null)
        {
            return new EnemyData
            {
                Id = id,
                Name = name,
                Type = "Beast",
                Level = 0, // Scaling enemy
                MaxHP = maxHP,
                Attack = attack,
                Defense = defense,
                Speed = 10,
                GoldReward = 50,
                ExpReward = 100,
                MinLevel = minLevel,
                IsBoss = isBoss,
                Abilities = abilities ?? new List<EnemyAbilityData>()
            };
        }

        /// <summary>
        /// Creates test enemy ability data (reference to an ability with threshold).
        /// </summary>
        public static EnemyAbilityData CreateTestEnemyAbilityData(
            string abilityId = "test_ability",
            int useThreshold = 100)
        {
            return new EnemyAbilityData
            {
                AbilityId = abilityId,
                UseThreshold = useThreshold
            };
        }

        private static List<AbilityData> CreateDefaultTestAbilities()
        {
            return new List<AbilityData>
            {
                CreateDefaultTestAbilityData("test_ability_1"),
                CreateDefaultTestAbilityData("test_ability_2")
            };
        }

        public static AbilityData CreateDefaultTestAbilityData(string id = "test_ability")
        {
            return new AbilityData
            {
                Id = id,
                Name = "Test Ability",
                Description = "A test ability",
                PlayerClass = "TestClass",
                ManaCost = 20,
                Cooldown = 3,
                Type = "Physical",
                UnlockLevel = 1,
                Effects = new List<AbilityEffectData>()
            };
        }

        private static List<ClassData> CreateDefaultTestClasses()
        {
            return new List<ClassData>
            {
                CreateDefaultTestClassData("Warrior"),
                CreateDefaultTestClassData("Mage"),
                CreateDefaultTestClassData("Rogue")
            };
        }

        private static ClassData CreateDefaultTestClassData(string name = "TestClass")
        {
            return new ClassData
            {
                Name = name,
                Description = $"Test {name} class",
                BaseMaxHP = 150,
                BaseMaxMana = 100,
                BaseAttack = 15,
                BaseDefense = 12,
                BaseMagicPower = 10,
                BaseSpeed = 10,
                BaseCritChance = 0.15,
                HPPerLevel = 10,
                ManaPerLevel = 8,
                AttackPerLevel = 2,
                DefensePerLevel = 2,
                MagicPowerPerLevel = 1,
                SpeedPerLevel = 1,
                StartingGold = 100,
                StartingPotions = 3,
                AbilityUnlockSchedule = new Dictionary<string, string>()
            };
        }

        #endregion

        #region Enemy Modifier Creation

        /// <summary>
        /// Creates test enemy prefix data with controlled multipliers.
        /// </summary>
        public static EnemyPrefixData CreateTestPrefix(
            string id = "test_prefix",
            string name = "Test Prefix",
            int minLevel = 1,
            double hpMultiplier = 1.3,
            double attackMultiplier = 1.2,
            double defenseMultiplier = 1.1,
            double speedMultiplier = 1.0,
            double goldMultiplier = 1.5,
            double expMultiplier = 1.5)
        {
            return new EnemyPrefixData
            {
                Id = id,
                Name = name,
                MinLevel = minLevel,
                HPMultiplier = hpMultiplier,
                AttackMultiplier = attackMultiplier,
                DefenseMultiplier = defenseMultiplier,
                SpeedMultiplier = speedMultiplier,
                GoldMultiplier = goldMultiplier,
                ExpMultiplier = expMultiplier
            };
        }

        /// <summary>
        /// Creates test enemy suffix data with controlled bonuses.
        /// </summary>
        public static EnemySuffixData CreateTestSuffix(
            string id = "test_suffix",
            string name = "the Tester",
            int minLevel = 1,
            List<string>? additionalAbilities = null,
            int goldBonus = 25,
            int expBonus = 50)
        {
            return new EnemySuffixData
            {
                Id = id,
                Name = name,
                MinLevel = minLevel,
                AdditionalAbilities = additionalAbilities ?? new List<string>(),
                GoldBonus = goldBonus,
                ExpBonus = expBonus
            };
        }

        /// <summary>
        /// Creates a list of test prefixes with varying level requirements.
        /// </summary>
        public static List<EnemyPrefixData> CreateTestPrefixes()
        {
            return new List<EnemyPrefixData>
            {
                CreateTestPrefix("weak_prefix", "Weak", minLevel: 1, hpMultiplier: 1.1, attackMultiplier: 1.1),
                CreateTestPrefix("strong_prefix", "Strong", minLevel: 5, hpMultiplier: 1.3, attackMultiplier: 1.3),
                CreateTestPrefix("elite_prefix", "Elite", minLevel: 10, hpMultiplier: 1.5, attackMultiplier: 1.5)
            };
        }

        /// <summary>
        /// Creates a list of test suffixes with varying level requirements.
        /// </summary>
        public static List<EnemySuffixData> CreateTestSuffixes()
        {
            return new List<EnemySuffixData>
            {
                CreateTestSuffix("basic_suffix", "the Basic", minLevel: 1, goldBonus: 10, expBonus: 20),
                CreateTestSuffix("advanced_suffix", "the Advanced", minLevel: 5,
                    additionalAbilities: new List<string> { "test_ability_1" }, goldBonus: 50, expBonus: 100),
                CreateTestSuffix("master_suffix", "the Master", minLevel: 10,
                    additionalAbilities: new List<string> { "test_ability_1", "test_ability_2" }, goldBonus: 100, expBonus: 200)
            };
        }

        #endregion
    }
}
