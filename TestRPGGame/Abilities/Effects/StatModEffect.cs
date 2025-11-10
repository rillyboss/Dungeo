using System;
using TestRPGGame.UI;

namespace TestRPGGame.Abilities.Effects
{
    // Stat modification effects
    public class StatModEffect : IAbilityEffect
    {
        public string StatName { get; set; }
        public int Amount { get; set; }

        public StatModEffect(string statName, int amount)
        {
            StatName = statName;
            Amount = amount;
        }

        public void Execute(AbilityContext context)
        {
            if (context.Enemy == null) return;

            switch (StatName.ToLower())
            {
                case "speed":
                    context.Enemy.Speed = Math.Max(1, context.Enemy.Speed + Amount);
                    UIHelper.PrintColoredLine($"⚡ Enemy speed {(Amount > 0 ? "increased" : "decreased")}!", ConsoleColor.Cyan);
                    break;
            }
        }

        public string GetDescription()
        {
            return $"Modify {StatName} by {Amount}";
        }
    }
}
