using System.Linq;
using Microsoft.EntityFrameworkCore;
using QuizLeaderboard.Data;
using QuizLeaderboard.Models;


namespace QuizLeaderboard.Services;

public enum LeaderboardPeriod
{
    Daily,
    Weekly,
    Monthly,
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
        // inkább ne 'from' legyen a változónév
        DateTime fromDate = period switch
        {
            LeaderboardPeriod.Daily   => DateTime.UtcNow.Date,
            LeaderboardPeriod.Weekly  => DateTime.UtcNow.Date.AddDays(-7),
            LeaderboardPeriod.Monthly => DateTime.UtcNow.Date.AddMonths(-1),
            _ => DateTime.UtcNow.Date
        };

        var list = await _db.QuizResults
            .Include(r => r.User)
            .Where(r => r.CompletedAt >= fromDate)
            .GroupBy(r => new { r.UserId, r.User!.DisplayName })
            .Select(g => new LeaderboardEntry
            {
                DisplayName = g.Key.DisplayName,
                TotalScore  = g.Sum(x => x.Score)
            })
            .OrderByDescending(e => e.TotalScore)
            .ToListAsync();

        int rank = 1;
        foreach (var entry in list)
        {
            entry.Rank = rank++;
        }

        return list;
    }

}