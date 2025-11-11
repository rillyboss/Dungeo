using TestRPGGame.Abilities.Effects;
using TestRPGGame.Combat.StatusEffects;

namespace TestRPGGame.Abilities.Applicators
{
    /// <summary>
    /// Applies a Regeneration status effect (heal over time).
    /// This is an applicator that triggers the actual HealOverTimeEffect status effect.
    /// </summary>
    public class RegenerationApplicator : IAbilityEffect
    {
        public int HealPerTurn { get; set; }
        public int Duration { get; set; }

        public RegenerationApplicator(int healPerTurn, int duration)
        {
            HealPerTurn = healPerTurn;
            Duration = duration;
        }

        public void Execute(AbilityContext context)
        {
            // Apply regeneration to source (caster heals themselves)
            context.Source.ApplyRegeneration(Duration, HealPerTurn);

            // Note: Output is handled by the combat system through events
        }

        public string GetDescription()
        {
            return $"Heal {HealPerTurn} HP per turn for {Duration} turns";
        }
    }
}
