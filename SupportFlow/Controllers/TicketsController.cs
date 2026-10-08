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

        private bool TryGetCurrentUser(out int userId, out UserRole role)
        {
            userId = 0;
            role = default;

            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            return int.TryParse(idClaim, out userId)
                && Enum.TryParse(roleClaim, out role);
        }
        [HttpGet] 
        public async Task<IActionResult> GetTickets(int page = 1, int pageSize = 10, TicketStatus? status = null, bool sortByPriority = false)
        {
            if (!TryGetCurrentUser(out var userId, out var role))
                return Unauthorized();


            return Ok(await _ticketService.GetTickets( userId, role, page, pageSize, status, sortByPriority));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketById(int id)
        {
            if (!TryGetCurrentUser(out var userId, out var role))
                return Unauthorized();
            try
            {
                var ticket = await _ticketService.GetTicketById(id, userId, role);
                if (ticket == null) return NotFound();
                return Ok(ticket);
            }
            catch (UnauthorizedAccessException )
            {
                return Forbid();
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddTicket([FromBody] CreateTicketDto ticket)
        {
            if (!TryGetCurrentUser(out var userId, out var role))
                return Unauthorized();
            try
            {
                var createdTicket = await _ticketService.AddTicket(ticket, userId);
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
            if (!TryGetCurrentUser(out var userId, out var role))
                return Unauthorized();
            try
            {
                var updatedTicket = await _ticketService.UpdateTicket(id, ticket, userId, role);

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
            catch (UnauthorizedAccessException )
            {
                return Forbid();
            }

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            if (!TryGetCurrentUser(out var userId, out var role))
                return Unauthorized();
            try
            {
                var deleted = await _ticketService.DeleteTicket(id, userId, role);
                if (!deleted)
                {
                return NotFound($"Ticket with ID {id} not found.");
                }
                return NoContent();

            }
            catch (UnauthorizedAccessException )
            {
                return Forbid();
            }
        }


    }
}
