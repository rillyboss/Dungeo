using System.Collections.Generic;
using TestRPGGame.Abilities.Effects;

namespace TestRPGGame.Abilities
{
    public class Ability
    {
        public string Name { get; set; }
        public int ManaCost { get; set; }
        public int Cooldown { get; set; }
        public int CurrentCooldown { get; set; }
        public string Description { get; set; }
        public AbilityType Type { get; set; }
        public List<IAbilityEffect> Effects { get; set; }
        public bool IsUnlocked { get; set; }
        public int UnlockLevel { get; set; }
        public int PurchaseCost { get; set; }
        /// <summary>
        /// If true, this ability always goes first regardless of speed
        /// </summary>
        public bool Priority { get; set; }

        public Ability(string name, int manaCost, int cooldown, string description, AbilityType type,
                      int unlockLevel = 1, int purchaseCost = 0)
        {
            Name = name;
            ManaCost = manaCost;
            Cooldown = cooldown;
            CurrentCooldown = 0;
            Description = description;
            Type = type;
            Effects = new List<IAbilityEffect>();
            IsUnlocked = unlockLevel <= 1 && purchaseCost <= 0; // Auto-unlock if level 1 and free
            UnlockLevel = unlockLevel;
            PurchaseCost = purchaseCost;
        }

        public bool CanUse(int currentMana)
        {
            return IsUnlocked && CurrentCooldown == 0 && currentMana >= ManaCost;
        }

        public void Use()
        {
            CurrentCooldown = Cooldown;
        }

        public void ReduceCooldown()
        {
            if (CurrentCooldown > 0)
            {
                CurrentCooldown--;
            }
        }

        public void Execute(AbilityContext context)
        {
            foreach (var effect in Effects)
            {
                effect.Execute(context);
            }
        }

        public bool CanUnlock(int playerLevel, int playerGold)
        {
            return !IsUnlocked && playerLevel >= UnlockLevel && playerGold >= PurchaseCost;
        }

        public void Unlock()
        {
            IsUnlocked = true;
        }
    }
}
