using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonBotManager.BotManager.Interfaces
{
    public interface IBotStatistics
    {
        void UpdateStats();

        string GetCurrentStats();
    }
}
