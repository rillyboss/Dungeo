using TestRPGGame.Abilities.Effects;
using TestRPGGame.Combat.StatusEffects;

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

            // Note: Output is handled by the combat system through events
        }

        public string GetDescription()
        {
            return $"Stun target for {Duration} turns";
        }
    }
}
