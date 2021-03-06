﻿using System;
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
    [Route("api/StartHealing")]
    public class StartHealingController : Controller
    {
        private readonly my_appContext _context;

        public StartHealingController(my_appContext context)
        {
            _context = context;
        }

        // POST: api/StartHealing
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
            if (hero.Status == 0)
            {
                var (hp, sl) = HeroCalculator.HPSLmax(hero, _context);
                if(hero.Hp >= hp)
                {
                    hero.Hp = hp;
                    try
                    {
                        await _context.SaveChangesAsync();
                        return BadRequest(new DataError("heroHpSucc", "Hero has already full HP."));
                    }
                    catch (DbUpdateException)
                    {
                        return BadRequest(new DataError("databaseErr", "Failed to update tokens."));
                    }
                }
                int time = HeroCalculator.RecoveryTime(hp, hero.Hp, hero.Lvl)/hero.VelocityFactor;
                Healing heal = new Healing()
                {
                    EndTime = now.AddSeconds(time),
                    HeroId = hero.HeroId,
                    StartHp = hero.Hp,
                    StartHpmax = hp,
                    StartTime = now,
                };
                this._context.Healing.Add(heal);
                hero.Status = 2;
                try
                {
                    await _context.SaveChangesAsync();
                    HealingResult result = heal.GenHealingResult(now);
                    return Ok(new { success = true, healing = result });
                }
                catch (DbUpdateException)
                {
                    return BadRequest(new DataError("databaseErr", "Failed to update tokens."));
                }
            }
            else
            {
                return BadRequest(new DataError("LocationErr", "Hero is not able to change state."));
            }
        }
    }
}