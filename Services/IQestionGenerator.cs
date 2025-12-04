using QuizLeaderboard.Models;

namespace QuizLeaderboard.Services;

public enum QuestionSourceMode
{
    Learning,
    Daily,
    Casual,
    Duel
}

public record QuestionRequest(
    string Topic,
    string Difficulty,
    int Count,
    QuestionSourceMode Mode
);

public interface IQuestionGenerator
{
    Task<List<Question>> GenerateQuestionsAsync(QuestionRequest request);
}