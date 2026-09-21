using SupportFlow.DTOs;

namespace SupportFlow.Services
{
    public interface IUserService
    {
        public Task<List<UserResponseDto>> GetUsers();
        public Task<UserResponseDto?> GetUserById(int id);
        public Task<UserResponseDto> AddUser(UserDto user);
        public Task<UserResponseDto?> UpdateUser(int id, UserDto user);
        public Task<bool> DeleteUser(int id);
    }
}
