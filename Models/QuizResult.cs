using System;

namespace QuizLeaderboard.Models
{

    public class QuizResult
    {
        public int Id { get; set; }

        // User GUID
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public int Score { get; set; }
        public DateTime CompletedAt { get; set; }

        public QuizMode Mode { get; set; }

        public string? Topic { get; set; }
        public string? Difficulty { get; set; }
    }
}