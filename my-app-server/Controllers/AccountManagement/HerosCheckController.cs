using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using my_app_server.Models;

namespace my_app_server.Controllers.AccountManagement
{
    [Produces("application/json")]
    [Route("api/HerosCheck")]
    public class HerosCheckController : Controller
    {
        private readonly my_appContext _context;

        public HerosCheckController(my_appContext context)
        {
            _context = context;
        }

        // POST: api/HerosCheck
        [HttpPost]
        public IActionResult PostHeros([FromBody] PassedCheck herosname)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            bool isUnique = !_context.Heros.Any(e => e.Name == herosname.Name);
            if (isUnique)
            {
                return Ok(new { isUnique });
            }
            else
            {
                return BadRequest(new DataError("nameErr", "There is already a character with this name."));
            }
            
        }
        public class PassedCheck
        {
            public string Name { get; set; }
        }
    }
}