using TestRPGGame.UI;

namespace TestRPGGame.Abilities.Effects
{
    /// <summary>
    /// Speed buff effect - temporarily increases speed for turn order advantage
    /// </summary>
    public class SpeedBuffEffect : IAbilityEffect
    {
        public int SpeedBonus { get; set; }
        public int Duration { get; set; }

        public SpeedBuffEffect(int speedBonus, int duration)
        {
            SpeedBonus = speedBonus;
            Duration = duration;
        }

        public void Execute(AbilityContext context)
        {
            // For player abilities, apply to player's StatusEffects
            if (context.IsPlayerAbility && context.Player.StatusEffects != null)
            {
                context.Player.StatusEffects.SpeedBuffValue = SpeedBonus;
                context.Player.StatusEffects.SpeedBuffTurns = Duration;
                UIHelper.PrintColoredLine($"⚡ {context.Player.Name}'s speed increased by {SpeedBonus} for {Duration} turns!", ConsoleColor.Yellow);
            }
            // For enemy abilities, apply to enemy's StatusEffects
            else if (!context.IsPlayerAbility && context.Enemy != null && context.Enemy.StatusEffects != null)
            {
                context.Enemy.StatusEffects.SpeedBuffValue = SpeedBonus;
                context.Enemy.StatusEffects.SpeedBuffTurns = Duration;
                UIHelper.PrintColoredLine($"⚡ {context.Enemy.Name}'s speed increased by {SpeedBonus} for {Duration} turns!", ConsoleColor.Yellow);
            }
        }

        public string GetDescription()
        {
            return $"Increase speed by {SpeedBonus} for {Duration} turns";
        }
    }
}
