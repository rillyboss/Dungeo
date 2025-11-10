namespace TestRPGGame.Equipment
{
    public enum EquipmentSlot
    {
        Weapon,
        Armor,
        Helmet,
        Boots,
        Gloves,
        Ring1,
        Ring2,
        Amulet,
        Relic
    }

    public static class EquipmentSlotExtensions
    {
        public static string GetDisplayName(this EquipmentSlot slot)
        {
            return slot switch
            {
                EquipmentSlot.Ring1 => "Ring",
                EquipmentSlot.Ring2 => "Ring",
                _ => slot.ToString()
            };
        }
    }
}
