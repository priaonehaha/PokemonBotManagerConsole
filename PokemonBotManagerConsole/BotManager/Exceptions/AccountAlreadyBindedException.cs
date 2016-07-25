using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonBotManagerConsole.BotManager.Exceptions
{
    public class AccountAlreadyBindedException : Exception
    {
        public AccountAlreadyBindedException()
        {
        }

        public AccountAlreadyBindedException(string message)
            : base(message)
        {
        }

        public AccountAlreadyBindedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
