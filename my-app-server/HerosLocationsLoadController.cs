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
    [Route("api/HerosLocationsLoad")]
    public class HerosLocationsLoadController : Controller
    {
        private readonly my_appContext _context;

        public HerosLocationsLoadController(my_appContext context)
        {
            _context = context;
        }
        //TODO!!!!
        // POST: api/HerosLocationsLoad
        [HttpPost]
        public async Task<IActionResult> PostHerosLocations([FromBody] HerosLocations herosLocations)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.HerosLocations.Add(herosLocations);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {

            }

            return CreatedAtAction("GetHerosLocations", new { id = herosLocations.LocationId }, herosLocations);
        }
    }
}