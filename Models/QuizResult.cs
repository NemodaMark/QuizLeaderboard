using QuizLeaderboard.Services; // szükséges az enum miatt (ha külön fájlban van)

namespace QuizLeaderboard.Models;

public class QuizResult
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int Score { get; set; }
    public DateTime CompletedAt { get; set; }

    public QuizMode Mode { get; set; }               // Learning / Daily / Casual / Duel
    public string Topic { get; set; } = "";          // pl. "C# basics"
    public string Difficulty { get; set; } = "Medium"; // Easy / Medium / Hard
}