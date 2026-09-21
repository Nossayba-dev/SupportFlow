using Microsoft.EntityFrameworkCore;
using SupportFlow.Data;
using SupportFlow.DTOs;
using SupportFlow.Models;

namespace SupportFlow.Services
{
    public class UserService : IUserService
    {
        private readonly SupportFlowDbContext _context;

        public UserService(SupportFlowDbContext context)
        {
            _context = context;
        }
        public async Task<List<UserResponseDto>> GetUsers()
        {
            var users = await _context.Users.Include(u => u.Tickets).ToListAsync();
            var result = new List<UserResponseDto>();
            foreach (var u in users)
            {
                var ticketDtos = new List<TicketSumaryDto>();
                foreach (var t in u.Tickets)
                {
                    ticketDtos.Add(new TicketSumaryDto { Id = t.Id, Title = t.Title });
                };
                var dto = new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Tickets = ticketDtos
                };
                result.Add(dto);
            }
            return result;
        }
        public async Task<UserResponseDto?> GetUserById(int id)
        {
            var u = await _context.Users.Include(u => u.Tickets).FirstOrDefaultAsync(u => u.Id == id);
            if (u == null) return null;
            var ticketDtos = new List<TicketSumaryDto>();
            foreach (var t in u.Tickets)
            {
                ticketDtos.Add(new TicketSumaryDto { Id = t.Id, Title = t.Title });
            };
            var dto = new UserResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Tickets = ticketDtos
            };
            return dto;
        }
        public async Task<UserResponseDto> AddUser(UserDto user)
        {
            var newUser = new User
            {
                Name = user.Name,
                Email = user.Email,
                Password = user.Password
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            var dto = new UserResponseDto
            {
                Id = newUser.Id,
                Name = newUser.Name,
                Email = newUser.Email,
                Tickets = new List<TicketSumaryDto>()
            };
            return dto;
        }
        public async Task<UserResponseDto?> UpdateUser(int id, UserDto user)
        {
            var existingUser = await _context.Users.Include(u => u.Tickets).FirstOrDefaultAsync(u => u.Id == id);
            if (existingUser == null) return null;
            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.Password = user.Password;
            await _context.SaveChangesAsync();

            var ticketDtos = new List<TicketSumaryDto>();
            foreach (var t in existingUser.Tickets)
            {
                ticketDtos.Add(new TicketSumaryDto { Id = t.Id, Title = t.Title });
            }
            ;
            var dto = new UserResponseDto
            {
                Id = existingUser.Id,
                Name = existingUser.Name,
                Email = existingUser.Email,
                Tickets = ticketDtos
            };
            return dto;
        }
        public async Task<bool> DeleteUser(int id)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (existingUser == null) return false;
            _context.Users.Remove(existingUser);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
