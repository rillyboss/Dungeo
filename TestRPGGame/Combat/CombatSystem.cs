using System;
using System.Collections.Generic;
using System.Threading;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.Equipment;
using TestRPGGame.Abilities;
using TestRPGGame.Abilities.Effects;
using TestRPGGame.Systems;
using TestRPGGame.UI;

namespace TestRPGGame.Combat
{
    public class CombatSystem
    {
        private Random random = new Random();
        private Dictionary<string, int> activeBuffs = new Dictionary<string, int>();
        private bool playerDodgeNext = false;
        private int poisonDamage = 0;
        private int poisonTurns = 0;
        private int bleedDamage = 0;
        private int bleedTurns = 0;
        private bool enemyStunNext = false;

        public bool StartBattle(Player player, Enemy enemy)
        {
            Console.Clear();
            activeBuffs.Clear();
            playerDodgeNext = false;
            poisonDamage = 0;
            poisonTurns = 0;
            bleedDamage = 0;
            bleedTurns = 0;
            enemyStunNext = false;
            player.ResetForNewBattle();

            AsciiArt.DrawCombatStart();
            Thread.Sleep(800);

            UIHelper.PrintColoredLine($"\n     ⚔️  A wild {enemy.Name} appears!  ⚔️\n", ConsoleColor.Red);
            UIHelper.PrintColoredLine($"Type: {enemy.Type}", ConsoleColor.Gray);
            AsciiArt.DrawEnemy(enemy.Name);

            Thread.Sleep(1500);

            // Determine turn order based on speed
            bool playerTurn = player.Speed >= enemy.Speed;

            while (player.CurrentHP > 0 && enemy.CurrentHP > 0)
            {
                DisplayBattleStatus(player, enemy);

                if (playerTurn)
                {
                    // Regenerate mana at start of player turn
                    int manaRegen = (int)(player.MaxMana * GameConfig.Config.ManaRegenRate);
                    if (manaRegen > 0)
                    {
                        player.RestoreMana(manaRegen);
                        UIHelper.PrintColoredLine($"💙 Restored {manaRegen} mana", ConsoleColor.Cyan);
                        Thread.Sleep(500);
                    }

                    // Apply poison damage at start of enemy's turn (on the enemy)
                    if (poisonTurns > 0)
                    {
                        enemy.CurrentHP -= poisonDamage;
                        UIHelper.PrintColoredLine($"💚 Poison deals {poisonDamage} damage to {enemy.Name}!", ConsoleColor.Green);
                        poisonTurns--;
                        Thread.Sleep(800);
                    }

                    // Player buff durations tick down at start of player turn
                    List<string> expiredBuffs = new List<string>();
                    foreach (var buff in activeBuffs)
                    {
                        activeBuffs[buff.Key]--;
                        if (activeBuffs[buff.Key] <= 0)
                        {
                            expiredBuffs.Add(buff.Key);
                        }
                    }
                    foreach (var buff in expiredBuffs)
                    {
                        activeBuffs.Remove(buff);
                        UIHelper.PrintColoredLine($"⏰ {buff} effect has worn off!", ConsoleColor.Gray);
                        Thread.Sleep(500);
                    }

                    PlayerTurn(player, enemy);
                    if (enemy.CurrentHP <= 0) break;
                    playerTurn = false;
                }
                else
                {
                    // Apply bleed damage at start of enemy turn
                    if (bleedTurns > 0)
                    {
                        player.CurrentHP -= bleedDamage;
                        UIHelper.PrintColoredLine($"🩸 Bleeding! You take {bleedDamage} damage!", ConsoleColor.Red);
                        bleedTurns--;
                        Thread.Sleep(800);
                    }

                    // Check if enemy is stunned
                    if (enemyStunNext)
                    {
                        UIHelper.PrintColoredLine($"⚡ {enemy.Name} is stunned and loses their turn!", ConsoleColor.Yellow);
                        enemyStunNext = false;
                    }
                    else
                    {
                        EnemyTurn(player, enemy);
                    }
                    if (player.CurrentHP <= 0) break;
                    playerTurn = true;
                }

                // Regenerate health per turn
                int healthRegen = (int)(player.MaxHP * GameConfig.Config.HealthRegenRate);
                if (healthRegen > 0)
                {
                    player.Heal(healthRegen);
                    UIHelper.PrintColoredLine($"❤️  Restored {healthRegen} HP", ConsoleColor.Green);
                    Thread.Sleep(500);
                }

                // Reduce ability cooldowns
                foreach (var ability in player.Abilities)
                {
                    ability.ReduceCooldown();
                }
                foreach (var ability in enemy.Abilities)
                {
                    ability.ReduceCooldown();
                }
            }

            return player.CurrentHP > 0;
        }

