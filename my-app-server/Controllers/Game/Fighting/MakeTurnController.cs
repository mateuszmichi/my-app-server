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
    [Route("api/MakeTurn")]
    public class MakeTurnController : Controller
    {
        private readonly my_appContext _context;

        public MakeTurnController(my_appContext context)
        {
            _context = context;
        }

        // POST: api/MakeTurn
        [HttpPost]
        public async Task<IActionResult> PostFighting([FromBody] PassedGameData<FightingCalculator.AttackType> passedData)
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
            if (hero.Status == 3)
            {
                var fighting = _context.Fighting.FirstOrDefault(e => e.HeroId == hero.HeroId);
                if(fighting == null)
                {
                    return BadRequest(new DataError("fightingErr", "Failed to load fight. Hero is not in fight."));
                }
                if (fighting.IsOver)
                {
                    return BadRequest(new DataError("fightingErr", "Fight is already finished!"));
                }
                var enemy = _context.Enemies.FirstOrDefault(e => e.EnemyId == fighting.EnemyId);
                if(enemy == null)
                {
                    return BadRequest(new DataError("fightingErr", "Failed to load enemy."));
                }
                var Logs = FightingCalculator.MakeTurn(fighting, hero, enemy, passedData.Data, this._context);
                try
                {
                    await _context.SaveChangesAsync();
                    var result = fighting.GenResult(_context, hero, enemy);
                    result.Log = Logs;
                    return Ok(new { success = true, fightingData = result, heroHP = hero.Hp });
                }
                catch (DbUpdateException)
                {
                    return BadRequest(new DataError("databaseErr", "Failed to update tokens."));
                }
            }
            else
            {
                return BadRequest(new DataError("LocationErr", "Hero is not fighting."));
            }
        }
    }
}