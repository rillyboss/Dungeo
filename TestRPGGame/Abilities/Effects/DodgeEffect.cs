using System;
using TestRPGGame.UI;

namespace TestRPGGame.Abilities.Effects
{
    // Dodge effect
    public class DodgeEffect : IAbilityEffect
    {
        public void Execute(AbilityContext context)
        {
            // This needs to be handled specially by the combat system
            UIHelper.PrintColoredLine("👤 Next attack will be dodged!", ConsoleColor.DarkGray);
        }

        public string GetDescription()
        {
            return "Dodge next attack";
        }
    }
}
