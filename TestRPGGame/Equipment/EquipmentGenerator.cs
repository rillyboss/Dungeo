using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TestRPGGame.DataLoading;
using TestRPGGame.Combat;
using TestRPGGame.Utils;
using TestRPGGame.Equipment.StatGenerators;
using TestRPGGame.Systems;

namespace TestRPGGame.Equipment
{
    public static class EquipmentGenerator
    {
        private static ItemGenerationData? _itemData;
        private static EquipmentAbilityPools? _abilityPools;
        private static IDataRepository _repository = new JsonDataRepository(); // Default repository

        // Strategy pattern: Dictionary dispatch instead of switch statements
        private static readonly Dictionary<EquipmentSlot, ISlotStatGenerator> _statGenerators = new()
        {
            { EquipmentSlot.Weapon, new WeaponStatGenerator() },
            { EquipmentSlot.Armor, new ArmorStatGenerator() },
            { EquipmentSlot.Helmet, new HelmetStatGenerator() },
            { EquipmentSlot.Boots, new BootsStatGenerator() },
            { EquipmentSlot.Gloves, new GlovesStatGenerator() },
            { EquipmentSlot.Ring1, new RingStatGenerator() },
            { EquipmentSlot.Ring2, new RingStatGenerator() },
            { EquipmentSlot.Amulet, new AmuletStatGenerator() },
            { EquipmentSlot.Relic, new RelicStatGenerator() }
        };

        /// <summary>
        /// Sets the data repository. Use for dependency injection (e.g., mock repository for tests).
        /// </summary>
        public static void SetRepository(IDataRepository repository)
        {
            _repository = repository;
        }

        // Legacy arrays for non-weapon/armor slots (Helmet, Boots, Gloves, Rings, Amulets, Relics)
        private static readonly string[] helmetTypes = { "Helmet", "Helm", "Crown", "Circlet", "Hood", "Cap" };
        private static readonly string[] bootsTypes = { "Boots", "Greaves", "Shoes", "Sabatons", "Slippers" };
        private static readonly string[] glovesTypes = { "Gloves", "Gauntlets", "Mitts", "Handguards", "Bracers" };

        private static readonly string[] ringPrefixes = { "Simple", "Fine", "Ornate", "Ancient", "Mystical", "Arcane", "Ethereal", "Celestial" };
        private static readonly string[] ringTypes = { "Ring", "Band", "Loop", "Circle" };

        private static readonly string[] amuletPrefixes = { "Simple", "Ornate", "Ancient", "Mystical", "Sacred", "Blessed", "Cursed", "Eternal" };
        private static readonly string[] amuletTypes = { "Amulet", "Pendant", "Talisman", "Medallion", "Charm" };

        private static readonly string[] relicNames = { "Orb", "Crystal", "Rune", "Totem", "Idol", "Artifact", "Essence", "Shard" };

        private static void EnsureDataLoaded()
        {
            if (_itemData == null)
            {
                _itemData = _repository.GetItemGenerationData();
            }

            if (_abilityPools == null)
            {
                try
                {
                    string abilityDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Items", "equipment-abilities.json");
                    if (File.Exists(abilityDataPath))
                    {
                        string json = File.ReadAllText(abilityDataPath);
                        _abilityPools = System.Text.Json.JsonSerializer.Deserialize<EquipmentAbilityPools>(json,
                            new System.Text.Json.JsonSerializerOptions
                            {
                                ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
                                PropertyNameCaseInsensitive = true
                            });
                    }
                }
                catch
                {
                    // If ability data can't be loaded, equipment just won't grant abilities
                    _abilityPools = new EquipmentAbilityPools();
                }
            }
        }

