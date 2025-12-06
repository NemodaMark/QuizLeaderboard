using Microsoft.EntityFrameworkCore;
using QuizLeaderboard.Data;
using QuizLeaderboard.Models;

namespace QuizLeaderboard.Services;

public class DuelService
{
    private readonly AppDbContext _db;
    private readonly IQuestionGenerator _questionGenerator;

    public DuelService(AppDbContext db, IQuestionGenerator questionGenerator)
    {
        _db = db;
        _questionGenerator = questionGenerator;
    }

    public async Task<Guid> StartDuel(Guid player1Id, Guid player2Id)
    {
        const int questionCount = 4;

        var request = new QuestionRequest(
            Topic: "C# basics",
            Difficulty: "Medium",
            Count: questionCount,
            Mode: QuestionSourceMode.Duel
        );

        var generated = await _questionGenerator.GenerateQuestionsAsync(request);

        var duel = new DuelMatch
        {
            Player1Id = player1Id,
            Player2Id = player2Id,
            CreatedAt = DateTime.UtcNow
        };

        int index = 1;
        foreach (var q in generated)
        {
            var dq = new DuelQuestion
            {
                Index = index++,
                Text = q.Text,
                Options = q.Options,
                CorrectIndex = q.CorrectIndex,
                Topic = q.Topic,
                Difficulty = q.Difficulty
            };
            duel.Questions.Add(dq);
        }

        _db.Duels.Add(duel);
        await _db.SaveChangesAsync();

        return duel.Id;
    }

    public async Task<DuelMatch?> GetDuelAsync(Guid duelId)
    {
        return await _db.Duels
            .Include(d => d.Questions)
            .FirstOrDefaultAsync(d => d.Id == duelId);
    }
}