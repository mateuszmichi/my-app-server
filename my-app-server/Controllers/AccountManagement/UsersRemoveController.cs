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
    [Route("api/UsersRemove")]
    public class UsersRemoveController : Controller
    {
        private readonly my_appContext _context;

        public UsersRemoveController(my_appContext context)
        {
            _context = context;
        }

        // POST: api/UsersRemove
        [HttpPost]
        public async Task<IActionResult> PostUsers([FromBody] PassedData<PassedRemoveAccount> data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            DateTime now = DateTime.UtcNow;
            if (data.UserToken == null)
            {
                return BadRequest(new DataError("securityErr", "No authorization controll."));
            }
            UserToken dbtoken = Security.CheckUserToken(this._context, data.UserToken);
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
            if (user.Password != HashClass.GenHash(data.Data.Password))
            {
                return BadRequest(new DataError("passwordErr", "Password is incorrect."));
            }
            //all went well

            Users deluser = _context.Users.FirstOrDefault(e => e.Name == dbtoken.UserName);
            var usersheros = _context.UsersHeros.Where(e => e.UserName == deluser.Name);
            var delheros = _context.Heros.Join(usersheros, e => e.HeroId, e => e.HeroId, (a, b) => a);
            var delusertoken = _context.UserToken.Where(e => e.UserName == deluser.Name);
            var deltoken = _context.Tokens.Where(e => e.UserName == deluser.Name);

            if (usersheros.Count() > 0)
            {
                _context.UsersHeros.RemoveRange(usersheros);
            }
            if(delheros.Count() > 0)
            {
                _context.Heros.RemoveRange(delheros);
            }
            if (delusertoken.Count() > 0)
            {
                _context.UserToken.RemoveRange(delusertoken);
            }
            if (deltoken.Count() > 0)
            {
                _context.Tokens.RemoveRange(deltoken);
            }
            _context.Users.Remove(deluser);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest(new DataError("serverErr", "Failed delete account."));
            }
            return Ok(new { success = true });
        }

        public class PassedRemoveAccount
        {
            public string Password { get; set; }
            public string Reason { get; set; }
        }
    }
}