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
    [Route("api/HerosRemove")]
    public class HerosRemoveController : Controller
    {
        private readonly my_appContext _context;

        public HerosRemoveController(my_appContext context)
        {
            _context = context;
        }

        // POST: api/HerosRemove
        [HttpPost]
        public async Task<IActionResult> PostHeros([FromBody] PassedData<PassedRemoveCharacter> passedData)
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
            Users user = _context.Users.FirstOrDefault(e => e.Name == dbtoken.UserName);
            if (user.Password != HashClass.GenHash(passedData.Data.Password))
            {
                return BadRequest(new DataError("passwordErr", "Password is incorrect."));
            }
            Heros herotoremove = _context.Heros.FirstOrDefault(e => e.Name == passedData.Data.HeroName);
            UsersHeros conntoremove = _context.UsersHeros.FirstOrDefault(e => e.UserName == dbtoken.UserName && e.HeroId == herotoremove.HeroId);
            _context.UsersHeros.Remove(conntoremove);
            _context.Heros.Remove(herotoremove);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest(new DataError("serverErr", "Failed to remove hero."));
            }
            return Ok(new { success = true, removedHero = herotoremove.Name });
        }

        public class PassedRemoveCharacter
        {
            public string Password { get; set; }
            public string HeroName { get; set; }
        }
    }
}