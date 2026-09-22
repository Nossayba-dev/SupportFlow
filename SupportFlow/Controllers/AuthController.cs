using Microsoft.AspNetCore.Mvc;
using SupportFlow.DTOs;
using SupportFlow.Services;

namespace SupportFlow.Controllers
{
    [ApiController]
    [Route("api/Auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController( IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]LoginDto login)
        {
            var l = await _authService.Login(login);
            if (l == null) { 
                return Unauthorized("Invalid email or password.");
            }
            return Ok(l);

        }
    }
}
