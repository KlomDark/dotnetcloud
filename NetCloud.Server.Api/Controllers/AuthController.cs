using DotNetCloud.Data;
using DotNetCloud.Server.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DotNetCloud.Server.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        public AuthController(DotNetCloudDbContext db, IConfiguration config)
        {
            _authService = new AuthService(db, config);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var user = await _authService.Register(req.UserName, req.Email, req.Password);
            if (user == null) return BadRequest("User already exists.");
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var token = await _authService.Login(req.UserName, req.Password);
            if (token == null) return Unauthorized();
            return Ok(new { token });
        }
    }

    public class RegisterRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class LoginRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
