using TestRPGGame.Abilities.Effects;
using TestRPGGame.Combat.StatusEffects;
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
            if (context.Target == null) return;

            // Apply stun to target
            context.Target.ApplyStun(Duration);
            UIHelper.PrintColoredLine($"⚡ {context.Target.Name} is stunned for {Duration} turns!", ConsoleColor.Yellow);
        }

        public string GetDescription()
        {
            return $"Stun target for {Duration} turns";
        }
    }
}
