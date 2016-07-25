using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PokemonBotManagerConsole.Pokemon
{
    [DataContract]
    public class Account
    {
        [DataMember]
        public string Username { get; private set; }
        [DataMember]
        public string Password { get; private set; }
        [DataMember]
        public string Email { get; private set; }
        [DataMember]
        public bool Verified { get; private set; }

        [DataMember]
        public int LatestLocationId { get; set; } = 0;

        public Account()
        {
        }
        public Account(string username, string password, string email, bool verified)
        {
            Username = username;
            Password = password;
            Email = email;
            Verified = verified;
        }
    }
}
