using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Rpc;
using PokemonGoBotLogic.Interfaces;

namespace PokemonGoBotLogic
{
    public class Logic : ILogic
    {
        public Client PClient { get; set; }
        public LogicStatus Status { get; set; }
        private readonly ISettings _clientSettings;
        private readonly Inventory _inventory;
        private bool stop = false;

        public Logic(Client client)
        {
            PClient = client;
            _clientSettings = client.Settings;
            _inventory = new Inventory(client);
        }

        public async Task Execute()
        {
            while (!stop)
            {
                try
                {
                    if (_clientSettings.AuthType == AuthType.Ptc)
                        await PClient.Login.DoPtcLogin(_clientSettings.PtcUsername, _clientSettings.PtcPassword);
                    else if (_clientSettings.AuthType == AuthType.Google)
                        throw new NotImplementedException();

                    await PostLoginExecute();
                }
                catch (AccessTokenExpiredException)
                {
                    while (!stop)
                    {

                    }
                }
            }
            await Task.Delay(10 * 1000);
        }

        public async Task PostLoginExecute()
        {

        }

        public async Task StopBot()
        {
            stop = true;
        }

    }
}
