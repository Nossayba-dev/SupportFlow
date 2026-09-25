using Microsoft.EntityFrameworkCore;
using SupportFlow.Data;
using SupportFlow.DTOs;
using SupportFlow.Enums;
using SupportFlow.Models;
using System.Data;


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
                    Role = u.Role,
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
                Role = u.Role,
                Email = u.Email,
                Tickets = ticketDtos
            };
            return dto;
        }
        public async Task<UserResponseDto> AddUser(UserDto user)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
            {
                throw new ArgumentException("User with this email already exists.");
            }
            var newUser = new User
            {
                Name = user.Name,
                Role = UserRole.Customer,
                Email = user.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(user.Password)
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            var dto = new UserResponseDto
            {
                Id = newUser.Id,
                Name = newUser.Name,
                Role = newUser.Role,
                Email = newUser.Email,
                Tickets = new List<TicketSumaryDto>()
            };
            return dto;
        }
        public async Task<UserResponseDto?> UpdateUser(int id, UserDto user)
        {
            var existingUser = await _context.Users.Include(u => u.Tickets ).FirstOrDefaultAsync(u => u.Id == id);

            if (existingUser == null) return null;
            var duplicateUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email && u.Id != id); if (duplicateUser != null)
            {
                throw new ArgumentException("User with this email already exists.");
            }
            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
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
                Role = existingUser.Role,
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
