using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Rpc;
using POGOProtos.Data;
using POGOProtos.Data.Player;
using POGOProtos.Enums;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using POGOProtos.Settings.Master;

namespace PokemonGoBotLogic
{
    public class Inventory : PokemonGo.RocketAPI.Rpc.Inventory
    {
        private GetInventoryResponse _cachedInventoryResponse;
        private DateTime _lastUpdated;
        public GetInventoryResponse InventoryResponse
        {
            get
            {
                if (_lastUpdated.AddSeconds(30) < DateTime.UtcNow)
                {
                    _cachedInventoryResponse = GetInventory().Result;
                    _lastUpdated = DateTime.UtcNow;
                }

                return _cachedInventoryResponse;
            }
        }

        public IEnumerable<ItemData> Items => InventoryResponse.InventoryDelta.InventoryItems.Select(t => t?.InventoryItemData?.Item).Where(i => i != null);

        private readonly Client _client;
        public Inventory(Client client) : base(client)
        {
            if (client == null)
            {
                throw new ArgumentNullException();
            }
            _client = client;
        }


        public async Task<IEnumerable<PokemonData>> GetPokemons()
        {
            return
                InventoryResponse.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PokemonData)
                    .Where(p => p?.PokemonId > 0);
        }

        public async Task<IEnumerable<PlayerStats>> GetPlayerStats()
        {
            var inventory = await _client.Inventory.GetInventory();
            return inventory.InventoryDelta.InventoryItems
                .Select(i => i.InventoryItemData?.PlayerStats)
                .Where(p => p != null);
        }

