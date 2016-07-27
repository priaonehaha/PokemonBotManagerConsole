using System.Runtime.Serialization;

namespace PokemonBotManager.Pokemon
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

        public override string ToString()
        {
            return $"{Username}, {Email}";
        }
    }
}
