using System;

namespace QuizLeaderboard.Models;

public class DuelQuestion
{
    public int Id { get; set; }

    public Guid DuelMatchId { get; set; }
    public DuelMatch DuelMatch { get; set; } = null!;

    public int Index { get; set; }

    public string Text { get; set; } = "";
    public string[] Options { get; set; } = Array.Empty<string>();
    public int CorrectIndex { get; set; }

    public string Topic { get; set; } = "";
    public string Difficulty { get; set; } = "";
}