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
    [Route("api/Admins")]
    public class AdminsController : Controller
    {
        private readonly my_appContext _context;

        public AdminsController(my_appContext context)
        {
            _context = context;
        }
        // PUT: api/Admins/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAdmins([FromRoute] string id, [FromBody] PassedGameData<Admins> passedData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != passedData.Data.Login)
            {
                return BadRequest();
            }

            _context.Admins.Add(new Admins()
            {
                Login = passedData.Data.Login,
                Password = HashClass.GenHash(passedData.Data.Password)
            });
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AdminsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return BadRequest(new DataError("adminError", "Duplicate of names."));
                }
            }
            return Ok();
        }
        // POST: api/Admins
        [HttpPost]
        public async Task<IActionResult> PostAdmins([FromBody] Admins admin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (AdminsExists(admin.Login) && PasswordMatches(admin.Login, admin.Password))
            {
                AdminsTokens usertoken = Security.GenerateAdminsToken(admin.Login, this._context);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    return BadRequest(new DataError("tokenErr", "Failed to remember login."));
                }

                UserTokenResult userResult = new UserTokenResult()
                {
                    Token = usertoken.Token,
                    UserName = usertoken.Login,
                };
                return Ok(new { success = true, usertoken = userResult });
            }
            else return BadRequest(new DataError("loginErr", "Invalid login or password."));
        }
        // 
        private bool AdminsExists(string id)
        {
            return _context.Admins.Any(e => e.Login == id);
        }
        private bool PasswordMatches(string username, string password)
        {
            return _context.Admins.First(e => e.Login == username).Password == HashClass.GenHash(password);
        }
    }
}