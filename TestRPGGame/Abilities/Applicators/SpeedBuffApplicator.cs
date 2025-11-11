using TestRPGGame.Abilities.Effects;
using TestRPGGame.Combat.StatusEffects;

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
            // Apply speed buff to source (caster buffs themselves)
            context.Source.ApplySpeedBuff(Duration, SpeedBonus);

            // Note: Output is handled by the combat system through events
        }

        public string GetDescription()
        {
            return $"Increase speed by {SpeedBonus} for {Duration} turns";
        }
    }
}
