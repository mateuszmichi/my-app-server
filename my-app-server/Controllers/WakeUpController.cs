using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace my_app_server.Controllers
{
    [Produces("application/json")]
    [Route("api/WakeUp")]
    public class WakeUpController : Controller
    {
        // POST: api/WakeUp
        [HttpPost]
        public IActionResult Post()
        {
            return Ok();
        }
    }
}
