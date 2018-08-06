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
    [Route("api/LoginToken")]
    public class LoginTokenController : Controller
    {
        private readonly my_appContext _context;

        public LoginTokenController(my_appContext context)
        {
            _context = context;
        }

        // POST: api/LoginToken
        [HttpPost]
        public async Task<IActionResult> PostTokens([FromBody] LoginToken token)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DateTime time = DateTime.UtcNow;

            if (!TokensExists(token.TokenName,out Tokens dbtoken))
            {
                return BadRequest(new DataError("securityErr", "Your data has probably been stolen or modified manually. We suggest password's change."));
            }
            if (!IsTokenValid(token, dbtoken, time))
            {
                return BadRequest(new DataError("securityErr", "Your data has probably been stolen or modified manually. We suggest password's change."));
            }
            if (!dbtoken.IsTimeValid(time))
            {
                _context.Tokens.Remove(dbtoken);
                return BadRequest(new DataError("tokenErr", "Relogin is required. Autologin has timedout."));
            }

            UserToken usertoken = Security.GenerateUsersToken(dbtoken.UserName, this._context);
            dbtoken.UpdateToken(time);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest(new DataError("tokenErr", "Failed to remember login."));
            }
            PernamentTokenResult result = new PernamentTokenResult()
            {
                Token = dbtoken.HashedToken,
                TokenName = dbtoken.TokenName,
                ExpireDate = dbtoken.ExpireDate,
            };
            UserTokenResult userResult = new UserTokenResult()
            {
                Token = usertoken.HashedToken,
                UserName = usertoken.UserName,
            };
            var h = _context.Heros.Join(_context.UsersHeros.Where(e => e.UserName == dbtoken.UserName), e => e.HeroId, e => e.HeroId, (a, b) => new HeroBrief()
            {
                Name = a.Name,
                Nickname = a.Nickname,
                Level = a.Lvl,
                Orders = a.Orders,
            });

            return Ok(new { success = true, usertoken = userResult, logintoken = result, user = new UserBrief() { Username = dbtoken.UserName, Characters = h.ToArray() } });
        }
        

        private bool TokensExists(string id,out Tokens token)
        {
            if(_context.Tokens.Any(e => e.TokenName == id))
            {
                token = _context.Tokens.First(e => e.TokenName == id);
                return true;
            }
            else { 
                token = null;
                return false;
            }   
        }
        private bool IsTokenValid(LoginToken passedtoken,Tokens token, DateTime now)
        {
            if (!token.IsTimeValid(now)) return false;
            return (passedtoken.Token == token.HashedToken) && (passedtoken.TokenName == token.TokenName);
        }
        public class LoginToken
        {
            public string Token { get; set; }
            public string TokenName { get; set; }
        }
    }
}