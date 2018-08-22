using System;
using System.Collections.Generic;

namespace my_app_server.Models
{
    public partial class HerosLocations
    {
        public int HeroId { get; set; }
        public int LocationIdentifier { get; set; }
        public int LocationId { get; set; }
        public string Description { get; set; }

        public Heros Hero { get; set; }
        public LocationsDb LocationIdentifierNavigation { get; set; }
    }
}
