using TestRPGGame.DataLoading;

namespace TestRPGGame.Equipment.StatGenerators
{
    public class GlovesStatGenerator : ISlotStatGenerator
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
                item.DefenseBonus = (int)(baseStat * rarityMultiplier * armorPrefix.DefenseMultiplier * 0.4);
            }
            else
            {
                item.DefenseBonus = (int)(baseStat * rarityMultiplier * 0.4);
            }
            item.AttackBonus = (int)(baseStat * rarityMultiplier * 0.6);
            item.SpeedBonus = (int)(baseStat * rarityMultiplier * 0.3);
        }
    }
}
