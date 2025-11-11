using System;

namespace TestRPGGame.Abilities.Effects
{
    // Dodge effect
    public class DodgeEffect : IAbilityEffect
    {
        public void Execute(AbilityContext context)
        {
            // This needs to be handled specially by the combat system
            // Note: Output is handled by the combat system through events
        }

        public string GetDescription()
        {
            return "Dodge next attack";
        }
    }
}
