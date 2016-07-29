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
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;

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
        private readonly Inventory _inventory;
        private DateTime lastLuckyEggTime;
        public bool ShouldEvolvePokemon;
        public bool ShouldRecycleItems;
        public bool ShouldTransferPokemon;
        private bool stopRequested;
        private readonly Dictionary<TaskJob, Func<Task>> taskDictionary = new Dictionary<TaskJob, Func<Task>>(3);

        private List<Pair<TaskJob, Task>> taskList;

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
                await PClient.Login.DoPtcLogin(PClient.Settings.PtcUsername, PClient.Settings.PtcPassword);
                var stat =
                    PClient.Inventory.GetInventory()
                        .Result.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PlayerStats)
                        .FirstOrDefault(p => p != null);
                if (stat?.Level == 1 && stat.Experience == 0)
                {
                    await PClient.Misc.ClaimCodename(PClient.Settings.PtcUsername);
                    await PClient.Misc.MarkTutorialComplete();
                }
            }
            catch (AccessTokenExpiredException e)
            {
                OnCaughtException(new CaughtExceptionEventArg(e));
                return;
            }
            catch (PtcOfflineException e)
            {
                OnCaughtException(new CaughtExceptionEventArg(e));
                return;
            }
            catch (Exception e)
            {
                OnCaughtException(new CaughtExceptionEventArg(e, true));
                return;
            }
            foreach (var item in taskDictionary)
            {
                taskList.Add(new Pair<TaskJob, Task>(item.Key,
                    Task.Run(item.Value).ContinueWith(_ => OnTaskStopped(item))));
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
            await Task.Delay(100);
            var tupleIndex = taskList.FindIndex(t => t.Item1 == itemPair.Key);
            //You are not allowed to stop
            taskList[tupleIndex].Item2 = Task.Run(itemPair.Value).ContinueWith(_ => OnTaskStopped(itemPair));
        }

        private async Task FarmPokeStops()
        {
            var numberOfPokestopsVisited = 0;
            var returnToStart = DateTime.Now;
            var pokeStopList = (await GetPokeStops())
                .OrderBy(
                    p =>
                        Navigation.DistanceBetween2Coordinates(PClient.CurrentLatitude, PClient.CurrentLongitude,
                            p.Latitude, p.Longitude)).ToList();
            var firstPokestop = pokeStopList.FirstOrDefault();

            //var pokestopList = (await _map.GetPokeStops()).Where(t => t.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime()).ToList();
            while (!stopRequested)
            {
                try
                {
                    //TODO: implement snipe
                    if (returnToStart.AddMinutes(2) <= DateTime.Now)
                    {
                        Debug.WriteLine("TP to {0} {1}", firstPokestop?.Latitude, firstPokestop.Longitude);
                        await TeleportToPokeStop(firstPokestop);
                        returnToStart = DateTime.Now;
                    }
                    if (!pokeStopList.Any())
                    {
                        await TeleportToPokeStop(firstPokestop);
                        var oldPokestopList = await GetPokeStops();
                        if (oldPokestopList.Any())
                            pokeStopList = oldPokestopList;
                    }
                    var newPokestopList = await GetPokeStops();
                    if (newPokestopList.Any())
                        pokeStopList = newPokestopList;
                    if (!pokeStopList.Any())
                        continue;
                    var closestPokestop = newPokestopList.OrderBy(
                        i =>
                            Navigation.DistanceBetween2Coordinates(PClient.CurrentLatitude,
                                PClient.CurrentLongitude, i.Latitude, i.Longitude)).First();

                    if (firstPokestop == null)
                        firstPokestop = closestPokestop;

                    var distance = Navigation.DistanceBetween2Coordinates(PClient.CurrentLatitude, PClient.CurrentLongitude,
                        closestPokestop.Latitude, closestPokestop.Longitude);

                    //var fortWithPokemon = (await _map.GetFortWithPokemon());
                    //var biggestFort = fortWithPokemon.MaxBy(x => x.GymPoints);
                    if (distance > 100)
                    {
                        var r = new Random((int)DateTime.Now.Ticks);
                        closestPokestop =
                            pokeStopList.ElementAt(r.Next(pokeStopList.Count));
                    }

                    await TeleportToPokeStop(closestPokestop);
                    var pokestopBooty =
                        await
                            PClient.Fort.SearchFort(closestPokestop.Id, closestPokestop.Latitude, closestPokestop.Longitude);
                    if (pokestopBooty.ExperienceAwarded > 0)
                    {
                        Debug.WriteLine(
                            $"[{numberOfPokestopsVisited++}] Pokestop rewarded us with {pokestopBooty.ExperienceAwarded} exp. {pokestopBooty.GemsAwarded} gems..");
                        //_stats.ExperienceSinceStarted += pokestopBooty.ExperienceAwarded;
                        //_stats.
                    }
                    else
                    {
                        while (pokestopBooty.Result == FortSearchResponse.Types.Result.Success)
                        {
                            pokestopBooty =
                                await
                                    PClient.Fort.SearchFort(closestPokestop.Id, closestPokestop.Latitude,
                                        closestPokestop.Longitude);
                        }
                    }
                }
                catch (AccessTokenExpiredException e)
                {
                    OnCaughtException(new CaughtExceptionEventArg(e));
                    break;
                }
                catch (Exception e)
                {
                    //ignored?
                }

                await Task.Delay(100);
            }
        }

        private async Task<List<FortData>> GetPokeStops()
        {
            return (await PClient.Map.GetMapObjects()).MapCells.SelectMany(i => i.Forts)
                .Where(
                    f => f.Type == FortType.Checkpoint && f.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime())
                .ToList();
        }

        private async Task CatchNearbyPokemmon(FortData pokeStop)
        {
            var mapObjects = (await PClient.Map.GetMapObjects());
            var catchable = mapObjects.MapCells.SelectMany(i => i.CatchablePokemons).ToList();
            var wild = mapObjects.MapCells.SelectMany(x => x.WildPokemons).Select(x => new MapPokemon()
            {
                EncounterId = x.EncounterId,
                SpawnPointId = x.SpawnPointId,
                PokemonId = x.PokemonData.PokemonId
            });
            catchable.AddRange(wild);
            var pokemon = catchable.OrderBy(p => Navigation
                .DistanceBetween2Coordinates(p.Latitude, p.Longitude, PClient.CurrentLatitude, PClient.CurrentLongitude))
                .ToList();
            if (pokemon.Any())
            {
                var pokemonList = string.Join(", ", pokemon.Select(x => x.PokemonId).ToArray());
                Debug.WriteLine($"{pokemon.Count()} Pokemon found: {pokemonList}");
            }

            if (pokeStop?.LureInfo != null && pokeStop.LureInfo.ActivePokemonId != PokemonId.Missingno)
            {
                var encounterId = pokeStop.LureInfo.EncounterId;
                var encounter = await PClient.Encounter.EncounterLurePokemon(encounterId, pokeStop.Id);
                if (encounter.Result == DiskEncounterResponse.Types.Result.Success)
                {

                    await CatchLurePokemon(encounterId, pokeStop.Id, encounter, encounter.PokemonData.PokemonId);
                }
            }
            var catchPokemonTaskList = new List<Task>();
            foreach (var mapPokemon in pokemon)
            {
                var encounter = await PClient.Encounter.EncounterPokemon(mapPokemon.EncounterId, mapPokemon.SpawnPointId);
                if (encounter.Status == EncounterResponse.Types.Status.EncounterSuccess)
                {
                    catchPokemonTaskList.Add(new Task(async () =>
                    {
                        try
                        {
                            await CatchWildPokemon(encounter, mapPokemon);
                        }
                        catch (Exception)
                        {
                        }
                    }));
                }
                else
                {
                    if (encounter.Status != EncounterResponse.Types.Status.EncounterAlreadyHappened)
                        Debug.WriteLine($"Unable to catch pokemon. Reason: {encounter.Status}");
                }
            }
            catchPokemonTaskList.ForEach(x => x.Start());
            Task.WaitAll(catchPokemonTaskList.ToArray());

        }

        private async Task TeleportToPokeStop(FortData pokestop)
        {
            await PClient.Player.UpdatePlayerLocation(pokestop.Latitude, pokestop.Longitude, 10);
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
                    OnCaughtException(new CaughtExceptionEventArg(e));
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
                    EvolvePokemon();
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
                    OnCaughtException(new CaughtExceptionEventArg(e));
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
            try
            {
                UseLuckyEgg();
                var pokemonData = _inventory.GetPokemonToEvolve().Result;
                foreach (var pokemon in pokemonData)
                {
                    Debug.WriteLine($" Evolving {pokemon.PokemonId} with cp {pokemon.Cp}");
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
                OnCaughtException(new CaughtExceptionEventArg(e));
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
                Debug.WriteLine("Used Luccky egg");
            }
            catch (AccessTokenExpiredException e)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.InnerException}: Failed to Use Lucky Egg");
            }
            await Task.Delay(10 * 1000);

        }
    }
}