        public static EquipmentItem GenerateItem(int playerLevel, EquipmentSlot? forceSlot = null, ItemRarity? forceRarity = null)
        {
            EnsureDataLoaded();
            var item = new EquipmentItem();

            // Determine slot
            if (forceSlot.HasValue)
            {
                item.Slot = forceSlot.Value;
            }
            else
            {
                Array slotValues = Enum.GetValues(typeof(EquipmentSlot));
                item.Slot = (EquipmentSlot)slotValues.GetValue(RandomProvider.Next(slotValues.Length))!;
            }

            // Determine level (can be +/- 1 from player level)
            item.Level = Math.Max(1, playerLevel + RandomProvider.Next(-1, 2));

            // Determine rarity (higher level = better chance for rare items)
            item.Rarity = forceRarity ?? DetermineRarity(item.Level);

            // Generate name and get data for stats generation
            var (name, weaponPrefix, weaponType, weaponSuffix, armorPrefix, armorSuffix) = GenerateNameAndData(item.Slot, item.Rarity);
            item.Name = name;

            // Generate stats based on slot, level, rarity, and item data
            GenerateStats(item, weaponPrefix, weaponType, armorPrefix);

            // Generate special effects based on rarity and suffix data
            GenerateSpecialEffects(item, weaponSuffix, armorSuffix);

            // Generate equipment-granted abilities (signature + random for rare items)
            GenerateAbilities(item, weaponType);

            // Calculate price
            item.Price = CalculatePrice(item);

            return item;
        }

        private static ItemRarity DetermineRarity(int level)
        {
            // Use GameConfig for configurable rarity thresholds
            int roll = RandomProvider.Next(100);

            // Check rarities in order from highest to lowest
            // Legendary (min level 10)
            if (level >= 10 && roll >= GameConfig.Config.RarityLegendaryThreshold)
            {
                return ItemRarity.Legendary;
            }

            // Epic (min level 7)
            if (level >= 7 && roll >= GameConfig.Config.RarityEpicThreshold)
            {
                return ItemRarity.Epic;
            }

            // Rare (check special low-level rare first - very rare at levels 1-3)
            if (level < 4 && roll >= 95)
            {
                return ItemRarity.Rare;
            }

            // Rare (normal - min level 4)
            if (level >= 4 && roll >= GameConfig.Config.RarityRareThreshold)
            {
                return ItemRarity.Rare;
            }

            // Uncommon (scale threshold with level for better loot at higher levels)
            int uncommonThreshold = Math.Max(GameConfig.Config.RarityUncommonThreshold, 65 - (level * 3));
            if (roll >= uncommonThreshold)
            {
                return ItemRarity.Uncommon;
            }

            // Default to Common
            return ItemRarity.Common;
        }

