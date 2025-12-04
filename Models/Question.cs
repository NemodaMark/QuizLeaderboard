namespace QuizLeaderboard.Models;

public class Question
{
    public string Topic { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Medium";

    public string Text { get; set; } = string.Empty;
    public string[] Options { get; set; } = Array.Empty<string>();
    public int CorrectIndex { get; set; }
}