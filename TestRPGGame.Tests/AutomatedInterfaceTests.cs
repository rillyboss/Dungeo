using Xunit;
using TestRPGGame.Interfaces;
using TestRPGGame.Entities.Player;
using TestRPGGame.Equipment;
using System.Collections.Generic;
using System.Linq;

namespace TestRPGGame.Tests
{
    public class AutomatedInterfaceTests : TestBase
    {
        [Fact]
        public void AutomatedInterface_RequestSaveSlotSelection_ChoosesEmptySlot()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var slots = new List<InterfaceSaveSlotInfo>
            {
                new InterfaceSaveSlotInfo { SlotNumber = 1, IsEmpty = true },
                new InterfaceSaveSlotInfo { SlotNumber = 2, IsEmpty = false, Name = "Existing", Level = 5, Class = PlayerClass.Warrior },
                new InterfaceSaveSlotInfo { SlotNumber = 3, IsEmpty = true }
            };

            // Act
            var (slotNumber, isNewCharacter) = autoInterface.RequestSaveSlotSelection(slots);

            // Assert
            Assert.Equal(1, slotNumber); // Should choose first empty slot
            Assert.True(isNewCharacter);
        }

        [Fact]
        public void AutomatedInterface_RequestSaveSlotSelection_LoadsExistingWhenNoEmpty()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var slots = new List<InterfaceSaveSlotInfo>
            {
                new InterfaceSaveSlotInfo { SlotNumber = 1, IsEmpty = false, Name = "Hero1", Level = 3, Class = PlayerClass.Warrior },
                new InterfaceSaveSlotInfo { SlotNumber = 2, IsEmpty = false, Name = "Hero2", Level = 5, Class = PlayerClass.Mage },
                new InterfaceSaveSlotInfo { SlotNumber = 3, IsEmpty = false, Name = "Hero3", Level = 2, Class = PlayerClass.Rogue }
            };

            // Act
            var (slotNumber, isNewCharacter) = autoInterface.RequestSaveSlotSelection(slots);

