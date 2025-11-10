using TestRPGGame.UI;

namespace TestRPGGame.Abilities.Effects
{
    // Stun effect - prevents target from acting
    public class StunEffect : IAbilityEffect
    {
        public int Duration { get; set; }

        public StunEffect(int duration)
        {
            Duration = duration;
        }

        public void Execute(AbilityContext context)
        {
            // Stun is handled in combat loop via context
            UIHelper.PrintColoredLine($"⚡ Stunned for {Duration} turns!", ConsoleColor.Yellow);
        }

        public string GetDescription()
        {
            return $"Stun target for {Duration} turns";
        }
    }
}
