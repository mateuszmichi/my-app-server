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
    [Route("api/UsersChangeEmail")]
    public class UsersChangeEmailController : Controller
    {
        private readonly my_appContext _context;

        public UsersChangeEmailController(my_appContext context)
        {
            _context = context;
        }

        // POST: api/UsersChangeEmail
        [HttpPost]
        public async Task<IActionResult> PostUsers([FromBody] PassedData<PassedNewEmail> data)
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
            if (data.Data.NewEmail != data.Data.ConfirmEmail)
            {
                return BadRequest(new DataError("newEmailErr", "New email was not confirmed correctly."));
            }
            //all went well
            user.Email = data.Data.NewEmail;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if(_context.Users.FirstOrDefault(e=> ((e.Email == data.Data.NewEmail) && (e.Name != dbtoken.UserName))) != null)
                {
                    return BadRequest(new DataError("newEmailErr", "New email has been already used."));
                }
                return BadRequest(new DataError("serverErr", "Failed to save new password."));
            }
            return Ok(new { success = true });
        }

        public class PassedNewEmail
        {
            public string Password { get; set; }
            public string NewEmail { get; set; }
            public string ConfirmEmail { get; set; }
        }
    }
}