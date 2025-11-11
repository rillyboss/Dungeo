using TestRPGGame.Abilities.Effects;
using TestRPGGame.Combat.StatusEffects;

namespace TestRPGGame.Abilities.Applicators
{
    /// <summary>
    /// Applies a Thorns status effect that reflects damage back to attackers.
    /// This is an applicator that triggers the actual ThornsEffect status effect.
    /// </summary>
    public class ThornsApplicator : IAbilityEffect
    {
        public int ReflectDamage { get; set; }
        public int Duration { get; set; }

        public ThornsApplicator(int reflectDamage, int duration)
        {
            ReflectDamage = reflectDamage;
            Duration = duration;
        }

        public void Execute(AbilityContext context)
        {
            // Apply thorns to source (caster protects themselves)
            context.Source.ApplyThorns(Duration, ReflectDamage);

            // Note: Output is handled by the combat system through events
        }

        public string GetDescription()
        {
            return $"Reflect {ReflectDamage} damage for {Duration} turns";
        }
    }
}