        private static (string name, WeaponPrefixData? weaponPrefix, WeaponTypeData? weaponType, WeaponSuffixData? weaponSuffix, ArmorPrefixData? armorPrefix, ArmorSuffixData? armorSuffix) GenerateNameAndData(EquipmentSlot slot, ItemRarity rarity)
        {
            string prefix = "";
            string baseType = "";
            string suffix = "";
            WeaponPrefixData? weaponPrefix = null;
            WeaponTypeData? weaponType = null;
            WeaponSuffixData? weaponSuffix = null;
            ArmorPrefixData? armorPrefix = null;
            ArmorSuffixData? armorSuffix = null;

            int rarityInt = (int)rarity;

            switch (slot)
            {
                case EquipmentSlot.Weapon:
                    // Select weapon prefix based on rarity
                    var validWeaponPrefixes = _itemData!.WeaponPrefixes.Values.Where(p => p.MinRarity <= rarityInt).ToList();
                    if (validWeaponPrefixes.Count > 0)
                    {
                        weaponPrefix = validWeaponPrefixes[RandomProvider.Next(validWeaponPrefixes.Count)];
                        prefix = weaponPrefix.Name;
                    }

                    // Select weapon type
                    if (_itemData!.WeaponTypes.Count > 0)
                    {
                        weaponType = _itemData.WeaponTypes.Values.ElementAt(RandomProvider.Next(_itemData.WeaponTypes.Count));
                        baseType = weaponType.Name;
                    }

                    // Select weapon suffix if rare or better
                    if (rarity >= ItemRarity.Rare && _itemData.WeaponSuffixes.Count > 1)
                    {
                        var validWeaponSuffixes = _itemData.WeaponSuffixes.Values.Where(s => !string.IsNullOrEmpty(s.Name)).ToList();
                        if (validWeaponSuffixes.Count > 0)
                        {
                            weaponSuffix = validWeaponSuffixes[RandomProvider.Next(validWeaponSuffixes.Count)];
                            suffix = weaponSuffix.Name;
                        }
                    }
                    break;

                case EquipmentSlot.Armor:
                case EquipmentSlot.Helmet:
                case EquipmentSlot.Boots:
                case EquipmentSlot.Gloves:
                    // Select armor prefix based on rarity
                    var validArmorPrefixes = _itemData!.ArmorPrefixes.Values.Where(p => p.MinRarity <= rarityInt).ToList();
                    if (validArmorPrefixes.Count > 0)
                    {
                        armorPrefix = validArmorPrefixes[RandomProvider.Next(validArmorPrefixes.Count)];
                        prefix = armorPrefix.Name;
                    }

                    // Select base type
                    baseType = slot switch
                    {
                        EquipmentSlot.Armor => "Armor",
                        EquipmentSlot.Helmet => helmetTypes[RandomProvider.Next(helmetTypes.Length)],
                        EquipmentSlot.Boots => bootsTypes[RandomProvider.Next(bootsTypes.Length)],
                        EquipmentSlot.Gloves => glovesTypes[RandomProvider.Next(glovesTypes.Length)],
                        _ => ""
                    };

                    // Select armor suffix if rare or better
                    if (rarity >= ItemRarity.Rare && _itemData.ArmorSuffixes.Count > 1 && slot == EquipmentSlot.Armor)
                    {
                        var validArmorSuffixes = _itemData.ArmorSuffixes.Values.Where(s => !string.IsNullOrEmpty(s.Name)).ToList();
                        if (validArmorSuffixes.Count > 0)
                        {
                            armorSuffix = validArmorSuffixes[RandomProvider.Next(validArmorSuffixes.Count)];
                            suffix = armorSuffix.Name;
                        }
                    }
                    break;

                case EquipmentSlot.Ring1:
                case EquipmentSlot.Ring2:
                    prefix = ringPrefixes[Math.Min(rarityInt, ringPrefixes.Length - 1)];
                    baseType = ringTypes[RandomProvider.Next(ringTypes.Length)];
                    break;

                case EquipmentSlot.Amulet:
                    prefix = amuletPrefixes[Math.Min(rarityInt, amuletPrefixes.Length - 1)];
                    baseType = amuletTypes[RandomProvider.Next(amuletTypes.Length)];
                    break;

                case EquipmentSlot.Relic:
                    prefix = rarity >= ItemRarity.Rare ? amuletPrefixes[Math.Min(rarityInt, amuletPrefixes.Length - 1)] : "";
                    baseType = relicNames[RandomProvider.Next(relicNames.Length)];
                    break;
            }

            string name = string.IsNullOrEmpty(suffix)
                ? $"{prefix} {baseType}".Trim()
                : $"{prefix} {baseType} {suffix}".Trim();

            return (name, weaponPrefix, weaponType, weaponSuffix, armorPrefix, armorSuffix);
        }

        private static void GenerateStats(EquipmentItem item, WeaponPrefixData? weaponPrefix, WeaponTypeData? weaponType, ArmorPrefixData? armorPrefix)
        {
            // Get rarity multiplier from data or fallback to default
            double rarityMultiplier = _itemData!.RarityMultipliers.TryGetValue(item.Rarity.ToString(), out double mult)
                ? mult
                : 1.0 + ((int)item.Rarity * 0.5);

            int baseStat = item.Level * 2;

            // Strategy pattern: Use dictionary dispatch instead of switch statement
            if (_statGenerators.TryGetValue(item.Slot, out var generator))
            {
                generator.GenerateStats(item, baseStat, rarityMultiplier, weaponPrefix, weaponType, armorPrefix);
            }
        }

