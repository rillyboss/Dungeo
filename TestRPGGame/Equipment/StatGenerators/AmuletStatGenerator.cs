using TestRPGGame.DataLoading;
using TestRPGGame.Utils;

namespace TestRPGGame.Equipment.StatGenerators
{
    public class AmuletStatGenerator : ISlotStatGenerator
    {
        public void GenerateStats(
            EquipmentItem item,
            int baseStat,
            double rarityMultiplier,
            WeaponPrefixData? weaponPrefix,
            WeaponTypeData? weaponType,
            ArmorPrefixData? armorPrefix)
        {
            item.HPBonus = (int)(baseStat * rarityMultiplier * 2);
            item.ManaBonus = (int)(baseStat * rarityMultiplier * 2);
            if (item.Rarity >= ItemRarity.Rare)
                item.CritBonus = 0.03 + (RandomProvider.NextDouble() * 0.1);
        }
    }
}
