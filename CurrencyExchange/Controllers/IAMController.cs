using CurrencyExchange.Infrastructure.Models;
using CurrencyExchange.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchange.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IAMController : ControllerBase
    {

        private readonly AuthService _authService;

        public IAMController(AuthService authService)
        {
            _authService = authService;
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest login)
        {
            if (login == null || string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Role) || string.IsNullOrEmpty(login.Password))
            {
                return BadRequest("Invalid login request");
            }
            if (login.Password != "password")
            {
                return Unauthorized();
            }
            // Normally, you'd verify username/password here
            var token = _authService.GenerateToken(login.Username, login.Role); // Accept "Admin" or "User"
            return Ok(new { Token = token });
        }

    }
}
