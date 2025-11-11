using System;
using System.Collections.Generic;
using TestRPGGame.Abilities;
using TestRPGGame.Combat;
using TestRPGGame.DataLoading;
using TestRPGGame.Equipment;
using TestRPGGame.Factories;
using TestRPGGame.Systems;


namespace TestRPGGame.Entities.Player
{
    public class Player : Combatant
    {
        // Name inherited from Combatant
        public PlayerClass Class { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int ExperienceToNextLevel { get; set; }

        // Core Stats
        // MaxHP, CurrentHP, Attack, Defense, Speed inherited from Combatant
        public int MaxMana { get; set; }
        public int CurrentMana { get; set; }
        public int MagicPower { get; set; }
        public double CritChance { get; set; }

        // Resources
        public int Gold { get; set; }
        public int PotionCount { get; set; }

        // Equipment - NEW SYSTEM
        public PlayerInventory Inventory { get; set; }

        // Abilities
        public List<Ability> Abilities { get; set; }

        // StatusEffects (old) and Effects (new) inherited from Combatant

        // Base stats (without equipment)
        private int BaseMaxHP { get; set; }
        private int BaseMaxMana { get; set; }
        private int BaseAttack { get; set; }
        private int BaseDefense { get; set; }
        private int BaseMagicPower { get; set; }
        private int BaseSpeed { get; set; }
        private double BaseCritChance { get; set; }

        public Player(string name, PlayerClass playerClass)
        {
            Name = name;
            Class = playerClass;
            Level = 1;
            Experience = 0;
            ExperienceToNextLevel = 100;
            Inventory = new PlayerInventory();
            Abilities = new List<Ability>();
            // StatusEffects (old) and Effects (new) initialized by base Combatant constructor

            InitializeFromClassData();
            InitializeAbilities();
            InitializeStartingEquipment();
            UpdateStatsFromEquipment();
        }

        private void InitializeFromClassData()
        {
            // Load class configuration from data
            var classData = DataLoader.GetClass(Class.ToString());

            // Set base stats from class data
            BaseMaxHP = classData.BaseMaxHP;
            BaseMaxMana = classData.BaseMaxMana;
            BaseAttack = classData.BaseAttack;
            BaseDefense = classData.BaseDefense;
            BaseMagicPower = classData.BaseMagicPower;
            BaseSpeed = classData.BaseSpeed;
            BaseCritChance = classData.BaseCritChance;

            // Set starting resources
            Gold = classData.StartingGold;
            PotionCount = classData.StartingPotions;

            // Set current stats (will be updated after equipment is added)
            MaxHP = BaseMaxHP;
            MaxMana = BaseMaxMana;
            Attack = BaseAttack;
            Defense = BaseDefense;
            MagicPower = BaseMagicPower;
            Speed = BaseSpeed;
            CritChance = BaseCritChance;

            // Don't set CurrentHP/CurrentMana yet - wait until after equipment is added
        }

        private void InitializeStartingEquipment()
        {
            // Give each class randomized low-tier starting equipment
            // Force Common rarity for starting gear to ensure fair starts
            var weapon = EquipmentGenerator.GenerateItem(1, EquipmentSlot.Weapon, ItemRarity.Common);
            var armor = EquipmentGenerator.GenerateItem(1, EquipmentSlot.Armor, ItemRarity.Common);

            // Filter to appropriate weapon types for each class
            weapon = EnsureClassAppropriateWeapon(weapon);

            // Equip the starting gear directly
            Inventory.Weapon = weapon;
            Inventory.Armor = armor;
        }

        private EquipmentItem EnsureClassAppropriateWeapon(EquipmentItem weapon)
        {
            // Regenerate weapon until we get an appropriate type for the class
            // This ensures warriors/rogues get physical weapons, mages get magical weapons
            int maxAttempts = 10;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                bool isAppropriate = Class switch
                {
                    PlayerClass.Warrior => weapon.WeaponAttackType == Combat.AttackType.Physical,
                    PlayerClass.Mage => weapon.WeaponAttackType != Combat.AttackType.Physical && weapon.WeaponAttackType != null,
                    PlayerClass.Rogue => weapon.WeaponAttackType == Combat.AttackType.Physical,
                    _ => true
                };

                if (isAppropriate)
                {
                    return weapon;
                }

                weapon = EquipmentGenerator.GenerateItem(1, EquipmentSlot.Weapon);
                attempts++;
            }

            // If we can't find appropriate after max attempts, just return what we have
            return weapon;
        }

        private void InitializeAbilities()
        {
            // Load abilities from data - code has zero knowledge of specific abilities
            var classAbilities = DataLoader.GetAbilitiesForClass(Class.ToString());

            foreach (var abilityData in classAbilities)
            {
                var ability = EntityFactory.CreateAbility(abilityData);
                Abilities.Add(ability);
            }
        }

        public bool GainExperience(int exp)
        {
            Experience += exp;
            if (Experience >= ExperienceToNextLevel)
            {
                LevelUp();
                return true;
            }
            return false;
        }

