using TestRPGGame.UI;

namespace TestRPGGame.Abilities.Effects
{
    // Thorns effect - reflects damage back to attacker
    public class ThornsEffect : IAbilityEffect
    {
        public int ReflectDamage { get; set; }
        public int Duration { get; set; }

        public ThornsEffect(int reflectDamage, int duration)
        {
            ReflectDamage = reflectDamage;
            Duration = duration;
        }

        public void Execute(AbilityContext context)
        {
            // Thorns is handled via StatusEffects in combat
            UIHelper.PrintColoredLine($"🌵 Thorns active: {ReflectDamage} damage reflection for {Duration} turns!", ConsoleColor.Yellow);
        }

        public string GetDescription()
        {
            return $"Reflect {ReflectDamage} damage for {Duration} turns";
        }
    }
}
