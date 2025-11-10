using System;
using System.Collections.Generic;
using TestRPGGame.Abilities;
using TestRPGGame.Combat;
using TestRPGGame.DataLoading;
using TestRPGGame.Factories;
using TestRPGGame.Systems;
using TestRPGGame.UI;

namespace TestRPGGame.Entities.Player
{
    public class Player
    {
        public string Name { get; set; }
        public PlayerClass Class { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int ExperienceToNextLevel { get; set; }

        // Core Stats
        public int MaxHP { get; set; }
        public int CurrentHP { get; set; }
        public int MaxMana { get; set; }
        public int CurrentMana { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int MagicPower { get; set; }
        public int Speed { get; set; }
        public double CritChance { get; set; }

        // Resources
        public int Gold { get; set; }
        public int PotionCount { get; set; }

        // Equipment - NEW SYSTEM
        public PlayerInventory Inventory { get; set; }

        // Abilities
        public List<Ability> Abilities { get; set; }

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
            Gold = 100;
            PotionCount = 3;
            Inventory = new PlayerInventory();
            Abilities = new List<Ability>();

            InitializeStats();
            InitializeAbilities();
            UpdateStatsFromEquipment();
        }

        private void InitializeStats()
        {
            switch (Class)
            {
                case PlayerClass.Warrior:
                    BaseMaxHP = 150;
                    BaseMaxMana = 80;
                    BaseAttack = 15;
                    BaseDefense = 12;
                    BaseMagicPower = 5;
                    BaseSpeed = 8;
                    BaseCritChance = 0.15;
                    break;

                case PlayerClass.Mage:
                    BaseMaxHP = 80;
                    BaseMaxMana = 120;
                    BaseAttack = 8;
                    BaseDefense = 6;
                    BaseMagicPower = 20;
                    BaseSpeed = 10;
                    BaseCritChance = 0.20;
                    break;

                case PlayerClass.Rogue:
                    BaseMaxHP = 100;
                    BaseMaxMana = 70;
                    BaseAttack = 18;
                    BaseDefense = 8;
                    BaseMagicPower = 8;
                    BaseSpeed = 16;
                    BaseCritChance = 0.35;
                    break;
            }

            // Set current stats
            MaxHP = BaseMaxHP;
            MaxMana = BaseMaxMana;
            Attack = BaseAttack;
            Defense = BaseDefense;
            MagicPower = BaseMagicPower;
            Speed = BaseSpeed;
            CritChance = BaseCritChance;

            CurrentHP = MaxHP;
            CurrentMana = MaxMana;
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

            // Stat increases based on class - update BASE stats
            switch (Class)
            {
                case PlayerClass.Warrior:
                    BaseMaxHP += 25;
                    BaseMaxMana += 5;
                    BaseAttack += 3;
                    BaseDefense += 3;
                    BaseMagicPower += 1;
                    BaseSpeed += 1;
                    break;

                case PlayerClass.Mage:
                    BaseMaxHP += 12;
                    BaseMaxMana += 15;
                    BaseAttack += 1;
                    BaseDefense += 2;
                    BaseMagicPower += 4;
                    BaseSpeed += 2;
                    break;

                case PlayerClass.Rogue:
                    BaseMaxHP += 15;
                    BaseMaxMana += 8;
                    BaseAttack += 4;
                    BaseDefense += 1;
                    BaseMagicPower += 1;
                    BaseSpeed += 3;
                    BaseCritChance += 0.02;
                    break;
            }

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
        }

        public AttackType GetWeaponAttackType()
        {
            if (Inventory.Weapon != null && Inventory.Weapon.WeaponAttackType.HasValue)
            {
                return Inventory.Weapon.WeaponAttackType.Value;
            }
            return AttackType.Physical; // Default to physical
        }

        public int GetTotalAttack()
        {
            return Attack; // Already includes equipment from UpdateStatsFromEquipment
        }

        public int GetTotalDefense()
        {
            return Defense; // Already includes equipment from UpdateStatsFromEquipment
        }

        public int GetTotalMagicPower()
        {
            return MagicPower; // Already includes equipment from UpdateStatsFromEquipment
        }

        public void Heal(int amount)
        {
            CurrentHP = Math.Min(CurrentHP + amount, MaxHP);
        }

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
            UIHelper.PrintColoredLine("═══════════════════════════════════════════", ConsoleColor.Cyan);
            UIHelper.PrintColoredLine("          CHARACTER SHEET", ConsoleColor.Yellow);
            UIHelper.PrintColoredLine("═══════════════════════════════════════════\n", ConsoleColor.Cyan);

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
                    UIHelper.PrintColored($"  ✓ {ability.Name}", ConsoleColor.Green);
                    Console.WriteLine($" (Cost: {ability.ManaCost} mana, CD: {ability.Cooldown})");
                    Console.WriteLine($"    {ability.Description}");
                }
                else
                {
                    UIHelper.PrintColored($"  🔒 {ability.Name}", ConsoleColor.DarkGray);
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
        }
    }
}