        private void DisplayBattleStatus(Player player, Enemy enemy)
        {
            Console.WriteLine("\n" + new string('═', 60));

            // Player status
            UIHelper.PrintColoredLine($"👤 {player.Name} (Level {player.Level} {player.Class})", ConsoleColor.Cyan);
            Console.Write("   ");
            DrawHealthBar(player.CurrentHP, player.MaxHP, ConsoleColor.Green);
            Console.Write("   ");
            DrawManaBar(player.CurrentMana, player.MaxMana, ConsoleColor.Blue);

            Console.WriteLine();

            // Enemy status
            UIHelper.PrintColoredLine($"👹 {enemy.Name}", ConsoleColor.Red);
            Console.Write("   ");
            DrawHealthBar(enemy.CurrentHP, enemy.MaxHP, ConsoleColor.Red);

            Console.WriteLine("\n" + new string('═', 60) + "\n");
        }

        private void DrawHealthBar(int current, int max, ConsoleColor color)
        {
            int barLength = 20;
            int filledLength = (int)((double)current / max * barLength);
            filledLength = Math.Max(0, Math.Min(barLength, filledLength));

            Console.Write("❤️  [");
            Console.ForegroundColor = color;
            Console.Write(new string('█', filledLength));
            Console.ResetColor();
            Console.Write(new string('░', barLength - filledLength));
            Console.Write($"] {current}/{max}");
            Console.WriteLine();
        }

        private void DrawManaBar(int current, int max, ConsoleColor color)
        {
            int barLength = 20;
            int filledLength = (int)((double)current / max * barLength);
            filledLength = Math.Max(0, Math.Min(barLength, filledLength));

            Console.Write("💙 [");
            Console.ForegroundColor = color;
            Console.Write(new string('█', filledLength));
            Console.ResetColor();
            Console.Write(new string('░', barLength - filledLength));
            Console.Write($"] {current}/{max}");
            Console.WriteLine();
        }

        private void PlayerTurn(Player player, Enemy enemy)
        {
            UIHelper.PrintColoredLine("YOUR TURN:", ConsoleColor.Yellow);
            Console.WriteLine("1. ⚔️  Attack");
            Console.WriteLine("2. 🎯 Use Ability");
            Console.WriteLine("3. 🧪 Use Potion");

            Console.Write("\nChoose action: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    PerformBasicAttack(player, enemy);
                    break;
                case "2":
                    UseAbility(player, enemy);
                    break;
                case "3":
                    if (player.UsePotion())
                    {
                        UIHelper.PrintColoredLine($"\n🧪 You used a potion and restored {player.MaxHP / 2} HP!", ConsoleColor.Green);
                        UIHelper.PrintColoredLine($"Potions remaining: {player.PotionCount}", ConsoleColor.Gray);
                    }
                    else
                    {
                        UIHelper.PrintColoredLine("\n❌ No potions left!", ConsoleColor.Red);
                        PlayerTurn(player, enemy); // Try again
                        return;
                    }
                    break;
                default:
                    UIHelper.PrintColoredLine("\n❌ Invalid choice! Lost your turn!", ConsoleColor.Red);
                    break;
            }

            Thread.Sleep(1000);
        }

