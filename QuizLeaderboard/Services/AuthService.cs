using Microsoft.EntityFrameworkCore;
using QuizLeaderboard.Data;
using QuizLeaderboard.Models;

namespace QuizLeaderboard.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly UserSession _session;

    public AuthService(AppDbContext db, UserSession session)
    {
        _db = db;
        _session = session;
    }

    public async Task<User> LoginOrRegisterAsync(string displayName)
    {
        var name = displayName.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(displayName));

        var user = await _db.Users.FirstOrDefaultAsync(u => u.DisplayName == name);
        if (user is null)
        {
            user = new User { DisplayName = name };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        _session.SetUser(user);
        return user;
    }
}