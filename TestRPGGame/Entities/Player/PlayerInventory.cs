using System;
using System.Collections.Generic;
using System.Linq;
using TestRPGGame.Equipment;
using TestRPGGame.Interfaces;

namespace TestRPGGame.Entities.Player
{
    public class PlayerInventory
    {
        // Equipment slots
        public EquipmentItem? Weapon { get; set; }
        public EquipmentItem? Armor { get; set; }
        public EquipmentItem? Helmet { get; set; }
        public EquipmentItem? Boots { get; set; }
        public EquipmentItem? Gloves { get; set; }
        public EquipmentItem? Ring1 { get; set; }
        public EquipmentItem? Ring2 { get; set; }
        public EquipmentItem? Amulet { get; set; }
        public EquipmentItem? Relic { get; set; }

        public List<EquipmentItem> BackpackItems { get; set; }

        public PlayerInventory()
        {
            BackpackItems = new List<EquipmentItem>();
        }

        public void ManageInventory(Player player, IGameInterface gameInterface)
        {
            bool managing = true;

            while (managing)
            {
                // Build equipped items dictionary
                var equipped = GetEquippedItems();

                // REQUEST action from interface
                var action = gameInterface.RequestInventoryAction(BackpackItems, equipped);

                // EXECUTE business logic based on action
                switch (action.ActionType)
                {
                    case InventoryActionType.EquipItem:
                        if (action.ItemIndex.HasValue)
                        {
                            ProcessEquip(player, action.ItemIndex.Value, gameInterface);
                        }
                        break;

                    case InventoryActionType.UnequipItem:
                        if (action.Slot.HasValue)
                        {
                            ProcessUnequip(action.Slot.Value, gameInterface);
                        }
                        break;

                    case InventoryActionType.ViewDetails:
                        if (action.ItemIndex.HasValue)
                        {
                            ProcessViewDetails(action.ItemIndex.Value, gameInterface);
                        }
                        break;

                    case InventoryActionType.Exit:
                        managing = false;
                        break;
                }
            }
        }

        private Dictionary<EquipmentSlot, EquipmentItem?> GetEquippedItems()
        {
            return new Dictionary<EquipmentSlot, EquipmentItem?>
            {
                { EquipmentSlot.Weapon, Weapon },
                { EquipmentSlot.Armor, Armor },
                { EquipmentSlot.Helmet, Helmet },
                { EquipmentSlot.Boots, Boots },
                { EquipmentSlot.Gloves, Gloves },
                { EquipmentSlot.Ring1, Ring1 },
                { EquipmentSlot.Ring2, Ring2 },
                { EquipmentSlot.Amulet, Amulet },
                { EquipmentSlot.Relic, Relic }
            };
        }

        private void ProcessEquip(Player player, int itemIndex, IGameInterface gameInterface)
        {
            if (itemIndex < 0 || itemIndex >= BackpackItems.Count)
            {
                gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = "Invalid item selection!",
                    Type = GameEvents.MessageType.Error
                });
                return;
            }

            var item = BackpackItems[itemIndex];
            BackpackItems.RemoveAt(itemIndex);

            EquipmentItem? unequipped = null;

            switch (item.Slot)
            {
                case EquipmentSlot.Weapon:
                    unequipped = Weapon;
                    Weapon = item;
                    break;
                case EquipmentSlot.Armor:
                    unequipped = Armor;
                    Armor = item;
                    break;
                case EquipmentSlot.Helmet:
                    unequipped = Helmet;
                    Helmet = item;
                    break;
                case EquipmentSlot.Boots:
                    unequipped = Boots;
                    Boots = item;
                    break;
                case EquipmentSlot.Gloves:
                    unequipped = Gloves;
                    Gloves = item;
                    break;
                case EquipmentSlot.Ring1:
                    if (Ring1 == null)
                    {
                        Ring1 = item;
                    }
                    else if (Ring2 == null)
                    {
                        Ring2 = item;
                    }
                    else
                    {
                        // Both slots full, replace Ring1
                        unequipped = Ring1;
                        Ring1 = item;
                    }
                    break;
                case EquipmentSlot.Ring2:
                    if (Ring1 == null)
                    {
                        Ring1 = item;
                    }
                    else if (Ring2 == null)
                    {
                        Ring2 = item;
                    }
                    else
                    {
                        unequipped = Ring2;
                        Ring2 = item;
                    }
                    break;
                case EquipmentSlot.Amulet:
                    unequipped = Amulet;
                    Amulet = item;
                    break;
                case EquipmentSlot.Relic:
                    unequipped = Relic;
                    Relic = item;
                    break;
            }

