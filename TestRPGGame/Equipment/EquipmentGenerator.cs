using System;
using System.Collections.Generic;
using System.Linq;
using TestRPGGame.DataLoading;
using TestRPGGame.Combat;

namespace TestRPGGame.Equipment
{
    public static class EquipmentGenerator
    {
        private static Random random = new Random();
        private static ItemGenerationData? _itemData;

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
                _itemData = DataLoader.GetItemGenerationData();
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
                item.Slot = (EquipmentSlot)slotValues.GetValue(random.Next(slotValues.Length))!;
            }

            // Determine level (can be +/- 1 from player level)
            item.Level = Math.Max(1, playerLevel + random.Next(-1, 2));

            // Determine rarity (higher level = better chance for rare items)
            item.Rarity = forceRarity ?? DetermineRarity(item.Level);

            // Generate name and get data for stats generation
            var (name, weaponPrefix, weaponType, weaponSuffix, armorPrefix, armorSuffix) = GenerateNameAndData(item.Slot, item.Rarity);
            item.Name = name;

            // Generate stats based on slot, level, rarity, and item data
            GenerateStats(item, weaponPrefix, weaponType, armorPrefix);

            // Generate special effects based on rarity and suffix data
            GenerateSpecialEffects(item, weaponSuffix, armorSuffix);

            // Calculate price
            item.Price = CalculatePrice(item);