        private static void GenerateSpecialEffects(EquipmentItem item, WeaponSuffixData? weaponSuffix, ArmorSuffixData? armorSuffix)
        {
            // If item has a suffix from data, use those effects
            bool hasDataEffects = false;

            if (weaponSuffix != null && weaponSuffix.Effects.Count > 0)
            {
                foreach (var effectData in weaponSuffix.Effects)
                {
                    var effect = CreateSpecialEffectFromData(effectData, item.Level, item.Rarity);
                    if (effect != null)
                    {
                        item.SpecialEffects.Add(effect);
                        hasDataEffects = true;
                    }
                }
            }

            if (armorSuffix != null && armorSuffix.Effects.Count > 0)
            {
                foreach (var effectData in armorSuffix.Effects)
                {
                    var effect = CreateSpecialEffectFromData(effectData, item.Level, item.Rarity);
                    if (effect != null)
                    {
                        item.SpecialEffects.Add(effect);
                        hasDataEffects = true;
                    }
                }
            }

            // If no data effects, use legacy random generation
            if (!hasDataEffects)
            {
                int effectCount = item.Rarity switch
                {
                    ItemRarity.Common => 0,
                    ItemRarity.Uncommon => RandomProvider.Next(2) == 0 ? 1 : 0,
                    ItemRarity.Rare => RandomProvider.Next(2),
                    ItemRarity.Epic => 1 + RandomProvider.Next(2),
                    ItemRarity.Legendary => 2 + RandomProvider.Next(2),
                    _ => 0
                };

                for (int i = 0; i < effectCount; i++)
                {
                    var effect = GenerateRandomEffect(item.Level, item.Rarity);
                    if (!item.SpecialEffects.Any(e => e.Type == effect.Type))
                    {
                        item.SpecialEffects.Add(effect);
                    }
                }
            }
        }

        private static SpecialEffect? CreateSpecialEffectFromData(SpecialEffectData effectData, int level, ItemRarity rarity)
        {
            if (!Enum.TryParse<EffectType>(effectData.Type, out var effectType))
            {
                return null;
            }

            double procChance = effectData.ProcChance / 100.0; // Convert percentage to 0-1 range
            int value = effectData.Value;

            // Scale value with level for certain effects
            if (effectType == EffectType.LifeSteal || effectType == EffectType.ChainLightning || effectType == EffectType.Bleed)
            {
                value = Math.Max(value, value + level);
            }

            return effectType switch
            {
                EffectType.DoubleDamage => new SpecialEffect(
                    "Double Strike",
                    $"{procChance:P0} chance to deal double damage",
                    procChance,
                    EffectType.DoubleDamage,
                    2),

                EffectType.LifeSteal => new SpecialEffect(
                    "Life Steal",
                    $"{procChance:P0} chance to heal for {value} HP on hit",
                    procChance,
                    EffectType.LifeSteal,
                    value),

                EffectType.ManaSiphon => new SpecialEffect(
                    "Mana Siphon",
                    $"{procChance:P0} chance to restore {value} mana on hit",
                    procChance,
                    EffectType.ManaSiphon,
                    value),

                EffectType.CritBonus => new SpecialEffect(
                    "Critical Focus",
                    $"+{value}% critical hit chance",
                    1.0,
                    EffectType.CritBonus,
                    value),

                EffectType.ChainLightning => new SpecialEffect(
                    "Chain Lightning",
                    $"{procChance:P0} chance to strike enemy with lightning for {value} damage",
                    procChance,
                    EffectType.ChainLightning,
                    value),

                EffectType.Bleed => new SpecialEffect(
                    "Bleed",
                    $"{procChance:P0} chance to cause bleeding for {value} damage over 3 turns",
                    procChance,
                    EffectType.Bleed,
                    value),

                EffectType.Stun => new SpecialEffect(
                    "Stun",
                    $"{procChance:P0} chance to stun enemy for 1 turn",
                    procChance,
                    EffectType.Stun,
                    1),

                EffectType.Thorns => new SpecialEffect(
                    "Thorns",
                    $"Reflect {value}% of damage taken back to attacker",
                    1.0,
                    EffectType.Thorns,
                    value),

                EffectType.AttackBonus => new SpecialEffect(
                    "Mighty",
                    $"+{value} attack",
                    1.0,
                    EffectType.AttackBonus,
                    value),

                EffectType.DefenseBonus => new SpecialEffect(
                    "Fortified",
                    $"+{value} defense",
                    1.0,
                    EffectType.DefenseBonus,
                    value),

                EffectType.SpeedBonus => new SpecialEffect(
                    "Swift",
                    $"+{value} speed",
                    1.0,
                    EffectType.SpeedBonus,
                    value),

                EffectType.ManaRegen => new SpecialEffect(
                    "Mana Regeneration",
                    $"+{value} mana per turn",
                    1.0,
                    EffectType.ManaRegen,
                    value),

                EffectType.HealthRegen => new SpecialEffect(
                    "Health Regeneration",
                    $"+{value} HP per turn",
                    1.0,
                    EffectType.HealthRegen,
                    value),

                EffectType.DodgeChance => new SpecialEffect(
                    "Evasion",
                    $"+{value}% dodge chance",
                    1.0,
                    EffectType.DodgeChance,
                    value),

                EffectType.BonusGold => new SpecialEffect(
                    "Fortune",
                    $"+{value}% gold from enemies",
                    1.0,
                    EffectType.BonusGold,
                    value),

                EffectType.Execute => new SpecialEffect(
                    "Execute",
                    $"{procChance:P0} chance to deal {value}% bonus damage to enemies below 30% HP",
                    procChance,
                    EffectType.Execute,
                    value),

                EffectType.FirstStrike => new SpecialEffect(
                    "First Strike",
                    $"+{value}% damage on first attack in combat",
                    1.0,
                    EffectType.FirstStrike,
                    value),

                _ => null
            };
        }

