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
    [Route("api/HerosLoad")]
    public class HerosLoadController : Controller
    {
        private readonly my_appContext _context;

        public HerosLoadController(my_appContext context)
        {
            _context = context;
        }
        // POST: api/HerosLoad
        [HttpPost]
        public async Task<IActionResult> PostHeros([FromBody] PassedData<string> passedData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            DateTime now = DateTime.UtcNow;
            if (passedData.UserToken == null)
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
            Heros hero = _context.Heros.Where(e => e.Name == passedData.Data).Join(_context.UsersHeros.Where(e => e.UserName == dbtoken.UserName), e => e.HeroId, e => e.HeroId, (a, b) => a).FirstOrDefault();
            if (hero == null)
            {
                return BadRequest(new DataError("noHeroErr", "Hero is not available."));
            }
            ActionToken actionToken = Security.GenerateActionToken(hero.HeroId, _context);
            ActionTokenResult tokenResult = new ActionTokenResult()
            {
                HeroName = hero.Name,
                Token = actionToken.HashedToken,
            };
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest(new DataError("databaseErr", "Failed to update tokens."));
            }

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
                LocationDescription description = JsonConvert.DeserializeObject<LocationDescription>(descr.Sketch);
                LocationState state = JsonConvert.DeserializeObject<LocationState>(location.Description);
                if(hero.Status == 1)
                {
                    Traveling travel = _context.Traveling.FirstOrDefault(e => e.HeroId == hero.HeroId);
                    if(travel == null)
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
                    }
                    else
                    {
                        //TODO
                    }

                }
                LocationResult locationResult = description.GenLocalForm(state);
                return Ok(new { success = true, actiontoken = tokenResult, hero = (HeroResult)hero, location = locationResult });
            }
            catch
            {
                return BadRequest(new DataError("LocationErr", "Location is not available."));
            }
        }

    }
}