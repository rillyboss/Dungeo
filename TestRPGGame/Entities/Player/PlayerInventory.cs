using System;
using System.Collections.Generic;
using System.Linq;
using TestRPGGame.Equipment;
using TestRPGGame.Interfaces;

namespace TestRPGGame.Entities.Player
{
    public class PlayerInventory
    {
        // Internal dictionary-based slot management
        private readonly Dictionary<EquipmentSlot, EquipmentItem?> _slots = new()
        {
            { EquipmentSlot.Weapon, null },
            { EquipmentSlot.Armor, null },
            { EquipmentSlot.Helmet, null },
            { EquipmentSlot.Boots, null },
            { EquipmentSlot.Gloves, null },
            { EquipmentSlot.Ring1, null },
            { EquipmentSlot.Ring2, null },
            { EquipmentSlot.Amulet, null },
            { EquipmentSlot.Relic, null }
        };

        // Public properties that wrap dictionary access (preserve API compatibility)
        public EquipmentItem? Weapon
        {
            get => _slots[EquipmentSlot.Weapon];
            set => _slots[EquipmentSlot.Weapon] = value;
        }

        public EquipmentItem? Armor
        {
            get => _slots[EquipmentSlot.Armor];
            set => _slots[EquipmentSlot.Armor] = value;
        }

        public EquipmentItem? Helmet
        {
            get => _slots[EquipmentSlot.Helmet];
            set => _slots[EquipmentSlot.Helmet] = value;
        }

        public EquipmentItem? Boots
        {
            get => _slots[EquipmentSlot.Boots];
            set => _slots[EquipmentSlot.Boots] = value;
        }

        public EquipmentItem? Gloves
        {
            get => _slots[EquipmentSlot.Gloves];
            set => _slots[EquipmentSlot.Gloves] = value;
        }

        public EquipmentItem? Ring1
        {
            get => _slots[EquipmentSlot.Ring1];
            set => _slots[EquipmentSlot.Ring1] = value;
        }

        public EquipmentItem? Ring2
        {
            get => _slots[EquipmentSlot.Ring2];
            set => _slots[EquipmentSlot.Ring2] = value;
        }

        public EquipmentItem? Amulet
        {
            get => _slots[EquipmentSlot.Amulet];
            set => _slots[EquipmentSlot.Amulet] = value;
        }

        public EquipmentItem? Relic
        {
            get => _slots[EquipmentSlot.Relic];
            set => _slots[EquipmentSlot.Relic] = value;
        }

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

        public Dictionary<EquipmentSlot, EquipmentItem?> GetEquippedItems()
        {
            // Return a copy of the internal dictionary
            return new Dictionary<EquipmentSlot, EquipmentItem?>(_slots);
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

            // Special handling for ring slots (can use either Ring1 or Ring2)
            if (item.Slot == EquipmentSlot.Ring1 || item.Slot == EquipmentSlot.Ring2)
            {
                if (_slots[EquipmentSlot.Ring1] == null)
                {
                    _slots[EquipmentSlot.Ring1] = item;
                }
                else if (_slots[EquipmentSlot.Ring2] == null)
                {
                    _slots[EquipmentSlot.Ring2] = item;
                }
                else
                {
                    // Both ring slots full, replace based on original slot preference
                    if (item.Slot == EquipmentSlot.Ring1)
                    {
                        unequipped = _slots[EquipmentSlot.Ring1];
                        _slots[EquipmentSlot.Ring1] = item;
                    }
                    else
                    {
                        unequipped = _slots[EquipmentSlot.Ring2];
                        _slots[EquipmentSlot.Ring2] = item;
                    }
                }
            }
            else
            {
                // For all other slots, simple dictionary-based swap
                if (!_slots.ContainsKey(item.Slot))
                {
                    gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                    {
                        Message = "Invalid equipment slot!",
                        Type = GameEvents.MessageType.Error
                    });
                    BackpackItems.Insert(itemIndex, item); // Put item back
                    return;
                }

                unequipped = _slots[item.Slot];
                _slots[item.Slot] = item;
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
            // Dictionary-based approach: no switch statement needed
            if (!_slots.ContainsKey(slot))
            {
                gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = "Invalid equipment slot!",
                    Type = GameEvents.MessageType.Error
                });
                return;
            }

            var item = _slots[slot];

            if (item != null)
            {
                _slots[slot] = null;
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

            // Iterate through all equipped items in dictionary
            foreach (var item in _slots.Values)
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

            // Iterate through all equipped items in dictionary
            foreach (var item in _slots.Values)
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