            return item;
        }

        private static ItemRarity DetermineRarity(int level)
        {
            // Data-driven rarity system
            // Load thresholds from items.json
            var thresholds = _itemData!.RarityThresholds;
            if (thresholds == null || thresholds.Count == 0)
            {
                // Fallback to simple system if no data
                return ItemRarity.Common;
            }

            int roll = random.Next(100);

            // Check rarities in order from highest to lowest
            // Legendary
            if (thresholds.TryGetValue("Legendary", out var legendaryThreshold) &&
                level >= legendaryThreshold.MinLevel &&
                level <= legendaryThreshold.MaxLevel &&
                roll >= legendaryThreshold.RollThreshold)
            {
                return ItemRarity.Legendary;
            }

            // Epic
            if (thresholds.TryGetValue("Epic", out var epicThreshold) &&
                level >= epicThreshold.MinLevel &&
                level <= epicThreshold.MaxLevel &&
                roll >= epicThreshold.RollThreshold)
            {
                return ItemRarity.Epic;
            }

            // Rare (check special low-level rare first)
            if (thresholds.TryGetValue("RareLowLevel", out var rareLowLevel) &&
                level >= rareLowLevel.MinLevel &&
                level <= rareLowLevel.MaxLevel &&
                roll >= rareLowLevel.RollThreshold)
            {
                return ItemRarity.Rare;
            }

            // Rare (normal)
            if (thresholds.TryGetValue("Rare", out var rareThreshold) &&
                level >= rareThreshold.MinLevel &&
                level <= rareThreshold.MaxLevel &&
                roll >= rareThreshold.RollThreshold)
            {
                return ItemRarity.Rare;
            }

            // Uncommon (scale with level)
            if (thresholds.TryGetValue("Uncommon", out var uncommonThreshold))
            {
                // Make uncommon more common as level increases
                int adjustedThreshold = Math.Max(uncommonThreshold.RollThreshold, 65 - (level * 3));
                if (level >= uncommonThreshold.MinLevel &&
                    level <= uncommonThreshold.MaxLevel &&
                    roll >= adjustedThreshold)
                {
                    return ItemRarity.Uncommon;
                }
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
                        weaponPrefix = validWeaponPrefixes[random.Next(validWeaponPrefixes.Count)];
                        prefix = weaponPrefix.Name;
                    }

                    // Select weapon type
                    if (_itemData!.WeaponTypes.Count > 0)
                    {
                        weaponType = _itemData.WeaponTypes.Values.ElementAt(random.Next(_itemData.WeaponTypes.Count));
                        baseType = weaponType.Name;
                    }

                    // Select weapon suffix if rare or better
                    if (rarity >= ItemRarity.Rare && _itemData.WeaponSuffixes.Count > 1)
                    {
                        var validWeaponSuffixes = _itemData.WeaponSuffixes.Values.Where(s => !string.IsNullOrEmpty(s.Name)).ToList();
                        if (validWeaponSuffixes.Count > 0)
                        {
                            weaponSuffix = validWeaponSuffixes[random.Next(validWeaponSuffixes.Count)];
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
                        armorPrefix = validArmorPrefixes[random.Next(validArmorPrefixes.Count)];
                        prefix = armorPrefix.Name;
                    }

                    // Select base type
                    baseType = slot switch
                    {
                        EquipmentSlot.Armor => "Armor",
                        EquipmentSlot.Helmet => helmetTypes[random.Next(helmetTypes.Length)],
                        EquipmentSlot.Boots => bootsTypes[random.Next(bootsTypes.Length)],
                        EquipmentSlot.Gloves => glovesTypes[random.Next(glovesTypes.Length)],
                        _ => ""
                    };

                    // Select armor suffix if rare or better
                    if (rarity >= ItemRarity.Rare && _itemData.ArmorSuffixes.Count > 1 && slot == EquipmentSlot.Armor)
                    {
                        var validArmorSuffixes = _itemData.ArmorSuffixes.Values.Where(s => !string.IsNullOrEmpty(s.Name)).ToList();
                        if (validArmorSuffixes.Count > 0)
                        {
                            armorSuffix = validArmorSuffixes[random.Next(validArmorSuffixes.Count)];
                            suffix = armorSuffix.Name;
                        }
                    }
                    break;

                case EquipmentSlot.Ring1:
                case EquipmentSlot.Ring2:
                    prefix = ringPrefixes[Math.Min(rarityInt, ringPrefixes.Length - 1)];
                    baseType = ringTypes[random.Next(ringTypes.Length)];
                    break;

                case EquipmentSlot.Amulet:
                    prefix = amuletPrefixes[Math.Min(rarityInt, amuletPrefixes.Length - 1)];
                    baseType = amuletTypes[random.Next(amuletTypes.Length)];
                    break;

                case EquipmentSlot.Relic:
                    prefix = rarity >= ItemRarity.Rare ? amuletPrefixes[Math.Min(rarityInt, amuletPrefixes.Length - 1)] : "";
                    baseType = relicNames[random.Next(relicNames.Length)];
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

            switch (item.Slot)
            {
                case EquipmentSlot.Weapon:
                    if (weaponPrefix != null && weaponType != null)
                    {
                        // Use data-driven weapon stats
                        double randomVariance = 1.5 + random.NextDouble() * 0.5;
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
                        item.AttackBonus = (int)(baseStat * rarityMultiplier * (1.5 + random.NextDouble() * 0.5));
                        item.MagicBonus = random.Next(2) == 0 ? (int)(baseStat * rarityMultiplier * 0.8) : 0;
                        Array attackTypes = Enum.GetValues(typeof(AttackType));
                        item.WeaponAttackType = (AttackType)attackTypes.GetValue(random.Next(attackTypes.Length))!;
                    }

                    if (item.Rarity >= ItemRarity.Rare)
                        item.CritBonus = 0.05 + (random.NextDouble() * 0.15);
                    break;

                case EquipmentSlot.Armor:
                    if (armorPrefix != null)
                    {
                        // Use data-driven armor stats
                        double randomVariance = 1.5 + random.NextDouble() * 0.5;
                        item.DefenseBonus = (int)(baseStat * rarityMultiplier * armorPrefix.DefenseMultiplier * randomVariance);
                        item.HPBonus = (int)(baseStat * rarityMultiplier * armorPrefix.HPMultiplier * 3);
                    }
                    else
                    {
                        // Fallback to legacy generation
                        item.DefenseBonus = (int)(baseStat * rarityMultiplier * (1.5 + random.NextDouble() * 0.5));
                        item.HPBonus = (int)(baseStat * rarityMultiplier * 3);
                    }
                    break;

                case EquipmentSlot.Helmet:
                    if (armorPrefix != null)
                    {
                        item.DefenseBonus = (int)(baseStat * rarityMultiplier * armorPrefix.DefenseMultiplier * 0.7);
                        item.HPBonus = (int)(baseStat * rarityMultiplier * armorPrefix.HPMultiplier * 2);
                    }
                    else
                    {
                        item.DefenseBonus = (int)(baseStat * rarityMultiplier * 0.7);
                        item.HPBonus = (int)(baseStat * rarityMultiplier * 2);
                    }
                    item.ManaBonus = random.Next(3) == 0 ? (int)(baseStat * rarityMultiplier * 1.5) : 0;
                    break;

                case EquipmentSlot.Boots:
                    if (armorPrefix != null)
                    {
                        item.DefenseBonus = (int)(baseStat * rarityMultiplier * armorPrefix.DefenseMultiplier * 0.5);
                    }
                    else
                    {
                        item.DefenseBonus = (int)(baseStat * rarityMultiplier * 0.5);
                    }
                    item.SpeedBonus = (int)(baseStat * rarityMultiplier * 0.6);
                    break;

                case EquipmentSlot.Gloves:
                    if (armorPrefix != null)
                    {
                        item.DefenseBonus = (int)(baseStat * rarityMultiplier * armorPrefix.DefenseMultiplier * 0.4);
                    }
                    else
                    {
                        item.DefenseBonus = (int)(baseStat * rarityMultiplier * 0.4);
                    }
                    item.AttackBonus = (int)(baseStat * rarityMultiplier * 0.6);
                    item.SpeedBonus = (int)(baseStat * rarityMultiplier * 0.3);
                    break;

                case EquipmentSlot.Ring1:
                case EquipmentSlot.Ring2:
                    // Rings have varied stats
                    int statChoice = random.Next(4);
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
                    break;

                case EquipmentSlot.Amulet:
                    item.HPBonus = (int)(baseStat * rarityMultiplier * 2);
                    item.ManaBonus = (int)(baseStat * rarityMultiplier * 2);
                    if (item.Rarity >= ItemRarity.Rare)
                        item.CritBonus = 0.03 + (random.NextDouble() * 0.1);
                    break;

                case EquipmentSlot.Relic:
                    // Relics are magical and provide varied bonuses
                    item.MagicBonus = (int)(baseStat * rarityMultiplier);
                    item.ManaBonus = (int)(baseStat * rarityMultiplier * 2.5);
                    item.AttackBonus = (int)(baseStat * rarityMultiplier * 0.4);
                    break;
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
                    ItemRarity.Uncommon => random.Next(2) == 0 ? 1 : 0,
                    ItemRarity.Rare => random.Next(2),
                    ItemRarity.Epic => 1 + random.Next(2),
                    ItemRarity.Legendary => 2 + random.Next(2),
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

                _ => null
            };
        }

        private static SpecialEffect GenerateRandomEffect(int level, ItemRarity rarity)
        {
            EffectType type = (EffectType)random.Next(Enum.GetValues(typeof(EffectType)).Length);
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

                _ => new SpecialEffect("Unknown", "Unknown effect", 0, type, 0)
            };
        }

        private static int CalculatePrice(EquipmentItem item)
        {
            int basePrice = 50 + (item.Level * 30);
            double rarityMultiplier = 1.0 + ((int)item.Rarity * 0.8);
            int effectBonus = item.SpecialEffects.Count * 100;

            return (int)(basePrice * rarityMultiplier) + effectBonus;
        }
    }
}
