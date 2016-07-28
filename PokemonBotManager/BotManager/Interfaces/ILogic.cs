using System;
using System.Threading.Tasks;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Exceptions;
using POGOProtos.Data.Player;

namespace PokemonBotManager.BotManager.Interfaces
{
    //TODO: This :^ ) 

    public enum LogicStatus
    {
    }

    public class CaughtExceptionEventArg : EventArgs
    {
        private readonly bool _isFatal;

        public CaughtExceptionEventArg(Exception exception)
        {
            Exception = exception;
        }

        public CaughtExceptionEventArg(Exception exception, bool isFatalException) : this(exception)
        {
            _isFatal = isFatalException;
        }

        public Exception Exception { get; }
        public bool IsFatal => _isFatal || Exception is AccessTokenExpiredException || Exception is PtcOfflineException;
    }


    public interface ILogic //Idk why I did this
    {
        Client PClient { get; set; }
        LogicStatus Status { get; set; }
        event EventHandler<CaughtExceptionEventArg> CaughtException;
        void StopBot();
        Task Execute();
        Task<PlayerStats> GetPlayerStats();
    }
}