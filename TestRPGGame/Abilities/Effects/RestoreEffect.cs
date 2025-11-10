using System;
using TestRPGGame.UI;

namespace TestRPGGame.Abilities.Effects
{
    // Healing/Restoration effects
    public class RestoreEffect : IAbilityEffect
    {
        public int Amount { get; set; }
        public bool IsMana { get; set; }

        public RestoreEffect(int amount, bool isMana = false)
        {
            Amount = amount;
            IsMana = isMana;
        }

        public void Execute(AbilityContext context)
        {
            if (IsMana)
            {
                context.Player.RestoreMana(Amount);
                UIHelper.PrintColoredLine($"💙 Restored {Amount} mana!", ConsoleColor.Blue);
            }
            else
            {
                context.Player.Heal(Amount);
                UIHelper.PrintColoredLine($"❤️  Restored {Amount} HP!", ConsoleColor.Green);
            }
        }

        public string GetDescription()
        {
            return $"Restore {Amount} {(IsMana ? "mana" : "HP")}";
        }
    }
}
