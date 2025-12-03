namespace QuizLeaderboard.Models;

public class LeaderboardEntry
{
    public int Rank { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int TotalScore { get; set; }
}