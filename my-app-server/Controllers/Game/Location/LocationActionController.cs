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
    [Route("api/LocationAction")]
    public class LocationActionController : Controller
    {
        private readonly my_appContext _context;

        public LocationActionController(my_appContext context)
        {
            _context = context;
        }

        // POST: api/LocationAction
        [HttpPost]
        public async Task<IActionResult> PostLocationAction([FromBody] PassedGameData<int> passedData)
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
            // now do your stuff...
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
                    int LocationType = descr.LocationGlobalType;
                    if (LocationType != 2)
                    {
                        LocationDescription description = JsonConvert.DeserializeObject<LocationDescription>(descr.Sketch);
                        LocationState state = JsonConvert.DeserializeObject<LocationState>(location.Description);
                        description.LocationGlobalType = descr.LocationGlobalType;

                        var CurrentNode = description.MainNodes.FirstOrDefault(e => e.NodeID == state.CurrentLocation);

                        if (CurrentNode.Data == -1)
                        {
                            try
                            {
                                await _context.SaveChangesAsync();
                            }
                            catch (DbUpdateException)
                            {
                                return BadRequest(new DataError("databaseErr", "Failed to remember travel."));
                            }
                            return BadRequest(new DataError("notImplementedErr", "This feature has not been implemented yet. We are working on it!"));
                        }
                        try
                        {
                            if (!LocationHandler.OptionsForLocation.ContainsKey(CurrentNode.LocationType))
                            {
                                throw new Exception();
                            }
                            if (!LocationHandler.LocationTypeFunctions.ContainsKey(LocationHandler.OptionsForLocation[CurrentNode.LocationType][passedData.Data]))
                            {
                                throw new Exception();
                            }
                            var func = LocationHandler.LocationTypeFunctions[LocationHandler.OptionsForLocation[CurrentNode.LocationType][passedData.Data]];
                            func(_context, hero, CurrentNode.Data);
                            try
                            {
                                await _context.SaveChangesAsync();
                            }
                            catch (DbUpdateException)
                            {
                                return BadRequest(new DataError("databaseErr", "Failed to remember action."));
                            }
                        }
                        catch(OperationException e)
                        {
                            return BadRequest(new DataError(e.ErrorClass, e.Message));
                        }
                        catch
                        {
                            return BadRequest(new DataError("notImplementedErr", "This feature has not been implemented yet. We are working on it!"));
                        }
                    }
                    else
                    {
                        InstanceDescription description = JsonConvert.DeserializeObject<InstanceDescription>(descr.Sketch);
                        InstanceState state = JsonConvert.DeserializeObject<InstanceState>(location.Description);
                        description.LocationGlobalType = descr.LocationGlobalType;

                        var CurrentNode = description.MainNodes.FirstOrDefault(e => e.NodeID == state.CurrentLocation);

                        if (CurrentNode.Data == -1)
                        {
                            try
                            {
                                await _context.SaveChangesAsync();
                            }
                            catch (DbUpdateException)
                            {
                                return BadRequest(new DataError("databaseErr", "Failed to remember travel."));
                            }
                            return BadRequest(new DataError("notImplementedErr", "This feature has not been implemented yet. We are working on it!"));
                        }
                        try
                        {
                            if (!LocationHandler.OptionsForInstances.ContainsKey(CurrentNode.InstanceType))
                            {
                                throw new Exception();
                            }
                            if (!LocationHandler.InstanceTypeFunctions.ContainsKey(LocationHandler.OptionsForInstances[CurrentNode.InstanceType][passedData.Data]))
                            {
                                throw new Exception();
                            }
                            var func = LocationHandler.InstanceTypeFunctions[LocationHandler.OptionsForInstances[CurrentNode.InstanceType][passedData.Data]];
                            func(_context, hero, CurrentNode.Data);
                            try
                            {
                                await _context.SaveChangesAsync();
                            }
                            catch (DbUpdateException)
                            {
                                return BadRequest(new DataError("databaseErr", "Failed to remember action."));
                            }
                        }
                        catch
                        {
                            return BadRequest(new DataError("notImplementedErr", "This feature has not been implemented yet. We are working on it!"));
                        }
                    }
                    // load new hero status
                    try
                    {
                        var heroStatus = LocationHandler.GetHeroGeneralStatus(_context, hero, now);
                        return Ok(new { success = true, location = heroStatus.Location,statusData = heroStatus.StatusData,heroStatus = heroStatus.HeroStatus });
                    }
                    catch (Exception e)
                    {
                        return BadRequest(new DataError("statusErr", e.Message));
                    }
                }
                catch
                {
                    return BadRequest(new DataError("LocationErr", "Location is not available."));
                }
            }
            else
            {
                return BadRequest(new DataError("LocationErr", "Hero is not able to change state."));
            }
        }
    }
}