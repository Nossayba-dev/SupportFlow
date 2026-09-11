using SupportFlow.Models;

namespace SupportFlow.Services
{
    public interface ITicketService
    {
        public Task<List<Ticket>> GetTickets();
        public Task<Ticket?> GetTicketById(int id);
        public Task<Ticket> AddTicket(Ticket ticket);
        public Task<Ticket?> UpdateTicket(int id, Ticket ticket);
        public Task<bool> DeleteTicket(int id);


    }
}
