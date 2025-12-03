using QuizLeaderboard.Models;

namespace QuizLeaderboard.Services;

// Később ide jöhet valós OpenAI hívás.
// Most csak értelmes, de statikus kérdéseket generál.
public class DummyQuestionGenerator : IQuestionGenerator
{
    public Task<List<Question>> GenerateQuestionsAsync(QuestionRequest request)
    {
        var list = new List<Question>();

        for (int i = 0; i < request.Count; i++)
        {
            list.Add(new Question
            {
                Topic = request.Topic,
                Difficulty = request.Difficulty,
                Text = $"[{request.Topic}] ({request.Mode}) Question #{i + 1}: What does 'async' mean in C#?",
                Options = new[]
                {
                    "It allows non-blocking operations.",
                    "It makes code faster by default.",
                    "It runs only on another thread.",
                    "It disables exceptions."
                },
                CorrectIndex = 0
            });
        }

        return Task.FromResult(list);
    }
}