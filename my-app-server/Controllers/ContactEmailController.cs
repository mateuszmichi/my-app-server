using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using my_app_server.Models;

namespace my_app_server.Controllers
{
    [Produces("application/json")]
    [Route("api/ContactEmail")]
    public class ContactEmailController : Controller
    {
        public async Task<IActionResult> PostUsers([FromBody] EmailForm emailForm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if(emailForm.Content.Length > 300)
            {
                return BadRequest(new DataError("eMailErr", "Too big message to send."));
            }
            try
            {
                SendEmail.SendContactEmail(emailForm.From,emailForm.Subject,emailForm.Content);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return BadRequest(new DataError("eMailErr", "SmtpClient is probably down. Please excuse us :("));
            }
            return Ok();
        }
        public class EmailForm
        {
            public string From { get; set; }
            public string Subject { get; set; }
            public string Content { get; set; }
        }
    }
}