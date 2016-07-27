using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.GeneratedCode;

namespace PokemonBotManager.BotManager.Interfaces
{
    //TODO: This :^ ) 
    public enum LogicStatus
    {

    }

    public interface ILogic //Idk why I did this
    {
        Client PClient { get; set; }
        LogicStatus Status { get; set; }
        void StopBot();
        Task Execute();
        Task<PlayerStats> GetPlayerStats();
    }
}
}
