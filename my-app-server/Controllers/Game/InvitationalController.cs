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
    [Route("api/Invitational")]
    public class InvitationalController : Controller
    {
        private readonly my_appContext _context;

        public InvitationalController(my_appContext context)
        {
            _context = context;
        }
        // POST: api/Invitational
        [HttpPost]
        public async Task<IActionResult> PostHeros([FromBody] PassedGameData<bool[]> passedData)
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
            if (hero.Invitational)
            {
                try
                {
                    if (passedData.Data[0])
                    {
                        hero.VelocityFactor = 10;
                    }
                    if (passedData.Data[1])
                    {
                        hero.Lvl = 5;
                        hero.Experience = HeroCalculator.ExpToLevel(5);
                    }
                    if (passedData.Data[2])
                    {
                        var list = new List<Backpack>();
                        for(int i = 1; i <= 16; i++)
                        {
                            list.Add(new Backpack()
                            {
                                HeroId = hero.HeroId,
                                ItemId = i,
                                SlotNr = i - 1,
                            });
                        }
                        _context.Backpack.AddRange(list);
                    }
                    hero.Invitational = false;
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true});
                }
                catch (DbUpdateException)
                {
                    return BadRequest(new DataError("databaseErr", "Failed to update hero."));
                }
            }
            else
            {
                return BadRequest(new DataError("securityErr", "This option should be impossible to reach."));
            }
        }
    }
}