        private void LevelUp()
        {
            Level++;
            Experience -= ExperienceToNextLevel;
            ExperienceToNextLevel = (int)(ExperienceToNextLevel * 1.5);

            // Load class data for stat growth
            var classData = DataLoader.GetClass(Class.ToString());

            // Apply data-driven stat increases
            BaseMaxHP += classData.HPPerLevel;
            BaseMaxMana += classData.ManaPerLevel;
            BaseAttack += classData.AttackPerLevel;
            BaseDefense += classData.DefensePerLevel;
            BaseMagicPower += classData.MagicPowerPerLevel;
            BaseSpeed += classData.SpeedPerLevel;

            // Recalculate with equipment
            UpdateStatsFromEquipment();

            CurrentHP = MaxHP;
            CurrentMana = MaxMana;
        }

        public void UpdateStatsFromEquipment()
        {
            var equipStats = Inventory.GetTotalStats();

            MaxHP = BaseMaxHP + equipStats.HP;
            MaxMana = BaseMaxMana + equipStats.Mana;
            Attack = BaseAttack + equipStats.Attack;
            Defense = BaseDefense + equipStats.Defense;
            MagicPower = BaseMagicPower + equipStats.Magic;
            Speed = BaseSpeed + equipStats.Speed;
            CritChance = BaseCritChance + equipStats.Crit;

            // Initialize CurrentHP and CurrentMana if they're not set yet (new character)
            if (CurrentHP == 0)
            {
                CurrentHP = MaxHP;
            }
            if (CurrentMana == 0)
            {
                CurrentMana = MaxMana;
            }
        }

        public AttackType GetWeaponAttackType()
        {
            if (Inventory.Weapon != null && Inventory.Weapon.WeaponAttackType.HasValue)
            {
                return Inventory.Weapon.WeaponAttackType.Value;
            }
            return AttackType.Physical; // Default to physical
        }

        // GetTotalAttack and GetTotalDefense inherited from Combatant
        // Player's stats already include equipment from UpdateStatsFromEquipment
        public override int GetTotalAttack()
        {
            return Attack; // Already includes equipment from UpdateStatsFromEquipment
        }

        public override int GetTotalDefense()
        {
            return Defense; // Already includes equipment from UpdateStatsFromEquipment
        }

        public int GetTotalMagicPower()
        {
            return MagicPower; // Already includes equipment from UpdateStatsFromEquipment
        }

        // Heal method inherited from Combatant base class

        public void RestoreMana(int amount)
        {
            CurrentMana = Math.Min(CurrentMana + amount, MaxMana);
        }

        public bool UsePotion()
        {
            if (PotionCount > 0)
            {
                PotionCount--;
                int healAmount = (int)(MaxHP * GameConfig.Config.PotionHealPercent);
                Heal(healAmount);
                return true;
            }
            return false;
        }

        public void DisplayCharacterSheet()
        {
            Console.Clear();
            Console.WriteLine("═══════════════════════════════════════════");
            Console.WriteLine("          CHARACTER SHEET");
            Console.WriteLine("═══════════════════════════════════════════\n");

            Console.WriteLine($"Name: {Name}");
            Console.WriteLine($"Class: {Class}");
            Console.WriteLine($"Level: {Level}");
            Console.WriteLine($"Experience: {Experience}/{ExperienceToNextLevel}\n");

            var equipStats = Inventory.GetTotalStats();

            Console.WriteLine("╔════════════ STATS ════════════╗");
            Console.WriteLine($"  ❤️  HP: {CurrentHP}/{MaxHP} ({BaseMaxHP} + {equipStats.HP})");
            Console.WriteLine($"  💙 Mana: {CurrentMana}/{MaxMana} ({BaseMaxMana} + {equipStats.Mana})");
            Console.WriteLine($"  ⚔️  Attack: {Attack} ({BaseAttack} + {equipStats.Attack})");
            Console.WriteLine($"  🛡️  Defense: {Defense} ({BaseDefense} + {equipStats.Defense})");
            Console.WriteLine($"  🔮 Magic: {MagicPower} ({BaseMagicPower} + {equipStats.Magic})");
            Console.WriteLine($"  ⚡ Speed: {Speed} ({BaseSpeed} + {equipStats.Speed})");
            Console.WriteLine($"  💥 Crit Chance: {CritChance:P0}");
            Console.WriteLine("╚═══════════════════════════════╝\n");

            Console.WriteLine("╔════════════ RESOURCES ════════════╗");
            Console.WriteLine($"  💰 Gold: {Gold}");
            Console.WriteLine($"  🧪 Potions: {PotionCount}");
            Console.WriteLine("╚═══════════════════════════════════╝\n");

            Console.WriteLine("╔════════════ ABILITIES ════════════╗");
            foreach (var ability in Abilities)
            {
                if (ability.IsUnlocked)
                {
                    Console.Write($"  ✓ {ability.Name}");
                    Console.WriteLine($" (Cost: {ability.ManaCost} mana, CD: {ability.Cooldown})");
                    Console.WriteLine($"    {ability.Description}");
                }
                else
                {
                    Console.Write($"  🔒 {ability.Name}");
                    Console.WriteLine($" - Unlock at Level {ability.UnlockLevel} for {ability.PurchaseCost} gold");
                }
            }
            Console.WriteLine("╚═══════════════════════════════════╝");
        }

        public void ResetForNewBattle()
        {
            foreach (var ability in Abilities)
            {
                ability.CurrentCooldown = 0;
            }

            // Reset status effects for new battle
            Effects.ClearAll();
        }
    }
}
