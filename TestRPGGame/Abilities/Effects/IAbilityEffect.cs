namespace TestRPGGame.Abilities.Effects
{
    // Interface for ability effects
    public interface IAbilityEffect
    {
        void Execute(AbilityContext context);
        string GetDescription();
    }
}
