using TestRPGGame.Abilities.Effects;
using TestRPGGame.Combat.StatusEffects;

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
            // Apply shield to source (caster shields themselves)
            context.Source.ApplyShield(Duration, ShieldAmount);

            // Note: Output is handled by the combat system through events
        }

        public string GetDescription()
        {
            return $"Shield absorbing {ShieldAmount} damage for {Duration} turns";
        }
    }
}
