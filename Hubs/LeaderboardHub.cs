using Microsoft.AspNetCore.SignalR;
using QuizLeaderboard.Data;
using QuizLeaderboard.Models;
using QuizLeaderboard.Services;

namespace QuizLeaderboard.Hubs;

public class LeaderboardHub : Hub
{
    private readonly AppDbContext _db;
    private readonly LeaderboardService _leaderboard;

    public LeaderboardHub(AppDbContext db, LeaderboardService leaderboard)
    {
        _db = db;
        _leaderboard = leaderboard;
    }

    public async Task SubmitScore(string userName, int score, LeaderboardPeriod period)
    {
        if (string.IsNullOrWhiteSpace(userName) || score <= 0)
            return;

        var user = _db.Users.FirstOrDefault(u => u.DisplayName == userName);
        if (user is null)
        {
            user = new User { DisplayName = userName };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        _db.QuizResults.Add(new QuizResult
        {
            UserId = user.Id,
            Score = score,
            CompletedAt = DateTime.UtcNow,
            Mode = QuizMode.Casual,
            Topic = "Demo",
            Difficulty = "N/A"
        });

        await _db.SaveChangesAsync();

        var leaderboard = await _leaderboard.GetLeaderboardAsync(period);
        await Clients.All.SendAsync("LeaderboardUpdated", leaderboard);
    }
}   