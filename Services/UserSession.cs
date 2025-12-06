using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuizLeaderboard.Data;
using QuizLeaderboard.Models;

namespace QuizLeaderboard.Services
{
    public class UserSession
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public User? CurrentUser { get; private set; }
        public bool IsLoaded { get; private set; }
        public bool IsLoggedIn => CurrentUser != null;

        public UserSession(AppDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task EnsureLoadedAsync()
        {
            if (IsLoaded)
                return;

            var http = _httpContextAccessor.HttpContext;
            if (http == null)
            {
                IsLoaded = true;
                return;
            }

            if (http.Request.Cookies.TryGetValue("QuizAuth", out var cookie) &&
                Guid.TryParse(cookie, out var userId))
            {
                // Fetch the user from the database
                CurrentUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            }

            IsLoaded = true;
        }

        // Consolidated private method to clear in-memory state
        private void Clear()
        {
            CurrentUser = null;
            IsLoaded = false;
        }
        
        // Sets the authentication cookie and loads the user
        public async Task SetAuthCookie(string userId)
        {
            var http = _httpContextAccessor.HttpContext;
            if (http != null)
            {
                http.Response.Cookies.Append(
                    "QuizAuth", 
                    userId, 
                    new CookieOptions { 
                        Expires = DateTimeOffset.UtcNow.AddDays(30),
                        HttpOnly = true, 
                        Secure = true,   
                        SameSite = SameSiteMode.Strict 
                    });

                // Immediately load the user into the session
                if (Guid.TryParse(userId, out var userGuid))
                {
                    CurrentUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == userGuid);
                }
            }
            IsLoaded = true;
        }

        // Clears the authentication cookie and the in-memory state
        public void ClearAuthCookie()
        {
            var http = _httpContextAccessor.HttpContext;
            if (http != null)
            {
                http.Response.Cookies.Delete("QuizAuth");
            }
            Clear();
        }
    }
}