        private void PerformBasicAttack(Player player, Enemy enemy)
        {
            int damage = player.GetTotalAttack();

            // Apply attack buff
            if (activeBuffs.ContainsKey("Battle Rage"))
            {
                damage = (int)(damage * 1.5);
            }

            // Critical hit chance
            bool isCrit = random.NextDouble() < player.CritChance;
            if (isCrit)
            {
                damage = (int)(damage * 2);
            }

            // Get weapon attack type and apply weakness multiplier
            AttackType weaponType = player.GetWeaponAttackType();
            double effectiveness = AttackTypeSystem.GetDamageMultiplier(weaponType, enemy.Type);
            damage = (int)(damage * effectiveness);

            // Apply damage
            int actualDamage = Math.Max(1, damage - enemy.Defense);

            // Check for special effects BEFORE applying damage
            var specialEffects = player.Inventory.GetAllSpecialEffects();
            bool doubleProc = false;
            int lifestealAmount = 0;
            int manaSiphonAmount = 0;
            bool stunProc = false;
            bool bleedProc = false;
            int bleedDmg = 0;
            bool chainLightningProc = false;
            int chainLightningDmg = 0;

            foreach (var effect in specialEffects)
            {
                double roll = random.NextDouble() * 100;
                if (roll < effect.ProcChance)
                {
                    switch (effect.Type)
                    {
                        case EffectType.DoubleDamage:
                            actualDamage *= 2;
                            doubleProc = true;
                            break;
                        case EffectType.LifeSteal:
                            lifestealAmount = effect.Value;
                            break;
                        case EffectType.ManaSiphon:
                            manaSiphonAmount = effect.Value;
                            break;
                        case EffectType.Stun:
                            stunProc = true;
                            break;
                        case EffectType.Bleed:
                            bleedProc = true;
                            bleedDmg = effect.Value;
                            break;
                        case EffectType.ChainLightning:
                            chainLightningProc = true;
                            chainLightningDmg = effect.Value;
                            break;
                    }
                }
            }

            enemy.CurrentHP -= actualDamage;

            Console.WriteLine();

            // Show attack type effectiveness
            string icon = AttackTypeSystem.GetAttackTypeIcon(weaponType);
            ConsoleColor typeColor = AttackTypeSystem.GetAttackTypeColor(weaponType);
            UIHelper.PrintColored($"{icon} {weaponType} Attack", typeColor);

            string effectText = AttackTypeSystem.GetEffectivenessText(effectiveness);
            if (!string.IsNullOrEmpty(effectText))
            {
                ConsoleColor effectColor = AttackTypeSystem.GetEffectivenessColor(effectiveness);
                UIHelper.PrintColored($" - {effectText}", effectColor);
            }
            Console.WriteLine();

            if (isCrit)
            {
                AsciiArt.DrawCriticalHit();
                UIHelper.PrintColoredLine($"\n💥 CRITICAL HIT! You deal {actualDamage} damage to {enemy.Name}!", ConsoleColor.Yellow);
            }
            else
            {
                UIHelper.PrintColoredLine($"⚔️  You deal {actualDamage} damage to {enemy.Name}!", ConsoleColor.White);
            }

            // Display special effect procs
            if (doubleProc)
            {
                UIHelper.PrintColoredLine("   ✨ DOUBLE DAMAGE proc!", ConsoleColor.Magenta);
            }
            if (lifestealAmount > 0)
            {
                player.Heal(lifestealAmount);
                UIHelper.PrintColoredLine($"   💉 Life Steal: Restored {lifestealAmount} HP!", ConsoleColor.Green);
            }
            if (manaSiphonAmount > 0)
            {
                player.RestoreMana(manaSiphonAmount);
                UIHelper.PrintColoredLine($"   💫 Mana Siphon: Restored {manaSiphonAmount} mana!", ConsoleColor.Cyan);
            }
            if (chainLightningProc)
            {
                enemy.CurrentHP -= chainLightningDmg;
                UIHelper.PrintColoredLine($"   ⚡ CHAIN LIGHTNING! Deals {chainLightningDmg} bonus damage!", ConsoleColor.Yellow);
            }
            if (bleedProc)
            {
                bleedDamage = bleedDmg;
                bleedTurns = 3; // Bleed lasts 3 turns
                UIHelper.PrintColoredLine($"   🩸 BLEED! Enemy inflicts {bleedDmg} damage per turn!", ConsoleColor.DarkRed);
            }
            if (stunProc)
            {
                enemyStunNext = true;
                UIHelper.PrintColoredLine($"   ⚡ STUNNED! {enemy.Name} loses their next turn!", ConsoleColor.Yellow);
            }

            // Apply Thorns damage to player
            foreach (var effect in specialEffects)
            {
                if (effect.Type == EffectType.Thorns)
                {
                    // Thorns is defensive, applies when taking damage, not dealing it
                }
            }
        }

