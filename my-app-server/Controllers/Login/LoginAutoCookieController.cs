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
    [Route("api/LoginAutoCookie")]
    public class LoginAutoCookieController : Controller
    {
        private readonly my_appContext _context;
        public LoginAutoCookieController(my_appContext context)
        {
            _context = context;
        }

        // POST: api/LoginAutoCookie
        [HttpPost]
        public async Task<IActionResult> PostTokens([FromBody] PassedData<string> passedData)
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
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest(new DataError("databaseErr", "Failed to update tokens."));
            }
            UserTokenResult userResult = new UserTokenResult()
            {
                Token = dbtoken.HashedToken,
                UserName = dbtoken.UserName,
            };
            var h = _context.Heros.Join(_context.UsersHeros.Where(e => e.UserName == dbtoken.UserName), e => e.HeroId, e => e.HeroId, (a, b) => new HeroBrief()
            {
                Name = a.Name,
                Nickname = a.Nickname,
                Level = a.Lvl,
                Orders = a.Orders,
            });

            return Ok(new { success = true, usertoken = userResult, user = new UserBrief() { Username = dbtoken.UserName, Characters = h.ToArray() } });
        }
        
    }
}
