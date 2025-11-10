using TestRPGGame.UI;

namespace TestRPGGame.Abilities.Effects
{
    // Heal over time effect - regenerates HP each turn
    public class HealOverTimeEffect : IAbilityEffect
    {
        public int HealPerTurn { get; set; }
        public int Duration { get; set; }

        public HealOverTimeEffect(int healPerTurn, int duration)
        {
            HealPerTurn = healPerTurn;
            Duration = duration;
        }

        public void Execute(AbilityContext context)
        {
            // HoT is handled via StatusEffects in combat
            UIHelper.PrintColoredLine($"💚 Regeneration active: {HealPerTurn} HP per turn for {Duration} turns!", ConsoleColor.Green);
        }

        public string GetDescription()
        {
            return $"Heal {HealPerTurn} HP per turn for {Duration} turns";
        }
    }
}
