using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonBotManagerConsole.Pokemon;

//TODO:
//BotManager to DLL for other easy integrations
namespace PokemonBotManagerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var kek = new LocationHelper.Location("Central Park, New York", 40.7752279, -73.9715349);
            LocationHelper.LocationManager.Instance.AddLocation(kek);
            LocationHelper.LocationManager.Instance.Serialize();
        }
    }
}
