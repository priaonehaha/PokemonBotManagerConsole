using System;
using System.Threading.Tasks;
using PokemonBotManager.LocationHelper;
using PokemonBotManager.Pokemon;
using PokemonGo.RocketAPI;
using PokemonGoBotLogic;
using PokemonGoBotLogic.Interfaces;

namespace PokemonBotManager.BotManager
{
    public class Bot
    {
        public bool IsWorking
        {
            get { return botTask?.Status == TaskStatus.Running; }
        }
        public int BotId { get; }
        public bool IsValid => BotId != -1;
        public BotSettings Settings { get; }
        private Client Client { get; }
        private ILogic logic;

        private Task botTask;// = Task.Run(logic.Execute());


        public Bot(int botId, Account assignedAccount)
        {
            Settings = new BotSettings(assignedAccount);

            Client = new Client(Settings);
            logic = new Logic(Client, Settings);
        }

        public Bot(int botId, Account assignedAccount, Location assignedLocation) : this(botId, assignedAccount)
        {
            Settings.BottingLocation = assignedLocation;
        }

        public void SetLogic(ILogic newLogic)
        {
            logic = newLogic;
        }

        public void StartBot()
        {

            botTask = Task.Run(logic.Execute).ContinueWith(BotStopped);
            botTask.Start();
            //_client.Login.DoPtcLogin(Settings.AccountData.Username, Settings.AccountData.Password).Wait();
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
        void BotStopped(Task task)
        {
            
        }

        public bool Equals(Bot oBot)
        {
            return BotId == oBot?.BotId;
        }
        public override int GetHashCode()
        {
            return BotId;
        }
        public override bool Equals(Object oBot)
        {
            if (oBot == null || oBot.GetType() != typeof(Bot))
            {
                return false;
            }
            return BotId == ((Bot)oBot).BotId;
        }

        public static bool operator ==(Bot a, Bot b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
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
    }
}
