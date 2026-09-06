using Microsoft.AspNetCore.Mvc;

namespace SupportFlow.Controllers
{
    [ApiController]
    [Route("api/Tickets")]
    public class TicketsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetTickets()
        {
            return Ok("some data");
        }
    }
}
