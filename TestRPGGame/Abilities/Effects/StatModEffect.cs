using System;
using TestRPGGame.Constants;
using TestRPGGame.UI;

namespace TestRPGGame.Abilities.Effects
{
    /// <summary>
    /// Stat modification effects - temporarily modify target's stats
    /// </summary>
    public class StatModEffect : IAbilityEffect
    {
        public StatType StatType { get; set; }
        public int Amount { get; set; }

        // Constructor accepting enum
        public StatModEffect(StatType statType, int amount)
        {
            StatType = statType;
            Amount = amount;
        }

        // Constructor accepting string for backward compatibility with JSON deserialization
        public StatModEffect(string statName, int amount)
        {
            StatType = StatTypeExtensions.FromIdentifier(statName);
            Amount = amount;
        }

        public void Execute(AbilityContext context)
        {
            if (context.Target == null) return;

            switch (StatType)
            {
                case Constants.StatType.Speed:
                    context.Target.Speed = Math.Max(1, context.Target.Speed + Amount);
                    UIHelper.PrintColoredLine($"⚡ {context.Target.Name}'s speed {(Amount > 0 ? "increased" : "decreased")}!", ConsoleColor.Cyan);
                    break;
                case Constants.StatType.Attack:
                    context.Target.Attack = Math.Max(1, context.Target.Attack + Amount);
                    UIHelper.PrintColoredLine($"⚔️  {context.Target.Name}'s attack {(Amount > 0 ? "increased" : "decreased")}!", ConsoleColor.Red);
                    break;
                case Constants.StatType.Defense:
                    context.Target.Defense = Math.Max(0, context.Target.Defense + Amount);
                    UIHelper.PrintColoredLine($"🛡️  {context.Target.Name}'s defense {(Amount > 0 ? "increased" : "decreased")}!", ConsoleColor.Blue);
                    break;
                // HP and MagicPower typically modified through other systems, not direct stat mods
            }
        }

        public string GetDescription()
        {
            return $"Modify {StatType} by {Amount}";
        }
    }
}
