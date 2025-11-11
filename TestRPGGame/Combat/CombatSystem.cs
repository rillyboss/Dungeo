using System;
using System.Collections.Generic;
using System.Threading;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.Equipment;
using TestRPGGame.Abilities;
using TestRPGGame.Abilities.Effects;
using TestRPGGame.Abilities.Applicators;
using TestRPGGame.Combat.StatusEffects;
using TestRPGGame.Systems;
using TestRPGGame.UI;

namespace TestRPGGame.Combat
{
    public class CombatSystem
    {
        private Random random = new Random();
        private bool playerDodgeNext = false;
        private bool enemyStunNext = false;
        private Ability? queuedPlayerAbility = null;
        private EnemyAbility? queuedEnemyAbility = null;

        /// <summary>
        /// Get the effective speed of the player including buffs
        /// </summary>
        private int GetPlayerEffectiveSpeed(Player player)
        {
            return player.Speed + player.Effects.GetSpeedModifier();
        }

        /// <summary>
        /// Get the effective speed of the enemy including buffs
        /// </summary>
        private int GetEnemyEffectiveSpeed(Enemy enemy)
        {
            return enemy.Speed + enemy.Effects.GetSpeedModifier();
        }

        /// <summary>
        /// Determine who goes first this turn based on speed and priority abilities
        /// Returns true if player goes first, false if enemy goes first
        /// </summary>
        private bool DetermineInitialTurnOrder(Player player, Enemy enemy)
        {
            bool playerHasPriority = queuedPlayerAbility != null && queuedPlayerAbility.Priority;
            bool enemyHasPriority = queuedEnemyAbility != null && queuedEnemyAbility.Ability.Priority;

            // If both have priority or neither has priority, use speed
            if (playerHasPriority == enemyHasPriority)
            {
                int playerSpeed = GetPlayerEffectiveSpeed(player);
                int enemySpeed = GetEnemyEffectiveSpeed(enemy);
                return playerSpeed >= enemySpeed; // Player wins ties
            }

            // Whoever has priority goes first
            return playerHasPriority;
        }

        /// <summary>
        /// Player chooses their action for this turn (doesn't execute it yet)
        /// Returns true if player fled, false otherwise
        /// </summary>
        private bool PlayerChooseAction(Player player, Enemy enemy, bool canFlee)
        {
            UIHelper.PrintColoredLine("\nYOUR TURN:", ConsoleColor.Yellow);
            Console.WriteLine("1. ⚔️  Attack");
            Console.WriteLine("2. 🎯 Use Ability");
            Console.WriteLine("3. 🧪 Use Potion");
            if (canFlee)
            {
                Console.WriteLine("4. 🏃 Flee");
            }

            Console.Write("\nChoose action: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    // Basic attack queued
                    queuedPlayerAbility = null;
                    break;
                case "2":
                    // Select ability (will set queuedPlayerAbility)
                    SelectPlayerAbility(player, enemy, canFlee);
                    break;
                case "3":
                    if (player.UsePotion())
                    {
                        UIHelper.PrintColoredLine($"\n🧪 You used a potion and restored {player.MaxHP / 2} HP!", ConsoleColor.Green);
                        UIHelper.PrintColoredLine($"Potions remaining: {player.PotionCount}", ConsoleColor.Gray);
                        Thread.Sleep(1000);
                        queuedPlayerAbility = null; // Potion use counts as basic action
                    }
                    else
                    {
                        UIHelper.PrintColoredLine("\n❌ No potions left!", ConsoleColor.Red);
                        return PlayerChooseAction(player, enemy, canFlee);
                    }
                    break;
                case "4":
                    if (canFlee)
                    {
                        return AttemptFlee(player, enemy);
                    }
                    else
                    {
                        UIHelper.PrintColoredLine("\n❌ You cannot flee from this battle!", ConsoleColor.Red);
                        return PlayerChooseAction(player, enemy, canFlee);
                    }
                default:
                    UIHelper.PrintColoredLine("\n❌ Invalid choice! Please try again.", ConsoleColor.Red);
                    Thread.Sleep(1000);
                    return PlayerChooseAction(player, enemy, canFlee);
            }

            return false;
        }

