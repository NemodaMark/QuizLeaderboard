using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuizLeaderboard.Data;
using QuizLeaderboard.Models;

namespace QuizLeaderboard.Services
{
    public class AuthService
    {
        private readonly AppDbContext _db;
        private readonly UserSession _session;

        public AuthService(AppDbContext db, UserSession session)
        {
            _db = db;
            _session = session;
        }

        // ==============================
        // REGISTER
        // ==============================
        public async Task<AuthResponse> RegisterAsync(string email, string password, string displayName)
        {
            try
            {
                // Ellenőrizzük, hogy létezik-e már ilyen email
                var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (existingUser != null)
                {
                    return new AuthResponse 
                    { 
                        Success = false, 
                        ErrorMessage = "This email is already registered." 
                    };
                }

                // Új user létrehozása
                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    DisplayName = displayName,
                    PasswordHash = HashPassword(password),
                };

                _db.Users.Add(newUser);
                await _db.SaveChangesAsync();

                // NE állítsunk be cookie-t itt, mert interaktív módban hibát okoz
                // A user majd login-nál kap cookie-t

                return new AuthResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new AuthResponse 
                { 
                    Success = false, 
                    ErrorMessage = $"Registration error: {ex.Message}" 
                };
            }
        }

        // ==============================
        // LOGIN
        // ==============================
        public async Task<AuthResponse> LoginAsync(string email, string password)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
                
                if (user == null)
                {
                    return new AuthResponse 
                    { 
                        Success = false, 
                        ErrorMessage = "Invalid email or password." 
                    };
                }

                // Jelszó ellenőrzése
                if (!VerifyPassword(password, user.PasswordHash))
                {
                    return new AuthResponse 
                    { 
                        Success = false, 
                        ErrorMessage = "Invalid email or password." 
                    };
                }

                // Cookie beállítása - csak ha van HttpContext
                try
                {
                    await _session.SetAuthCookie(user.Id.ToString());
                }
                catch (InvalidOperationException)
                {
                    // Headers már read-only - ez interaktív módban történik
                    // Ilyenkor a Success flag-gel jelezzük, hogy a login sikeres volt
                    // De a cookie-t majd egy újratöltésnél állítjuk be
                }

                return new AuthResponse { Success = true, UserId = user.Id.ToString() };
            }
            catch (Exception ex)
            {
                return new AuthResponse 
                { 
                    Success = false, 
                    ErrorMessage = $"Login error: {ex.Message}" 
                };
            }
        }

        // ==============================
        // LOGOUT
        // ==============================
        public Task LogoutAsync()
        {
            _session.ClearAuthCookie();
            return Task.CompletedTask;
        }

        // ==============================
        // CURRENT USER
        // ==============================
        public async Task<User?> GetCurrentUserAsync()
        {
            await _session.EnsureLoadedAsync();
            return _session.CurrentUser;
        }

        // ==============================
        // PASSWORD HASHING (EGYSZERŰ VERZIÓ)
        // ==============================
        // FIGYELEM: Ez egy egyszerű példa! Production-ben használj BCrypt.Net vagy ASP.NET Core Identity-t!
        private string HashPassword(string password)
        {
            // Ideális: BCrypt.Net.BCrypt.HashPassword(password)
            // Egyszerű demo verzió (NE HASZNÁLD ÉLESBEN!):
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password + "SALT_KEY_12345");
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }

    // Response osztály
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? UserId { get; set; }
    }
}