using TestRPGGame.Abilities.Effects;
using TestRPGGame.UI;

namespace TestRPGGame.Abilities.Applicators
{
    /// <summary>
    /// Applies a Shield status effect that absorbs damage.
    /// This is an applicator that triggers the actual ShieldEffect status effect.
    /// </summary>
    public class ShieldApplicator : IAbilityEffect
    {
        public int ShieldAmount { get; set; }
        public int Duration { get; set; }

        public ShieldApplicator(int shieldAmount, int duration)
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
