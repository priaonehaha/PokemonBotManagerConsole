using System;

namespace PokemonBotManager.BotManager.Exceptions
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
