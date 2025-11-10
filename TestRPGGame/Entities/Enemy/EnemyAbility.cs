using TestRPGGame.Abilities;

namespace TestRPGGame.Entities.Enemy
{
    /// <summary>
    /// Wrapper for enemy abilities. Enemies use the same Ability system as players
    /// but with simpler AI (no mana requirements).
    /// </summary>
    public class EnemyAbility
    {
        public Ability Ability { get; set; }
        public int UseThreshold { get; set; } // HP% threshold to use this ability (0-100)

        public EnemyAbility(Ability ability, int useThreshold = 100)
        {
            Ability = ability;
            UseThreshold = useThreshold;
        }

        public bool CanUse(int currentHP, int maxHP)
        {
            int hpPercent = (int)((currentHP / (double)maxHP) * 100);
            return Ability.CurrentCooldown == 0 && hpPercent <= UseThreshold;
        }

        public void Use()
        {
            Ability.Use();
        }

        public void ReduceCooldown()
        {
            Ability.ReduceCooldown();
        }
    }
}
