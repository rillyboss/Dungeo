using TestRPGGame.Abilities.Effects;
using TestRPGGame.Combat.StatusEffects;
using TestRPGGame.Constants;

namespace TestRPGGame.Abilities.Applicators
{
    /// <summary>
    /// Applies named buff status effects like Battle Rage and Shield Wall.
    /// This is an applicator that triggers actual stat modifier status effects.
    /// </summary>
    public class BuffApplicator : IAbilityEffect
    {
        public BuffType BuffType { get; set; }
        public int Duration { get; set; }

        // Constructor accepting enum
        public BuffApplicator(BuffType buffType, int duration)
        {
            BuffType = buffType;
            Duration = duration;
        }

        // Constructor accepting string for backward compatibility with JSON deserialization
        public BuffApplicator(string buffName, int duration)
        {
            BuffType = ParseBuffName(buffName);
            Duration = duration;
        }

        public void Execute(AbilityContext context)
        {
            // Apply buff to source (caster buffs themselves)
            switch (BuffType)
            {
                case Constants.BuffType.BattleRage:
                    context.Source.ApplyBattleRage(Duration);
                    break;
                case Constants.BuffType.ShieldWall:
                    context.Source.ApplyShieldWall(Duration);
                    break;
            }

            // Note: Output is handled by the combat system through events
        }

        public string GetDescription()
        {
            return $"Apply {BuffType.GetDisplayName()} for {Duration} turns";
        }

        private static BuffType ParseBuffName(string buffName)
        {
            return buffName switch
            {
                "Battle Rage" => Constants.BuffType.BattleRage,
                "Shield Wall" => Constants.BuffType.ShieldWall,
                _ => throw new System.ArgumentException($"Unknown buff name: {buffName}")
            };
        }
    }
}
