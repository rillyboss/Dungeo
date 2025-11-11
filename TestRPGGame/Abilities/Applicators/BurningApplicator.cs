using TestRPGGame.Abilities.Effects;
using TestRPGGame.UI;

namespace TestRPGGame.Abilities.Applicators
{
    /// <summary>
    /// Applies a Burning/Poison status effect (damage over time).
    /// This is an applicator that triggers the actual DamageOverTimeEffect status effect.
    /// </summary>
    public class BurningApplicator : IAbilityEffect
    {
        public int DamagePerTurn { get; set; }
        public int Duration { get; set; }
        public int InitialDamage { get; set; }

        public BurningApplicator(int damagePerTurn, int duration, int initialDamage = 0)
        {
            DamagePerTurn = damagePerTurn;
            Duration = duration;
            InitialDamage = initialDamage;
        }

        public void Execute(AbilityContext context)
        {
            if (context.Enemy == null) return;

            if (InitialDamage > 0)
            {
                context.Enemy.CurrentHP -= InitialDamage;
                UIHelper.PrintColoredLine($"💚 {InitialDamage} poison damage!", ConsoleColor.Green);
            }

            // Status effect applied by CombatSystem
            UIHelper.PrintColoredLine($"💚 Poison applied: {DamagePerTurn} damage per turn for {Duration} turns!", ConsoleColor.Green);
        }

        public string GetDescription()
        {
            return $"Apply poison: {DamagePerTurn} damage/turn for {Duration} turns";
        }
    }
}
