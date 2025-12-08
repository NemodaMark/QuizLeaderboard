using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizLeaderboard.Data;
using QuizLeaderboard.Models;

namespace QuizLeaderboard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AuthController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DisplayName))
            {
                return BadRequest(new { message = "Display name is required." });
            }

            // ugyanazzal a névvel ne lehessen több user
            var existing = await _db.Users
                .FirstOrDefaultAsync(u => u.DisplayName == request.DisplayName);

            if (existing != null)
            {
                return BadRequest(new { message = "This display name is already taken." });
            }

            var user = new User
            {
                DisplayName = request.DisplayName
                // Email, PasswordHash üresen maradhat, nem használjuk
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new { userId = user.Id.ToString() });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.DisplayName))
            {
                return BadRequest(new { message = "Display name is required." });
            }

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.DisplayName == request.DisplayName);

            if (user == null)
            {
                return Unauthorized(new { message = "Unknown user." });
            }

            return Ok(new { userId = user.Id.ToString() });
        }

        public class RegisterRequest
        {
            public string DisplayName { get; set; } = string.Empty;
        }

        public class LoginRequest
        {
            public string DisplayName { get; set; } = string.Empty;
        }
    }
}
