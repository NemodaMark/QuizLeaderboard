using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizLeaderboard.Data;
using QuizLeaderboard.Models;
using System.Security.Cryptography;
using System.Text;

namespace QuizLeaderboard.Controllers // IMPORTANT: Use Controllers namespace
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

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(hash))
                return false;
            var computed = HashPassword(password);
            return string.Equals(computed, hash, StringComparison.Ordinal);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || 
                    string.IsNullOrEmpty(request.Password) || 
                    string.IsNullOrEmpty(request.DisplayName))
                {
                    return BadRequest(new { message = "All fields are required." });
                }

                var existing = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (existing != null)
                {
                    return BadRequest(new { message = "This email address is already in use." });
                }

                var user = new User
                {
                    Email = request.Email,
                    DisplayName = request.DisplayName,
                    PasswordHash = HashPassword(request.Password)
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                return Ok(new { userId = user.Id.ToString() });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Register error: {ex}");
                return StatusCode(500, new { message = "An error occurred during registration." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { message = "Email and password are required." });
                }

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null || !VerifyPassword(request.Password, user.PasswordHash ?? string.Empty))
                {
                    return Unauthorized(new { message = "Invalid email or password." });
                }

                return Ok(new { userId = user.Id.ToString() });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex}");
                return StatusCode(500, new { message = "An error occurred during login." });
            }
        }
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}