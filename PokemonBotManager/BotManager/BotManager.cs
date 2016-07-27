using System;
using System.Collections.Generic;
using System.Linq;
using PokemonBotManager.BotManager.Exceptions;
using PokemonBotManager.LocationHelper;
using PokemonBotManager.Pokemon;

namespace PokemonBotManager.BotManager
{
    public class BotManager
    {
        private static readonly object SyncRoot = new Object();
        private static bool initialized = false;
        private static BotManager _instance;

        public static BotManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                            _instance = new BotManager();
                    }
                }
                return _instance;
            }
        }

        private int lastAssigned = -1;

        public IReadOnlyCollection<Bot> ReadOnlyBotList => botList.AsReadOnly();

        private List<Bot> botList;

        private BotManager()
        {
            botList = new List<Bot>();
        }

        public Bot RequestBot(Account account)
        {
            var reuseBot = botList.FirstOrDefault(b => b.Settings.AccountData == account);
            if (reuseBot != null)
            {
                return reuseBot;
            }
            ++lastAssigned;
            var bot = new Bot(lastAssigned, account, LocationManager.Instance.GetLocations().FirstOrDefault());
            botList.Add(bot);
            return bot;
            
        }

        public Bot RequestBot(Account account, Location location)
        {
            var bot = RequestBot(account);
            bot.SetLocation(location);
            return bot;

        }

        public void RemoveBot(Bot rBot)
        {
            botList.Remove(rBot);

        }
    }
}