        private static SpecialEffect GenerateRandomEffect(int level, ItemRarity rarity)
        {
            EffectType type = (EffectType)RandomProvider.Next(Enum.GetValues(typeof(EffectType)).Length);
            double rarityBonus = (int)rarity * 0.05;

            return type switch
            {
                EffectType.DoubleDamage => new SpecialEffect(
                    "Double Strike",
                    $"{(0.1 + rarityBonus):P0} chance to deal double damage",
                    0.1 + rarityBonus,
                    EffectType.DoubleDamage,
                    2),

                EffectType.LifeSteal => new SpecialEffect(
                    "Life Steal",
                    $"{(0.15 + rarityBonus):P0} chance to heal for {10 + level * 2} HP on hit",
                    0.15 + rarityBonus,
                    EffectType.LifeSteal,
                    10 + level * 2),

                EffectType.ManaSiphon => new SpecialEffect(
                    "Mana Siphon",
                    $"{(0.2 + rarityBonus):P0} chance to restore {5 + level} mana on hit",
                    0.2 + rarityBonus,
                    EffectType.ManaSiphon,
                    5 + level),

                EffectType.CritBonus => new SpecialEffect(
                    "Critical Focus",
                    $"+{(10 + (int)rarity * 5):F0}% critical hit chance",
                    1.0,
                    EffectType.CritBonus,
                    10 + (int)rarity * 5),

                EffectType.ChainLightning => new SpecialEffect(
                    "Chain Lightning",
                    $"{(0.08 + rarityBonus):P0} chance to strike enemy with lightning for {15 + level * 3} damage",
                    0.08 + rarityBonus,
                    EffectType.ChainLightning,
                    15 + level * 3),

                EffectType.Bleed => new SpecialEffect(
                    "Bleed",
                    $"{(0.15 + rarityBonus):P0} chance to cause bleeding for {5 + level} damage over 3 turns",
                    0.15 + rarityBonus,
                    EffectType.Bleed,
                    5 + level),

                EffectType.Stun => new SpecialEffect(
                    "Stun",
                    $"{(0.05 + rarityBonus):P0} chance to stun enemy for 1 turn",
                    0.05 + rarityBonus,
                    EffectType.Stun,
                    1),

                EffectType.Thorns => new SpecialEffect(
                    "Thorns",
                    $"Reflect {5 + level}% of damage taken back to attacker",
                    1.0,
                    EffectType.Thorns,
                    5 + level),

                EffectType.AttackBonus => new SpecialEffect(
                    "Mighty",
                    $"+{5 + (int)rarity * 3} attack",
                    1.0,
                    EffectType.AttackBonus,
                    5 + (int)rarity * 3),

                EffectType.DefenseBonus => new SpecialEffect(
                    "Fortified",
                    $"+{5 + (int)rarity * 3} defense",
                    1.0,
                    EffectType.DefenseBonus,
                    5 + (int)rarity * 3),

                EffectType.SpeedBonus => new SpecialEffect(
                    "Swift",
                    $"+{3 + (int)rarity * 2} speed",
                    1.0,
                    EffectType.SpeedBonus,
                    3 + (int)rarity * 2),

                EffectType.ManaRegen => new SpecialEffect(
                    "Mana Regeneration",
                    $"+{2 + (int)rarity} mana per turn",
                    1.0,
                    EffectType.ManaRegen,
                    2 + (int)rarity),

                EffectType.HealthRegen => new SpecialEffect(
                    "Health Regeneration",
                    $"+{3 + level} HP per turn",
                    1.0,
                    EffectType.HealthRegen,
                    3 + level),

                EffectType.DodgeChance => new SpecialEffect(
                    "Evasion",
                    $"+{5 + (int)rarity * 3}% dodge chance",
                    1.0,
                    EffectType.DodgeChance,
                    5 + (int)rarity * 3),

                EffectType.BonusGold => new SpecialEffect(
                    "Fortune",
                    $"+{10 + (int)rarity * 5}% gold from enemies",
                    1.0,
                    EffectType.BonusGold,
                    10 + (int)rarity * 5),

                EffectType.Execute => new SpecialEffect(
                    "Execute",
                    $"{(0.2 + rarityBonus):P0} chance to deal {30 + (int)rarity * 10}% bonus damage to enemies below 30% HP",
                    0.2 + rarityBonus,
                    EffectType.Execute,
                    30 + (int)rarity * 10),

                EffectType.FirstStrike => new SpecialEffect(
                    "First Strike",
                    $"+{20 + (int)rarity * 10}% damage on first attack in combat",
                    1.0,
                    EffectType.FirstStrike,
                    20 + (int)rarity * 10),

                _ => new SpecialEffect("Unknown", "Unknown effect", 0, type, 0)
            };
        }

