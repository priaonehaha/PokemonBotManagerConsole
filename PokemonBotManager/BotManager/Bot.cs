using System;
using System.Linq;
using System.Threading.Tasks;
using PokemonBotManager.BotManager.Exceptions;
using PokemonBotManager.BotManager.Interfaces;
using PokemonBotManager.LocationHelper;
using PokemonBotManager.Pokemon;
using PokemonGo.RocketAPI;

namespace PokemonBotManager.BotManager
{
    public class Bot
    {
        private Task botTask; // = Task.Run(logic.Execute());
        private ILogic logic;

        public bool IsWorking
        {
            get { return botTask?.Status == TaskStatus.WaitingForActivation; }
        }

        public int BotId { get; }
        public bool IsValid => BotId != -1;
        public BotSettings Settings { get; }
        public Client Client { get; }

        public Bot(int botId, Account assignedAccount)
        {
            BotId = botId;
            Settings = new BotSettings(assignedAccount)
            {
                BottingLocation = LocationManager.Instance.GetLocations().FirstOrDefault()
            };
            Client = new Client(Settings);
           // logic = new Logic(Client, Settings);
        }

        public Bot(int botId, Account assignedAccount, Location assignedLocation) : this(botId, assignedAccount)
        {
            Settings.BottingLocation = assignedLocation;
        }

        public void PrintBotStatus()
        {
            Console.WriteLine($"{this}");
            if (IsWorking)
            {
                try
                {
                    var stats = logic.GetPlayerStats().Result;
                    var profile = Client.Player.GetPlayer().Result.PlayerData;
                    var pokemons = Client.Inventory.GetInventory().Result.InventoryDelta.InventoryItems.Select(i => i.InventoryItemData?.PokemonData).Where(p => p?.PokemonId > 0).ToArray();
                    Console.WriteLine(
                        $"Level: {stats.Level}({stats.Experience}/{stats.NextLevelXp}) | Team: {profile.Team} " +
                        $"| Stardust: {profile.Currencies[1].Amount} | Pokemons: {pokemons.Length}");
                }
                catch (Exception)
                {
                    Console.WriteLine("Caugth Exception, Bot still not logged in");
                }

            }
        }

        public void SetLogic(ILogic newLogic)
        {
            logic = newLogic;
        }

        public void StartBot()
        {
            if (Settings.BottingLocation == null)
            {
                throw new LocationNotSetException($"The location for bot {BotId}, with Account {Settings.AccountData}");
            }
            Settings.AccountData.LatestLocationId = Settings.BottingLocation.LocationId;
            botTask = Task.Run(logic.Execute).ContinueWith(BotStopped);
            //botTask.Start();
            //_client.Login.DoPtcLogin(Settings.AccountData.Username, Settings.AccountData.Password).Wait();
        }

        public void SetLocation(Location location)
        {
            if (IsWorking)
            {
                throw new Exception("Cant change location of running Bot");
            }
            Settings.BottingLocation = location;
        }

        public void StopBot(bool waitForBot = true)
        {
            logic.StopBot();
            if (waitForBot)
            {
                botTask.Wait();
            }
        }

        //TODO: idk
        private void BotStopped(Task task)
        {
            //YOU ARE  NOT ALLOWED TO STOP
            Console.WriteLine($"{this} stopped, restarting");
            StartBot();
        }

        public bool Equals(Bot oBot)
        {
            return BotId == oBot?.BotId;
        }

        public override int GetHashCode()
        {
            return BotId;
        }

        public override bool Equals(object oBot)
        {
            if (oBot == null || oBot.GetType() != typeof(Bot))
            {
                return false;
            }
            return BotId == ((Bot) oBot).BotId;
        }

        public static bool operator ==(Bot a, Bot b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object) a == null) || ((object) b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.BotId == b.BotId;
        }

        public static bool operator !=(Bot a, Bot b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return
                $"ID: {BotId}, Account: {Settings.AccountData}, Location: {Settings.BottingLocation} Working: {IsWorking}";
        }
    }
}