        public async Task<IEnumerable<Candy>> GetPokemonCandies()
        {
            return  InventoryResponse.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.Candy);
        }


        public async Task<IEnumerable<PokemonSettings>> GetPokemonSettings()
        {
           // var t = await PokemonGo.RocketAPI.Rpc.
            var templates = await _client.Download.GetItemTemplates();
            return
                templates.ItemTemplates.Select(i => i.PokemonSettings)
                    .Where(p => p != null && p?.FamilyId != PokemonFamilyId.FamilyUnset);
        }


        public async Task<IEnumerable<PokemonData>> GetDuplicatePokemonToTransfer(bool keepPokemonsThatCanEvolve = false)
        {
            var myPokemon = await GetPokemons();

            var pokemonList = myPokemon.Where(p => p.DeployedFortId == "0").ToList(); //Don't evolve pokemon in gyms
            if (keepPokemonsThatCanEvolve)
            {
                var results = new List<PokemonData>();
                var pokemonsThatCanBeTransfered = pokemonList.GroupBy(p => p.PokemonId)
                    .Where(x => x.Count() > 2).ToList();

                var pokemonSettings = (await GetPokemonSettings()).ToList();
                var pokemonCandies = (await GetPokemonCandies()).ToArray();


                foreach (var pokemon in pokemonsThatCanBeTransfered)
                {
                    var settings = pokemonSettings.Single(x => x.PokemonId == pokemon.Key);
                    var familyCandy = pokemonCandies.Single(x => settings.FamilyId == x.FamilyId);
                    if (settings.CandyToEvolve == 0)
                        continue;

                    var amountToSkip = (familyCandy.Candy_ + settings.CandyToEvolve - 1) / settings.CandyToEvolve + 2;

                    results.AddRange(pokemonList.Where(x => x.PokemonId == pokemon.Key && x.Favorite == 0)
                        .OrderByDescending(x => x.Cp)
                        .ThenBy(n => n.StaminaMax)
                        .Skip(amountToSkip)
                        .ToList());

                }

                return results;
            }

            return pokemonList
                .GroupBy(p => p.PokemonId)
                .Where(x => x.Count() > 1)
                .SelectMany(p => p.Where(x => x.Favorite == 0).OrderByDescending(x => x.Cp).ThenBy(n => n.StaminaMax).Skip(2).ToList());
        }


        public async Task<IEnumerable<PokemonData>> GetPokemonToEvolve()
        {
            var myPokemons = await GetPokemons();
            var pokemons = myPokemons.Where(p => p.DeployedFortId == "0").ToList(); //Don't evolve pokemon in gyms

            var myPokemonSettings = await GetPokemonSettings();
            var pokemonSettings = myPokemonSettings.ToList();

            var pokemonCandies = (await GetPokemonCandies()).ToArray();
            var pokemonToEvolve = new List<PokemonData>();

            foreach (var pokemon in pokemons)
            {
                var settings = pokemonSettings.Single(x => x.PokemonId == pokemon.PokemonId);
                var familyCandy = pokemonCandies.Single(x => settings.FamilyId == x.FamilyId);

                //Don't evolve if we can't evolve it
                if (settings.EvolutionIds.Count == 0)
                    continue;

                var pokemonCandyNeededAlready = pokemonToEvolve.Count(p => pokemonSettings.Single(x => x.PokemonId == p.PokemonId).FamilyId == settings.FamilyId) * settings.CandyToEvolve;
                if (familyCandy.Candy_ - pokemonCandyNeededAlready > settings.CandyToEvolve)
                    pokemonToEvolve.Add(pokemon);
            }

            return pokemonToEvolve;
        }



        public async Task<IEnumerable<ItemData>> GetItems()
        {
            return InventoryResponse.InventoryDelta.InventoryItems
                .Select(i => i.InventoryItemData?.Item)
                .Where(p => p != null);
        }

        public async Task<int> GetItemAmountByType(ItemId type)
        {
            var pokeballs = await GetItems();
            return pokeballs.FirstOrDefault(i => i.ItemId == type)?.Count ?? 0;
        }

        /*
         * TODO:
         * Make this cleaner
         */
        public async Task<IEnumerable<ItemData>> GetItemsToRecycle()
        {
            var myItems = await GetItems();

            return myItems
                .Where(x => Recicable.ItemRecycleFilter.Any(f => f.Key == x.ItemId && x.Count > f.Value))
                .Select(x => new ItemData { ItemId = x.ItemId, Count = x.Count - Recicable.ItemRecycleFilter.Single(f => f.Key == x.ItemId).Value, Unseen = x.Unseen });
        }
    }

    static class Recicable
    {
        public static ICollection<KeyValuePair<ItemId, int>> ItemRecycleFilter
        {
            get
            {
                //Type and amount to keep
                return new[]
                {
                    new KeyValuePair<ItemId, int>(ItemId.ItemUnknown, 0),
                    new KeyValuePair<ItemId, int>(ItemId.ItemPokeBall, 20),
                    new KeyValuePair<ItemId, int>(ItemId.ItemGreatBall, 20),
                    new KeyValuePair<ItemId, int>(ItemId.ItemUltraBall, 50),
                    new KeyValuePair<ItemId, int>(ItemId.ItemMasterBall, 100),

                    new KeyValuePair<ItemId, int>(ItemId.ItemPotion, 0),
                    new KeyValuePair<ItemId, int>(ItemId.ItemSuperPotion, 0),
                    new KeyValuePair<ItemId, int>(ItemId.ItemHyperPotion, 20),
                    new KeyValuePair<ItemId, int>(ItemId.ItemMaxPotion, 50),

                    new KeyValuePair<ItemId, int>(ItemId.ItemRevive, 10),
                    new KeyValuePair<ItemId, int>(ItemId.ItemMaxRevive, 50),

                     new KeyValuePair<ItemId, int>(ItemId.ItemLuckyEgg, 200),

                     new KeyValuePair<ItemId, int>(ItemId.ItemIncenseOrdinary, 100),
                     new KeyValuePair<ItemId, int>(ItemId.ItemIncenseSpicy, 100),
                     new KeyValuePair<ItemId, int>(ItemId.ItemIncenseCool, 100),
                     new KeyValuePair<ItemId, int>(ItemId.ItemIncenseFloral, 100),

                     new KeyValuePair<ItemId, int>(ItemId.ItemTroyDisk, 100),
                     new KeyValuePair<ItemId, int>(ItemId.ItemXAttack, 100),
                     new KeyValuePair<ItemId, int>(ItemId.ItemXDefense, 100),
                     new KeyValuePair<ItemId, int>(ItemId.ItemXMiracle, 100),

                     new KeyValuePair<ItemId, int>(ItemId.ItemRazzBerry, 20),
                     new KeyValuePair<ItemId, int>(ItemId.ItemBlukBerry, 10),
                     new KeyValuePair<ItemId, int>(ItemId.ItemNanabBerry, 10),
                     new KeyValuePair<ItemId, int>(ItemId.ItemWeparBerry, 30),
                     new KeyValuePair<ItemId, int>(ItemId.ItemPinapBerry, 30),

                     new KeyValuePair<ItemId, int>(ItemId.ItemSpecialCamera, 100),
                     new KeyValuePair<ItemId, int>(ItemId.ItemIncubatorBasicUnlimited, 100),
                     new KeyValuePair<ItemId, int>(ItemId.ItemIncubatorBasic, 100),
                     new KeyValuePair<ItemId, int>(ItemId.ItemPokemonStorageUpgrade, 100),
                     new KeyValuePair<ItemId, int>(ItemId.ItemItemStorageUpgrade, 100),
                };
            }

            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
