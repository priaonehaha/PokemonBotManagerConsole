using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonBotManager.BotManager;
using PokemonBotManager.Pokemon;
using PokemonGoBotLogic;

namespace BotManagerConsole
{
    static class Menu //Ghetto as fuck m8
    {//Got Cancer from writting this
        private static int MainMenu()
        {
            Console.Clear();
            /*
             * TODO:
             * Check Accounts
             * Check Locations
             * */
            Console.WriteLine("Bot Manager by Andre - Max Bots: " + PokemonBotManager.Properties.Settings.Default.MaxBots);
            Console.WriteLine("1) Check Bot Status");
            Console.WriteLine("2) Check Accounts");
            Console.WriteLine("3) Check Locations");
            Console.WriteLine("4) Exit");
            try
            {
                return int.Parse(Console.ReadLine());
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /**
         * TODO:
         * Manage each bot
         */
        private static void BotMenuDoStuff(int option)
        {
            switch (option)
            {
                case -1:
                    return;
                case 0:
                    foreach (var account in AccountList.Instance.Accounts)
                    {
                       var bot =  BotManager.Instance.RequestBot(account);
                        bot.SetLogic(new Logic(bot.Client));
                    }
                    break;
                case 1:
                    foreach (Bot bot in BotManager.Instance.ReadOnlyBotList.Where(b => !b.IsWorking))
                    {
                        bot.StartBot();
                    }
                    break;
            }
        }

        private static int BotMenu()
        {
            Console.Clear();
            var botList = BotManager.Instance.ReadOnlyBotList;
            var UnassignedAccs = AccountList.Instance.Accounts.Count - botList.Count;
            Console.WriteLine($"Unassigned accounts {UnassignedAccs}");
            if (botList.Count == 0)
            {
                Console.WriteLine("Not bots Created");
            }
            foreach (var bot in botList)
            {
                bot.PrintBotStatus();
            }
            if (UnassignedAccs > 0)
            {
                Console.WriteLine("0) Created bots for missing accounts");
            }
            if (botList.Count > 0)
            {
                Console.WriteLine("1) Start all bots");
            }
            try
            {
                return int.Parse(Console.ReadLine());
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public static void MainLoop()
        {
            while (true)
            {
                switch (MainMenu())
                {
                    case 1:
                        BotMenuDoStuff(BotMenu());
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                    case 4:
                        return;
                }
            }
        }
    }
}
