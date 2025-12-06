using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuizLeaderboard.Data;
using QuizLeaderboard.Models;

namespace QuizLeaderboard.Services
{
    public class AuthService
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(AppDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        // ---- Egyszerű (nem-BCrypt) jelszó hash ----

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

        // ---- Publikus API ----

        public async Task RegisterAsync(string email, string password, string displayName)
        {
            var existing = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existing != null)
            {
                throw new InvalidOperationException("This email address is already in use.");
            }

            var user = new User
            {
                Email = email,
                DisplayName = displayName,
                PasswordHash = HashPassword(password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            await SignInAsync(user);
        }

        public async Task LoginAsync(string email, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !VerifyPassword(password, user.PasswordHash ?? string.Empty))
            {
                throw new InvalidOperationException("Invalid email or password.");
            }

            await SignInAsync(user);
        }

        private Task SignInAsync(User user)
        {
            var http = _httpContextAccessor.HttpContext
                       ?? throw new InvalidOperationException("No HTTP context available.");

            // Egyszerű auth cookie – nem Identity, csak user Id
            http.Response.Cookies.Append(
                "QuizAuth",
                user.Id.ToString(),
                new CookieOptions
                {
                    HttpOnly = true,
                    IsEssential = true,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

            return Task.CompletedTask;
        }

        public Task LogoutAsync()
        {
            var http = _httpContextAccessor.HttpContext;
            http?.Response.Cookies.Delete("QuizAuth");
            return Task.CompletedTask;
        }
    }
}
