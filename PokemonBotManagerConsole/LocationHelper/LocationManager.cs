using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace PokemonBotManagerConsole.LocationHelper
{
    [DataContract]
    public class LocationManager
    {
        private static readonly object SyncRoot = new Object();

        private static LocationManager _instance;
        public static LocationManager Instance
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
                                _instance = new LocationManager();

                            }
                        }
                    }
                }
                return _instance;
            }
        }

        private static readonly DataContractSerializer Serializer = new DataContractSerializer(typeof(LocationManager));

        private const string SerializedFile = "Locations.xml";


        [DataMember]
        private List<Location> locations { get; set; } = new List<Location>();

        [DataMember]
        public int NextId { get; set; } = 0;

        private LocationManager()
        {
        }

        public ReadOnlyCollection<Location> GetLocations()
        {
            return locations.AsReadOnly();
        } 

        public void AddLocation(Location location)
        {
            if (locations.Any(l=>l == location))
            {
                return;
            }
            location.LocationId = NextId;
            locations.Add(location);
            ++NextId;

        }

        public void Remove(Location location)
        {
            locations.RemoveAll(l => l == location);
        }

        public void RemoveByName(string locationName)
        {
            locations.RemoveAll(l => l.LocationName == locationName);
        }

        public void RemoveById(int locationId)
        {
            locations.RemoveAll(l => l.LocationId == locationId);
        }

        public void Serialize()
        {
            using (var xw = XmlWriter.Create(SerializedFile, new XmlWriterSettings() {Indent = true}))
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
                    _instance = (LocationManager)Serializer.ReadObject(fs);
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
