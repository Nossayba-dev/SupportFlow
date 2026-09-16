using Microsoft.EntityFrameworkCore;
using SupportFlow.Data;
using SupportFlow.DTOs;
using SupportFlow.Enums;
using SupportFlow.Models;
using System.Net.Sockets;

namespace SupportFlow.Services
{
    public class TicketService : ITicketService 
    {
        private readonly SupportFlowDbContext _context;
        public TicketService(SupportFlowDbContext context) 
        {
            _context = context;
        }
        public async Task<List<TicketResponseDto>> GetTickets() 
        {
            var tickets = await _context.Tickets.Include(t => t.User).Include(t => t.Category).ToListAsync();

            var result = new List<TicketResponseDto>();
            foreach (var t in tickets){
                var userDto = new UserSummaryDto
                {
                    Id = t.User.Id,
                    Name = t.User.Name,
                    Email = t.User.Email
                };

                var categoryDto = new CategorySummaryDto
                {
                    Id = t.Category.Id,
                    Name = t.Category.Name
                };

                var dto = new TicketResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    User = userDto,
                    Category = categoryDto,
                    Status = t.Status,
                    Priority = t.Priority,
                    Comments = t.Comments
                };

                result.Add(dto);
            }

            return result;
        }
        public async Task<TicketResponseDto?> GetTicketById(int id)
        {
            var t = await _context.Tickets.Include(t => t.User).Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == id);
            if (t == null) return null;

            var userDto = new UserSummaryDto
            {
                Id = t.User.Id,
                Name = t.User.Name,
                Email = t.User.Email
            };

            var categoryDto = new CategorySummaryDto
            {
                Id = t.Category.Id,
                Name = t.Category.Name
            };

            return new TicketResponseDto
            {
                Id = t.Id,
                Title = t.Title,
                User = userDto,
                Category = categoryDto,
                Status = t.Status,
                Priority = t.Priority,
                Comments = t.Comments
            };
        }

        public async Task<TicketResponseDto> AddTicket(CreateTicketDto dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (user == null || category == null)
            {
                throw new ArgumentException("Invalid UserId or CategoryId");
            }
            var ticket = new Ticket
            {
                Title = dto.Title,
                User = user,
                Category = category,
                Status = TicketStatus.Open,
                Priority = dto.Priority,
                Comments = dto.Comments
            };
            
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();

            var userDto = new UserSummaryDto { Id = ticket.User.Id, Name = ticket.User.Name, Email = ticket.User.Email };
            var categoryDto = new CategorySummaryDto { Id = ticket.Category.Id, Name = ticket.Category.Name };

            return new TicketResponseDto
            {
                Id = ticket.Id,
                Title = ticket.Title,
                User = userDto,
                Category = categoryDto,
                Status = ticket.Status,
                Priority = ticket.Priority,
                Comments = ticket.Comments
            };
        }


        public async Task<TicketResponseDto?> UpdateTicket(int id, UpdateTicketDto dto)
        {
            var existingTicket = await _context.Tickets.FindAsync(id);

            if (existingTicket == null)
            {
                return null;
            }

            var user = await _context.Users.FindAsync(dto.UserId);
            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (user == null || category == null)
            {
                throw new ArgumentException("Invalid UserId or CategoryId");
            }

            existingTicket.Title = dto.Title;
            existingTicket.User = user;
            existingTicket.Category = category;
            existingTicket.Status = dto.Status;
            existingTicket.Priority = dto.Priority;
            existingTicket.Comments = dto.Comments;

            await _context.SaveChangesAsync();


            var userDto = new UserSummaryDto { Id = existingTicket.User.Id, Name = existingTicket.User.Name, Email = existingTicket.User.Email };
            var categoryDto = new CategorySummaryDto { Id = existingTicket.Category.Id, Name = existingTicket.Category.Name };

            return new TicketResponseDto
            {
                Id = existingTicket.Id,
                Title = existingTicket.Title,
                User = userDto,
                Category = categoryDto,
                Status = existingTicket.Status,
                Priority = existingTicket.Priority,
                Comments = existingTicket.Comments
            };
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
