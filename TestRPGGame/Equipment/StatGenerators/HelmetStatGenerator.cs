using TestRPGGame.DataLoading;
using TestRPGGame.Utils;

namespace TestRPGGame.Equipment.StatGenerators
{
    public class HelmetStatGenerator : ISlotStatGenerator
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
                item.DefenseBonus = (int)(baseStat * rarityMultiplier * armorPrefix.DefenseMultiplier * 0.7);
                item.HPBonus = (int)(baseStat * rarityMultiplier * armorPrefix.HPMultiplier * 2);
            }
            else
            {
                item.DefenseBonus = (int)(baseStat * rarityMultiplier * 0.7);
                item.HPBonus = (int)(baseStat * rarityMultiplier * 2);
            }
            item.ManaBonus = RandomProvider.Next(3) == 0 ? (int)(baseStat * rarityMultiplier * 1.5) : 0;
        }
    }
}
