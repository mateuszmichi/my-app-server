using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace my_app_server.Models
{
    public partial class HerosLocations
    {
        public static HerosLocations GenInitialLocation(my_appContext _context,int heroID, int locationID = 1)
        {
            var locSk = _context.LocationsDb.FirstOrDefault(e => e.LocationIdentifier == locationID);
            if(locSk == null)
            {
                throw new Exception("Starting in unknown location.");
            }
            LocationDescription description = JsonConvert.DeserializeObject<LocationDescription>(locSk.Sketch);
            return new HerosLocations()
            {
                Description = JsonConvert.SerializeObject(description.InitializeLocation()),
                HeroId = heroID,
                LocationIdentifier = locationID,
                LocationId = 1000 * heroID + locationID,
            };
        }
    }
}
