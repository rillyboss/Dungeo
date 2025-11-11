using TestRPGGame.DataLoading;
using TestRPGGame.Utils;

namespace TestRPGGame.Equipment.StatGenerators
{
    public class ArmorStatGenerator : ISlotStatGenerator
    {
        public void GenerateStats(
            EquipmentItem item,
            int baseStat,
            double rarityMultiplier,
            WeaponPrefixData? weaponPrefix,
            WeaponTypeData? weaponType,
            ArmorPrefixData? armorPrefix)
        {
            if (armorPrefix != null)
            {
                // Use data-driven armor stats
                double randomVariance = 1.5 + RandomProvider.NextDouble() * 0.5;
                item.DefenseBonus = (int)(baseStat * rarityMultiplier * armorPrefix.DefenseMultiplier * randomVariance);
                item.HPBonus = (int)(baseStat * rarityMultiplier * armorPrefix.HPMultiplier * 3);
            }
            else
            {
                // Fallback to legacy generation
                item.DefenseBonus = (int)(baseStat * rarityMultiplier * (1.5 + RandomProvider.NextDouble() * 0.5));
                item.HPBonus = (int)(baseStat * rarityMultiplier * 3);
            }
        }
    }
}
