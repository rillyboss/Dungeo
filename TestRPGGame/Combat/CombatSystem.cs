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
                queuedEnemyAbility = enemy.AI.SelectAbility(enemy, player, activeBuffs);
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

            // Also apply old system effects during migration
            if (player.StatusEffects != null)
            {
                player.StatusEffects.ApplyPlayerTurnEffects(player, enemy);
            }
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

            // Also apply old system effects during migration
            if (enemy.StatusEffects != null)
            {
                enemy.StatusEffects.ApplyEnemyTurnEffects(enemy, player);
            }
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
                    ExecuteEnemyAbilityEffects(player, enemy, enemyAbility);
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

            // Tick down legacy activeBuffs (for backwards compatibility with Battle Rage, etc.)
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

        /// <summary>
        /// Execute enemy ability effects
        /// </summary>
        private void ExecuteEnemyAbilityEffects(Player player, Enemy enemy, EnemyAbility enemyAbility)
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
                    // Enemy applies burning/DOT to player via player StatusEffects
                    if (player.StatusEffects != null)
                    {
                        player.StatusEffects.DamageOverTimeAmount = poisonEffect.DamagePerTurn;
                        player.StatusEffects.DamageOverTimeTurns = poisonEffect.Duration;
                        UIHelper.PrintColoredLine($"   🔥 You are burning! ({poisonEffect.DamagePerTurn} damage/turn for {poisonEffect.Duration} turns)", ConsoleColor.Red);
                    }
                }
                else if (effect is RestoreEffect restoreEffect)
                {
                    // Enemy heals itself
                    int healAmount = Math.Min(restoreEffect.Amount, enemy.MaxHP - enemy.CurrentHP);
                    enemy.CurrentHP += healAmount;
                    UIHelper.PrintColoredLine($"   💚 {enemy.Name} heals for {healAmount} HP!", ConsoleColor.Green);

                    // Notify AI that enemy used a heal ability
                    if (enemy.AI != null)
                    {
                        enemy.AI.RecordHealUsed();
                    }
                }
                else if (effect is HealOverTimeEffect hotEffect)
                {
                    // Apply heal over time to enemy
                    if (enemy.StatusEffects != null)
                    {
                        enemy.StatusEffects.HealOverTimeAmount = hotEffect.HealPerTurn;
                        enemy.StatusEffects.HealOverTimeTurns = hotEffect.Duration;
                        UIHelper.PrintColoredLine($"   💚 {enemy.Name} begins regenerating! ({hotEffect.HealPerTurn} HP/turn for {hotEffect.Duration} turns)", ConsoleColor.Green);
                    }
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
                else if (effect is ThornsEffect thornsEffect)
                {
                    // Apply Thorns to enemy
                    if (enemy.StatusEffects != null)
                    {
                        enemy.StatusEffects.ThornsValue = thornsEffect.ReflectDamage;
                        enemy.StatusEffects.ThornsTurns = thornsEffect.Duration;
                        UIHelper.PrintColoredLine($"   🌵 {enemy.Name} is surrounded by thorns! ({thornsEffect.ReflectDamage} damage reflection for {thornsEffect.Duration} turns)", ConsoleColor.Yellow);
                    }
                }

                Thread.Sleep(500);
            }
        }

        public bool StartBossBattle(Player player, Enemy boss, bool isMiniboss = false)
        {
            Console.Clear();

            // Initialize boss status effects
            boss.EnsureStatusEffects();

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
            activeBuffs.Clear();
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

            // Legacy display (keep during migration)
            List<string> legacyPlayerEffects = new List<string>();
            if (activeBuffs.ContainsKey("Battle Rage"))
                legacyPlayerEffects.Add($"⚡ Battle Rage ({activeBuffs["Battle Rage"]} turns)");
            if (activeBuffs.ContainsKey("Shield Wall"))
                legacyPlayerEffects.Add($"🛡️  Shield Wall ({activeBuffs["Shield Wall"]} turns)");
            if (playerDodgeNext)
                legacyPlayerEffects.Add($"💨 Dodge Ready");
            if (legacyPlayerEffects.Count > 0)
            {
                Console.Write("   [Legacy Effects]: ");
                UIHelper.PrintColoredLine(string.Join(", ", legacyPlayerEffects), ConsoleColor.DarkGray);
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
                // Apply bleed via enemy's status effects
                if (enemy.StatusEffects != null)
                {
                    enemy.StatusEffects.BleedAmount = bleedDmg;
                    enemy.StatusEffects.BleedTurns = 3; // Bleed lasts 3 turns
                }
                UIHelper.PrintColoredLine($"   🩸 BLEED! {enemy.Name} is bleeding {bleedDmg} damage per turn!", ConsoleColor.DarkRed);
            }
            if (stunProc)
            {
                enemyStunNext = true;
                UIHelper.PrintColoredLine($"   ⚡ STUNNED! {enemy.Name} loses their next turn!", ConsoleColor.Yellow);
            }

            // Check if enemy has Thorns active and reflect damage to player
            if (enemy.StatusEffects != null && enemy.StatusEffects.ThornsValue > 0)
            {
                player.CurrentHP -= enemy.StatusEffects.ThornsValue;
                UIHelper.PrintColoredLine($"   🌵 THORNS! You take {enemy.StatusEffects.ThornsValue} reflected damage!", ConsoleColor.Yellow);
            }
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

            // Create ability context
            var context = new AbilityContext(player, enemy)
            {
                ActiveBuffs = activeBuffs,
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

        private void EnemyTurn(Player player, Enemy enemy)
        {
            UIHelper.PrintColoredLine($"\n{enemy.Name}'s TURN:", ConsoleColor.Red);
            Thread.Sleep(800);

            // Apply status effects at start of enemy turn
            if (enemy.StatusEffects != null)
            {
                enemy.StatusEffects.ApplyEnemyTurnEffects(enemy, player);
                Thread.Sleep(500);
            }

            // Reduce cooldowns on all enemy abilities
            foreach (var enemyAbility in enemy.Abilities)
            {
                enemyAbility.ReduceCooldown();
            }

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

            // Use AI to select ability if available, otherwise fall back to simple logic
            EnemyAbility? selectedAbility = null;
            if (enemy.AI != null)
            {
                selectedAbility = enemy.AI.SelectAbility(enemy, player, activeBuffs);
            }
            else
            {
                // Fallback: simple iteration through abilities (old behavior)
                foreach (var enemyAbility in enemy.Abilities)
                {
                    if (enemyAbility.CanUse(enemy.CurrentHP, enemy.MaxHP))
                    {
                        selectedAbility = enemyAbility;
                        break;
                    }
                }
            }

            // Execute selected ability
            bool usedAbility = false;
            if (selectedAbility != null)
            {
                var enemyAbility = selectedAbility;
                enemyAbility.Use();

                // Execute ability effects
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
                            // Enemy applies burning/DOT to player via StatusEffects
                            if (enemy.StatusEffects != null)
                            {
                                enemy.StatusEffects.DamageOverTimeAmount = poisonEffect.DamagePerTurn;
                                enemy.StatusEffects.DamageOverTimeTurns = poisonEffect.Duration;
                                UIHelper.PrintColoredLine($"   🔥 You are burning! ({poisonEffect.DamagePerTurn} damage/turn for {poisonEffect.Duration} turns)", ConsoleColor.Red);
                            }
                        }
                        else if (effect is RestoreEffect restoreEffect)
                        {
                            // Enemy heals itself
                            int healAmount = Math.Min(restoreEffect.Amount, enemy.MaxHP - enemy.CurrentHP);
                            enemy.CurrentHP += healAmount;
                            UIHelper.PrintColoredLine($"   💚 {enemy.Name} heals for {healAmount} HP!", ConsoleColor.Green);

                            // Notify AI that enemy used a heal ability
                            if (enemy.AI != null)
                            {
                                enemy.AI.RecordHealUsed();
                            }
                        }
                        else if (effect is HealOverTimeEffect hotEffect)
                        {
                            // Apply heal over time to enemy
                            if (enemy.StatusEffects != null)
                            {
                                enemy.StatusEffects.HealOverTimeAmount = hotEffect.HealPerTurn;
                                enemy.StatusEffects.HealOverTimeTurns = hotEffect.Duration;
                                UIHelper.PrintColoredLine($"   💚 {enemy.Name} begins regenerating! ({hotEffect.HealPerTurn} HP/turn for {hotEffect.Duration} turns)", ConsoleColor.Green);
                            }
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
                        else if (effect is ThornsEffect thornsEffect)
                        {
                            // Apply Thorns to enemy
                            if (enemy.StatusEffects != null)
                            {
                                enemy.StatusEffects.ThornsValue = thornsEffect.ReflectDamage;
                                enemy.StatusEffects.ThornsTurns = thornsEffect.Duration;
                                UIHelper.PrintColoredLine($"   🌵 {enemy.Name} is surrounded by thorns! ({thornsEffect.ReflectDamage} damage reflection for {thornsEffect.Duration} turns)", ConsoleColor.Yellow);
                            }
                        }

                        Thread.Sleep(500);
                    }
                }

                usedAbility = true;
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

            // Notify AI of turn end (for memory and tracking)
            if (enemy.AI != null)
            {
                enemy.AI.OnTurnEnd();
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
