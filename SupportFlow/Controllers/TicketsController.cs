using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportFlow.DTOs;
using SupportFlow.Enums;
using SupportFlow.Services;
using System.Security.Claims;


namespace SupportFlow.Controllers
{
    [ApiController] //tell that is a api controller
    [Route("api/Tickets")]//directly map to the route api/Tickets
    [Authorize] //require authentication for all actions in this controller
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
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var currentUserRole = Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role));
            return Ok(await _ticketService.GetTickets( currentUserId, currentUserRole));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketById(int id)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var currentUserRole = Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role));
            try
            {
                var ticket = await _ticketService.GetTicketById(id, currentUserId, currentUserRole);
                if (ticket == null) return NotFound();
                return Ok(ticket);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddTicket([FromBody] CreateTicketDto ticket)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            try
            {
                var createdTicket = await _ticketService.AddTicket(ticket, currentUserId);
                return CreatedAtAction(nameof(GetTicketById), new { id = createdTicket.Id }, createdTicket);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicket(int id, [FromBody] UpdateTicketDto ticket)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var currentUserRole = Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role));
            try
            {
                var updatedTicket = await _ticketService.UpdateTicket(id, ticket, currentUserId, currentUserRole);

                if (updatedTicket == null)
                {
                    return NotFound($"Ticket with ID {id} not found.");
                }
                return Ok(updatedTicket);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var currentUserRole = Enum.Parse<UserRole>(User.FindFirstValue(ClaimTypes.Role));
            try
            {
                var deleted = await _ticketService.DeleteTicket(id, currentUserId, currentUserRole);
                if (!deleted)
                {
                return NotFound($"Ticket with ID {id} not found.");
                }
                return NoContent();

            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
        }


    }
}