        private void UseAbility(Player player, Enemy enemy)
        {
            var unlockedAbilities = player.Abilities.Where(a => a.IsUnlocked).ToList();

            if (unlockedAbilities.Count == 0)
            {
                UIHelper.PrintColoredLine("\n❌ No abilities unlocked yet!", ConsoleColor.Red);
                Thread.Sleep(1000);
                PlayerTurn(player, enemy);
                return;
            }

            Console.WriteLine("\n╔════ ABILITIES ════╗");
            for (int i = 0; i < unlockedAbilities.Count; i++)
            {
                var ability = unlockedAbilities[i];
                string status = ability.CanUse(player.CurrentMana) ? "✓" : "✗";
                string cdInfo = ability.CurrentCooldown > 0 ? $" (CD: {ability.CurrentCooldown})" : "";
                Console.WriteLine($"{i + 1}. {status} {ability.Name} ({ability.ManaCost} mana){cdInfo}");
            }
            Console.WriteLine("0. Cancel");
            Console.WriteLine("╚═══════════════════╝");

            Console.Write("\nChoose ability: ");
            string choice = Console.ReadLine();

            if (choice == "0")
            {
                PlayerTurn(player, enemy);
                return;
            }

            if (int.TryParse(choice, out int abilityIndex) && abilityIndex > 0 && abilityIndex <= unlockedAbilities.Count)
            {
                var ability = unlockedAbilities[abilityIndex - 1];

                if (!ability.CanUse(player.CurrentMana))
                {
                    UIHelper.PrintColoredLine("\n❌ Cannot use this ability! (Not enough mana or on cooldown)", ConsoleColor.Red);
                    Thread.Sleep(1000);
                    UseAbility(player, enemy);
                    return;
                }

                player.CurrentMana -= ability.ManaCost;
                ability.Use();

                Console.WriteLine();
                AsciiArt.DrawAbilityUse(ability.Name);
                ExecuteAbility(player, enemy, ability);
            }
            else
            {
                UIHelper.PrintColoredLine("\n❌ Invalid choice!", ConsoleColor.Red);
                Thread.Sleep(1000);
                UseAbility(player, enemy);
            }
        }

        private void ExecuteAbility(Player player, Enemy enemy, Ability ability)
        {
            UIHelper.PrintColoredLine($"✨ {player.Name} uses {ability.Name}!", ConsoleColor.Magenta);

            // Create ability context
            var context = new AbilityContext(player, enemy)
            {
                ActiveBuffs = activeBuffs,
                PlayerDodgeNext = playerDodgeNext,
                PoisonDamage = poisonDamage,
                PoisonTurns = poisonTurns,
                Random = random
            };

            // Execute all effects
            ability.Execute(context);

            // Handle special effects that need to modify combat state
            foreach (var effect in ability.Effects)
            {
                if (effect is DodgeEffect)
                {
                    playerDodgeNext = true;
                    player.Speed += 5;
                }
                else if (effect is PoisonEffect poisonEffect)
                {
                    poisonDamage = poisonEffect.DamagePerTurn;
                    poisonTurns = poisonEffect.Duration;
                }
            }
        }

