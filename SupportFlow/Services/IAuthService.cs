using SupportFlow.DTOs;


namespace SupportFlow.Services
{
    public interface IAuthService
    {
        public Task<string?> Login(LoginDto login);
    }
}
