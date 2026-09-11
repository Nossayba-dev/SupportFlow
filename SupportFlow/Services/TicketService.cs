using Microsoft.EntityFrameworkCore;
using SupportFlow.Data;
using SupportFlow.Models;

namespace SupportFlow.Services
{
    public class TicketService : ITicketService 
    {
        private readonly SupportFlowDbContext _context;
        public TicketService(SupportFlowDbContext context) 
        {
            _context = context;
        }
        public async Task<List<Ticket>> GetTickets() 
        {
            return await _context.Tickets.Include(t => t.User).Include(t => t.Category).ToListAsync();
        }
       public async Task<Ticket?> GetTicketById(int id)
        {
            var t = await _context.Tickets.Include(t => t.User).Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == id);
            if (t == null) return null;
            return t;
        }

        public async Task<Ticket> AddTicket(Ticket ticket)
        {
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();

            return ticket;
        }


        public async Task<Ticket?> UpdateTicket(int id, Ticket ticket)
        {
            var existingTicket = await _context.Tickets.FindAsync(id);
            if (existingTicket == null)
            {
                return null;
            }

            existingTicket.Title = ticket.Title;
            //existingTicket.User = ticket.User;
            //existingTicket.Category = ticket.Category;
            existingTicket.Status = ticket.Status;
            existingTicket.Priority = ticket.Priority;
            existingTicket.Comments = ticket.Comments;

            await _context.SaveChangesAsync();

            return existingTicket;
        }
        public async Task<bool> DeleteTicket(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);


            if (ticket == null)
            {
                return false;
            }


            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return true;

        }

    }
}
