using TestRPGGame.Abilities.Effects;
using TestRPGGame.Combat.StatusEffects;
using TestRPGGame.Constants;

namespace TestRPGGame.Abilities.Applicators
{
    /// <summary>
    /// OLD FORMAT: Applies named buff status effects like Battle Rage and Shield Wall.
    /// DEPRECATED: Use EffectApplicator for new abilities.
    /// Kept for backward compatibility with existing JSON data.
    /// </summary>
    public class BuffApplicator : IAbilityEffect
    {
        private string _buffName;
        public int Duration { get; set; }

        // Constructor accepting string for backward compatibility with JSON deserialization
        public BuffApplicator(string buffName, int duration)
        {
            _buffName = buffName;
            Duration = duration;
        }

        public void Execute(AbilityContext context)
        {
            // Apply buff to source (caster buffs themselves)
            // Map old names to StatusEffectFactory methods
            switch (_buffName)
            {
                case "Battle Rage":
                    context.Source.ApplyBattleRage(Duration);
                    break;
                case "Shield Wall":
                    context.Source.ApplyShieldWall(Duration);
                    break;
                case "Smoke Screen":
                    context.Source.ApplySmokeScreen(Duration);
                    break;
                default:
                    // Try to apply as a generic effect - some abilities might use descriptive names
                    // For now, default to Battle Rage for attack buffs, Shield Wall for defense
                    if (_buffName.ToLower().Contains("attack") || _buffName.ToLower().Contains("rage") ||
                        _buffName.ToLower().Contains("fury") || _buffName.ToLower().Contains("power"))
                    {
                        context.Source.ApplyBattleRage(Duration);
                    }
                    else
                    {
                        context.Source.ApplyShieldWall(Duration);
                    }
                    break;
            }

            // Note: Output is handled by the combat system through events
        }

        public string GetDescription()
        {
            return $"Apply {_buffName} for {Duration} turns";
        }
    }
}
