using TestRPGGame.UI;

namespace TestRPGGame.Abilities.Effects
{
    // Shield effect - absorbs damage
    public class ShieldEffect : IAbilityEffect
    {
        public int ShieldAmount { get; set; }
        public int Duration { get; set; }

        public ShieldEffect(int shieldAmount, int duration)
        {
            ShieldAmount = shieldAmount;
            Duration = duration;
        }

        public void Execute(AbilityContext context)
        {
            // Shield is handled via StatusEffects in combat
            UIHelper.PrintColoredLine($"🛡️  Shield active: Absorbs {ShieldAmount} damage for {Duration} turns!", ConsoleColor.Cyan);
        }

        public string GetDescription()
        {
            return $"Shield absorbing {ShieldAmount} damage for {Duration} turns";
        }
    }
}
