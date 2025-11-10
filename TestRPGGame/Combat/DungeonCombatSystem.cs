using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TestRPGGame.Entities.Player;
using TestRPGGame.Entities.Enemy;
using TestRPGGame.Entities.Boss;
using TestRPGGame.Entities.Dungeon;
using TestRPGGame.Abilities.Effects;
using TestRPGGame.Equipment;
using TestRPGGame.Systems;
using TestRPGGame.UI;

namespace TestRPGGame.Combat
{
    public class DungeonCombatSystem : CombatSystem
    {
        public bool StartBossBattle(Player player, Enemy boss, bool isMiniboss)
        {
            Console.Clear();
            boss.StatusEffects = new BossStatusEffects();

            string bossTitle = isMiniboss ? "MINIBOSS" : "FINAL BOSS";
            UIHelper.PrintColoredLine($"\n╔══════════════════════════════════════════╗", ConsoleColor.Red);
            UIHelper.PrintColoredLine($"║        {bossTitle} ENCOUNTER!        ║", ConsoleColor.Red);
            UIHelper.PrintColoredLine($"╚══════════════════════════════════════════╝\n", ConsoleColor.Red);

            Thread.Sleep(1000);

            UIHelper.PrintColoredLine($"     💀  {boss.Name} appears!  💀", ConsoleColor.DarkRed);
            UIHelper.PrintColoredLine($"\nType: {boss.Type}", ConsoleColor.Gray);
            UIHelper.PrintColoredLine($"HP: {boss.MaxHP} | Attack: {boss.Attack} | Defense: {boss.Defense}", ConsoleColor.Gray);

            if (boss.BossAbilities != null && boss.BossAbilities.Count > 0)
            {
                Console.WriteLine("\n⚡ Special Abilities:");
                foreach (var ability in boss.BossAbilities)
                {
                    UIHelper.PrintColoredLine($"  • {ability.Name}: {ability.Description}", ConsoleColor.Yellow);
                }
            }

            Thread.Sleep(2000);

            // Use base combat logic with boss modifications
            return StartBossBattleLogic(player, boss);
        }

        private bool StartBossBattleLogic(Player player, Enemy boss)
        {
            player.ResetForNewBattle();
            var activeBuffs = new Dictionary<string, int>();
            bool playerDodgeNext = false;
            int poisonDamage = 0;
            int poisonTurns = 0;
            int bleedDamage = 0;
            int bleedTurns = 0;
            bool enemyStunNext = false;
            var random = new Random();

            // Determine turn order based on speed
            bool playerTurn = player.Speed >= boss.Speed;

            while (player.CurrentHP > 0 && boss.CurrentHP > 0)
            {
                DisplayBossBattleStatus(player, boss);

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

                    // Apply poison damage at start of player's turn (on the boss)
                    if (poisonTurns > 0)
                    {
                        boss.CurrentHP -= poisonDamage;
                        UIHelper.PrintColoredLine($"💚 Poison deals {poisonDamage} damage to {boss.Name}!", ConsoleColor.Green);
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

                    if (boss.StatusEffects!.StunTurnsRemaining > 0)
                    {
                        UIHelper.PrintColoredLine($"\n⏸️  {boss.Name} is stunned and cannot act!", ConsoleColor.Cyan);
                        boss.StatusEffects.StunTurnsRemaining--;
                    }
                    else
                    {
                        PlayerTurnInBossFight(player, boss, ref activeBuffs, ref playerDodgeNext, ref poisonDamage, ref poisonTurns, ref bleedDamage, ref bleedTurns, ref enemyStunNext, random);
                        if (boss.CurrentHP <= 0) break;
                    }
                    playerTurn = false;
                }
                else
                {
                    // Apply bleed damage at start of boss turn
                    if (bleedTurns > 0)
                    {
                        player.CurrentHP -= bleedDamage;
                        UIHelper.PrintColoredLine($"🩸 Bleeding! You take {bleedDamage} damage!", ConsoleColor.Red);
                        bleedTurns--;
                        Thread.Sleep(800);
                    }

                    // Boss status effects tick at start of boss turn
                    boss.StatusEffects!.ApplyTurnEffects(boss, player);

                    // Check if boss is stunned
                    if (enemyStunNext)
                    {
                        UIHelper.PrintColoredLine($"⚡ {boss.Name} is stunned and loses their turn!", ConsoleColor.Yellow);
                        enemyStunNext = false;
                    }
                    else
                    {
                        BossTurn(player, boss, ref playerDodgeNext, ref activeBuffs, random);
                    }
                    if (player.CurrentHP <= 0) break;
                    playerTurn = true;
                }

                // Reduce ability cooldowns
                foreach (var ability in player.Abilities)
                {
                    ability.ReduceCooldown();
                }
                if (boss.BossAbilities != null)
                {
                    foreach (var ability in boss.BossAbilities)
                    {
                        ability.ReduceCooldown();
                    }
                }
            }

            return player.CurrentHP > 0;
        }

