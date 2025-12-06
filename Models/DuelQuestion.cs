using System;

namespace QuizLeaderboard.Models;

public class DuelQuestion
{
    public int Id { get; set; }

    public int DuelId { get; set; }
    public Duel Duel { get; set; } = null!;

    // 1..QuestionCount
    public int Index { get; set; }

    public string Text { get; set; } = "";

    // EF Core-hoz string[] lesz, erre van a HasConversion az AppDbContext-ben
    public string[] Options { get; set; } = Array.Empty<string>();

    public int CorrectIndex { get; set; }
}