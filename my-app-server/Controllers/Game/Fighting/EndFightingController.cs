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
    [Route("api/EndFighting")]
    public class EndFightingController : Controller
    {
        private readonly my_appContext _context;

        public EndFightingController(my_appContext context)
        {
            _context = context;
        }


        // POST: api/EndFighting
        [HttpPost]
        public async Task<IActionResult> PostFighting([FromBody] PassedGameData<int?> passedData)
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
                // to pass
                object statusData = null;
                var Added = new List<EquipmentModifyResult.EquipmentModification>();
                var newItems = new List<ItemResult>();

                var fight = _context.Fighting.FirstOrDefault(e => e.HeroId == hero.HeroId);
                if (fight == null)
                {
                    return BadRequest(new DataError("fightErr", "Hero has no fight data."));
                }
                if (!fight.IsOver)
                {
                    return BadRequest(new DataError("fightErr", "Fight has not been finished."));
                }
                if (hero.Hp > 0)
                {
                    hero.Status = 0;
                    if (fight.Loot.HasValue)
                    {
                        var Item = _context.Items.FirstOrDefault(e => e.ItemId == fight.Loot.Value);
                        if (Item == null)
                        {
                            return BadRequest(new DataError("itemErr", "Looted item not found."));
                        }
                        newItems.Add((ItemResult)Item);
                        var heroBackpack = _context.Backpack.Where(e => e.HeroId == hero.HeroId);
                        int count = heroBackpack.Count();
                        if (count > 0)
                        {
                            var Equipment = _context.Equipment.FirstOrDefault(e => e.HeroId == hero.HeroId);
                            if (Equipment == null)
                            {
                                return BadRequest(new DataError("heroErr", "Hero without equipment."));
                            }
                            if (Equipment.BackpackSize <= count)
                            {
                                return BadRequest(new DataError("backpackErr", "In order to add this reward, you need to remove one item from backpack or resign from reward."));
                            }
                            if (heroBackpack.Where(e => e.SlotNr == 0).Count() > 0)
                            {
                                var preSlot = heroBackpack.FirstOrDefault(e => heroBackpack.Where(f => f.SlotNr == e.SlotNr + 1).Count() == 0);
                                _context.Backpack.Add(new Backpack()
                                {
                                    HeroId = hero.HeroId,
                                    ItemId = fight.Loot.Value,
                                    SlotNr = preSlot.SlotNr + 1,
                                });
                                Added.Add(new EquipmentModifyResult.EquipmentModification() { ItemID = fight.Loot.Value, Target = "Backpack" + (preSlot.SlotNr + 1) });
                            }
                            else
                            {
                                _context.Backpack.Add(new Backpack()
                                {
                                    HeroId = hero.HeroId,
                                    ItemId = fight.Loot.Value,
                                    SlotNr = 0,
                                });
                                Added.Add(new EquipmentModifyResult.EquipmentModification() { ItemID = fight.Loot.Value, Target = "Backpack" + 0 });
                            }

                        }
                        else
                        {
                            _context.Backpack.Add(new Backpack()
                            {
                                HeroId = hero.HeroId,
                                ItemId = fight.Loot.Value,
                                SlotNr = 0,

                            });
                            Added.Add(new EquipmentModifyResult.EquipmentModification() { ItemID = fight.Loot.Value, Target = "Backpack" + 0 });
                        }
                    }
                    var enemy = _context.Enemies.FirstOrDefault(e => e.EnemyId == fight.EnemyId);
                    if (enemy == null)
                    {
                        return BadRequest(new DataError("fightErr", "Enemy not found."));
                    }
                    hero.Experience += HeroCalculator.ExpForFight(hero.Lvl, enemy.Lvl);
                    bool update = false;
                    while (hero.Experience >= HeroCalculator.ExpToLevel(hero.Lvl + 1))
                    {
                        hero.Lvl += 1;
                        update = true;
                    }
                    if (update)
                    {
                        var (hp, sl) = HeroCalculator.HPSLmax(hero, _context);
                        hero.Hp = hp;
                    }
                }
                else
                {
                    hero.Status = 2;
                    var (hp, sl) = HeroCalculator.HPSLmax(hero, _context);
                    if (hero.Hp >= hp)
                    {
                        hero.Hp = hp;
                        try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (DbUpdateException)
                        {
                            return BadRequest(new DataError("databaseErr", "Failed to update tokens."));
                        }
                    }
                    int time = HeroCalculator.RecoveryTime(hp, hero.Hp, hero.Lvl) / hero.VelocityFactor;
                    Healing heal = new Healing()
                    {
                        EndTime = now.AddSeconds(time),
                        HeroId = hero.HeroId,
                        StartHp = hero.Hp,
                        StartHpmax = hp,
                        StartTime = now,
                    };
                    this._context.Healing.Add(heal);

                    statusData = heal.GenHealingResult(now);
                }
                // TODO update map!
                _context.Fighting.Remove(fight);
                try
                {
                    var location = LocationHandler.InstanceClearCurrent(_context, hero);
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateException)
                    {
                        return BadRequest(new DataError("databaseErr", "Failed to update tokens."));
                    }
                    return Ok(new { success = true, heroStatus = hero.Status, newHP = hero.Hp, newHPmax = HeroCalculator.BaseHP(hero.Lvl), newLvl = hero.Lvl, newExp = hero.Experience, location, statusData, added = Added.ToArray(), newItems = newItems.ToArray() });
                }
                catch (OperationException e)
                {
                    return BadRequest(new DataError(e.ErrorClass, e.Message));
                }
            }
            else
            {
                return BadRequest(new DataError("LocationErr", "Hero is not in the fight."));
            }
        }

    }
}