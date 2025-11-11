using TestRPGGame.DataLoading;
using TestRPGGame.Utils;

namespace TestRPGGame.Equipment.StatGenerators
{
    public class RingStatGenerator : ISlotStatGenerator
    {
        public void GenerateStats(
            EquipmentItem item,
            int baseStat,
            double rarityMultiplier,
            WeaponPrefixData? weaponPrefix,
            WeaponTypeData? weaponType,
            ArmorPrefixData? armorPrefix)
        {
            // Rings have varied stats
            int statChoice = RandomProvider.Next(4);
            switch (statChoice)
            {
                case 0:
                    item.AttackBonus = (int)(baseStat * rarityMultiplier * 0.8);
                    break;
                case 1:
                    item.DefenseBonus = (int)(baseStat * rarityMultiplier * 0.8);
                    break;
                case 2:
                    item.MagicBonus = (int)(baseStat * rarityMultiplier * 0.8);
                    break;
                case 3:
                    item.HPBonus = (int)(baseStat * rarityMultiplier * 2.5);
                    break;
            }
        }
    }
}
