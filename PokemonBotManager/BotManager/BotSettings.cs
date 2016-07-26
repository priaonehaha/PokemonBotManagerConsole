using System;
using System.Collections.Generic;
using AllEnum;
using PokemonBotManager.LocationHelper;
using PokemonBotManager.Pokemon;
using PokemonGo.RocketAPI;
using PokemonGo.RocketAPI.Enums;

namespace PokemonBotManager.BotManager
{
    class BotSettings : ISettings
    {
        public AuthType AuthType => AuthType.Ptc;
        public double DefaultAltitude => 10d;
        public double DefaultLatitude { get; set; }
        public double DefaultLongitude { get; set; }

        public string GoogleRefreshToken
        {
            get { return ""; }
            set { throw new NotImplementedException(); }
        }

        public string PtcUsername
        {
            get { return AccountData.Username; }
            set { throw new NotImplementedException(); }
        }

        public string PtcPassword
        {
            get { return AccountData.Password; }
            set { throw new NotImplementedException(); }
        }

        public Location BottingLocation { get; set; }
        public Account AccountData { get; private set; }

        ICollection<KeyValuePair<ItemId, int>> ISettings.itemRecycleFilter
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

        public BotSettings(Account account)
        {
            AccountData = account;
        }

        public BotSettings(Account account, Location location)
        {
            AccountData = account;
            BottingLocation = location;
        }
    }
}
