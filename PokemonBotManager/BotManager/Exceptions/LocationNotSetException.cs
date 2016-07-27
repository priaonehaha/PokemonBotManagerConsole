using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonBotManager.BotManager.Exceptions
{
    
    class LocationNotSetException : Exception
    {
        public LocationNotSetException()
        {
        }

        public LocationNotSetException(string message)
            : base(message)
        {
        }

        public LocationNotSetException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
