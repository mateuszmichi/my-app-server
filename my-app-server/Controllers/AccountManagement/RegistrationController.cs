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
    [Route("api/Registration")]
    
    public class RegistrationController : Controller
    {
        private readonly my_appContext _context;

        public RegistrationController(my_appContext context)
        {
            _context = context;
        }
        // POST: api/Registration
        [HttpPost]
        public async Task<IActionResult> PostUsers([FromBody] RegistrationUser users)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (UsersExists(users.Username)) return BadRequest(new DataError("sameloginErr", "Login with same name already exists."));
            if (EmailExists(users.Email)) return BadRequest(new DataError("sameemailErr", "Email already used."));
            if (users.Password != users.Password_Confrim) return BadRequest(new DataError("confirmErr", "Bad confirmation password."));


            //TODO: regex validation
            Users user = new Users() { Email = users.Email, Name = users.Username, Password = HashClass.GenHash(users.Password), RegistryDate = DateTime.UtcNow, LastLogin = DateTime.UtcNow };
            _context.Users.Add(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UsersExists(users.Username))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return Ok();
        }
        private bool UsersExists(string id)
        {
            return _context.Users.Any(e => e.Name == id);
        }
        private bool EmailExists(string email)
        {
            return _context.Users.Any(e => e.Email == email);
        }

    }
    public class RegistrationUser
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Password_Confrim { get; set; }

    }
    public class DataError
    {
        public DataError(string _type, string _desc) {
            this.Type = _type;
            this.Description = _desc;
            }
        public string Type { get; set; }
        public string Description { get; set; }
    }
}