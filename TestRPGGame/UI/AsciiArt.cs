using System;
using TestRPGGame.DataLoading;

namespace TestRPGGame.UI
{
    public static class AsciiArt
    {
        public static void DrawWarrior()
        {
            UIHelper.PrintColoredLine(@"
                    /)
            /\_/\  //
           /     \//
          /  o o  /
         /  ===  /
        /__________\
           [====]
          /[____]\
         / |    | \
        /  |    |  \
       /   |    |   \
          _|    |_
         [__][__]
        /  \    /  \
", ConsoleColor.Red);
        }

        public static void DrawMage()
        {
            UIHelper.PrintColoredLine(@"
           *    .
       .    ___    *
         .  |___|  .
       *   /\ _ /\   *
          // ( ) \\
         //  \_/  \\
        ||   |||   ||
             |||
            // \\
           //   \\
          (_)   (_)
", ConsoleColor.Magenta);
        }

        public static void DrawRogue()
        {
            UIHelper.PrintColoredLine(@"
              /|
             / |
            /  |___
           |   /o o\
           |  |  >  |
           |   \___/
            \  /||
             \/|/|
              / |
             /| |\
            / | | \
           /  | |  \
", ConsoleColor.Green);
        }

        public static void DrawEnemy(string enemyName)
        {
            // Get art from data-driven system
            var (artLines, color) = EnemyArtDatabase.GetEnemyArt(enemyName);

            if (artLines.Count > 0)
            {
                // Combine all art lines into a single string
                var artString = "\n" + string.Join("\n", artLines) + "\n";
                UIHelper.PrintColoredLine(artString, color);
            }
            else
            {
                // Fallback if no art found
                UIHelper.PrintColoredLine(@"
            .--.
           |o_o |
           |:_/ |
          //   \ \
         (|     | )
        /'\_   _/`\
        \___)=(___/
", ConsoleColor.DarkRed);
            }
        }

        public static void DrawVictory()
        {
            Console.Clear();
            UIHelper.PrintColoredLine(@"
    ██╗   ██╗██╗ ██████╗████████╗ ██████╗ ██████╗ ██╗   ██╗
    ██║   ██║██║██╔════╝╚══██╔══╝██╔═══██╗██╔══██╗╚██╗ ██╔╝
    ██║   ██║██║██║        ██║   ██║   ██║██████╔╝ ╚████╔╝
    ╚██╗ ██╔╝██║██║        ██║   ██║   ██║██╔══██╗  ╚██╔╝
     ╚████╔╝ ██║╚██████╗   ██║   ╚██████╔╝██║  ██║   ██║
      ╚═══╝  ╚═╝ ╚═════╝   ╚═╝    ╚═════╝ ╚═╝  ╚═╝   ╚═╝
", ConsoleColor.Yellow);

            UIHelper.PrintColoredLine(@"
                    🏆
                   /||\
                  / || \
                 |  ||  |
                  \ || /
                   \||/
                    ||
                   /  \
                  /____\
", ConsoleColor.Yellow);
        }

        public static void DrawDefeat()
        {
            Console.Clear();
            UIHelper.PrintColoredLine(@"
    ██████╗ ███████╗███████╗███████╗ █████╗ ████████╗███████╗██████╗
    ██╔══██╗██╔════╝██╔════╝██╔════╝██╔══██╗╚══██╔══╝██╔════╝██╔══██╗
    ██║  ██║█████╗  █████╗  █████╗  ███████║   ██║   █████╗  ██║  ██║
    ██║  ██║██╔══╝  ██╔══╝  ██╔══╝  ██╔══██║   ██║   ██╔══╝  ██║  ██║
    ██████╔╝███████╗██║     ███████╗██║  ██║   ██║   ███████╗██████╔╝
    ╚═════╝ ╚══════╝╚═╝     ╚══════╝╚═╝  ╚═╝   ╚═╝   ╚══════╝╚═════╝
", ConsoleColor.Red);

            UIHelper.PrintColoredLine(@"
                        💀
                       X X
                      /   \
                     |  ~  |
                      \___/
", ConsoleColor.DarkRed);
        }

        public static void DrawLevelUp()
        {
            UIHelper.PrintColoredLine(@"
    ██╗     ███████╗██╗   ██╗███████╗██╗         ██╗   ██╗██████╗ ██╗
    ██║     ██╔════╝██║   ██║██╔════╝██║         ██║   ██║██╔══██╗██║
    ██║     █████╗  ██║   ██║█████╗  ██║         ██║   ██║██████╔╝██║
    ██║     ██╔══╝  ╚██╗ ██╔╝██╔══╝  ██║         ██║   ██║██╔═══╝ ╚═╝
    ███████╗███████╗ ╚████╔╝ ███████╗███████╗    ╚██████╔╝██║     ██╗
    ╚══════╝╚══════╝  ╚═══╝  ╚══════╝╚══════╝     ╚═════╝ ╚═╝     ╚═╝
", ConsoleColor.Magenta);

            UIHelper.PrintColoredLine(@"
                    ⭐
                   ✨ ✨
                  ✨ ⭐ ✨
                   ✨ ✨
                    ⭐
", ConsoleColor.Yellow);
        }

        public static void DrawShop()
        {
            UIHelper.PrintColoredLine(@"
          ___________________
         /\  ___________    \
        /  \ |  🏪 SHOP |    \
       /    \|___________|    \
      /  /\  \    ___    /\    \
     /  /  \  \  |   |  /  \    \
    /__/____\__\ |___| /____\____\
    |  ___  |  |      |  ___  |  |
    | |   | |  | OPEN | |   | |  |
    | |___| |  |      | |___| |  |
    |_______|__|______|_______|__|
", ConsoleColor.Yellow);
        }

        public static void DrawRest()
        {
            UIHelper.PrintColoredLine(@"
         ☁️  ☁️    ⭐
      ☁️         ☁️      ✨
           😴           ⭐
          /||\    ☁️
         / || \        ☁️
        |  ||  |
         \ || /
          \||/    💤  💤
          / \       💤
         /   \
    ══════════════════════════
", ConsoleColor.Cyan);
        }

        public static void DrawCriticalHit()
        {
            UIHelper.PrintColoredLine(@"
    💥💥💥💥💥💥💥💥💥💥
    💥  CRITICAL HIT!  💥
    💥💥💥💥💥💥💥💥💥💥
", ConsoleColor.Yellow);
        }

        public static void DrawAbilityUse(string abilityName)
        {
            if (abilityName.Contains("Fire") || abilityName.Contains("Rage"))
            {
                UIHelper.PrintColoredLine(@"
        🔥  🔥  🔥
      🔥  🔥  🔥  🔥
        🔥  🔥  🔥
", ConsoleColor.Red);
            }
            else if (abilityName.Contains("Ice"))
            {
                UIHelper.PrintColoredLine(@"
        ❄️  ❄️  ❄️
      ❄️  ❄️  ❄️  ❄️
        ❄️  ❄️  ❄️
", ConsoleColor.Cyan);
            }
            else if (abilityName.Contains("Shadow") || abilityName.Contains("Poison"))
            {
                UIHelper.PrintColoredLine(@"
        💚  💀  💚
      💀  💚  💀  💚
        💚  💀  💚
", ConsoleColor.Green);
            }
            else if (abilityName.Contains("Shield"))
            {
                UIHelper.PrintColoredLine(@"
         🛡️  🛡️
       🛡️  🛡️  🛡️
         🛡️  🛡️
", ConsoleColor.Blue);
            }
            else
            {
                UIHelper.PrintColoredLine(@"
        ⚡  ⚡  ⚡
      ⚡  ⚡  ⚡  ⚡
        ⚡  ⚡  ⚡
", ConsoleColor.Yellow);
            }
        }

        public static void DrawCombatStart()
        {
            UIHelper.PrintColoredLine(@"
    ⚔️═══════════════════════════════════════════════⚔️
    ║                                                 ║
    ║            ⚡ BATTLE BEGINS! ⚡                 ║
    ║                                                 ║
    ⚔️═══════════════════════════════════════════════⚔️
", ConsoleColor.Red);
        }

        public static void DrawLootDrop()
        {
            UIHelper.PrintColoredLine(@"
            ✨  ✨
          ✨  📦  ✨
            ✨  ✨
         LOOT DROPPED!
", ConsoleColor.Yellow);
        }

        public static void DrawGoldReward()
        {
            UIHelper.PrintColoredLine(@"
            💰
          💰 💰 💰
            💰
", ConsoleColor.Yellow);
        }
    }
}
