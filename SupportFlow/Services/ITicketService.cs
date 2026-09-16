using SupportFlow.DTOs;

namespace SupportFlow.Services
{
    public interface ITicketService
    {
        public Task<List<TicketResponseDto>> GetTickets();
        public Task<TicketResponseDto?> GetTicketById(int id);
        public Task<TicketResponseDto> AddTicket(CreateTicketDto ticket);
        public Task<TicketResponseDto?> UpdateTicket(int id, UpdateTicketDto ticket);
        public Task<bool> DeleteTicket(int id);


    }
}
