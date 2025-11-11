using TestRPGGame.DataLoading;

namespace TestRPGGame.Equipment
{
    /// <summary>
    /// Strategy interface for generating equipment stats based on slot type.
    /// Each slot (Weapon, Armor, Helmet, etc.) has its own stat generation logic.
    /// </summary>
    public interface ISlotStatGenerator
    {
        /// <summary>
        /// Generates stats for an equipment item based on its level, rarity, and item data.
        /// </summary>
        /// <param name="item">The equipment item to populate with stats</param>
        /// <param name="baseStat">Base stat value (typically level * 2)</param>
        /// <param name="rarityMultiplier">Rarity multiplier from item data</param>
        /// <param name="weaponPrefix">Weapon prefix data (null for non-weapons)</param>
        /// <param name="weaponType">Weapon type data (null for non-weapons)</param>
        /// <param name="armorPrefix">Armor prefix data (null for non-armor)</param>
        void GenerateStats(
            EquipmentItem item,
            int baseStat,
            double rarityMultiplier,
            WeaponPrefixData? weaponPrefix,
            WeaponTypeData? weaponType,
            ArmorPrefixData? armorPrefix);
    }
}
