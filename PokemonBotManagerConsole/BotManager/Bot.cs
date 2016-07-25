using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonBotManagerConsole.Pokemon;
using PokemonBotManagerConsole.LocationHelper;
using PokemonGo.RocketAPI;
using PokemonGoBotLogic;
using PokemonGoBotLogic.Interfaces;

namespace PokemonBotManagerConsole.BotManager
{
    class Bot
    {
        public bool IsWorking = false;
        public int BotId { get; }
        public bool IsValid => BotId != -1;
        public BotSettings Settings { get; }
        private Client _client;
        private ILogic logic;


        public Bot(int botId, Account assignedAccount)
        {
            Settings = new BotSettings(assignedAccount);
        }

        public Bot(int botId, Account assignedAccount, Location assignedLocation) : this(botId, assignedAccount)
        {
            Settings.BottingLocation = assignedLocation;
        }

        public void StartBot()
        {
            _client = new Client(Settings);
            logic = new Logic(_client);
            logic.Execute().Wait();
            //_client.Login.DoPtcLogin(Settings.AccountData.Username, Settings.AccountData.Password).Wait();
        }

        public bool Equals(Bot oBot)
        {
            return BotId == oBot?.BotId;
        }
        public override int GetHashCode()
        {
            return BotId;
        }
        public override bool Equals(Object oBot)
        {
            if (oBot == null || oBot.GetType() != typeof(Bot))
            {
                return false;
            }
            return BotId == ((Bot)oBot).BotId;
        }

        public static bool operator ==(Bot a, Bot b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.BotId == b.BotId;
        }

        public static bool operator !=(Bot a, Bot b)
        {
            return !(a == b);
        }
    }
}
