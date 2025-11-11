using System;
using TestRPGGame.Combat;
using TestRPGGame.DataLoading;
using TestRPGGame.Utils;

namespace TestRPGGame.Equipment.StatGenerators
{
    public class WeaponStatGenerator : ISlotStatGenerator
    {
        public void GenerateStats(
            EquipmentItem item,
            int baseStat,
            double rarityMultiplier,
            WeaponPrefixData? weaponPrefix,
            WeaponTypeData? weaponType,
            ArmorPrefixData? armorPrefix)
        {
            if (weaponPrefix != null && weaponType != null)
            {
                // Use data-driven weapon stats
                double randomVariance = 1.5 + RandomProvider.NextDouble() * 0.5;
                item.AttackBonus = (int)(baseStat * rarityMultiplier * weaponPrefix.AttackMultiplier * weaponType.AttackWeight * randomVariance);
                item.MagicBonus = (int)(baseStat * rarityMultiplier * weaponPrefix.MagicMultiplier * weaponType.MagicWeight);
                item.SpeedBonus = weaponType.SpeedBonus;

                // Set attack type from weapon type data
                if (Enum.TryParse<AttackType>(weaponType.AttackType, out var attackType))
                {
                    item.WeaponAttackType = attackType;
                }
            }
            else
            {
                // Fallback to legacy generation
                item.AttackBonus = (int)(baseStat * rarityMultiplier * (1.5 + RandomProvider.NextDouble() * 0.5));
                item.MagicBonus = RandomProvider.Next(2) == 0 ? (int)(baseStat * rarityMultiplier * 0.8) : 0;
                Array attackTypes = Enum.GetValues(typeof(AttackType));
                item.WeaponAttackType = (AttackType)attackTypes.GetValue(RandomProvider.Next(attackTypes.Length))!;
            }

            if (item.Rarity >= ItemRarity.Rare)
                item.CritBonus = 0.05 + (RandomProvider.NextDouble() * 0.15);
        }
    }
}
