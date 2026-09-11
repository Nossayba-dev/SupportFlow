using Microsoft.AspNetCore.Mvc;
using SupportFlow.Models;
using SupportFlow.Services;


namespace SupportFlow.Controllers
{
    [ApiController] //tell that is a api controller
    [Route("api/Tickets")]//directly map to the route api/Tickets
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }
        [HttpGet] 
        public async Task<IActionResult> GetTickets()
        {
            return Ok(await _ticketService.GetTickets());
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketById(int id)
        {
            var ticket = await _ticketService.GetTicketById(id);
            if (ticket == null) return NotFound();
            return Ok(ticket);
        }
        [HttpPost]
        public async Task<IActionResult> AddTicket([FromBody]Ticket ticket)
        {
            var createdTicket = await _ticketService.AddTicket(ticket);
            return CreatedAtAction(nameof(GetTicketById), new { id = createdTicket.Id }, createdTicket);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicket(int id, [FromBody] Ticket ticket)
        {
            var updatedTicket = await _ticketService.UpdateTicket(id, ticket);

            if (updatedTicket == null)
            {
                return NotFound($"Ticket with ID {id} not found.");
            }
            return Ok(updatedTicket);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id )
        {
            var deleted = await _ticketService.DeleteTicket(id);

            if (!deleted)
            {
                return NotFound($"Ticket with ID {id} not found.");
            }
            

            return NoContent();
        }


    }
}