            // Assert
            Assert.Equal(1, slotNumber); // Should load first save
            Assert.False(isNewCharacter);
        }

        [Fact]
        public void AutomatedInterface_RequestCharacterCreation_ReturnsDefault()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();

            // Act
            var (name, playerClass) = autoInterface.RequestCharacterCreation();

            // Assert
            Assert.Equal("AIHero", name);
            Assert.Equal(PlayerClass.Warrior, playerClass);
        }

        [Fact]
        public void AutomatedInterface_RequestMainMenuChoice_RestWhenLowHP()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();

            // Act
            var choice = autoInterface.RequestMainMenuChoice(
                playerName: "Test",
                level: 1,
                playerClass: PlayerClass.Warrior,
                currentHP: 30,
                maxHP: 100,
                gold: 100
            );

            // Assert
            Assert.Equal(MainMenuChoice.Rest, choice);
        }

        [Fact]
        public void AutomatedInterface_RequestMainMenuChoice_CombatWhenHealthy()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();

            // Act
            var choice = autoInterface.RequestMainMenuChoice(
                playerName: "Test",
                level: 1,
                playerClass: PlayerClass.Warrior,
                currentHP: 100,
                maxHP: 100,
                gold: 100
            );

            // Assert
            Assert.Equal(MainMenuChoice.Combat, choice);
        }

        [Fact]
        public void AutomatedInterface_RequestCombatAction_UsesPotionWhenLowHP()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var state = new CombatState
            {
                PlayerCurrentHP = 20,
                PlayerMaxHP = 100,
                PlayerCurrentMana = 50,
                PlayerMaxMana = 100,
                PlayerPotions = 3,
                AvailableAbilities = new List<AbilityInfo>()
            };

            // Act
            var action = autoInterface.RequestCombatAction(state);

            // Assert
            Assert.Equal(CombatActionType.UsePotion, action.ActionType);
        }

        [Fact]
        public void AutomatedInterface_RequestCombatAction_UsesAbilityWhenAvailable()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var state = new CombatState
            {
                PlayerCurrentHP = 80,
                PlayerMaxHP = 100,
                PlayerCurrentMana = 50,
                PlayerMaxMana = 100,
                PlayerPotions = 0,
                AvailableAbilities = new List<AbilityInfo>
                {
                    new AbilityInfo { Index = 0, Name = "Fireball", ManaCost = 20, CanUse = true, CurrentCooldown = 0 }
                }
            };

            // Act
            var action = autoInterface.RequestCombatAction(state);

            // Assert
            Assert.Equal(CombatActionType.UseAbility, action.ActionType);
            Assert.Equal(0, action.AbilityIndex);
        }

        [Fact]
        public void AutomatedInterface_RequestCombatAction_AttacksWhenNoAbilitiesOrPotions()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var state = new CombatState
            {
                PlayerCurrentHP = 80,
                PlayerMaxHP = 100,
                PlayerCurrentMana = 10,
                PlayerMaxMana = 100,
                PlayerPotions = 0,
                AvailableAbilities = new List<AbilityInfo>()
            };

            // Act
            var action = autoInterface.RequestCombatAction(state);

            // Assert
            Assert.Equal(CombatActionType.Attack, action.ActionType);
        }

        [Fact]
        public void AutomatedInterface_RequestShopAction_BuysAffordableItem()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var forSale = new List<ShopItemInfo>
            {
                new ShopItemInfo { Index = 0, Name = "Sword", Price = 50, Level = 1 },
                new ShopItemInfo { Index = 1, Name = "Shield", Price = 150, Level = 2 }
            };

            // Act
            var action = autoInterface.RequestShopAction(forSale, new List<EquipmentItem>(), playerGold: 100);

            // Assert
            Assert.Equal(ShopActionType.BuyItem, action.ActionType);
            Assert.NotNull(action.ItemIndex);
            Assert.Equal(0, action.ItemIndex.Value); // Should buy the affordable one
        }

        [Fact]
        public void AutomatedInterface_RequestShopAction_ExitsWhenNothingAffordable()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var forSale = new List<ShopItemInfo>
            {
                new ShopItemInfo { Index = 0, Name = "Sword", Price = 500, Level = 1 }
            };

            // Act
            var action = autoInterface.RequestShopAction(forSale, new List<EquipmentItem>(), playerGold: 10);

            // Assert
            Assert.Equal(ShopActionType.Exit, action.ActionType);
        }

        [Fact]
        public void AutomatedInterface_RequestInventoryAction_EquipsFirstItem()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var backpack = new List<EquipmentItem>
            {
                new EquipmentItem { Name = "Sword", Slot = EquipmentSlot.Weapon }
            };
            var equipped = new Dictionary<EquipmentSlot, EquipmentItem?>();

            // Act
            var action = autoInterface.RequestInventoryAction(backpack, equipped);

            // Assert
            Assert.Equal(InventoryActionType.EquipItem, action.ActionType);
            Assert.Equal(0, action.ItemIndex);
        }

        [Fact]
        public void AutomatedInterface_RequestInventoryAction_ExitsWhenBackpackEmpty()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var backpack = new List<EquipmentItem>();
            var equipped = new Dictionary<EquipmentSlot, EquipmentItem?>();

            // Act
            var action = autoInterface.RequestInventoryAction(backpack, equipped);

            // Assert
            Assert.Equal(InventoryActionType.Exit, action.ActionType);
        }

        [Fact]
        public void AutomatedInterface_RequestConfirmation_AlwaysReturnsTrue()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();

            // Act
            bool result = autoInterface.RequestConfirmation("Test message?");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AutomatedInterface_RequestEncounterChoice_ChoosesFirstOption()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var choices = new List<string>
            {
                "Safe choice",
                "Risky choice",
                "Dangerous choice"
            };

            // Act
            int selected = autoInterface.RequestEncounterChoice("What do you do?", choices);

            // Assert
            Assert.Equal(0, selected); // Should always choose first (safe) option
        }

        [Fact]
        public void AutomatedInterface_OnEvent_LogsEvents()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var gameEvent = new GameEvents.CharacterCreatedEvent
            {
                Name = "TestHero",
                Class = PlayerClass.Warrior,
                MaxHP = 100,
                MaxMana = 50,
                Attack = 15,
                Defense = 10
            };

            // Act
            autoInterface.OnEvent(gameEvent);
            var log = autoInterface.GetLog();

            // Assert
            Assert.Contains("TestHero", log);
            Assert.Contains("Warrior", log);
        }

        [Fact]
        public void AutomatedInterface_RequestDungeonSelection_ChoosesFirstAvailable()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var dungeons = new List<DungeonSelectionInfo>
            {
                new DungeonSelectionInfo { Index = 0, Name = "Easy Dungeon", CanEnter = false },
                new DungeonSelectionInfo { Index = 1, Name = "Medium Dungeon", CanEnter = true },
                new DungeonSelectionInfo { Index = 2, Name = "Hard Dungeon", CanEnter = true }
            };

            // Act
            int selected = autoInterface.RequestDungeonSelection(dungeons);

            // Assert
            Assert.Equal(1, selected); // Should choose first available
        }

        [Fact]
        public void AutomatedInterface_RequestAbilityUnlock_ChoosesFirstUnlockable()
        {
            // Arrange
            var autoInterface = new AutomatedInterface();
            var abilities = new List<AbilityInfo>
            {
                new AbilityInfo { Index = 0, Name = "Expensive", UnlockLevel = 1, PurchaseCost = 1000 },
                new AbilityInfo { Index = 1, Name = "Affordable", UnlockLevel = 1, PurchaseCost = 50 }
            };

            // Act
            int selected = autoInterface.RequestAbilityUnlock(abilities, playerGold: 100, playerLevel: 1);

            // Assert
            Assert.Equal(1, selected); // Should choose affordable one
        }
    }
}
