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

    private const string ChatUrl = "https://api.groq.com/openai/v1/chat/completions";

    public OpenAIQuestionGenerator(HttpClient http, IConfiguration config)
    {
        _http = http;

        var section = config.GetSection("Groq");
        _apiKey = section["ApiKey"] ?? "";
        _model  = section["Model"]  ?? "llama3-8b-8192";

        Console.WriteLine($"[Groq] Loaded ApiKey length = {_apiKey?.Length ?? 0}, model = {_model}");
    }

    public async Task<List<Question>> GenerateQuestionsAsync(QuestionRequest request)
    {
        // 1) nincs kulcs → mindig local
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            Console.WriteLine("[Groq] No API key -> local questions");
            return GenerateFallbackQuestions(request, "noApi");
        }

        var systemPrompt = """
            You are a quiz question generator.
            Return ONLY a valid JSON array like:

            [
              {
                "text": "question text in Hungarian",
                "options": ["A", "B", "C", "D"],
                "correctIndex": 0
              }
            ]

            - Always 4 options.
            - Exactly one correctIndex (0-3).
            - No explanation, no markdown, no code fences, no extra text.
            """;

        var userPrompt =
            $"Generate {request.Count} {request.Difficulty} difficulty " +
            $"multiple-choice questions about {request.Topic} for mode {request.Mode}. " +
            $"Output ONLY JSON, nothing else.";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, ChatUrl);

        httpRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);

        var payload = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user",   content = userPrompt   }
            }
        };

        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(httpRequest);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Groq] Network error: " + ex.Message);
            return GenerateFallbackQuestions(request, "network");
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            Console.WriteLine("[Groq] 429 TooManyRequests, retrying once...");
            await Task.Delay(1000);

            try
            {
                response = await _http.SendAsync(httpRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Groq] Network error (retry): " + ex.Message);
                return GenerateFallbackQuestions(request, "network2");
            }
        }

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("[Groq] Bad HTTP status: " + 
                              (int)response.StatusCode + " " + response.StatusCode);
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine("[Groq] Response body: " + error);

            return GenerateFallbackQuestions(request, "badStatus"); 
        }


        return await ParseResponse(response, request);
    }

    private static async Task<List<Question>> ParseResponse(
        HttpResponseMessage response,
        QuestionRequest request)
    {
        var raw = await response.Content.ReadAsStringAsync();
        Console.WriteLine("[Groq] Raw response (first 400 chars):");
        Console.WriteLine(raw.Substring(0, Math.Min(400, raw.Length)));

        // --- 1) parse outer Groq JSON ---
        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(raw);
        }
        catch (JsonException ex)
        {
            Console.WriteLine("[Groq] Failed to parse outer JSON: " + ex.Message);
            return GenerateFallbackQuestions(request, "parseOuter");
        }
        using var _ = doc;

        string? content;
        try
        {
            content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Groq] Failed to read choices[0].message.content: " + ex.Message);
            return GenerateFallbackQuestions(request, "noContent");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            Console.WriteLine("[Groq] Content is empty");
            return GenerateFallbackQuestions(request, "emptyContent");
        }

        content = content.Trim();

        // --- 2) strip ``` fences if present ---
        if (content.StartsWith("```"))
        {
            var firstNewline = content.IndexOf('\n');
            var lastFence = content.LastIndexOf("```", StringComparison.Ordinal);

            if (firstNewline >= 0 && lastFence > firstNewline)
            {
                content = content.Substring(
                    firstNewline + 1,
                    lastFence - firstNewline - 1
                ).Trim();
            }
        }

        // repair the most frequent invalid escapes: \'  -> '
        content = content.Replace("\\'", "'");

        // --- 3) isolate JSON array between first '[' and last ']' ---
        var start = content.IndexOf('[');
        var end   = content.LastIndexOf(']');

        if (start >= 0 && end > start)
        {
            content = content.Substring(start, end - start + 1);
        }

        Console.WriteLine("[Groq] Inner content (first 400 chars):");
        Console.WriteLine(content.Substring(0, Math.Min(400, content.Length)));

        // --- 4) parse inner JSON array ---
        JsonDocument questionsDoc;
        try
        {
            questionsDoc = JsonDocument.Parse(content);
        }
        catch (JsonException ex)
        {
            Console.WriteLine("[Groq] Failed to parse inner JSON: " + ex.Message);
            return GenerateFallbackQuestions(request, "parseInner");
        }
        using var __ = questionsDoc;

        var root = questionsDoc.RootElement;
        if (root.ValueKind != JsonValueKind.Array)
        {
            Console.WriteLine("[Groq] Inner JSON is not an array");
            return GenerateFallbackQuestions(request, "notArray");
        }

        var list = new List<Question>();

        foreach (var q in root.EnumerateArray())
        {
            try
            {
                var options = q.GetProperty("options")
                    .EnumerateArray()
                    .Select(x => x.GetString() ?? "")
                    .ToArray();

                if (options.Length != 4)
                    continue;

                list.Add(new Question
                {
                    Topic        = request.Topic,
                    Difficulty   = request.Difficulty,
                    Text         = q.GetProperty("text").GetString() ?? "",
                    Options      = options,
                    CorrectIndex = q.GetProperty("correctIndex").GetInt32()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Groq] Skipping malformed question: " + ex.Message);
            }
        }

        if (list.Count == 0)
        {
            Console.WriteLine("[Groq] No valid questions parsed");
            return GenerateFallbackQuestions(request, "noValid");
        }

        return list;
    }

    private static List<Question> GenerateFallbackQuestions(QuestionRequest request, string reason)
    {
        // reason-t ráírjuk, hogy lásd, hol akad el
        var label = $"[LOCAL:{reason}]";

        return new List<Question>
        {
            new Question
            {
                Topic = request.Topic,
                Difficulty = request.Difficulty,
                Text = $"{label} Mivel foglalkozik a(z) {request.Topic} témakör?",
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
