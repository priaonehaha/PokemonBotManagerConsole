using System;
using System.Runtime.Serialization;

namespace PokemonBotManager.LocationHelper
{
    [DataContract]
    public class Location
    {
        [DataMember]
        public int LocationId { get; set; }
        [DataMember]
        public string LocationName { get; private set; }
        [DataMember]
        public double Latitude { get; private set; }
        [DataMember]
        public double Longitude { get; private set; }

        public Location()
        {
            
        }

        public Location(string name, double latitude, double longitude)
        {
            LocationName = name;
            Latitude = latitude;
            Longitude = longitude;
        }

        public static bool operator == (Location l1, Location l2)
        {
            if ((object)l1 == null || (object)l2 == null)
            {
                return false;
            }
            return Math.Abs(l1.Latitude - l2.Latitude) <= double.Epsilon && Math.Abs(l1.Longitude - l2.Longitude) <= double.Epsilon;
        }

        public static bool operator !=(Location l1, Location l2)
        {
            return !(l1 == l2);
        }
        protected bool Equals(Location other)
        {
            return LocationId == other.LocationId && string.Equals(LocationName, other.LocationName) && Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Location)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = LocationId;
                hashCode = (hashCode * 397) ^ (LocationName != null ? LocationName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Latitude.GetHashCode();
                hashCode = (hashCode * 397) ^ Longitude.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{LocationId}, {LocationName}";
        }
    }
}
