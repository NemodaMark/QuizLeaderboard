using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using QuizLeaderboard.Models;

namespace QuizLeaderboard.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly UserSession _session;

        public AuthService(HttpClient http, UserSession session)
        {
            _http = http;
            _session = session;
        }

        // ==============================
        // REGISTER
        // ==============================
        public async Task<bool> RegisterAsync(string email, string password, string displayName)
        {
            var response = await _http.PostAsJsonAsync("api/Auth/register", new
            {
                Email = email,
                Password = password,
                DisplayName = displayName
            });

            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<AuthResult>();
            if (result?.UserId is { } id)
            {
                await _session.SetAuthCookie(id);
                return true;
            }

            return false;
        }

        // ==============================
        // LOGIN
        // ==============================
        public async Task<bool> LoginAsync(string email, string password)
        {
            var response = await _http.PostAsJsonAsync("api/Auth/login", new
            {
                Email = email,
                Password = password
            });

            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<AuthResult>();
            if (result?.UserId is { } id)
            {
                await _session.SetAuthCookie(id);
                return true;
            }

            return true;
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

        // belső DTO a /api/Auth válaszhoz
        private class AuthResult
        {
            public string? UserId { get; set; }
        }
    }
}
