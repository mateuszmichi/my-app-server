﻿using System;
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
    [Route("api/HerosLocationsLoad")]
    public class HerosLocationsLoadController : Controller
    {
        private readonly my_appContext _context;

        public HerosLocationsLoadController(my_appContext context)
        {
            _context = context;
        }
        //TODO!!!!
        // POST: api/HerosLocationsLoad
        [HttpPost]
        public async Task<IActionResult> PostHerosLocations([FromBody] PassedGameData<int?> passedData)
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
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest(new DataError("databaseErr", "Failed to update tokens."));
            }

            // can load location status - update status -> function (?)

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
                // TODO check location type -> vitual class or what?
                int LocationType = descr.LocationGlobalType;
                if(LocationType != 2)
                {
                    LocationDescription description = JsonConvert.DeserializeObject<LocationDescription>(descr.Sketch);
                    LocationState state = JsonConvert.DeserializeObject<LocationState>(location.Description);
                    description.LocationGlobalType = LocationType;

                    if (hero.Status == 1)
                    {
                        Traveling travel = _context.Traveling.FirstOrDefault(e => e.HeroId == hero.HeroId);
                        if (travel == null)
                        {
                            throw new Exception("Traveling hero without travel in DB.");
                        }
                        if (travel.HasEnded(now))
                        {
                            state = description.MoveTo(travel.UpdatedLocationID(), state);
                            hero.Status = 0;
                            location.Description = JsonConvert.SerializeObject(state);
                            _context.Traveling.Remove(travel);
                            try
                            {
                                await _context.SaveChangesAsync();
                            }
                            catch (DbUpdateException)
                            {
                                return BadRequest(new DataError("databaseErr", "Failed to remove travel."));
                            }
                            LocationResult<MainNodeResult> locationResult = description.GenLocalForm(state);
                            return Ok(new { success = true, location = locationResult });
                        }
                        else
                        {
                            return BadRequest(new DataError("LocationErr", "Travel is not finished"));
                        }
                    }
                    else
                    {
                        return BadRequest(new DataError("LocationErr", "Hero is not in travel mode"));
                    }
                }
                else
                {
                    InstanceDescription description = JsonConvert.DeserializeObject<InstanceDescription>(descr.Sketch);
                    InstanceState state = JsonConvert.DeserializeObject<InstanceState>(location.Description);
                    description.LocationGlobalType = LocationType;

                    if (hero.Status == 1)
                    {
                        Traveling travel = _context.Traveling.FirstOrDefault(e => e.HeroId == hero.HeroId);
                        if (travel == null)
                        {
                            throw new Exception("Traveling hero without travel in DB.");
                        }
                        if (travel.HasEnded(now))
                        {
                            state = description.MoveTo(travel.UpdatedLocationID(), state);
                            hero.Status = 0;
                            location.Description = JsonConvert.SerializeObject(state);
                            _context.Traveling.Remove(travel);
                            try
                            {
                                await _context.SaveChangesAsync();
                            }
                            catch (DbUpdateException)
                            {
                                return BadRequest(new DataError("databaseErr", "Failed to remove travel."));
                            }
                            LocationResult<InstanceNodeResult> locationResult = description.GenLocalForm(state);
                            return Ok(new { success = true, location = locationResult });
                        }
                        else
                        {
                            return BadRequest(new DataError("LocationErr", "Travel is not finished"));
                        }
                    }
                    else
                    {
                        return BadRequest(new DataError("LocationErr", "Hero is not in travel mode"));
                    }
                }
                
            }
            catch
            {
                return BadRequest(new DataError("LocationErr", "Location is not available."));
            }
        }
    }
}