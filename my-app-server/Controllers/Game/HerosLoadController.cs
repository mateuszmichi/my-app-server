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

            try
            {
                var heroStatus = LocationHandler.GetHeroGeneralStatus(_context, hero, now);

                // equipment generation
                var Equipment = _context.Equipment.FirstOrDefault(e => e.HeroId == hero.HeroId);
                if (Equipment == null)
                {
                    return BadRequest(new DataError("equipmentErr", "Hero is without equipment."));
                }

                List<int?> used = new List<int?>
                {
                    Equipment.Armour, Equipment.Bracelet, Equipment.FirstHand, Equipment.Gloves, Equipment.Helmet, Equipment.Neckles, Equipment.Ring1, Equipment.Ring2,
                    Equipment.SecondHand, Equipment.Shoes, Equipment.Trousers
                };
                var ItemsOn = used.Where(e => e.HasValue).Select(e => e.Value).ToList();

                var Backpack = _context.Backpack.Where(e => e.HeroId == hero.HeroId);
                var UsedItems = Backpack.Select(e => e.ItemId).Distinct().ToList();
                UsedItems.AddRange(ItemsOn);
                UsedItems = UsedItems.Distinct().OrderBy(e => e).ToList();

                var ItemsInUse = _context.Items.Join(UsedItems, e => e.ItemId, e => e, (a, b) => a).ToArray();

                EquipmentResult EQ = Equipment.GenResult(ItemsInUse.ToArray(), Backpack.ToList());

                return Ok(new { success = true, actiontoken = tokenResult, hero = hero.GenResult(EQ,heroStatus.Location, heroStatus.StatusData) });
            }
            catch (Exception e)
            {
                return BadRequest(new DataError("statusErr", e.Message));
            }
        }

    }
}