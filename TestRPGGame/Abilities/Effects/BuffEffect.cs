using System;
using TestRPGGame.UI;

namespace TestRPGGame.Abilities.Effects
{
    // Buff effects
    public class BuffEffect : IAbilityEffect
    {
        public string BuffName { get; set; }
        public int Duration { get; set; }

        public BuffEffect(string buffName, int duration)
        {
            BuffName = buffName;
            Duration = duration;
        }

        public void Execute(AbilityContext context)
        {
            if (context.ActiveBuffs != null)
            {
                context.ActiveBuffs[BuffName] = Duration;
                UIHelper.PrintColoredLine($"✨ {BuffName} active for {Duration} turns!", ConsoleColor.Cyan);
            }
        }

        public string GetDescription()
        {
            return $"Apply {BuffName} for {Duration} turns";
        }
    }
}