        private static int CalculatePrice(EquipmentItem item)
        {
            int basePrice = 50 + (item.Level * 30);
            double rarityMultiplier = 1.0 + ((int)item.Rarity * 0.8);
            int effectBonus = item.SpecialEffects.Count * 100;
            int abilityBonus = item.GrantedAbilityIds.Count * 150; // Abilities are more valuable than effects

            int finalPrice = (int)(basePrice * rarityMultiplier) + effectBonus + abilityBonus;
            return (int)(finalPrice * Systems.GameConfig.Config.EquipmentPurchasePriceMultiplier);
        }

        private static void GenerateAbilities(EquipmentItem item, WeaponTypeData? weaponType)
        {
            // FIRST: Grant signature ability for weapons
            if (item.Slot == EquipmentSlot.Weapon && weaponType != null && !string.IsNullOrEmpty(weaponType.SignatureAbility))
            {
                item.GrantedAbilityIds.Add(weaponType.SignatureAbility);
            }

            // SECOND: Grant additional random ability based on rarity chance
            if (_abilityPools == null)
                return;

            // Get ADDITIONAL ability grant chance from GameConfig based on rarity
            // Note: Weapons already have their signature ability, this is for a bonus ability
            double chance = item.Rarity switch
            {
                ItemRarity.Common => GameConfig.Config.AdditionalAbilityChanceCommon,
                ItemRarity.Uncommon => GameConfig.Config.AdditionalAbilityChanceUncommon,
                ItemRarity.Rare => GameConfig.Config.AdditionalAbilityChanceRare,
                ItemRarity.Epic => GameConfig.Config.AdditionalAbilityChanceEpic,
                ItemRarity.Legendary => GameConfig.Config.AdditionalAbilityChanceLegendary,
                _ => 0.0
            };

            // Roll for additional ability
            double roll = RandomProvider.NextDouble();
            if (roll >= chance)
                return; // No additional ability granted

            // Determine ability pool based on slot
            List<string>? abilityPool = GetAbilityPoolForSlot(item.Slot);
            if (abilityPool == null || abilityPool.Count == 0)
                return;

            // Select a random ability from the pool (make sure it's not the signature ability)
            string abilityId = abilityPool[RandomProvider.Next(abilityPool.Count)];

            // Don't add duplicate abilities
            if (!item.GrantedAbilityIds.Contains(abilityId))
            {
                item.GrantedAbilityIds.Add(abilityId);
            }
        }

