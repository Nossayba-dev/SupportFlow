using SupportFlow.DTOs;
using SupportFlow.Enums;

namespace SupportFlow.Services
{
    public interface ITicketService
    {
        public Task<List<TicketResponseDto>> GetTickets( int currentUserId, UserRole currentUserRole, int page = 1, int pageSize = 10, TicketStatus? status = null, bool sortByPriority = false);
        public Task<TicketResponseDto?> GetTicketById(int id, int currentUserId, UserRole currentUserRole);
        public Task<TicketResponseDto> AddTicket(CreateTicketDto ticket, int currentUserId);
        public Task<TicketResponseDto?> UpdateTicket(int id, UpdateTicketDto ticket, int currentUserId, UserRole currentUserRole);
        public Task<bool> DeleteTicket(int id, int currentUserId, UserRole currentUserRole );
    }
}
