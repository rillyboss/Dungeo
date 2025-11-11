using TestRPGGame.Abilities.Effects;
using TestRPGGame.Combat.StatusEffects;
using TestRPGGame.UI;

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
            UIHelper.PrintColoredLine($"💚 {context.Source.Name} begins regenerating! ({HealPerTurn} HP/turn for {Duration} turns)", ConsoleColor.Green);
        }

        public string GetDescription()
        {
            return $"Heal {HealPerTurn} HP per turn for {Duration} turns";
        }
    }
}