        /// <summary>
        /// Player selects an ability to use
        /// </summary>
        private void SelectPlayerAbility(Player player, Enemy enemy, bool canFlee)
        {
            var unlockedAbilities = player.Abilities.Where(a => a.IsUnlocked).ToList();

            if (unlockedAbilities.Count == 0)
            {
                UIHelper.PrintColoredLine("\n❌ No abilities unlocked yet!", ConsoleColor.Red);
                Thread.Sleep(1000);
                PlayerChooseAction(player, enemy, canFlee);
                return;
            }

            Console.WriteLine("\n╔════ ABILITIES ════╗");
            for (int i = 0; i < unlockedAbilities.Count; i++)
            {
                var ability = unlockedAbilities[i];
                string status = ability.CanUse(player.CurrentMana) ? "✓" : "✗";
                string cdInfo = ability.CurrentCooldown > 0 ? $" (CD: {ability.CurrentCooldown})" : "";
                string priorityIcon = ability.Priority ? "⚡" : "";
                Console.WriteLine($"{i + 1}. {status} {priorityIcon}{ability.Name} ({ability.ManaCost} mana){cdInfo}");
            }
            Console.WriteLine("0. Cancel");
            Console.WriteLine("╚═══════════════════╝");

            Console.Write("\nChoose ability: ");
            string choice = Console.ReadLine();

            if (choice == "0")
            {
                PlayerChooseAction(player, enemy, canFlee);
                return;
            }

            if (int.TryParse(choice, out int abilityIndex) && abilityIndex > 0 && abilityIndex <= unlockedAbilities.Count)
            {
                var ability = unlockedAbilities[abilityIndex - 1];

                if (!ability.CanUse(player.CurrentMana))
                {
                    UIHelper.PrintColoredLine("\n❌ Cannot use this ability! (Not enough mana or on cooldown)", ConsoleColor.Red);
                    Thread.Sleep(1000);
                    SelectPlayerAbility(player, enemy, canFlee);
                    return;
                }

                // Queue the ability for execution
                queuedPlayerAbility = ability;
            }
            else
            {
                UIHelper.PrintColoredLine("\n❌ Invalid choice!", ConsoleColor.Red);
                Thread.Sleep(1000);
                SelectPlayerAbility(player, enemy, canFlee);
            }
        }