        private void EnemyTurn(Player player, Enemy enemy)
        {
            UIHelper.PrintColoredLine($"\n{enemy.Name}'s TURN:", ConsoleColor.Red);
            Thread.Sleep(800);

            // Reduce cooldowns on all enemy abilities
            foreach (var enemyAbility in enemy.Abilities)
            {
                enemyAbility.ReduceCooldown();
            }

            // Check if enemy uses ability (prioritize abilities that meet HP threshold)
            bool usedAbility = false;
            foreach (var enemyAbility in enemy.Abilities)
            {
                if (enemyAbility.CanUse(enemy.CurrentHP, enemy.MaxHP))
                {
                    enemyAbility.Use();

                    // Execute ability effects
                    UIHelper.PrintColored($"💢 {enemy.Name} uses ", ConsoleColor.Red);
                    UIHelper.PrintColored($"{enemyAbility.Ability.Name}", ConsoleColor.Yellow);
                    UIHelper.PrintColoredLine($"!", ConsoleColor.Red);
                    Thread.Sleep(600);

                    if (playerDodgeNext)
                    {
                        UIHelper.PrintColoredLine($"💨 You dodged {enemy.Name}'s {enemyAbility.Ability.Name}!", ConsoleColor.Cyan);
                        playerDodgeNext = false;
                    }
                    else
                    {
                        // Execute each effect manually (simplified for enemy abilities)
                        foreach (var effect in enemyAbility.Ability.Effects)
                        {
                            if (effect is DamageEffect damageEffect)
                            {
                                // Calculate damage based on enemy attack
                                int baseDamage = (int)(enemy.Attack * damageEffect.Multiplier);
                                int actualDamage = ApplyDamageToPlayer(player, enemy, baseDamage);
                                UIHelper.PrintColoredLine($"   💥 {actualDamage} damage dealt!", ConsoleColor.Red);
                            }
                            else if (effect is PoisonEffect poisonEffect)
                            {
                                poisonDamage = poisonEffect.DamagePerTurn;
                                poisonTurns = poisonEffect.Duration;
                                UIHelper.PrintColoredLine($"   💚 You are poisoned! ({poisonEffect.DamagePerTurn} damage/turn for {poisonEffect.Duration} turns)", ConsoleColor.Green);
                            }
                            else if (effect is RestoreEffect restoreEffect)
                            {
                                // Enemy heals itself
                                int healAmount = Math.Min(restoreEffect.Amount, enemy.MaxHP - enemy.CurrentHP);
                                enemy.CurrentHP += healAmount;
                                UIHelper.PrintColoredLine($"   💚 {enemy.Name} heals for {healAmount} HP!", ConsoleColor.Green);
                            }
                            else if (effect is StatModEffect statModEffect)
                            {
                                // Reduce player's speed temporarily (simplified - just show message for now)
                                UIHelper.PrintColoredLine($"   🔻 Your combat effectiveness is reduced!", ConsoleColor.Magenta);
                            }
                            else if (effect is BuffEffect buffEffect)
                            {
                                // Enemy buffs are simplified for now - just show message
                                UIHelper.PrintColoredLine($"   ⚡ {enemy.Name} is empowered by {buffEffect.BuffName}!", ConsoleColor.Yellow);
                            }

                            Thread.Sleep(500);
                        }
                    }

                    usedAbility = true;
                    break; // Only use one ability per turn
                }
            }

            if (!usedAbility)
            {
                if (playerDodgeNext)
                {
                    UIHelper.PrintColoredLine($"💨 You dodged {enemy.Name}'s attack!", ConsoleColor.Cyan);
                    playerDodgeNext = false;
                }
                else
                {
                    int actualDamage = ApplyDamageToPlayer(player, enemy, enemy.Attack);
                    UIHelper.PrintColoredLine($"⚔️  {enemy.Name} attacks! {actualDamage} damage!", ConsoleColor.Red);
                }
            }

            Thread.Sleep(1000);
        }

        private int ApplyDamageToPlayer(Player player, Enemy enemy, int baseDamage)
        {
            int damage = baseDamage;

            // Apply shield wall
            if (activeBuffs.ContainsKey("Shield Wall"))
            {
                damage = damage / 2;
            }

            int actualDamage = Math.Max(1, damage - player.GetTotalDefense());
            player.CurrentHP -= actualDamage;

            // Check for Thorns effect
            var specialEffects = player.Inventory.GetAllSpecialEffects();
            foreach (var effect in specialEffects)
            {
                if (effect.Type == EffectType.Thorns)
                {
                    // Apply reflected damage to enemy
                    enemy.CurrentHP -= effect.Value;
                    UIHelper.PrintColoredLine($"   🌵 THORNS! Enemy takes {effect.Value} reflected damage!", ConsoleColor.Yellow);
                }
            }

            return actualDamage;
        }
    }
}
