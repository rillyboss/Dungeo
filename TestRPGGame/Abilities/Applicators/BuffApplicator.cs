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
            // Apply buff using new status effect system
            if (context.Player != null)
            {
                if (BuffName == "Battle Rage")
                {
                    context.Player.ApplyBattleRage(Duration);
                }
                else if (BuffName == "Shield Wall")
                {
                    context.Player.ApplyShieldWall(Duration);
                }
                UIHelper.PrintColoredLine($"✨ {BuffName} active for {Duration} turns!", ConsoleColor.Cyan);
            }
        }

        public string GetDescription()
        {
            return $"Apply {BuffName} for {Duration} turns";
        }
    }
}
