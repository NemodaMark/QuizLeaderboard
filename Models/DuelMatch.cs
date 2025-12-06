using System;
using System.Collections.Generic;

namespace QuizLeaderboard.Models;

public class DuelMatch
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid Player1Id { get; set; }
    public Guid Player2Id { get; set; }

    public int? Player1Score { get; set; }
    public int? Player2Score { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }

    public ICollection<DuelQuestion> Questions { get; set; } = new List<DuelQuestion>();
}