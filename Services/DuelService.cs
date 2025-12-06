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

    public async Task<Duel?> GetDuel(int id)
    {
        return await _db.Duels
            .Include(d => d.Questions)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Duel> CreateDuelAsync(
        int player1Id,
        int player2Id,
        string topic,
        string difficulty,
        int questionCount = 5)
    {
        var req = new QuestionRequest(
            Topic: topic,
            Difficulty: difficulty,
            Count: questionCount,
            Mode: QuestionSourceMode.Duel
        );

        var questions = await _questionGenerator.GenerateQuestionsAsync(req);

        var duel = new Duel
        {
            Player1Id = player1Id,
            Player2Id = player2Id,
            QuestionCount = questionCount,
            Topic = topic,
            Difficulty = difficulty,
            CreatedAt = DateTime.UtcNow
        };

        var index = 1;
        foreach (var q in questions)
        {
            duel.Questions.Add(new DuelQuestion
            {
                Index = index++,
                Text = q.Text,
                Options = q.Options,
                CorrectIndex = q.CorrectIndex
            });
        }

        _db.Duels.Add(duel);
        await _db.SaveChangesAsync();

        return duel;
    }
    
    public async Task<int> StartDuel(int challengerId, int opponentId)
    {
        // ideiglenesen fix topic + difficulty, később profilból is jöhet
        var duel = await CreateDuelAsync(
            player1Id: challengerId,
            player2Id: opponentId,
            topic: "C# basics",
            difficulty: "Medium",
            questionCount: 5
        );

        return duel.Id;
    }

}