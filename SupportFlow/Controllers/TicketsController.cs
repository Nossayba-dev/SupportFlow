using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupportFlow.Data;
using SupportFlow.Models;
using System.ComponentModel;

namespace SupportFlow.Controllers
{
    [ApiController] //tell that is a api controller
    [Route("api/Tickets")]//directly map to the route api/Tickets
    public class TicketsController : ControllerBase
    {
        private readonly SupportFlowDbContext _context;
        public TicketsController(SupportFlowDbContext context)
        {
            _context = context;
        }
        [HttpGet] 
        public async Task<IActionResult> GetTickets()
        {
            return Ok(await _context.Tickets.Include(t => t.User).Include(t => t.Category).ToListAsync());
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicketById(int id)
        {
            var t = await _context.Tickets.Include(t => t.User).Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == id);
            if (t == null) return NotFound();
            return Ok(t);
        }
        [HttpPost]
        public async Task<IActionResult> AddTicket([FromBody]Ticket ticket)
        {
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();


            return CreatedAtAction(nameof(GetTicketById),new { id = ticket.Id }, ticket);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicket(int id, [FromBody] Ticket ticket)
        {
            var existingTicket = await _context.Tickets.FindAsync(id);
            if (existingTicket == null)
            {
                return NotFound($"Ticket item with ID {id} not found.");
            }

            existingTicket.Title = ticket.Title;
            existingTicket.User = ticket.User;
            existingTicket.Category = ticket.Category;
            existingTicket.Status = ticket.Status;
            existingTicket.Priority = ticket.Priority;
            existingTicket.Comments = ticket.Comments;

            await _context.SaveChangesAsync();


            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id )
        {
            var ticket = await _context.Tickets.FindAsync(id);

            
            if (ticket == null)
            {
                return NotFound($"Ticket with ID {id} not found.");
            }

            
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            
            return NoContent();

        }


    }
}