        private void DisplayBossBattleStatus(Player player, Enemy boss)
        {
            Console.WriteLine("\n" + new string('═', 70));

            // Player status
            UIHelper.PrintColoredLine($"👤 {player.Name} (Level {player.Level} {player.Class})", ConsoleColor.Cyan);
            Console.Write("   ");
            DrawHealthBar(player.CurrentHP, player.MaxHP, ConsoleColor.Green);
            Console.Write("   ");
            DrawManaBar(player.CurrentMana, player.MaxMana, ConsoleColor.Blue);

            Console.WriteLine();

            // Boss status
            UIHelper.PrintColoredLine($"💀 {boss.Name}", ConsoleColor.Red);
            Console.Write("   ");
            DrawHealthBar(boss.CurrentHP, boss.MaxHP, ConsoleColor.Red);

            // Show active boss effects
            if (boss.StatusEffects != null)
            {
                Console.Write("   ");
                ShowBossEffects(boss.StatusEffects);
            }

            Console.WriteLine("\n" + new string('═', 70) + "\n");
        }

        private void ShowBossEffects(BossStatusEffects effects)
        {
            List<string> activeEffects = new List<string>();

            if (effects.HealOverTimeTurns > 0)
                activeEffects.Add($"💚 Regen({effects.HealOverTimeTurns})");
            if (effects.ThornsTurns > 0)
                activeEffects.Add($"🌵 Thorns({effects.ThornsTurns})");
            if (effects.ShieldTurns > 0)
                activeEffects.Add($"🛡️ Shield:{effects.ShieldValue}({effects.ShieldTurns})");
            if (effects.IsEnraged)
                activeEffects.Add($"😤 Enraged({effects.EnrageTurns})");
            if (effects.StunTurnsRemaining > 0)
                activeEffects.Add($"⏸️ Stunned({effects.StunTurnsRemaining})");

            if (activeEffects.Count > 0)
            {
                UIHelper.PrintColored("[Effects: " + string.Join(", ", activeEffects) + "]", ConsoleColor.Yellow);
            }
            Console.WriteLine();
        }

