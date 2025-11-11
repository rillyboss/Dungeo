using System;

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
            // Restore affects the source (caster heals/restores themselves)
            if (IsMana && context.Source is Entities.Player.Player player)
            {
                player.RestoreMana(Amount);
            }
            else
            {
                int actualHeal = Math.Min(Amount, context.Source.MaxHP - context.Source.CurrentHP);
                context.Source.CurrentHP += actualHeal;

                // Notify AI if enemy healed
                if (context.Source is Entities.Enemy.Enemy enemy && enemy.AI != null)
                {
                    enemy.AI.RecordHealUsed();
                }
            }

            // Note: Output is handled by the combat system through events
        }

        public string GetDescription()
        {
            return $"Restore {Amount} {(IsMana ? "mana" : "HP")}";
        }
    }
}
