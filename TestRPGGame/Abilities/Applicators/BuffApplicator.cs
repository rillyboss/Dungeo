using TestRPGGame.Abilities.Effects;
using TestRPGGame.Combat.StatusEffects;
using TestRPGGame.UI;

namespace TestRPGGame.Abilities.Applicators
{
    /// <summary>
    /// Applies named buff status effects like Battle Rage and Shield Wall.
    /// This is an applicator that triggers actual stat modifier status effects.
    /// </summary>
    public class BuffApplicator : IAbilityEffect
    {
        public string BuffName { get; set; }
        public int Duration { get; set; }

        public BuffApplicator(string buffName, int duration)
        {
            BuffName = buffName;
            Duration = duration;
        }

        public void Execute(AbilityContext context)
        {
            // Apply buff to source (caster buffs themselves)
            if (BuffName == "Battle Rage")
            {
                context.Source.ApplyBattleRage(Duration);
            }
            else if (BuffName == "Shield Wall")
            {
                context.Source.ApplyShieldWall(Duration);
            }
            UIHelper.PrintColoredLine($"✨ {context.Source.Name} empowered by {BuffName} for {Duration} turns!", ConsoleColor.Cyan);
        }

        public string GetDescription()
        {
            return $"Apply {BuffName} for {Duration} turns";
        }
    }
}
