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
    [Route("api/EndHealing")]
    public class EndHealingController : Controller
    {
        private readonly my_appContext _context;

        public EndHealingController(my_appContext context)
        {
            _context = context;
        }

        // POST: api/EndHealing
        [HttpPost]
        public async Task<IActionResult> PostHealing([FromBody] PassedGameData<int?> passedData)
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
            if (hero.Status == 2)
            {
                var heal = _context.Healing.FirstOrDefault(e => e.HeroId == hero.HeroId);
                if (heal == null)
                {
                    return BadRequest(new DataError("heroHpSucc", "Hero has no healing progress."));
                }
                int newHP = heal.FinalHealth(now);
                hero.Hp = newHP;
                HeroCalculator.CheckHeroHP(hero, _context);

                _context.Healing.Remove(heal);
                hero.Status = 0;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    return BadRequest(new DataError("databaseErr", "Failed to update tokens."));
                }
                return Ok(new { success = true, newHP = hero.Hp });
            }
            else
            {
                return BadRequest(new DataError("LocationErr", "Hero is not healing."));
            }
        }
    }
}