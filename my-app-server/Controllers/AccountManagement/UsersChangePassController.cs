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
    [Route("api/UsersChangePass")]
    public class UsersChangePassController : Controller
    {
        private readonly my_appContext _context;

        public UsersChangePassController(my_appContext context)
        {
            _context = context;
        }

        // POST: api/UsersChangePass
        [HttpPost]
        public async Task<IActionResult> PostUsers([FromBody] PassedData<PassedNewPassword> data)
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
            Users user = _context.Users.FirstOrDefault(e=> e.Name == dbtoken.UserName);
            if(user.Password != HashClass.GenHash(data.Data.OldPassword))
            {
                return BadRequest(new DataError("passwordErr", "Old password is incorrect."));
            }
            if(data.Data.NewPassword != data.Data.ConfirmPassword)
            {
                return BadRequest(new DataError("newpasswordErr", "New password was not confirmed correctly."));
            }
            //all went well
            user.Password = HashClass.GenHash(data.Data.NewPassword);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest(new DataError("serverErr", "Failed to save new password."));
            }
            return Ok(new { success = true });
        }

        public class PassedNewPassword
        {
            public string OldPassword { get; set; }
            public string NewPassword { get; set; }
            public string ConfirmPassword { get; set; }
        }
    }
}