using System;
using System.Collections.Generic;

namespace my_app_server.Models
{
    public partial class LocationsDb
    {
        public LocationsDb()
        {
            HerosLocations = new HashSet<HerosLocations>();
        }

        public int LocationIdentifier { get; set; }
        public string Sketch { get; set; }

        public ICollection<HerosLocations> HerosLocations { get; set; }
    }
}
