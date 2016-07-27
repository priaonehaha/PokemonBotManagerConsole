using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using PokemonBotManager.LocationHelper;

namespace PokemonBotManager.Pokemon
{
    [DataContract]
    public class AccountList
    {
        [DataMember]
        public List<Account> Accounts = new List<Account>();

        private static readonly object SyncRoot = new Object();

        private static AccountList _instance;
        public static AccountList Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                        {

                            if (!Deserialize())
                            {
                                _instance = new AccountList();

                            }
                        }
                    }
                }
                return _instance;
            }
        }

        private AccountList() { }

        private static readonly DataContractSerializer Serializer = new DataContractSerializer(typeof(AccountList));

        private const string SerializedFile = "AccountList.xml";

        public void AddAccount(Account person)
        {
            Accounts.Add(person);
        }

        public int LoadFromFile(string file = "PTCAccounts.txt")
        {
            int loadedAccs = 0;
            System.IO.StreamReader fileReader = new System.IO.StreamReader(file);
            string line;
            string[] separatingChars = { " - " };
            while ((line = fileReader.ReadLine()) != null)
            {
                string[] acc = line.Split(separatingChars, System.StringSplitOptions.RemoveEmptyEntries);
                if (Accounts.Any(a => a.Username == acc[0]))
                {
                    continue;
                }
                ++loadedAccs;
                Accounts.Add(new Account(acc[0], acc[2], acc[1], acc[3] == "True"));
            }
            return loadedAccs;
        }

        public void Serialize()
        {
            using (var xw = XmlWriter.Create(SerializedFile, new XmlWriterSettings() { Indent = true }))
            {
                Serializer.WriteObject(xw, this);
                xw.Close();
            }
        }

        private static bool Deserialize()
        {
            try
            {
                using (var fs = XmlReader.Create(SerializedFile))
                {
                    _instance = (AccountList)Serializer.ReadObject(fs);
                    fs.Close();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
