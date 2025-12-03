using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using QuizLeaderboard.Models;

namespace QuizLeaderboard.Services;

public class OpenAIQuestionGenerator : IQuestionGenerator
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _model;

    // Groq Chat Completions endpoint (MUST be absolute URL)
    private const string ChatUrl = "https://api.groq.com/openai/v1/chat/completions";

    public OpenAIQuestionGenerator(HttpClient http, IConfiguration config)
    {
        _http = http;

        var section = config.GetSection("Groq");
        _apiKey = section["ApiKey"] ?? throw new InvalidOperationException("Groq:ApiKey is missing");
        _model = section["Model"] ?? "llama3-8b-8192";
    }

    public async Task<List<Question>> GenerateQuestionsAsync(QuestionRequest request)
    {
        // Strong JSON-only system instruction
        var systemPrompt = """
You are a quiz question generator.
Return ONLY valid JSON array like this:

[
  {
    "text": "question text",
    "options": ["A","B","C","D"],
    "correctIndex": 0
  }
]

Do NOT add explanations, commentary, formatting, markdown, or additional text.
""";

        var userPrompt =
            $"Generate {request.Count} {request.Difficulty} difficulty " +
            $"multiple-choice questions about {request.Topic} for mode {request.Mode}. " +
            $"Output ONLY JSON, nothing else.";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, ChatUrl);

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        // GROQ DOES NOT SUPPORT response_format
        var payload = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        };

        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        using var response = await _http.SendAsync(httpRequest);

        // Rate limit handling
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            await Task.Delay(1000);
            using var retryResponse = await _http.SendAsync(httpRequest);

            if (retryResponse.StatusCode == HttpStatusCode.TooManyRequests)
                return GenerateFallbackQuestions(request);

            return await ParseResponse(retryResponse, request);
        }

        // 400 or other errors → fallback
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            return GenerateFallbackQuestions(request);
        }

        response.EnsureSuccessStatusCode();
        return await ParseResponse(response, request);
    }

    private static async Task<List<Question>> ParseResponse(HttpResponseMessage response, QuestionRequest request)
    {
        var raw = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(raw);

        // Extract AI JSON string
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(content))
            throw new Exception("Empty response from Groq.");

        // Parse JSON array inside content
        using var questionsDoc = JsonDocument.Parse(content);
        var arr = questionsDoc.RootElement;

        var list = new List<Question>();

        foreach (var q in arr.EnumerateArray())
        {
            list.Add(new Question
            {
                Topic = request.Topic,
                Difficulty = request.Difficulty,
                Text = q.GetProperty("text").GetString() ?? "",
                Options = q.GetProperty("options")
                    .EnumerateArray()
                    .Select(x => x.GetString() ?? "")
                    .ToArray(),
                CorrectIndex = q.GetProperty("correctIndex").GetInt32()
            });
        }

        return list;
    }

    // Offline fallback
    private List<Question> GenerateFallbackQuestions(QuestionRequest request)
    {
        return new List<Question>
        {
            new Question
            {
                Topic = request.Topic,
                Difficulty = request.Difficulty,
                Text = $"[LOCAL] Mivel foglalkozik a(z) {request.Topic} témakör?",
                Options = new[]
                {
                    "Programkód írásával és karbantartásával",
                    "Csak hardverek összeszerelésével",
                    "Csak grafikai tervezéssel",
                    "Csak irodai adminisztrációval"
                },
                CorrectIndex = 0
            }
        };
    }
}