        private static List<string>? GetAbilityPoolForSlot(EquipmentSlot slot)
        {
            if (_abilityPools == null)
                return null;

            return slot switch
            {
                EquipmentSlot.Weapon => GetWeaponAbilityPool(),
                EquipmentSlot.Armor => GetArmorAbilityPool(),
                EquipmentSlot.Helmet => GetArmorAbilityPool(),
                EquipmentSlot.Boots => GetArmorAbilityPool(),
                EquipmentSlot.Gloves => GetArmorAbilityPool(),
                EquipmentSlot.Ring1 => GetAccessoryAbilityPool(),
                EquipmentSlot.Ring2 => GetAccessoryAbilityPool(),
                EquipmentSlot.Amulet => GetAccessoryAbilityPool(),
                EquipmentSlot.Relic => GetAccessoryAbilityPool(),
                _ => null
            };
        }

        private static List<string>? GetWeaponAbilityPool()
        {
            if (_abilityPools?.WeaponAbilities == null)
                return null;

            // Combine all weapon ability pools into one
            var allAbilities = new List<string>();
            if (_abilityPools.WeaponAbilities.Physical != null)
                allAbilities.AddRange(_abilityPools.WeaponAbilities.Physical);
            if (_abilityPools.WeaponAbilities.Fire != null)
                allAbilities.AddRange(_abilityPools.WeaponAbilities.Fire);
            if (_abilityPools.WeaponAbilities.Ice != null)
                allAbilities.AddRange(_abilityPools.WeaponAbilities.Ice);
            if (_abilityPools.WeaponAbilities.Poison != null)
                allAbilities.AddRange(_abilityPools.WeaponAbilities.Poison);
            if (_abilityPools.WeaponAbilities.Shadow != null)
                allAbilities.AddRange(_abilityPools.WeaponAbilities.Shadow);

            return allAbilities.Count > 0 ? allAbilities : null;
        }

        private static List<string>? GetArmorAbilityPool()
        {
            if (_abilityPools?.ArmorAbilities == null)
                return null;

            // Combine defensive and offensive armor abilities
            var allAbilities = new List<string>();
            if (_abilityPools.ArmorAbilities.Defensive != null)
                allAbilities.AddRange(_abilityPools.ArmorAbilities.Defensive);
            if (_abilityPools.ArmorAbilities.Offensive != null)
                allAbilities.AddRange(_abilityPools.ArmorAbilities.Offensive);

            return allAbilities.Count > 0 ? allAbilities : null;
        }

        private static List<string>? GetAccessoryAbilityPool()
        {
            if (_abilityPools?.AccessoryAbilities?.Utility == null)
                return null;

            return _abilityPools.AccessoryAbilities.Utility;
        }
    }
}
