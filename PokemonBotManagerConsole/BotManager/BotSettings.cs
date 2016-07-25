using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonBotManagerConsole.LocationHelper;
using PokemonBotManagerConsole.Pokemon;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Enums;

namespace PokemonBotManagerConsole.BotManager
{
    class BotSettings : ISettings
    {
        public AuthType AuthType => AuthType.Ptc;
        public double DefaultAltitude => 10d;
        public double DefaultLatitude { get; set; }
        public double DefaultLongitude { get; set; }

        public string GoogleRefreshToken
        {
            get { return ""; }
            set { throw new NotImplementedException(); }
        }

        public string PtcUsername
        {
            get { return AccountData.Username; }
            set { throw new NotImplementedException(); }
        }

        public string PtcPassword
        {
            get { return AccountData.Password; }
            set { throw new NotImplementedException(); }
        }

        public Location BottingLocation { get; set; }
        public Account AccountData { get; private set; }

        public BotSettings(Account account)
        {
            AccountData = account;
        }

        public BotSettings(Account account, Location location)
        {
            AccountData = account;
            BottingLocation = location;
        }
    }
}
