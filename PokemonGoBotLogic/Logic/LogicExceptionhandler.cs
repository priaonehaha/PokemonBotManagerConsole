using System;
using PokemonBotManager.BotManager.Interfaces;
using PokemonGo.RocketAPI.Exceptions;

namespace PokemonGoBotLogic.Logic
{

    public partial class Logic
    {
        public event EventHandler<CaughtExceptionEventArg> CaughtException;

        protected virtual void OnCaughtException(CaughtExceptionEventArg e)
        {
            if (e.IsFatal)
            {
                if (stopRequested)
                {
                    return;
                }
                stopRequested = true;
            }
            if (CaughtException != null)
            {
                CaughtException(this, e);
            }
        }
    }
}