        /// <summary>
        /// Enemy chooses their action for this turn (doesn't execute it yet)
        /// </summary>
        private void EnemyChooseAction(Player player, Enemy enemy)
        {
            // Use AI to select ability if available
            if (enemy.AI != null)
            {
                queuedEnemyAbility = enemy.AI.SelectAbility(enemy, player);
            }
            else
            {
                // Fallback: simple iteration through abilities (old behavior)
                foreach (var enemyAbility in enemy.Abilities)
                {
                    if (enemyAbility.CanUse(enemy.CurrentHP, enemy.MaxHP))
                    {
                        queuedEnemyAbility = enemyAbility;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Execute the player's queued action
        /// </summary>
        private void ExecutePlayerAction(Player player, Enemy enemy)
        {
            // Apply status effects at start of player's turn (new system)
            player.Effects.ProcessTurnStart();
            Thread.Sleep(500);

            if (queuedPlayerAbility != null)
            {
                // Use ability
                player.CurrentMana -= queuedPlayerAbility.ManaCost;
                queuedPlayerAbility.Use();

                Console.WriteLine();
                AsciiArt.DrawAbilityUse(queuedPlayerAbility.Name);
                ExecuteAbility(player, enemy, queuedPlayerAbility);
                Thread.Sleep(1000);
            }
            else
            {
                // Basic attack
                PerformBasicAttack(player, enemy);
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Execute the enemy's queued action
        /// </summary>
        private void ExecuteEnemyAction(Player player, Enemy enemy)
        {
            UIHelper.PrintColoredLine($"\n{enemy.Name}'s TURN:", ConsoleColor.Red);
            Thread.Sleep(800);

            // Apply status effects at start of enemy's turn (new system)
            enemy.Effects.ProcessTurnStart();
            Thread.Sleep(500);

            // Check for phase transition and show phase message
            if (enemy.AI != null)
            {
                string? phaseMessage = enemy.AI.GetCurrentPhaseMessage(enemy.Name);
                if (!string.IsNullOrEmpty(phaseMessage))
                {
                    UIHelper.PrintColoredLine($"\n⚡ {phaseMessage}", ConsoleColor.Magenta);
                    Thread.Sleep(1000);
                }
            }

            // Check if enemy is stunned
            if (enemyStunNext)
            {
                UIHelper.PrintColoredLine($"⚡ {enemy.Name} is stunned and loses their turn!", ConsoleColor.Yellow);
                enemyStunNext = false;
                Thread.Sleep(1000);
                return;
            }

            // Execute ability or basic attack
            if (queuedEnemyAbility != null)
            {
                var enemyAbility = queuedEnemyAbility;
                enemyAbility.Use();

                UIHelper.PrintColored($"💢 {enemy.Name} uses ", ConsoleColor.Red);
                UIHelper.PrintColored($"{enemyAbility.Ability.Name}", ConsoleColor.Yellow);
                UIHelper.PrintColoredLine($"!", ConsoleColor.Red);
                UIHelper.PrintColoredLine($"   {enemyAbility.Ability.Description}", ConsoleColor.Gray);
                Thread.Sleep(600);

                if (playerDodgeNext)
                {
                    UIHelper.PrintColoredLine($"💨 You dodged {enemy.Name}'s {enemyAbility.Ability.Name}!", ConsoleColor.Cyan);
                    playerDodgeNext = false;
                }
                else
                {
                    // Create ability context (enemy is source, player is target)
                    var context = new AbilityContext(source: enemy, target: player)
                    {
                        PlayerDodgeNext = playerDodgeNext,
                        Random = random,
                        IsPlayerAbility = false
                    };

                    // Execute all effects
                    enemyAbility.Ability.Execute(context);
                }
            }
            else
            {
                // Basic attack
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

            // Notify AI of turn end
            if (enemy.AI != null)
            {
                enemy.AI.OnTurnEnd();
            }

            Thread.Sleep(1000);
        }

        /// <summary>
        /// Apply end of turn effects: regeneration, cooldowns, etc.
        /// </summary>
        private void ApplyEndOfTurnEffects(Player player, Enemy enemy)
        {
            // Regenerate mana
            int manaRegen = (int)(player.MaxMana * GameConfig.Config.ManaRegenRate);
            if (manaRegen > 0)
            {
                player.RestoreMana(manaRegen);
                UIHelper.PrintColoredLine($"💙 Restored {manaRegen} mana", ConsoleColor.Cyan);
                Thread.Sleep(500);
            }

            // Regenerate health
            int healthRegen = (int)(player.MaxHP * GameConfig.Config.HealthRegenRate);
            if (healthRegen > 0)
            {
                player.Heal(healthRegen);
                UIHelper.PrintColoredLine($"❤️  Restored {healthRegen} HP", ConsoleColor.Green);
                Thread.Sleep(500);
            }

            // Status effects are now automatically managed by StatusEffectManager

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


        public bool StartBossBattle(Player player, Enemy boss, bool isMiniboss = false)
        {
            Console.Clear();

            // Boss status effects are automatically initialized via StatusEffectManager

            string bossTitle = isMiniboss ? "MINIBOSS" : "FINAL BOSS";
            UIHelper.PrintColoredLine($"\n╔══════════════════════════════════════════╗", ConsoleColor.Red);
            UIHelper.PrintColoredLine($"║        {bossTitle} ENCOUNTER!        ║", ConsoleColor.Red);
            UIHelper.PrintColoredLine($"╚══════════════════════════════════════════╝\n", ConsoleColor.Red);

            Thread.Sleep(1000);

            UIHelper.PrintColoredLine($"     💀  {boss.Name} appears!  💀", ConsoleColor.DarkRed);
            UIHelper.PrintColoredLine($"\nType: {boss.Type}", ConsoleColor.Gray);
            UIHelper.PrintColoredLine($"HP: {boss.MaxHP} | Attack: {boss.Attack} | Defense: {boss.Defense}", ConsoleColor.Gray);

            if (boss.Abilities != null && boss.Abilities.Count > 0)
            {
                Console.WriteLine("\n⚡ Special Abilities:");
                foreach (var ability in boss.Abilities)
                {
                    UIHelper.PrintColoredLine($"  • {ability.Ability.Name}: {ability.Ability.Description}", ConsoleColor.Yellow);
                }
            }

            Thread.Sleep(2000);

            // Use regular battle logic but disable fleeing
            return StartBattle(player, boss, canFlee: false);
        }

        public bool StartBattle(Player player, Enemy enemy, bool canFlee = true)
        {
            Console.Clear();
            playerDodgeNext = false;
            enemyStunNext = false;
            player.ResetForNewBattle();

            AsciiArt.DrawCombatStart();
            Thread.Sleep(800);

            UIHelper.PrintColoredLine($"\n     ⚔️  {enemy.Name} appears!  ⚔️\n", ConsoleColor.Red);
            UIHelper.PrintColoredLine($"Type: {enemy.Type}", ConsoleColor.Gray);
            AsciiArt.DrawEnemy(enemy.Name);

            Thread.Sleep(1500);

            while (player.CurrentHP > 0 && enemy.CurrentHP > 0)
            {
                DisplayBattleStatus(player, enemy);

                // Reset queued abilities for this turn
                queuedPlayerAbility = null;
                queuedEnemyAbility = null;

                // PHASE 1: Player chooses action (this may queue an ability)
                bool playerFled = PlayerChooseAction(player, enemy, canFlee);
                if (playerFled) return false;
                if (enemy.CurrentHP <= 0) break;

                // PHASE 2: Enemy chooses action (this may queue an ability)
                EnemyChooseAction(player, enemy);
                if (player.CurrentHP <= 0) break;

                // PHASE 3: Determine turn order based on speed and priority
                bool playerGoesFirst = DetermineInitialTurnOrder(player, enemy);

                // PHASE 4: Execute actions in order
                if (playerGoesFirst)
                {
                    ExecutePlayerAction(player, enemy);
                    if (enemy.CurrentHP <= 0) break;
                    ExecuteEnemyAction(player, enemy);
                }
                else
                {
                    ExecuteEnemyAction(player, enemy);
                    if (player.CurrentHP <= 0) break;
                    ExecutePlayerAction(player, enemy);
                }

                // End of turn: regeneration, status effects, cooldowns
                ApplyEndOfTurnEffects(player, enemy);
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

            // Display player status effects using new system
            if (player.Effects.ActiveEffects.Count > 0)
            {
                player.Effects.DisplayAllEffects("Player");
            }

            // Display Dodge Ready separately (not part of status effects system)
            if (playerDodgeNext)
            {
                UIHelper.PrintColoredLine("   💨 Dodge Ready", ConsoleColor.Cyan);
            }

            Console.WriteLine();

            // Enemy status
            UIHelper.PrintColoredLine($"👹 {enemy.Name}", ConsoleColor.Red);
            Console.Write("   ");
            DrawHealthBar(enemy.CurrentHP, enemy.MaxHP, ConsoleColor.Red);

            // Display enemy status effects using new system
            if (enemy.Effects.ActiveEffects.Count > 0 || enemyStunNext)
            {
                enemy.Effects.DisplayAllEffects("Enemy");
            }

            // Show stun status (legacy)
            if (enemyStunNext)
            {
                Console.Write("   [Legacy]: ");
                UIHelper.PrintColoredLine("⚡ Stunned (next turn)", ConsoleColor.DarkGray);
            }

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

        private bool PlayerTurn(Player player, Enemy enemy, bool canFlee)
        {
            UIHelper.PrintColoredLine("YOUR TURN:", ConsoleColor.Yellow);
            Console.WriteLine("1. ⚔️  Attack");
            Console.WriteLine("2. 🎯 Use Ability");
            Console.WriteLine("3. 🧪 Use Potion");
            if (canFlee)
            {
                Console.WriteLine("4. 🏃 Flee");
            }

            Console.Write("\nChoose action: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    PerformBasicAttack(player, enemy);
                    break;
                case "2":
                    UseAbility(player, enemy, canFlee);
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
                        return PlayerTurn(player, enemy, canFlee); // Try again
                    }
                    break;
                case "4":
                    if (canFlee)
                    {
                        return AttemptFlee(player, enemy);
                    }
                    else
                    {
                        UIHelper.PrintColoredLine("\n❌ You cannot flee from this battle!", ConsoleColor.Red);
                        return PlayerTurn(player, enemy, canFlee);
                    }
                default:
                    UIHelper.PrintColoredLine("\n❌ Invalid choice! Please try again.", ConsoleColor.Red);
                    Thread.Sleep(1000);
                    return PlayerTurn(player, enemy, canFlee); // Try again instead of losing turn
            }

            Thread.Sleep(1000);
            return false; // Continue combat
        }

        private bool AttemptFlee(Player player, Enemy enemy)
        {
            // Calculate flee chance based on speed difference
            double baseFleeChance = 0.5; // 50% base chance
            double speedDifference = (player.Speed - enemy.Speed) / 100.0;
            double fleeChance = Math.Clamp(baseFleeChance + speedDifference, 0.2, 0.9);

            UIHelper.PrintColoredLine("\n🏃 Attempting to flee...", ConsoleColor.Yellow);
            Thread.Sleep(1000);

            if (random.NextDouble() < fleeChance)
            {
                // Successful flee - apply penalties
                int goldLost = Math.Min(player.Gold, player.Gold / 4); // Lose 25% of gold
                int hpLost = player.MaxHP / 5; // Lose 20% of max HP

                player.Gold -= goldLost;
                player.CurrentHP = Math.Max(1, player.CurrentHP - hpLost);

                UIHelper.PrintColoredLine("✅ You successfully fled from combat!", ConsoleColor.Green);
                UIHelper.PrintColoredLine($"💰 Lost {goldLost} gold in the escape", ConsoleColor.Red);
                UIHelper.PrintColoredLine($"💔 Lost {hpLost} HP in the escape", ConsoleColor.Red);
                Thread.Sleep(2000);
                return true;
            }
            else
            {
                // Failed to flee - enemy gets a free attack
                UIHelper.PrintColoredLine("❌ Failed to escape! The enemy attacks!", ConsoleColor.Red);
                Thread.Sleep(1000);

                // Enemy performs a basic attack
                int actualDamage = ApplyDamageToPlayer(player, enemy, enemy.Attack);
                UIHelper.PrintColoredLine($"⚔️  {enemy.Name} strikes! {actualDamage} damage!", ConsoleColor.Red);
                Thread.Sleep(1000);

                return false; // Continue combat
            }
        }

        private void PerformBasicAttack(Player player, Enemy enemy)
        {
            int damage = player.GetTotalAttack();

            // Apply damage multiplier from status effects (Battle Rage, etc.)
            double damageMultiplier = player.Effects.GetTotalDamageMultiplier();
            damage = (int)(damage * damageMultiplier);

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

            // Apply damage to enemy using unified method (handles defense, shields, thorns)
            enemy.ApplyDamage(actualDamage, applyShieldAbsorption: true, attacker: player);

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
                enemy.ApplyDamage(chainLightningDmg, applyShieldAbsorption: true, attacker: player);
                UIHelper.PrintColoredLine($"   ⚡ CHAIN LIGHTNING! Deals {chainLightningDmg} bonus damage!", ConsoleColor.Yellow);
            }
            if (bleedProc)
            {
                // Apply bleed using new status effect system
                enemy.ApplyBleed(player, 3, bleedDmg);
                UIHelper.PrintColoredLine($"   🩸 BLEED! {enemy.Name} is bleeding {bleedDmg} damage per turn!", ConsoleColor.DarkRed);
            }
            if (stunProc)
            {
                enemyStunNext = true;
                UIHelper.PrintColoredLine($"   ⚡ STUNNED! {enemy.Name} loses their next turn!", ConsoleColor.Yellow);
            }

            // Thorns damage is now handled automatically by the new status effect system via ProcessTakeDamage hook
        }

        private void UseAbility(Player player, Enemy enemy, bool canFlee)
        {
            var unlockedAbilities = player.Abilities.Where(a => a.IsUnlocked).ToList();

            if (unlockedAbilities.Count == 0)
            {
                UIHelper.PrintColoredLine("\n❌ No abilities unlocked yet!", ConsoleColor.Red);
                Thread.Sleep(1000);
                PlayerTurn(player, enemy, canFlee);
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
                PlayerTurn(player, enemy, canFlee);
                return;
            }

            if (int.TryParse(choice, out int abilityIndex) && abilityIndex > 0 && abilityIndex <= unlockedAbilities.Count)
            {
                var ability = unlockedAbilities[abilityIndex - 1];

                if (!ability.CanUse(player.CurrentMana))
                {
                    UIHelper.PrintColoredLine("\n❌ Cannot use this ability! (Not enough mana or on cooldown)", ConsoleColor.Red);
                    Thread.Sleep(1000);
                    UseAbility(player, enemy, canFlee);
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
                UseAbility(player, enemy, canFlee);
            }
        }

        private void ExecuteAbility(Player player, Enemy enemy, Ability ability)
        {
            UIHelper.PrintColoredLine($"✨ {player.Name} uses {ability.Name}!", ConsoleColor.Magenta);
            UIHelper.PrintColoredLine($"   {ability.Description}", ConsoleColor.Gray);
            Thread.Sleep(600);

            // Create ability context (player is source, enemy is target)
            var context = new AbilityContext(source: player, target: enemy)
            {
                PlayerDodgeNext = playerDodgeNext,
                Random = random,
                IsPlayerAbility = true
            };

            // Execute all effects
            ability.Execute(context);

            // Handle special effects that need to modify combat state
            foreach (var effect in ability.Effects)
            {
                if (effect is DodgeEffect)
                {
                    playerDodgeNext = true;
                }
                // Other effects like Poison are now handled via Player.StatusEffects
            }
        }

        private int ApplyDamageToPlayer(Player player, Enemy enemy, int baseDamage)
        {
            int damage = baseDamage;

            // Apply Shield Wall damage reduction from status effects
            if (player.Effects.HasEffect("shield_wall"))
            {
                damage = damage / 2;
            }

            // Use unified damage application (handles defense, shields, status effect thorns)
            int actualDamage = player.ApplyDamage(damage, applyShieldAbsorption: true, attacker: enemy);

            // Check for equipment-based Thorns effect
            var specialEffects = player.Inventory.GetAllSpecialEffects();
            foreach (var effect in specialEffects)
            {
                if (effect.Type == EffectType.Thorns)
                {
                    // Apply reflected damage to enemy
                    enemy.CurrentHP -= effect.Value;
                    UIHelper.PrintColoredLine($"   🌵 THORNS (Equipment)! Enemy takes {effect.Value} reflected damage!", ConsoleColor.Yellow);
                }
            }

            return actualDamage;
        }
    }
}
