using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonBotManager.LocationHelper;

namespace BotManagerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var kek = new Location("Central Park, New York", 40.7752279, -73.9715349);
            LocationManager.Instance.AddLocation(kek);
            LocationManager.Instance.Serialize();
        }
    }
}
