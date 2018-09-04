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
    [Route("api/Heros")]
    public class HerosController : Controller
    {
        private readonly my_appContext _context;

        public HerosController(my_appContext context)
        {
            _context = context;
        }

        // POST: api/Heros
        [HttpPost]
        public async Task<IActionResult> PostHeros([FromBody] PassedData<HeroPassed> data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            DateTime now = DateTime.UtcNow;
            if(data.UserToken == null)
            {
                return BadRequest(new DataError("securityErr", "No authorization controll."));
            }
            UserToken dbtoken = Security.CheckUserToken(this._context, data.UserToken);
            if(dbtoken == null)
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
            int currheros = this._context.UsersHeros.Where(e => e.UserName == dbtoken.UserName).Count();
            if (currheros >= ServerOptions.MaxHerosPerAccount)
            {
                return BadRequest(new DataError("herolimitErr", "You have reached maximum amount of heros per account."));
            }
            int ID = this._context.Heros.Select(x => x.HeroId).DefaultIfEmpty(0).Max();
            Heros newly = new Heros()
            {
                Charisma = data.Data.Attributes[6],
                Country = data.Data.Country,
                // starting location of type??
                CurrentLocation = 1,
                Dexterity = data.Data.Attributes[2],
                Endurance = data.Data.Attributes[1],
                Experience = 0,
                HeroId = ID + 1,
                Hp = HeroCalculator.PureMaxHP(HeroCalculator.BaseHP(1),data.Data.Attributes),
                Intelligence = data.Data.Attributes[5],
                Lvl = 1,
                Name = data.Data.Name,
                Nickname = data.Data.Nickname,
                Orders = 0,
                Origin = data.Data.Origin,
                Reflex = data.Data.Attributes[3],
                Sl = 0,
                Slbase = 0,
                Status = 0,
                Strength = data.Data.Attributes[0],
                Willpower = data.Data.Attributes[7],
                Wisdom = data.Data.Attributes[4],
            };
            UsersHeros userheros = new UsersHeros()
            {
                HeroId = newly.HeroId,
                UserName = dbtoken.UserName,
            };
            Equipment eq = Equipment.GenFreshEquipment(newly.HeroId);
            HerosLocations location = HerosLocations.GenInitialLocation(_context, newly.HeroId);
            
            _context.Heros.Add(newly);
            _context.UsersHeros.Add(userheros);
            _context.Equipment.Add(eq);
            _context.HerosLocations.Add(location);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest(new DataError("tokenErr", "Hero already exists."));
            }
            return Ok((HeroBrief)newly);
        }

        private bool HerosExists(int id)
        {
            return _context.Heros.Any(e => e.HeroId == id);
        }

        public class HeroPassed
        {
            public string Name { get; set; }
            public string Nickname { get; set; }
            public int[] Attributes { get; set; }
            public int Country { get; set; }
            public int Origin { get; set; }
            public bool IsMan { get; set; }
        }
    }    
}