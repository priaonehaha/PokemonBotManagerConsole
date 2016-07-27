using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PokemonBotManager.BotManager.Interfaces;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Extensions;
using POGOProtos.Data;
using POGOProtos.Data.Player;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;

namespace PokemonGoBotLogic
{
    public class Logic : ILogic
    {
        private readonly Inventory _inventory;
        private bool stop;

        public PokemonId[] UnwantedPokemonTypes =
        {
            PokemonId.Pidgey,
            PokemonId.Rattata,
            PokemonId.Weedle,
            PokemonId.Zubat,
            PokemonId.Caterpie,
            PokemonId.Pidgeotto,
            PokemonId.NidoranFemale,
            PokemonId.Paras,
            PokemonId.Venonat,
            PokemonId.Psyduck,
            PokemonId.Poliwag,
            PokemonId.Slowpoke,
            PokemonId.Drowzee,
            PokemonId.Gastly,
            PokemonId.Goldeen,
            PokemonId.Staryu,
            PokemonId.Magikarp,
            PokemonId.Eevee,
            PokemonId.Kakuna,
            PokemonId.Krabby,
            PokemonId.Spearow,
            PokemonId.Raticate,
            PokemonId.Zubat,
            PokemonId.Metapod,
            PokemonId.Dratini,
            PokemonId.Pidgeot,
            PokemonId.Poliwhirl,
            PokemonId.Voltorb,
            PokemonId.Horsea,
            PokemonId.Seaking,
            PokemonId.Ekans,
            PokemonId.Golbat,
            PokemonId.Pinsir,
            PokemonId.Bellsprout,
            PokemonId.Mankey,
            PokemonId.Parasect,
            PokemonId.Venomoth,
            PokemonId.Oddish,
            PokemonId.Gloom,
            PokemonId.Jigglypuff,
            PokemonId.Clefairy,
            PokemonId.Electabuzz,
            PokemonId.Clefable,
            PokemonId.Sandshrew
        };

        public Logic(Client client)
        {
            PClient = client;
            _inventory = new Inventory(client);
        }

        public Client PClient { get; set; }
        public LogicStatus Status { get; set; }

        public async Task Execute()
        {
            //Console.WriteLine($"Bot for {PClientSettings.PtcUsername} started {PClient.CurrentLat}, {PClient.CurrentLng}");
            while (!stop)
            {
                try
                {
                    if (PClient.Settings.AuthType == AuthType.Ptc)
                        await PClient.Login.DoPtcLogin();
                    else if (PClient.Settings.AuthType == AuthType.Google)
                        throw new NotImplementedException();

                    await PostLoginExecute();
                }
                catch (AccessTokenExpiredException)
                {
                }
                await Task.Delay(10*1000);
            }
        }

        public void StopBot()
        {
            stop = true;
        }

        public async Task<PlayerStats> GetPlayerStats()
        {
            var stat =
                PClient.Inventory.GetInventory()
                    .Result.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PlayerStats)
                    .FirstOrDefault(p => p != null);
            return stat;
        }

        public async Task PostLoginExecute()
        {
            while (!stop)
            {
                try
                {
                    //Console.WriteLine($"{PClientSettings.PtcUsername}: Post Login Execute");

                    //await PClient.SetServer(); // Not needed in new API
                    //var profile = await PClient.GetProfile();

                    await EvolveAllPokemonWithEnoughCandy();
                    await TransferDuplicatePokemon();
                    await RecycleItems();
                    await ExecuteFarmingPokestopsAndPokemons();
                }
                catch (AccessTokenExpiredException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{PClient.Settings.PtcUsername}'sPostLoginExecute exception: " + ex);
                }

                await Task.Delay(5*1000);
            }
        }

        public async Task RepeatAction(int repeat, Func<Task> action)
        {
            for (var i = 0; i < repeat; i++)
                await action();
        }

        private async Task TransferAllButStrongestUnwantedPokemon()
        {
            var pokemons = await _inventory.GetPokemons();

            foreach (var unwantedPokemonType in UnwantedPokemonTypes)
            {
                var pokemonOfDesiredType = pokemons.Where(p => p.PokemonId == unwantedPokemonType)
                    .OrderByDescending(p => p.Cp)
                    .ToList();

                var unwantedPokemon = pokemonOfDesiredType.Skip(1).ToList();

                await TransferAllGivenPokemons(unwantedPokemon);
            }
        }