        private void DrawHealthBar(int current, int max, ConsoleColor color)
        {
            int barLength = 25;
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
            int barLength = 25;
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

        private void PlayerTurnInBossFight(Player player, Enemy boss, ref Dictionary<string, int> activeBuffs,
            ref bool playerDodgeNext, ref int poisonDamage, ref int poisonTurns, ref int bleedDamage, ref int bleedTurns, ref bool enemyStunNext, Random random)
        {
            UIHelper.PrintColoredLine("YOUR TURN:", ConsoleColor.Yellow);
            Console.WriteLine("1. ⚔️  Attack");
            Console.WriteLine("2. 🎯 Use Ability");
            Console.WriteLine("3. 🧪 Use Potion");

            Console.Write("\nChoose action: ");
            string choice = Console.ReadLine() ?? "";

            switch (choice)
            {
                case "1":
                    PerformBossAttack(player, boss, activeBuffs, ref bleedDamage, ref bleedTurns, ref enemyStunNext, random);
                    break;
                case "2":
                    UseBossAbility(player, boss, activeBuffs, ref playerDodgeNext, ref poisonDamage, ref poisonTurns, ref bleedDamage, ref bleedTurns, ref enemyStunNext, random);
                    break;
                case "3":
                    if (player.UsePotion())
                    {
                        UIHelper.PrintColoredLine($"\n🧪 You used a potion and restored HP!", ConsoleColor.Green);
                        UIHelper.PrintColoredLine($"Potions remaining: {player.PotionCount}", ConsoleColor.Gray);
                    }
                    else
                    {
                        UIHelper.PrintColoredLine("\n❌ No potions left!", ConsoleColor.Red);
                        PlayerTurnInBossFight(player, boss, ref activeBuffs, ref playerDodgeNext, ref poisonDamage, ref poisonTurns, ref bleedDamage, ref bleedTurns, ref enemyStunNext, random);
                        return;
                    }
                    break;
                default:
                    UIHelper.PrintColoredLine("\n❌ Invalid choice! Lost your turn!", ConsoleColor.Red);
                    break;
            }

            Thread.Sleep(1000);
        }

        private void PerformBossAttack(Player player, Enemy boss, Dictionary<string, int> activeBuffs, ref int bleedDamage, ref int bleedTurns, ref bool enemyStunNext, Random random)
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

            // Check for shield
            int actualDamage = Math.Max(1, damage - boss.Defense);

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

            if (boss.StatusEffects!.ShieldValue > 0)
            {
                int shieldAbsorb = Math.Min(boss.StatusEffects.ShieldValue, actualDamage);
                boss.StatusEffects.ShieldValue -= shieldAbsorb;
                actualDamage -= shieldAbsorb;

                UIHelper.PrintColoredLine($"🛡️  Shield absorbs {shieldAbsorb} damage!", ConsoleColor.Cyan);

                if (boss.StatusEffects.ShieldValue == 0)
                {
                    boss.StatusEffects.ShieldTurns = 0;
                    UIHelper.PrintColoredLine($"🛡️  Shield shattered!", ConsoleColor.Gray);
                }
            }

            boss.CurrentHP -= actualDamage;

            // Apply thorns
            if (boss.StatusEffects.ThornsValue > 0)
            {
                player.CurrentHP -= boss.StatusEffects.ThornsValue;
                UIHelper.PrintColoredLine($"🌵 Thorns damage! You take {boss.StatusEffects.ThornsValue} damage!", ConsoleColor.Red);
            }

            if (isCrit)
            {
                UIHelper.PrintColoredLine($"\n💥 CRITICAL HIT! You deal {actualDamage} damage!", ConsoleColor.Yellow);
            }
            else
            {
                UIHelper.PrintColoredLine($"⚔️  You deal {actualDamage} damage!", ConsoleColor.White);
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
                boss.CurrentHP -= chainLightningDmg;
                UIHelper.PrintColoredLine($"   ⚡ CHAIN LIGHTNING! Deals {chainLightningDmg} bonus damage!", ConsoleColor.Yellow);
            }
            if (bleedProc)
            {
                bleedDamage = bleedDmg;
                bleedTurns = 3; // Bleed lasts 3 turns
                UIHelper.PrintColoredLine($"   🩸 BLEED! Boss inflicts {bleedDmg} damage per turn!", ConsoleColor.DarkRed);
            }
            if (stunProc)
            {
                enemyStunNext = true;
                UIHelper.PrintColoredLine($"   ⚡ STUNNED! {boss.Name} loses their next turn!", ConsoleColor.Yellow);
            }
        }

