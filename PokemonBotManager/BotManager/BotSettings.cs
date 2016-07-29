using System;
using System.Collections.Generic;
using PokemonBotManager.LocationHelper;
using PokemonBotManager.Pokemon;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Enums;

namespace PokemonBotManager.BotManager
{
    public class BotSettings : ISettings
    {
        public double DefaultAltitude => 10d;
        AuthType ISettings.AuthType {
            get
            {
             return AuthType.Ptc;   
            }
            set { throw new NotImplementedException(); }
        }

        public double DefaultLatitude {
            get { return BottingLocation.Latitude; }
            set { throw new NotImplementedException(); }
        }
        public double DefaultLongitude
        {
            get { return BottingLocation.Longitude; }
            set { throw new NotImplementedException(); }
        }

        double ISettings.DefaultAltitude { get; set; }

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

        public string GoogleUsername { get; set; }
        public string GooglePassword { get; set; }

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
