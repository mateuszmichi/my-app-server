using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using my_app_server.Models;
using Newtonsoft.Json;

namespace my_app_server.Controllers
{
    [Produces("application/json")]
    [Route("api/TravelingsStart")]
    public class TravelingsStartController : Controller
    {
        private readonly my_appContext _context;

        public TravelingsStartController(my_appContext context)
        {
            _context = context;
        }

        // POST: api/TravelingsStart
        [HttpPost]
        public async Task<IActionResult> PostTraveling([FromBody] PassedGameData<int> passedData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            DateTime now = DateTime.UtcNow;
            if (passedData.UserToken == null || passedData.ActionToken == null)
            {
                return BadRequest(new DataError("securityErr", "No authorization controll."));
            }
            UserToken dbtoken = Security.CheckUserToken(this._context, passedData.UserToken);
            if (dbtoken == null)
            {
                return BadRequest(new DataError("securityErr", "Your data has probably been stolen or modified manually. We suggest password's change."));
            }
            else
            {
                if (!dbtoken.IsTimeValid(now))
                {
                    return BadRequest(new DataError("timeoutErr", "You have been too long inactive. Relogin is required."));
                }
                else
                {
                    dbtoken.UpdateToken(now);
                }
            }
            Heros hero = _context.Heros.FirstOrDefault(e => e.Name == passedData.ActionToken.HeroName);
            ActionToken gametoken = Security.CheckActionToken(_context, passedData.ActionToken, hero.HeroId);
            if (gametoken == null)
            {
                return BadRequest(new DataError("securityErr", "Your data has probably been stolen or modified manually. We suggest password's change."));
            }
            else
            {
                if (!gametoken.IsTimeValid(now))
                {
                    return BadRequest(new DataError("timeoutErr", "You have been too long inactive. Relogin is required."));
                }
                else
                {
                    gametoken.UpdateToken(now);
                }
            }
            // if can go there
            if (hero.Status == 0)
            {
                var location = _context.HerosLocations.FirstOrDefault(e => (e.HeroId == hero.HeroId) && (e.LocationIdentifier == hero.CurrentLocation));
                if (location == null)
                {
                    return BadRequest(new DataError("LocationErr", "Location is not available."));
                }
                var descr = _context.LocationsDb.FirstOrDefault(e => e.LocationIdentifier == location.LocationIdentifier);
                if (descr == null)
                {
                    return BadRequest(new DataError("LocationErr", "LocationData is not available."));
                }
                try
                {
                    // TODO check location type
                    LocationDescription description = JsonConvert.DeserializeObject<LocationDescription>(descr.Sketch);
                    LocationState state = JsonConvert.DeserializeObject<LocationState>(location.Description);
                    description.LocationGlobalType = descr.LocationGlobalType;

                    int GlobalNodeID = description.GlobalMainNodeID(passedData.Data, state);
                    if(GlobalNodeID == state.CurrentLocation)
                    {
                        throw new Exception("Moving nowhere");
                    }
                    AstarResult astar = LocationHandler.DistanceToMove(description, state, passedData.Data);
                    double TravelTime = LocationHandler.TimeTravel(astar.Distance, description.TravelScale,18*hero.VelocityFactor);

                    Traveling travel = new Traveling()
                    {
                        EndTime = now.AddSeconds(TravelTime),
                        HeroId = hero.HeroId,
                        IsReverse = false,
                        ReverseTime = null,
                        Start = state.CurrentLocation,
                        StartName = description.MainNodes.First(e => e.NodeID == state.CurrentLocation).Name,
                        StartTime = now,
                        Target = GlobalNodeID,
                        TargetName = description.MainNodes.First(e => e.NodeID == GlobalNodeID).Name,
                    };
                    hero.Status = 1;
                    _context.Traveling.Add(travel);
                    TravelResult travelResult = travel.GenTravelResult(now);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return Ok(new { success = true, travel = travelResult });
                    }
                    catch (DbUpdateException)
                    {
                        return BadRequest(new DataError("databaseErr", "Failed to remember travel."));
                    }
                }
                catch
                {
                    return BadRequest(new DataError("LocationErr", "Location is not available."));
                }
            }
            else
            {
                return BadRequest(new DataError("LocationErr", "Hero is not able to travel."));
            }
        }
    }
}