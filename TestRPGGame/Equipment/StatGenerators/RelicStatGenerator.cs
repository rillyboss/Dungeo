using TestRPGGame.DataLoading;

namespace TestRPGGame.Equipment.StatGenerators
{
    public class RelicStatGenerator : ISlotStatGenerator
    {
        public void GenerateStats(
            EquipmentItem item,
            int baseStat,
            double rarityMultiplier,
            WeaponPrefixData? weaponPrefix,
            WeaponTypeData? weaponType,
            ArmorPrefixData? armorPrefix)
        {
            // Relics are magical and provide varied bonuses
            item.MagicBonus = (int)(baseStat * rarityMultiplier);
            item.ManaBonus = (int)(baseStat * rarityMultiplier * 2.5);
            item.AttackBonus = (int)(baseStat * rarityMultiplier * 0.4);
        }
    }
}
