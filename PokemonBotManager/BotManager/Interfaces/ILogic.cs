using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonGo.RocketAPI;
using POGOProtos.Data.Player;

namespace PokemonBotManager.BotManager.Interfaces
{
    //TODO: This :^ ) 

    public enum LogicStatus
    {

    }

    public class CaughtExceptionEventArg : EventArgs
    {
        public Exception Exception { get; set; }
    }


    public interface ILogic //Idk why I did this
    {
        event EventHandler<CaughtExceptionEventArg> CaughtException;
        Client PClient { get; set; }
        LogicStatus Status { get; set; }
        void StopBot();
        Task Execute();
        Task<PlayerStats> GetPlayerStats();
    }
}