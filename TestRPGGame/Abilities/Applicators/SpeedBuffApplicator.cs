using TestRPGGame.Abilities.Effects;
using TestRPGGame.Combat.StatusEffects;
using TestRPGGame.UI;

namespace TestRPGGame.Abilities.Applicators
{
    /// <summary>
    /// Applies a Speed Buff status effect that increases speed stat.
    /// This is an applicator that triggers the actual SpeedBuffEffect status effect.
    /// </summary>
    public class SpeedBuffApplicator : IAbilityEffect
    {
        public int SpeedBonus { get; set; }
        public int Duration { get; set; }

        public SpeedBuffApplicator(int speedBonus, int duration)
        {
            SpeedBonus = speedBonus;
            Duration = duration;
        }

        public void Execute(AbilityContext context)
        {
            // Speed buff is handled via StatusEffects in combat
            if (context.Player != null)
            {
                context.Player.ApplySpeedBuff(Duration, SpeedBonus);
                UIHelper.PrintColoredLine($"⚡ Speed increased by {SpeedBonus} for {Duration} turns!", ConsoleColor.Cyan);
            }
        }

        public string GetDescription()
        {
            return $"Increase speed by {SpeedBonus} for {Duration} turns";
        }
    }
}
