using System;
using System.Collections.Generic;

namespace QuizLeaderboard.Models;

public class Duel
{
    public int Id { get; set; }

    // Résztvevők
    public int Player1Id { get; set; }
    public User Player1 { get; set; } = null!;

    public int Player2Id { get; set; }
    public User Player2 { get; set; } = null!;

    // Kérdések száma
    public int QuestionCount { get; set; }

    // Eredmények (null, amíg nincs kész)
    public int? Player1Score { get; set; }
    public int? Player2Score { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }

    // Meta
    public string Topic { get; set; } = "C# basics";
    public string Difficulty { get; set; } = "Medium";

    // Navigációs property a kérdésekhez
    public List<DuelQuestion> Questions { get; set; } = new();
}