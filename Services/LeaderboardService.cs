using Microsoft.EntityFrameworkCore;
using QuizLeaderboard.Data;
using QuizLeaderboard.Models;

namespace QuizLeaderboard.Services;

public enum LeaderboardPeriod
{
    Daily,
    Weekly,
    Monthly
}

public class LeaderboardEntry
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = "";
    public int TotalScore { get; set; }
    public int Rank { get; set; }
}

public class LeaderboardService
{
    private readonly AppDbContext _db;

    public LeaderboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(LeaderboardPeriod period)
    {
        var now = DateTime.UtcNow;

        DateTime startDate = period switch
        {
            LeaderboardPeriod.Daily   => now.Date,
            LeaderboardPeriod.Weekly  => now.Date.AddDays(-7),
            LeaderboardPeriod.Monthly => now.Date.AddMonths(-1),
            _                         => now.Date
        };

        // 1) QuizResults -> csoportosítás user szerint
        // 2) Join Users táblával
        // → minden EF-kompatibilis, nincs AsQueryable, nincs client eval.
        var list = await _db.QuizResults
            .Where(r => r.CompletedAt >= startDate)
            .GroupBy(r => r.UserId)
            .Select(g => new
            {
                UserId     = g.Key,
                TotalScore = g.Sum(x => x.Score)
            })
            .Join(
                _db.Users,
                g => g.UserId,
                u => u.Id,
                (g, u) => new LeaderboardEntry
                {
                    UserId      = u.Id,
                    DisplayName = u.DisplayName,
                    TotalScore  = g.TotalScore
                })
            .OrderByDescending(e => e.TotalScore)
            .ToListAsync();

        // Rangszám kiosztása memóriában
        int rank = 1;
        foreach (var e in list)
        {
            e.Rank = rank++;
        }

        return list;
    }
}