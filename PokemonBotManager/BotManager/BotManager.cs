using System;
using System.Linq;
using PokemonBotManager.BotManager.Exceptions;
using PokemonBotManager.Pokemon;

namespace PokemonBotManager.BotManager
{
    public class BotManager
    {
        private static readonly object SyncRoot = new Object();
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

        public int MaxBots { get; } = Properties.Settings.Default.MaxBots;
        public int BotCount { get; private set; } = 0;

        private readonly Bot[] botList;

        private BotManager()
        {
            botList = new Bot[MaxBots];
        }

        public Bot RequestBot(Account account)
        {
            if (MaxBots <= BotCount)
            {
                throw new BotLimitExceededException();
            }
            if (botList.Any(b => b.Settings.AccountData == account))
            {
                throw new AccountAlreadyBindedException();
            }
            ++BotCount;
            return new Bot(GetFirstAvailableSlot(), account);
            
        }

        public void RemoveBot(Bot rBot)
        {
            for (int i = 0; i < botList.Length; i++)
            {
                if (rBot == botList[i] )
                {
                    botList[i] = null;
                    return;
                }
            }
            
        }

        private int GetFirstAvailableSlot()
        {
            for (int i = 0; i < botList.Length; i++)
            {
                if (botList[i] != null)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
