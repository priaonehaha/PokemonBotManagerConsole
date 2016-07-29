using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Rpc;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Responses;

namespace PokemonGoBotLogic.Logic
{
    public partial class Logic
    {
        public ItemId GetPokeball(dynamic encounter)
        {
            int pokemonCp;
            if (encounter is EncounterResponse)
            {
                pokemonCp =  encounter.WildPokemon.PokemonData.Cp;
            }
            else if (encounter is DiskEncounterResponse)
            {
                pokemonCp = encounter?.PokemonData?.Cp;
            }
            else
            {
                throw new NotImplementedException();
            }
            pokemonCp = encounter?.PokemonData?.Cp;
            var probability = encounter?.CaptureProbability?.CaptureProbability_.First();

            var pokeBallsCount = _inventory.GetItemAmountByType(ItemId.ItemPokeBall).Result;
            var greatBallsCount = _inventory.GetItemAmountByType(ItemId.ItemGreatBall).Result;
            var ultraBallsCount = _inventory.GetItemAmountByType(ItemId.ItemUltraBall).Result;
            var masterBallsCount = _inventory.GetItemAmountByType(ItemId.ItemMasterBall).Result;

            if (masterBallsCount > 0 && pokemonCp >= 1800)
                return ItemId.ItemMasterBall;
            if (ultraBallsCount > 0 && pokemonCp >= 1000)
                return ItemId.ItemUltraBall;
            if (greatBallsCount > 0 && pokemonCp >= 750)
                return ItemId.ItemGreatBall;

            if (ultraBallsCount > 0 && probability < 0.40)
                return ItemId.ItemUltraBall;

            if (greatBallsCount > 0 && probability < 0.50)
                return ItemId.ItemGreatBall;

            if (greatBallsCount > 0 && pokemonCp >= 300)
                return ItemId.ItemGreatBall;

            if (pokeBallsCount > 0)
                return ItemId.ItemPokeBall;
            if (greatBallsCount > 0)
                return ItemId.ItemGreatBall;
            if (ultraBallsCount > 0)
                return ItemId.ItemUltraBall;
            if (masterBallsCount > 0)
                return ItemId.ItemMasterBall;

            return ItemId.ItemUnknown;
        }
    }
}
