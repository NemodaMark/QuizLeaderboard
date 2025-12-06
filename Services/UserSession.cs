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
                CurrentUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            }

            IsLoaded = true;
        }

        public void Clear()
        {
            CurrentUser = null;
            IsLoaded = false;
        }
    }
}