using System;
using System.Collections.Generic;
using System.Linq;
using TestRPGGame.Entities.Player;
using TestRPGGame.DataLoading;
using TestRPGGame.Equipment;
using TestRPGGame.Interfaces;

namespace TestRPGGame.Systems
{
    public class Shop
    {
        private List<EquipmentItem> shopInventory;
        private Random random = new Random();
        private IGameInterface gameInterface;

        public Shop(IGameInterface gameInterface)
        {
            this.gameInterface = gameInterface;
            shopInventory = new List<EquipmentItem>();
        }

        public void Enter(Player player)
        {
            // Generate shop inventory if empty or refresh
            RefreshShopInventory(player.Level);

            // Notify that shop was entered
            gameInterface.OnEvent(new GameEvents.ShopEnteredEvent
            {
                AvailableItems = BuildShopItemList()
            });

            bool shopping = true;

            while (shopping)
            {
                // Build shop state
                var shopItems = BuildShopItemInfoList();

                // REQUEST decision from interface
                var action = gameInterface.RequestShopAction(
                    forSale: shopItems,
                    inventory: player.Inventory.BackpackItems,
                    playerGold: player.Gold
                );

                // EXECUTE business logic based on action
                switch (action.ActionType)
                {
                    case ShopActionType.BuyItem:
                        if (action.ItemIndex.HasValue)
                        {
                            ProcessPurchase(player, action.ItemIndex.Value);
                        }
                        break;

                    case ShopActionType.SellItem:
                        if (action.ItemIndex.HasValue)
                        {
                            ProcessSale(player, action.ItemIndex.Value);
                        }
                        break;

                    case ShopActionType.RefreshShop:
                        ProcessRefresh(player);
                        break;

                    case ShopActionType.BuyPotion:
                        if (action.Quantity.HasValue)
                        {
                            ProcessPotionPurchase(player, action.Quantity.Value);
                        }
                        break;

                    case ShopActionType.Exit:
                        shopping = false;
                        break;
                }
            }
        }

        private void RefreshShopInventory(int playerLevel)
        {
            shopInventory.Clear();

            // Generate 8-12 random items
            int itemCount = 8 + random.Next(5);

            for (int i = 0; i < itemCount; i++)
            {
                EquipmentItem item = EquipmentGenerator.GenerateItem(playerLevel);
                shopInventory.Add(item);
            }
        }

        private List<ShopItemInfo> BuildShopItemInfoList()
        {
            return shopInventory.Select((item, index) => new ShopItemInfo
            {
                Index = index,
                Name = item.Name,
                Type = item.Slot.GetDisplayName(),
                Rarity = item.Rarity.ToString(),
                Price = item.Price,
                Level = item.Level,
                Item = item
            }).ToList();
        }

        private List<GameEvents.ShopItemInfo> BuildShopItemList()
        {
            return shopInventory.Select(item => new GameEvents.ShopItemInfo
            {
                Name = item.Name,
                Type = item.Slot.GetDisplayName(),
                Rarity = item.Rarity.ToString(),
                Price = item.Price,
                Level = item.Level
            }).ToList();
        }

        private void ProcessPurchase(Player player, int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= shopInventory.Count)
            {
                gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = "Invalid item selection!",
                    Type = GameEvents.MessageType.Error
                });
                return;
            }

            var item = shopInventory[itemIndex];

            if (player.Gold >= item.Price)
            {
                player.Gold -= item.Price;
                player.Inventory.BackpackItems.Add(item);
                shopInventory.RemoveAt(itemIndex);

                gameInterface.OnEvent(new GameEvents.ItemPurchasedEvent
                {
                    ItemName = item.Name,
                    Price = item.Price,
                    GoldRemaining = player.Gold
                });
            }
            else
            {
                int needed = item.Price - player.Gold;
                gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = $"Not enough gold! You have {player.Gold}, need {needed} more gold.",
                    Type = GameEvents.MessageType.Error
                });
            }
        }

        private void ProcessSale(Player player, int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= player.Inventory.BackpackItems.Count)
            {
                gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = "Invalid item selection!",
                    Type = GameEvents.MessageType.Error
                });
                return;
            }

            var item = player.Inventory.BackpackItems[itemIndex];
            int sellPrice = (int)(item.Price * 0.6); // Sell for 60% of buy price

            player.Gold += sellPrice;
            player.Inventory.BackpackItems.RemoveAt(itemIndex);

            gameInterface.OnEvent(new GameEvents.ItemSoldEvent
            {
                ItemName = item.Name,
                Price = sellPrice,
                GoldRemaining = player.Gold
            });
        }

        private void ProcessRefresh(Player player)
        {
            const int REFRESH_COST = 50;

            if (player.Gold >= REFRESH_COST)
            {
                player.Gold -= REFRESH_COST;
                RefreshShopInventory(player.Level);

                gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = "Shop inventory refreshed!",
                    Type = GameEvents.MessageType.Success
                });
            }
            else
            {
                gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = $"Not enough gold! Need {REFRESH_COST} gold.",
                    Type = GameEvents.MessageType.Error
                });
            }
        }

        private void ProcessPotionPurchase(Player player, int quantity)
        {
            const int POTION_PRICE = 50;

            if (quantity <= 0)
            {
                gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = "Invalid quantity!",
                    Type = GameEvents.MessageType.Error
                });
                return;
            }

            int totalCost = quantity * POTION_PRICE;

            if (player.Gold >= totalCost)
            {
                player.Gold -= totalCost;
                player.PotionCount += quantity;

                gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = $"Purchased {quantity} potion(s)!",
                    Type = GameEvents.MessageType.Success
                });
            }
            else
            {
                gameInterface.OnEvent(new GameEvents.InfoMessageEvent
                {
                    Message = "Not enough gold!",
                    Type = GameEvents.MessageType.Error
                });
            }
        }
    }
}