        private async Task TransferAllGivenPokemons(IEnumerable<PokemonData> unwantedPokemons)
        {
            foreach (var pokemon in unwantedPokemons)
            {
                await _inventory.TransferPokemon(pokemon.Id); //.TransferPokemon(pokemon.Id);
                await Task.Delay(500);
            }
        }

        private async Task ExecuteFarmingPokestopsAndPokemons()
        {
            var mapObjects = await PClient.Map.GetMapObjects();

            var pokeStops =
                mapObjects.MapCells.SelectMany(i => i.Forts)
                    .Where(
                        i =>
                            i.Type == FortType.Checkpoint &&
                            i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime())
                    .OrderBy(
                        p => Navigation.DistanceBetween2Coordinates(PClient.CurrentLatitude, PClient.CurrentLongitude,
                            p.Latitude, p.Longitude));
            foreach (var pokeStop in pokeStops)
            {
                var distance = Navigation.DistanceBetween2Coordinates(PClient.CurrentLatitude, PClient.CurrentLongitude,
                    pokeStop.Latitude, pokeStop.Longitude);

                if (distance > 100)
                    await Task.Delay((int) (distance/111)); //Don't tp faster than 400 Km/h (111 m/s)
                await PClient.Player.UpdatePlayerLocation(pokeStop.Latitude, pokeStop.Longitude, 10);
                // var fortInfo = await PClient.Fort.GetFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);
                await PClient.Fort.SearchFort(pokeStop.Id, pokeStop.Latitude, pokeStop.Longitude);
                //Console.Write($"{PClientSettings.PtcUsername}: Using Pokestop: {fortInfo.Name} in {Math.Round(distance)}m distance");
                //Console.Write($"{PClientSettings.PtcUsername}: Farmed XP: {fortSearch.ExperienceAwarded}, Gems: { fortSearch.GemsAwarded}, Eggs: {fortSearch.PokemonDataEgg} Items: {fortSearch.ItemsAwarded}");


                //var profile = await PClient.GetProfile();

                await Task.Delay(500);
                await RecycleItems();
                await CatchPokemonInPokestop(pokeStop);
                await ExecuteCatchAllNearbyPokemons();
                await TransferDuplicatePokemon();
            }
        }

        private async Task CatchPokemonInPokestop(FortData pokeStop)
        {
            if (pokeStop != null)
            {
                var encounter = await PClient.Encounter.EncounterLurePokemon(pokeStop.LureInfo.EncounterId, pokeStop.Id);
                if (encounter.Result == DiskEncounterResponse.Types.Result.Success)
                {
                    await CatchEncounter(encounter);
                }
            }
        }

        private async Task ExecuteCatchAllNearbyPokemons()
        {

            var mapObjects = await PClient.Map.GetMapObjects();

            var pokemons = mapObjects.MapCells.SelectMany(i => i.CatchablePokemons);
            foreach (var pokemon in pokemons)
            {
                var distance = Navigation.DistanceBetween2Coordinates(PClient.CurrentLatitude, PClient.CurrentLongitude,
                    pokemon.Latitude, pokemon.Longitude);
                if (distance > 100)
                    await Task.Delay((int) (distance/111)); //Don't tp faster than 400 Km/h (111 m/s)

                await
                    PClient.Player.UpdatePlayerLocation(pokemon.Latitude, pokemon.Longitude,
                        PClient.Settings.DefaultAltitude);

                var encounter = await PClient.Encounter.EncounterPokemon(pokemon.EncounterId, pokemon.SpawnPointId);
                await CatchEncounter(encounter, pokemon);
            }

            await Task.Delay(15 * 1000);
        }

