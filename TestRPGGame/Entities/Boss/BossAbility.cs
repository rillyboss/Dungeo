using System.Collections.Generic;

namespace TestRPGGame.Entities.Boss
{
    // Boss-specific ability effects
    public class BossAbility
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Cooldown { get; set; }
        public int CurrentCooldown { get; set; }

        // Support multiple effects per ability
        public List<BossAbilityEffect> Effects { get; set; }

        // Legacy single effect support for backward compatibility
        public BossAbilityEffect Effect
        {
            get => Effects.Count > 0 ? Effects[0] : new BossAbilityEffect(BossAbilityEffectType.HealOverTime, 0, 0, 1.0);
            set
            {
                if (Effects.Count == 0)
                    Effects.Add(value);
                else
                    Effects[0] = value;
            }
        }

        public BossAbility(string name, string description, int cooldown, BossAbilityEffect effect)
        {
            Name = name;
            Description = description;
            Cooldown = cooldown;
            CurrentCooldown = 0;
            Effects = new List<BossAbilityEffect> { effect };
        }

        public BossAbility(string name, string description, int cooldown, List<BossAbilityEffect> effects)
        {
            Name = name;
            Description = description;
            Cooldown = cooldown;
            CurrentCooldown = 0;
            Effects = effects ?? new List<BossAbilityEffect>();
        }

        public bool CanUse()
        {
            return CurrentCooldown == 0;
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
    }
}
