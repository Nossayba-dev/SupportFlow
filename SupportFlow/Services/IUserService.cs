using SupportFlow.DTOs;

namespace SupportFlow.Services
{
    public interface IUserService
    {
        public Task<List<UserResponseDto>> GetUsers();
        public Task<UserResponseDto?> GetUserById(int id);
        public Task<UserResponseDto> AddUser(CreateUserDto user);
        public Task<UserResponseDto?> UpdateUser(int id, UpdateUserDto user);
        public Task<bool> DeleteUser(int id);
    }
}
