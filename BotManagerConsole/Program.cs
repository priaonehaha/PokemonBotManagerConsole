using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonBotManager.BotManager;
using PokemonBotManager.LocationHelper;
using PokemonBotManager.Pokemon;

namespace BotManagerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WindowWidth = 120;
            LocationManager.Instance.AddLocation(new Location("Central Park, Manhattan", 40.782763, -73.967640));
            AccountList.Instance.LoadFromFile();
            Menu.MainLoop();
            // var bot = BotManager.Instance.RequestBot(cykaAcc, CentralPark);
            //bot.StartBot();
            LocationManager.Instance.Serialize();
            AccountList.Instance.Serialize();
        }
    }
}
