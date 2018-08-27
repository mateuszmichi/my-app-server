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
    [Route("api/HerosLocationsMoveTo")]
    public class HerosLocationsMoveToController : Controller
    {
        private readonly my_appContext _context;

        public HerosLocationsMoveToController(my_appContext context)
        {
            _context = context;
        }

        //// POST: api/HerosLocationsMoveTo
        //[HttpPost]
        //public async Task<IActionResult> PostHerosLocations([FromBody] PassedGameData<string> passedData)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }
        //    DateTime now = DateTime.UtcNow;
        //    if (passedData.UserToken == null || passedData.ActionToken == null)
        //    {
        //        return BadRequest(new DataError("securityErr", "No authorization controll."));
        //    }
        //    UserToken dbtoken = Security.CheckUserToken(this._context, passedData.UserToken);
        //    if (dbtoken == null)
        //    {
        //        return BadRequest(new DataError("securityErr", "Your data has probably been stolen or modified manually. We suggest password's change."));
        //    }
        //    else
        //    {
        //        if (!dbtoken.IsTimeValid(now))
        //        {
        //            return BadRequest(new DataError("timeoutErr", "You have been too long inactive. Relogin is required."));
        //        }
        //        else
        //        {
        //            dbtoken.UpdateToken(now);
        //        }
        //    }
        //    Heros hero = _context.Heros.FirstOrDefault(e => e.Name == passedData.ActionToken.HeroName);
        //    ActionToken gametoken = Security.CheckActionToken(_context, passedData.ActionToken, hero.HeroId);
        //    if (gametoken == null)
        //    {
        //        return BadRequest(new DataError("securityErr", "Your data has probably been stolen or modified manually. We suggest password's change."));
        //    }
        //    else
        //    {
        //        if (!gametoken.IsTimeValid(now))
        //        {
        //            return BadRequest(new DataError("timeoutErr", "You have been too long inactive. Relogin is required."));
        //        }
        //        else
        //        {
        //            gametoken.UpdateToken(now);
        //        }
        //    }
        //    // if can go there
                
        //    // go there 
        //    var location = _context.HerosLocations.FirstOrDefault(e => (e.HeroId == hero.HeroId) && (e.LocationIdentifier == hero.CurrentLocation));
        //    if (location == null)
        //    {
        //        return BadRequest(new DataError("LocationErr", "Location is not available."));
        //    }
        //    var descr = _context.LocationsDb.FirstOrDefault(e => e.LocationIdentifier == location.LocationIdentifier);
        //    if (descr == null)
        //    {
        //        return BadRequest(new DataError("LocationErr", "LocationData is not available."));
        //    }
        //    _context.Traveling.Add
        //}

    }
}