        private void UseBossAbility(Player player, Enemy boss, Dictionary<string, int> activeBuffs,
            ref bool playerDodgeNext, ref int poisonDamage, ref int poisonTurns, ref int bleedDamage, ref int bleedTurns, ref bool enemyStunNext, Random random)
        {
            var unlockedAbilities = player.Abilities.Where(a => a.IsUnlocked).ToList();

            if (unlockedAbilities.Count == 0)
            {
                UIHelper.PrintColoredLine("\n❌ No abilities unlocked yet!", ConsoleColor.Red);
                Thread.Sleep(1000);
                PlayerTurnInBossFight(player, boss, ref activeBuffs, ref playerDodgeNext, ref poisonDamage, ref poisonTurns, ref bleedDamage, ref bleedTurns, ref enemyStunNext, random);
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
            string choice = Console.ReadLine() ?? "";

            if (choice == "0")
            {
                PlayerTurnInBossFight(player, boss, ref activeBuffs, ref playerDodgeNext, ref poisonDamage, ref poisonTurns, ref bleedDamage, ref bleedTurns, ref enemyStunNext, random);
                return;
            }

            if (int.TryParse(choice, out int abilityIndex) && abilityIndex > 0 && abilityIndex <= unlockedAbilities.Count)
            {
                var ability = unlockedAbilities[abilityIndex - 1];

                if (!ability.CanUse(player.CurrentMana))
                {
                    UIHelper.PrintColoredLine("\n❌ Cannot use this ability!", ConsoleColor.Red);
                    Thread.Sleep(1000);
                    UseBossAbility(player, boss, activeBuffs, ref playerDodgeNext, ref poisonDamage, ref poisonTurns, ref bleedDamage, ref bleedTurns, ref enemyStunNext, random);
                    return;
                }

                player.CurrentMana -= ability.ManaCost;
                ability.Use();

                Console.WriteLine();
                UIHelper.PrintColoredLine($"✨ {player.Name} uses {ability.Name}!", ConsoleColor.Magenta);

                // Execute ability with boss context
                var context = new AbilityContext(player, boss)
                {
                    ActiveBuffs = activeBuffs,
                    Random = random
                };

                ability.Execute(context);

                // Handle special effects
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
            else
            {
                UIHelper.PrintColoredLine("\n❌ Invalid choice!", ConsoleColor.Red);
                Thread.Sleep(1000);
                UseBossAbility(player, boss, activeBuffs, ref playerDodgeNext, ref poisonDamage, ref poisonTurns, ref bleedDamage, ref bleedTurns, ref enemyStunNext, random);
            }
        }

        private void BossTurn(Player player, Enemy boss, ref bool playerDodgeNext, ref Dictionary<string, int> activeBuffs, Random random)
        {
            UIHelper.PrintColoredLine($"\n💀 {boss.Name}'s TURN:", ConsoleColor.Red);
            Thread.Sleep(800);

            // Boss uses special abilities
            if (boss.BossAbilities != null && boss.BossAbilities.Count > 0)
            {
                // Try to use a boss ability (60% chance if available)
                var availableAbilities = boss.BossAbilities.Where(a => a.CanUse()).ToList();

                if (availableAbilities.Count > 0 && random.Next(100) < 60)
                {
                    var ability = availableAbilities[random.Next(availableAbilities.Count)];
                    ability.Use();

                    UIHelper.PrintColoredLine($"⚡ {boss.Name} uses {ability.Name}!", ConsoleColor.DarkMagenta);
                    Thread.Sleep(500);

                    ExecuteBossAbility(player, boss, ability, ref playerDodgeNext, random);
                    return;
                }
            }

            // Regular attack
            if (playerDodgeNext)
            {
                UIHelper.PrintColoredLine($"💨 You dodged {boss.Name}'s attack!", ConsoleColor.Cyan);
                playerDodgeNext = false;
            }
            else
            {
                int damage = boss.Attack;

                // Apply enrage
                if (boss.StatusEffects!.IsEnraged)
                {
                    damage = (int)(damage * boss.StatusEffects.EnrageDamageMultiplier);
                    UIHelper.PrintColoredLine($"😤 ENRAGED! Damage increased!", ConsoleColor.Red);
                }

                // Apply shield wall
                if (activeBuffs.ContainsKey("Shield Wall"))
                {
                    damage = damage / 2;
                }

                int actualDamage = Math.Max(1, damage - player.GetTotalDefense());
                player.CurrentHP -= actualDamage;

                UIHelper.PrintColoredLine($"⚔️  {boss.Name} attacks! {actualDamage} damage!", ConsoleColor.Red);
            }

            Thread.Sleep(1000);
        }

        private void ExecuteBossAbility(Player player, Enemy boss, BossAbility ability, ref bool playerDodgeNext, Random random)
        {
            // Execute all effects in the ability
            foreach (var effect in ability.Effects)
            {
                ExecuteSingleBossEffect(player, boss, effect, ref playerDodgeNext, random);
            }
        }

        private void ExecuteSingleBossEffect(Player player, Enemy boss, BossAbilityEffect effect, ref bool playerDodgeNext, Random random)
        {
            switch (effect.Type)
            {
                case BossAbilityEffectType.HealOverTime:
                    boss.StatusEffects!.HealOverTimeTurns = effect.Duration;
                    boss.StatusEffects.HealOverTimeAmount = effect.Value;
                    UIHelper.PrintColoredLine($"💚 {boss.Name} begins regenerating {effect.Value} HP per turn!", ConsoleColor.Green);
                    break;

                case BossAbilityEffectType.DamageOverTime:
                    boss.StatusEffects!.DamageOverTimeTurns = effect.Duration;
                    boss.StatusEffects.DamageOverTimeAmount = effect.Value;
                    UIHelper.PrintColoredLine($"🔥 You're burning! Taking {effect.Value} damage per turn!", ConsoleColor.Red);
                    break;

                case BossAbilityEffectType.Stun:
                    if (!playerDodgeNext)
                    {
                        player.CurrentHP -= (int)(boss.Attack * effect.Multiplier);
                        UIHelper.PrintColoredLine($"⚡ STUNNED! You take {(int)(boss.Attack * effect.Multiplier)} damage and lose your next turn!", ConsoleColor.Red);
                        // Note: Stun implementation would require turn skipping
                    }
                    else
                    {
                        UIHelper.PrintColoredLine($"💨 You dodged the stun!", ConsoleColor.Cyan);
                        playerDodgeNext = false;
                    }
                    break;

                case BossAbilityEffectType.Thorns:
                    boss.StatusEffects!.ThornsValue = effect.Value;
                    boss.StatusEffects.ThornsTurns = effect.Duration;
                    UIHelper.PrintColoredLine($"🌵 {boss.Name} is surrounded by thorns! Reflecting {effect.Value} damage!", ConsoleColor.Yellow);
                    break;

                case BossAbilityEffectType.Enrage:
                    boss.StatusEffects!.IsEnraged = true;
                    boss.StatusEffects.EnrageTurns = effect.Duration;
                    boss.StatusEffects.EnrageDamageMultiplier = effect.Multiplier;
                    UIHelper.PrintColoredLine($"😤 {boss.Name} ENRAGES! Damage increased by {(effect.Multiplier - 1) * 100}%!", ConsoleColor.Red);
                    break;

                case BossAbilityEffectType.Shield:
                    boss.StatusEffects!.ShieldValue = effect.Value;
                    boss.StatusEffects.ShieldTurns = effect.Duration;
                    UIHelper.PrintColoredLine($"🛡️  {boss.Name} creates a shield absorbing {effect.Value} damage!", ConsoleColor.Cyan);
                    break;

                case BossAbilityEffectType.LifeSteal:
                    if (!playerDodgeNext)
                    {
                        int damage = (int)(boss.Attack * effect.Multiplier);
                        int actualDamage = Math.Max(1, damage - player.GetTotalDefense());
                        player.CurrentHP -= actualDamage;

                        int heal = effect.Value;
                        boss.CurrentHP = Math.Min(boss.MaxHP, boss.CurrentHP + heal);

                        UIHelper.PrintColoredLine($"💉 {boss.Name} drains your life! {actualDamage} damage dealt, {heal} HP stolen!", ConsoleColor.DarkRed);
                    }
                    else
                    {
                        UIHelper.PrintColoredLine($"💨 You dodged the life drain!", ConsoleColor.Cyan);
                        playerDodgeNext = false;
                    }
                    break;

                case BossAbilityEffectType.Bleed:
                    if (!playerDodgeNext)
                    {
                        int damage = (int)(boss.Attack * effect.Multiplier);
                        int actualDamage = Math.Max(1, damage - player.GetTotalDefense());
                        player.CurrentHP -= actualDamage;

                        boss.StatusEffects!.BleedTurns = effect.Duration;
                        boss.StatusEffects.BleedAmount = effect.Value;

                        UIHelper.PrintColoredLine($"🩸 Vicious strike! {actualDamage} damage and bleeding for {effect.Value}/turn!", ConsoleColor.DarkRed);
                    }
                    else
                    {
                        UIHelper.PrintColoredLine($"💨 You dodged the attack!", ConsoleColor.Cyan);
                        playerDodgeNext = false;
                    }
                    break;

                case BossAbilityEffectType.HeavyStrike:
                    if (!playerDodgeNext)
                    {
                        int damage = (int)(boss.Attack * effect.Multiplier + effect.Value);
                        int actualDamage = Math.Max(1, damage - player.GetTotalDefense());
                        player.CurrentHP -= actualDamage;

                        UIHelper.PrintColoredLine($"💥 DEVASTATING STRIKE! {actualDamage} damage!", ConsoleColor.DarkRed);
                    }
                    else
                    {
                        UIHelper.PrintColoredLine($"💨 You dodged the heavy strike!", ConsoleColor.Cyan);
                        playerDodgeNext = false;
                    }
                    break;

                case BossAbilityEffectType.StatBoost:
                    boss.Attack += effect.Value;
                    UIHelper.PrintColoredLine($"⚡ {boss.Name}'s attack increases by {effect.Value}!", ConsoleColor.Red);
                    break;
            }

            Thread.Sleep(1000);
        }
    }
}
