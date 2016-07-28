using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PokemonBotManager.BotManager.Interfaces;
using PokemonGo.RocketAPI;
using PokemonGoBotLogic.Helpers;
using POGOProtos.Data.Player;

namespace PokemonGoBotLogic
{
    internal enum TaskJob
    {
        RecycleItems,
        TransferDuplicatePokemon,
        FarmPokeStops
    }

    public class Logic : ILogic, IBotStatistics
    {
        private DateTime lastLuckyEggTime;
        private bool stopRequested = false;
        public bool ShouldEvolvePokemon;
        public bool ShouldRecycleItems;
        public bool ShouldTransferPokemon;

        private List<Pair<TaskJob, Task>> taskList = new List<Pair<TaskJob, Task>>(3);
        private Dictionary<TaskJob, Func<Task>> taskDictionary = new Dictionary<TaskJob, Func<Task>>(3);
        public Logic(Client client)
        {
            PClient = client;
            lastLuckyEggTime = DateTime.MinValue;
            taskDictionary.Add(TaskJob.RecycleItems, RecycleItems);
            taskDictionary.Add(TaskJob.TransferDuplicatePokemon, TransferDuplicatePokemon);
            taskDictionary.Add(TaskJob.FarmPokeStops, FarmPokeStops);
        }

        public void UpdateStats()
        {
            throw new NotImplementedException();
        }

        public string GetCurrentStats()
        {
            throw new NotImplementedException();
        }

        public Client PClient { get; set; }
        public LogicStatus Status { get; set; }

        public async Task Execute()
        {
            foreach (var item in taskDictionary)
            {
                taskList.Add(new Pair<TaskJob, Task>(item.Key, Task.Run(item.Value).ContinueWith(_ => OnTaskStopped(item))));
            }
        }

        public void StopBot()
        {
            stopRequested = true;
            foreach (var tuple in taskList)
            {
                tuple.Item2.Wait();
            }
        }

        public Task<PlayerStats> GetPlayerStats()
        {
            throw new NotImplementedException();
        }

        private async Task OnTaskStopped(KeyValuePair<TaskJob, Func<Task>> itemPair)
        {
            if (stopRequested)
            {
                return;
            }
            int tupleIndex = taskList.FindIndex(t => t.Item1 == itemPair.Key);
            taskList[tupleIndex].Item2 = Task.Run(itemPair.Value).ContinueWith(_ => OnTaskStopped(itemPair)); //Stack overflow exception?


        }

        private async Task RecycleItems()
        {
        }

        private async Task TransferDuplicatePokemon()
        {
        }

        private async Task FarmPokeStops()
        {
        }
    }
}