        private async Task CatchEncounter(EncounterResponse encounter, MapPokemon pokemon)
        {
            CatchPokemonResponse caughtPokemonResponse;
            do
            {
                if (encounter?.CaptureProbability.CaptureProbability_.First() < 0.35)
                {
                    //Throw berry is we can
                    await UseBerry(pokemon.EncounterId, pokemon.SpawnPointId);
                }

                var pokeball = await GetBestBall(encounter?.WildPokemon);
                /*var distance = Navigation.DistanceBetween2Coordinates(PClient.CurrentLatitude, PClient.CurrentLongitude,
                    pokemon.Latitude, pokemon.Longitude);*/
                caughtPokemonResponse =
                    await
                        PClient.Encounter.CatchPokemon(pokemon.EncounterId, pokemon.SpawnPointId, pokeball);
                //Console.Write(caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess ? $"We caught a {pokemon.PokemonId} with CP {encounter?.WildPokemon?.PokemonData?.Cp} and CaptureProbability: {encounter?.CaptureProbability.CaptureProbability_.First()} using a {pokeball} in {Math.Round(distance)}m distance" : $"{pokemon.PokemonId} with CP {encounter?.WildPokemon?.PokemonData?.Cp} CaptureProbability: {encounter?.CaptureProbability.CaptureProbability_.First()} in {Math.Round(distance)}m distance {caughtPokemonResponse.Status} while using a {pokeball}..", LogLevel.Info);
                await Task.Delay(2000);
            } while (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchMissed ||
                     caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape);

            if (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess)
            {
                await TransferAllButStrongestUnwantedPokemon();
            }
        }

        private async Task EvolveAllPokemonWithEnoughCandy()
        {
            var pokemonToEvolve = await _inventory.GetPokemonToEvolve();
            foreach (var pokemon in pokemonToEvolve)
            {
                await _inventory.EvolvePokemon(pokemon.Id);
                await Task.Delay(3000);
            }
        }

        private async Task TransferDuplicatePokemon(bool keepPokemonsThatCanEvolve = false)
        {
            var duplicatePokemons = await _inventory.GetDuplicatePokemonToTransfer(keepPokemonsThatCanEvolve);

            foreach (var duplicatePokemon in duplicatePokemons)
            {
                await _inventory.TransferPokemon(duplicatePokemon.Id);
                await Task.Delay(500);
            }
        }

        private async Task RecycleItems()
        {
            var items = await _inventory.GetItemsToRecycle();

            foreach (var item in items)
            {
                var transfer = await _inventory.RecycleItem(item.ItemId, item.Count);
                //Console.Write($"Recycled {item.Count}x {(AllEnum.ItemId)item.Item_}", LogLevel.Info);
                await Task.Delay(500);
            }
        }

        private async Task<ItemId> GetBestBall(WildPokemon pokemon)
        {
            var pokemonCp = pokemon?.PokemonData?.Cp;
            var pokeBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemPokeBall);
            var greatBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemGreatBall);
            var ultraBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemUltraBall);
            var masterBallsCount = await _inventory.GetItemAmountByType(ItemId.ItemMasterBall);

            if (masterBallsCount > 0 && pokemonCp >= 1000)
                return ItemId.ItemMasterBall;
            if (ultraBallsCount > 0 && pokemonCp >= 1000)
                return ItemId.ItemUltraBall;
            if (greatBallsCount > 0 && pokemonCp >= 1000)
                return ItemId.ItemGreatBall;

            if (ultraBallsCount > 0 && pokemonCp >= 600)
                return ItemId.ItemUltraBall;
            if (greatBallsCount > 0 && pokemonCp >= 600)
                return ItemId.ItemGreatBall;

            if (greatBallsCount > 0 && pokemonCp >= 350)
                return ItemId.ItemGreatBall;

            if (pokeBallsCount > 0)
                return ItemId.ItemPokeBall;
            if (greatBallsCount > 0)
                return ItemId.ItemGreatBall;
            if (ultraBallsCount > 0)
                return ItemId.ItemUltraBall;
            if (masterBallsCount > 0)
                return ItemId.ItemMasterBall;

            return ItemId.ItemPokeBall;
        }

        public async Task UseBerry(ulong encounterId, string spawnPointId)
        {
            var inventoryBalls = await _inventory.GetItems();
            var berries = inventoryBalls.Where(p => p.ItemId == ItemId.ItemRazzBerry);
            var berry = berries.FirstOrDefault();

            if (berry == null)
                return;

            var useRaspberry = await PClient.Encounter.UseCaptureItem(encounterId, ItemId.ItemRazzBerry, spawnPointId);
            // Logger.Write($"Use Rasperry. Remaining: {berry.Count}", LogLevel.Info);
            await Task.Delay(1 * 1000);
        }
    }
}