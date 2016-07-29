using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Rpc;
using POGOProtos.Enums;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Responses;

namespace PokemonGoBotLogic.Logic
{
    public partial class Logic
    {
        public async Task CatchLurePokemon(ulong encounterId, string id, DiskEncounterResponse diskEncounter, PokemonId pokemonId)
        {
            CatchPokemonResponse caughtPokemonResponse;
            var attempts = 0;
            do
            {
                var probability = diskEncounter.CaptureProbability.CaptureProbability_.FirstOrDefault();
                var pokeball = GetPokeball(diskEncounter);
                if (pokeball == ItemId.ItemUnknown)
                    return;
                caughtPokemonResponse =
                    await PClient.Encounter.CatchPokemon(encounterId, id, pokeball);
                Debug.WriteLine($"[{caughtPokemonResponse.Status} - {attempts}] {pokemonId} encountered. {diskEncounter?.PokemonData?.Cp} CP. Probabilty: {probability}");
                attempts++;
            } while (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchMissed ||
                     caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape);
        }

        public async Task CatchWildPokemon(EncounterResponse encounter, MapPokemon pokemon)
        {
            CatchPokemonResponse caughtPokemonResponse;
            var attempts = 0;
            do
            {
                var probability = encounter.CaptureProbability.CaptureProbability_?.FirstOrDefault();

                var pokeball = GetPokeball(encounter);
                if (pokeball == ItemId.ItemUnknown)
                    return;
                var isLowProbability = probability.HasValue && probability.Value < 0.35;
                var isHighCp = encounter != null && encounter.WildPokemon?.PokemonData?.Cp > 400;

                if (isLowProbability && isHighCp)
                {
                    var berry = _inventory.Items.FirstOrDefault(i => i.ItemId == ItemId.ItemRazzBerry);
                    if (berry != null && berry.Count > 0)
                    {
                        await PClient.Encounter.UseCaptureItem(pokemon.EncounterId, ItemId.ItemRazzBerry, pokemon.SpawnPointId);
                        berry.Count--;//Because value is cached
                    }
                    await Task.Delay(100);//Wait for berry effect
                }

                caughtPokemonResponse =
                    await PClient.Encounter.CatchPokemon(pokemon.EncounterId, pokemon.SpawnPointId, pokeball);
                Debug.WriteLine($"[{caughtPokemonResponse.Status} - {attempts}] {pokemon.PokemonId} encountered.{encounter.WildPokemon?.PokemonData?.Cp} CP. Probabilty: {probability}");
                attempts++;
            } while (caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchMissed ||
                     caughtPokemonResponse.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape);
        }
    }
}
