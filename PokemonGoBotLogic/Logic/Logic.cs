using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PokemonBotManager.BotManager.Interfaces;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Extensions;
using PokemonGoBotLogic.Helpers;
using POGOProtos.Data.Player;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;

namespace PokemonGoBotLogic.Logic
{
    internal enum TaskJob
    {
        RecycleItems,
        TransferDuplicatePokemon,
        FarmPokeStops
    }

    public partial class Logic : ILogic, IBotStatistics
    {
        private DateTime lastLuckyEggTime;
        private bool stopRequested = false;
        private Inventory _inventory;
        public bool ShouldEvolvePokemon;
        public bool ShouldRecycleItems;
        public bool ShouldTransferPokemon;

        private List<Pair<TaskJob, Task>> taskList;
        private Dictionary<TaskJob, Func<Task>> taskDictionary = new Dictionary<TaskJob, Func<Task>>(3);
        public Logic(Client client)
        {
            PClient = client;
            _inventory = new Inventory(client);
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
            taskList = new List<Pair<TaskJob, Task>>(3);
            try
            {
                await PClient.Login.DoPtcLogin();
                var stat =
                    PClient.Inventory.GetInventory()
                        .Result.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PlayerStats)
                           .FirstOrDefault(p => p != null);
                if (stat?.Level == 1 && stat.Experience == 0)
                {
                    await PClient.Misc.ClaimCodename(PClient.Settings.PtcUsername);

                }
            }
            catch (AccessTokenExpiredException e)
            {
                OnCaughtException(new CaughtExceptionEventArg {Exception = e});
                return;
            }
            foreach (var item in taskDictionary)
            {
                taskList.Add(new Pair<TaskJob, Task>(item.Key, Task.Run(item.Value).ContinueWith(_ => OnTaskStopped(item))));
            }
        }

        public async void StopBot()
        {
            stopRequested = true;
            foreach (var pair in taskList)
            {
                if (pair.Item2.IsCompleted)
                {
                    continue;
                }
                pair.Item2.Wait();
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
            //You are not allowed to stop
            taskList[tupleIndex].Item2 = Task.Run(itemPair.Value).ContinueWith(_ => OnTaskStopped(itemPair)); 
        }

        private async Task FarmPokeStops()
        {
            var numberOfPokestopsVisited = 0;
            var returnToStart = DateTime.Now;
            var pokeStopList = (await GetPokeStops())
                .OrderBy(p => Navigation.DistanceBetween2Coordinates(PClient.CurrentLatitude, PClient.CurrentLongitude, p.Latitude, p.Longitude));
            FortData firstPokestop = pokeStopList.FirstOrDefault();

            //var pokestopList = (await _map.GetPokeStops()).Where(t => t.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime()).ToList();
            while (!stopRequested)
            {
                //TODO: implement snipe
                if (returnToStart.AddMinutes(2) <= DateTime.Now)
                {
                    await PClient.Player.UpdatePlayerLocation(firstPokestop.Latitude, firstPokestop.Longitude, 10);
                    returnToStart = DateTime.Now;
                }
            }
        }

        private async Task<List<FortData>> GetPokeStops()
        {
            return (await PClient.Map.GetMapObjects()).MapCells.SelectMany(i => i.Forts)
                .Where(
                    f => f.Type == FortType.Checkpoint && f.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime())
                .ToList();
        }

        private async Task RecycleItems()
        {
            while (!stopRequested)
            {
                try
                {
                    var trashItems = await _inventory.GetItemsToRecycle();
                    foreach (var trashItem in trashItems)
                    {
                        try
                        {
                            await _inventory.RecycleItem(trashItem.ItemId, trashItem.Count);
                        }
                        catch (AccessTokenExpiredException)
                        {
                            throw; //up :^(
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine($"{e.InnerException}: Failed to recycle {trashItem.ItemId}");
                        }
                    }
                }
                catch (AccessTokenExpiredException e)
                {
                    OnCaughtException(new CaughtExceptionEventArg{Exception = e});
                    break;
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"{e.InnerException}: Failed to recycle items");
                }
                    
            }
        }

        private async Task TransferDuplicatePokemon()
        {
            while (!stopRequested)
            {
                try
                {
                    var pokemonList = await _inventory.GetDuplicatePokemonToTransfer();
                    foreach (var pokemonData in pokemonList)
                    {
                        if (pokemonData.Cp > 1800)
                        {
                            continue;
                        }
                        await _inventory.TransferPokemon(pokemonData.Id);
                    }
                }
                catch (AccessTokenExpiredException e)
                {
                    OnCaughtException(new CaughtExceptionEventArg { Exception = e });
                    break;
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"{e.InnerException}: Failed to evolve pokemons");
                }
                await Task.Delay(30*1000);
            }
        }

        private void EvolvePokemon()
        {
            UseLuckyEgg();
            try
            {
                var pokemonData = _inventory.GetPokemonToEvolve().Result;
                foreach (var pokemon in pokemonData)
                {
                    try
                    {
                        _inventory.EvolvePokemon(pokemon.Id).Wait();
                    }
                    catch (AccessTokenExpiredException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"{e.InnerException}: Failed to evolve {pokemon.PokemonId} with cp {pokemon.Cp}");
                    }
                }
            }
            catch (AccessTokenExpiredException e)
            {
                OnCaughtException(new CaughtExceptionEventArg { Exception = e });
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.InnerException}: Failed to evolve Pokemons");

            }
        }

        private async void UseLuckyEgg()
        {
            if (lastLuckyEggTime.AddMinutes(30) > DateTime.Now)
            {
                return;
            }
            try
            {
                var items = _inventory.Items;

                var luckyEgg = items.FirstOrDefault(i => i.ItemId == ItemId.ItemLuckyEgg);
                if (luckyEgg == null || luckyEgg.Count <= 0)
                {
                    return;
                }
                await _inventory.UseItemXpBoost();
                lastLuckyEggTime = DateTime.Now;
                await Task.Delay(10*1000);
            }
            catch (AccessTokenExpiredException e)
            {
                OnCaughtException(new CaughtExceptionEventArg { Exception = e });
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.InnerException}: Failed to Use Lucky Egg");
            }
        }
    }
}