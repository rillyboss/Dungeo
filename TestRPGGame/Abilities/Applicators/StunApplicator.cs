using TestRPGGame.Abilities.Effects;
using TestRPGGame.UI;

namespace TestRPGGame.Abilities.Applicators
{
    /// <summary>
    /// Applies a Stun status effect that prevents the target from acting.
    /// This is an applicator that triggers the actual StunEffect status effect.
    /// </summary>
    public class StunApplicator : IAbilityEffect
    {
        public int Duration { get; set; }

        public StunApplicator(int duration)
        {
            Duration = duration;
        }

        public void Execute(AbilityContext context)
        {
            // Stun is handled in combat loop via StatusEffects
            UIHelper.PrintColoredLine($"⚡ Stunned for {Duration} turns!", ConsoleColor.Yellow);
        }

        public string GetDescription()
        {
            return $"Stun target for {Duration} turns";
        }
    }
}
