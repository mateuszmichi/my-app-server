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
    [Route("api/Login")]
    public class LoginController : Controller
    {
        private readonly my_appContext _context;

        public LoginController(my_appContext context)
        {
            _context = context;
        }

        // POST: api/Login
        [HttpPost]
        public async Task<IActionResult> PostUsers([FromBody] LoginUser user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (UsersExists(user.Name) && PasswordMatches(user.Name, user.Password))
            {
                UserToken usertoken = GenerateUsersToken(user);
                Tokens token = null;
                if (user.isRemembered)
                {
                    token = GenerateUsersPernamentToken(user);
                }
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    return BadRequest(new DataError("tokenErr", "Failed to remember login."));
                }

                PernamentTokenResult result = null;
                if (user.isRemembered)
                {
                    result = new PernamentTokenResult()
                    {
                        Token = token.HashedToken,
                        TokenName = token.TokenName,
                        ExpireDate = token.ExpireDate,
                    };
                }
                UserTokenResult userResult = new UserTokenResult()
                {
                    Token = usertoken.HashedToken,
                    UserName = usertoken.UserName,
                };

                var h = _context.Heros.Join(_context.UsersHeros.Where(e => e.UserName == user.Name), e => e.HeroId, e => e.HeroId, (a, b) => new HeroBrief()
                {
                    Name = a.Name,
                    Nickname = a.Nickname,
                    Level = a.Lvl,
                    Orders = a.Orders,
                });

                if (user.isRemembered)
                {
                    return Ok(new { success = true, usertoken = userResult, logintoken = result, user = new UserBrief() { Username= user.Name,Characters=h.ToArray()} });
                }
                else
                {
                    return Ok(new { success = true, usertoken = userResult, user = new UserBrief() { Username = user.Name, Characters = h.ToArray() } });
                }
            }
            else return BadRequest(new DataError("loginErr", "Invalid login or password."));
        }

        private bool UsersExists(string id)
        {
            return _context.Users.Any(e => e.Name == id);
        }
        private bool PasswordMatches(string name, string password)
        {
            return _context.Users.First(e => e.Name == name).Password == HashClass.GenHash(password);
        }
        private Tokens GenerateUsersPernamentToken(LoginUser user)
        {
            DateTime time = DateTime.UtcNow;
            Tokens token;
            if ((token = _context.Tokens.FirstOrDefault(e => e.UserName == user.Name)) == null)
            {
                token = Tokens.GenToken(user.Name, time);
                _context.Tokens.Add(token);
            }
            else
            {
                if (DateTime.Compare(token.ExpireDate, time) < 0)
                {
                    _context.Tokens.Remove(token);
                    token = Tokens.GenToken(user.Name, time);
                    _context.Tokens.Add(token);
                }
                else
                {
                    token.UpdateToken(time);
                }
            }
            return token;
        }
        private UserToken GenerateUsersToken(LoginUser user)
        {
            DateTime time = DateTime.UtcNow;
            UserToken token;
            if ((token = _context.UserToken.FirstOrDefault(e => e.UserName == user.Name)) == null)
            {
                token = UserToken.GenToken(user.Name, time);
                _context.UserToken.Add(token);
            }
            else
            {
                UserToken ntoken = UserToken.GenToken(user.Name, time);
                token.HashedToken = ntoken.HashedToken;
                token.ExpireDate = ntoken.ExpireDate;
            }
            return token;
        }

        public class LoginUser
        {
            public string Name { get; set; }
            public string Password { get; set; }
            public bool isRemembered { get; set; }
        }
    }
    
}