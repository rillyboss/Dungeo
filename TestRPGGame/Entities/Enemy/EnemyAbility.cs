namespace TestRPGGame.Entities.Enemy
{
    public class EnemyAbility
    {
        public string Name { get; set; }
        public double DamageMultiplier { get; set; }
        public int Cooldown { get; set; }
        public int CurrentCooldown { get; set; }

        public EnemyAbility(string name, double damageMultiplier, int cooldown)
        {
            Name = name;
            DamageMultiplier = damageMultiplier;
            Cooldown = cooldown;
            CurrentCooldown = 0;
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
