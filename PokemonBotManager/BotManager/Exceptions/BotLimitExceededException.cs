using System;

namespace PokemonBotManager.BotManager.Exceptions
{
    public class BotLimitExceededException : Exception
    {
        public BotLimitExceededException()
        {
        }

        public BotLimitExceededException(string message)
            : base(message)
        {
        }

        public BotLimitExceededException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
