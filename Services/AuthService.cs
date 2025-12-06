using System.Net.Http.Json;
using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace QuizLeaderboard.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;

        public AuthService(HttpClient http, IJSRuntime js)
        {
            _http = http;
            _js = js;
        }

        public async Task<(bool success, string? errorMessage)> RegisterAsync(string email, string password, string displayName)
        {
            try
            {
                var request = new
                {
                    Email = email,
                    Password = password,
                    DisplayName = displayName
                };

                var response = await _http.PostAsJsonAsync("/api/auth/register", request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    if (result?.UserId != null)
                    {
                        // UserId mentése localStorage-ba
                        await _js.InvokeVoidAsync("localStorage.setItem", "userId", result.UserId);
                    }
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return (false, "Registration failed. Email may already be in use.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Register error: {ex}");
                return (false, "An error occurred during registration.");
            }
        }

        public async Task<(bool success, string? errorMessage)> LoginAsync(string email, string password)
        {
            try
            {
                var request = new
                {
                    Email = email,
                    Password = password
                };

                var response = await _http.PostAsJsonAsync("/api/auth/login", request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    if (result?.UserId != null)
                    {
                        // UserId mentése localStorage-ba
                        await _js.InvokeVoidAsync("localStorage.setItem", "userId", result.UserId);
                    }
                    return (true, null);
                }

                return (false, "Invalid email or password.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex}");
                return (false, "An error occurred during login.");
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", "userId");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string?> GetCurrentUserIdAsync()
        {
            try
            {
                return await _js.InvokeAsync<string?>("localStorage.getItem", "userId");
            }
            catch
            {
                return null;
            }
        }

        private class AuthResponse
        {
            public string? UserId { get; set; }
        }
    }
}