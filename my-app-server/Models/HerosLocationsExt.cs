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
            string Description = "";
            int LocationType = locSk.LocationGlobalType;
            if (LocationType != 2)
            {
                LocationDescription description = JsonConvert.DeserializeObject<LocationDescription>(locSk.Sketch);
                Description = JsonConvert.SerializeObject(description.InitializeLocation());
            } else
            {
                InstanceDescription description = JsonConvert.DeserializeObject<InstanceDescription>(locSk.Sketch);
                Description = JsonConvert.SerializeObject(description.InitializeLocation());
            }
                
            return new HerosLocations()
            {
                Description = Description,
                HeroId = heroID,
                LocationIdentifier = locationID,
                LocationId = 1000 * heroID + locationID,
            };
        }
    }
}
