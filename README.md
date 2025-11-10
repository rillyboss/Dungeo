# TestRPGGame

A text-based RPG game built exclusively with Claude Code to test and demonstrate its capabilities in full-stack game development.

## Project Overview

This is a feature-rich, data-driven console RPG with multiple character classes, procedurally generated equipment, dungeon crawling, boss battles, and a comprehensive ability system. The entire project was developed using Claude Code as an experiment in AI-assisted software development.

## Features

### Character System
- **3 Player Classes**: Warrior, Mage, and Rogue
- **Class-specific Abilities**: Each class has unique abilities with different effects
- **Equipment System**: 6 equipment slots (Weapon, Helmet, Chest, Legs, Accessory, Shield)
- **Inventory Management**: Backpack system with buying, selling, and equipping items

### Combat System
- **Turn-based Combat**: Strategic combat with abilities, attacks, and item usage
- **Ability System**: Multiple effect types (damage, buffs, healing, poison, dodge)
- **Boss Battles**: Special boss encounters with unique mechanics (enrage, shields, bleed)
- **Enemy Abilities**: Enemies have their own special attacks
- **Attack Types**: Physical and Magical damage with different calculations

### Progression
- **Leveling System**: Gain experience and level up to increase stats
- **Stat Scaling**: HP, Mana, Attack, Defense, and Magic scale with level
- **Procedurally Generated Equipment**: Randomized loot with prefixes, suffixes, and rarities
- **5 Rarity Tiers**: Common, Uncommon, Rare, Epic, Legendary
- **Gold Economy**: Earn gold from battles, buy and sell equipment

### Dungeons
- **Multiple Dungeon Types**: Forest, Cave, Ruins, Crypt
- **Dynamic Encounters**: Random battles, treasure, bosses, and choices
- **Miniboss & Boss Fights**: Special encounters with unique enemies
- **Dungeon Progression**: Different difficulty tiers with level requirements

### Data-Driven Design
- **JSON Configuration**: All game data (abilities, enemies, bosses, dungeons, items) stored in JSON
- **Hot-Reload Friendly**: Easy to modify game content without recompiling
- **Organized Data Files**: Separated by entity type for easy maintenance

## Project Structure

```
TestRPGGame/
├── Abilities/              # Ability system and effects
│   ├── Ability.cs
│   ├── AbilityType.cs
│   └── Effects/           # Various ability effect implementations
├── Combat/                # Combat systems
│   ├── CombatSystem.cs
│   ├── DungeonCombatSystem.cs
│   └── AttackTypeSystem.cs
├── DataLoading/           # JSON data loading and management
│   ├── DataLoader.cs
│   └── [Data classes]
├── Entities/              # Game entities
│   ├── Boss/             # Boss-specific mechanics
│   ├── Dungeon/          # Dungeon system
│   ├── Enemy/            # Enemy entities
│   └── Player/           # Player character
├── Equipment/             # Equipment and item generation
│   ├── EquipmentItem.cs
│   ├── EquipmentGenerator.cs
│   └── [Equipment types]
├── Factories/             # Entity creation factories
├── Systems/               # Game systems (shop, save, config)
├── UI/                    # UI helpers and ASCII art
├── Data/                  # JSON game data
│   ├── abilities.json
│   ├── enemies.json
│   ├── bosses.json
│   ├── dungeons.json
│   └── Items/            # Item generation data
│       ├── weapon-prefixes.json
│       ├── weapon-types.json
│       ├── weapon-suffixes.json
│       ├── armor-prefixes.json
│       ├── armor-suffixes.json
│       └── rarity-multipliers.json
├── Game.cs               # Main game loop
└── Program.cs            # Entry point

TestRPGGame.Tests/        # Unit tests (71 tests, 100% passing)
```

## Technology Stack

- **Language**: C# (.NET 8.0 / .NET 9.0)
- **Testing**: xUnit
- **Serialization**: System.Text.Json
- **Development Tool**: Claude Code (Anthropic)

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later

### Running the Game
```bash
cd TestRPGGame
dotnet run
```

### Running Tests
```bash
cd TestRPGGame.Tests
dotnet test
```

## Game Systems

### Equipment Generation
Equipment is procedurally generated using a sophisticated system:
- **Weapon Generation**: Prefix + Type + Suffix (e.g., "Infernal Katana of Flames")
- **Armor Generation**: Prefix + Slot + Suffix (e.g., "Reinforced Helmet of Protection")
- **Stat Formulas**: Level-based stat calculation with rarity multipliers
- **Special Effects**: Equipment can have special effects (HP drain, dodge boost, etc.)

### Ability System
Abilities support multiple effects that can be combined:
- **Damage Effects**: Direct damage to enemies
- **Buff Effects**: Temporary stat increases
- **Restore Effects**: HP/Mana restoration
- **Poison Effects**: Damage over time
- **Dodge Effects**: Temporary dodge chance boost
- **Stat Modification**: Permanent or temporary stat changes

### Save System
- **Multiple Save Slots**: 3 save slots available
- **Auto-save**: Optional auto-save after combat
- **Save Data**: Persists player state, inventory, equipment, and progress

## Testing

The project includes comprehensive unit tests covering:
- Data loading and validation
- Player mechanics (leveling, abilities, inventory)
- Equipment generation and stats
- Combat mechanics (buffs, debuffs, damage calculation)
- Boss abilities and status effects
- Game configuration

**Test Coverage**: 71 tests, 100% passing

## Development Philosophy

This project was built entirely using Claude Code to explore:
- AI-assisted software architecture and design
- Maintaining code quality and organization with AI help
- Test-driven development with AI
- Refactoring and code organization
- Game design and balancing

### Development Highlights
- **One Class Per File**: Clean C# best practices
- **Organized Namespace Structure**: Namespaces match folder hierarchy
- **Data-Driven Architecture**: Game content separated from code
- **Comprehensive Testing**: High test coverage ensuring reliability
- **Git Version Control**: Professional development workflow

## Future Roadmap

1. Enemy special abilities implementation
2. Equipment special effects (proc-based)
3. Merchant system with dynamic pricing
4. Quest system
5. Character progression trees
6. More dungeon types and mechanics
7. Achievements system
8. Combat difficulty options
9. Equipment set bonuses
10. Crafting system

## Credits

- **Development**: Built exclusively with Claude Code by Anthropic
- **Human Developer**: Billy (Project direction and testing)

## License

This is a personal project for testing Claude Code capabilities.

---

Built with Claude Code - Testing the future of AI-assisted development
