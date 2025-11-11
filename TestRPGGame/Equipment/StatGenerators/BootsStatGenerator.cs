using TestRPGGame.DataLoading;

namespace TestRPGGame.Equipment.StatGenerators
{
    public class BootsStatGenerator : ISlotStatGenerator
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
                item.DefenseBonus = (int)(baseStat * rarityMultiplier * armorPrefix.DefenseMultiplier * 0.5);
            }
            else
            {
                item.DefenseBonus = (int)(baseStat * rarityMultiplier * 0.5);
            }
            item.SpeedBonus = (int)(baseStat * rarityMultiplier * 0.6);
        }
    }
}
