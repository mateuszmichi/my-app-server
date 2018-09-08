using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using my_app_server.Models;

namespace my_app_server.Controllers
{
    [Produces("application/json")]
    [Route("api/TravelingsReverse")]
    public class TravelingsReverseController : Controller
    {
        private readonly my_appContext _context;

        public TravelingsReverseController(my_appContext context)
        {
            _context = context;
        }
        // POST: api/TravelingsReverse
        [HttpPost]
        public async Task<IActionResult> PostTraveling([FromBody] PassedGameData<int?> passedData)
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
            if (hero.Status == 1)
            {
                try
                {
                    var Travel = _context.Traveling.FirstOrDefault(e => e.HeroId == hero.HeroId);
                    if(Travel == null || Travel.HasEnded(now) || Travel.IsReverse)
                    {
                        throw new Exception();
                    }
                    Travel.IsReverse = true;
                    Travel.ReverseTime = now;
                    TravelResult travelResult = Travel.GenTravelResult(now);
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
                    return BadRequest(new DataError("TravelErr", "Travel reverse is not available."));
                }
            }
            else
            {
                return BadRequest(new DataError("LocationErr", "Hero is not in the travel."));
            }
        }
    }
}