            if (unequipped != null)
            {
                BackpackItems.Add(unequipped);
            }

            // Update player stats
            player.UpdateStatsFromEquipment();

            gameInterface.OnEvent(new GameEvents.ItemEquippedEvent
            {
                ItemName = item.Name,
                Slot = item.Slot.GetDisplayName()
            });
        }

        private void ProcessUnequip(EquipmentSlot slot, IGameInterface gameInterface)
        {
            EquipmentItem? item = null;

            switch (slot)
            {
                case EquipmentSlot.Weapon:
                    item = Weapon;
                    Weapon = null;
                    break;
                case EquipmentSlot.Armor:
                    item = Armor;
                    Armor = null;
                    break;
                case EquipmentSlot.Helmet:
                    item = Helmet;
                    Helmet = null;
                    break;
                case EquipmentSlot.Boots:
                    item = Boots;
                    Boots = null;
                    break;
                case EquipmentSlot.Gloves:
                    item = Gloves;
                    Gloves = null;
                    break;
                case EquipmentSlot.Ring1:
                    item = Ring1;
                    Ring1 = null;
                    break;
                case EquipmentSlot.Ring2:
                    item = Ring2;
                    Ring2 = null;
                    break;
                case EquipmentSlot.Amulet:
                    item = Amulet;
                    Amulet = null;
                    break;
                case EquipmentSlot.Relic:
                    item = Relic;
                    Relic = null;
                    break;
            }

            if (item != null)
            {
                BackpackItems.Add(item);
                gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = $"Unequipped {item.Name}",
                    Type = GameEvents.MessageType.Success
                });
            }
            else
            {
                gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = "No item equipped in that slot!",
                    Type = GameEvents.MessageType.Error
                });
            }
        }

        private void ProcessViewDetails(int itemIndex, IGameInterface gameInterface)
        {
            if (itemIndex >= 0 && itemIndex < BackpackItems.Count)
            {
                // For viewing details, we'll need to add proper event support later
                // For now, items have their own DisplayDetails method
            }
        }

        public (int Attack, int Defense, int Magic, int HP, int Mana, int Speed, double Crit) GetTotalStats()
        {
            int attack = 0, defense = 0, magic = 0, hp = 0, mana = 0, speed = 0;
            double crit = 0;

            var allEquipment = new[] { Weapon, Armor, Helmet, Boots, Gloves, Ring1, Ring2, Amulet, Relic };

            foreach (var item in allEquipment)
            {
                if (item != null)
                {
                    attack += item.AttackBonus;
                    defense += item.DefenseBonus;
                    magic += item.MagicBonus;
                    hp += item.HPBonus;
                    mana += item.ManaBonus;
                    speed += item.SpeedBonus;
                    crit += item.CritBonus;

                    // Add passive bonuses from special effects
                    foreach (var effect in item.SpecialEffects)
                    {
                        if (effect.Type == EffectType.AttackBonus && effect.ProcChance >= 1.0)
                            attack += effect.Value;
                        if (effect.Type == EffectType.DefenseBonus && effect.ProcChance >= 1.0)
                            defense += effect.Value;
                        if (effect.Type == EffectType.SpeedBonus && effect.ProcChance >= 1.0)
                            speed += effect.Value;
                        if (effect.Type == EffectType.CritBonus && effect.ProcChance >= 1.0)
                            crit += effect.Value / 100.0;
                    }
                }
            }

            return (attack, defense, magic, hp, mana, speed, crit);
        }

        public List<SpecialEffect> GetAllSpecialEffects()
        {
            var effects = new List<SpecialEffect>();
            var allEquipment = new[] { Weapon, Armor, Helmet, Boots, Gloves, Ring1, Ring2, Amulet, Relic };

            foreach (var item in allEquipment)
            {
                if (item != null)
                {
                    effects.AddRange(item.SpecialEffects);
                }
            }

            return effects;
        }
    }
}
