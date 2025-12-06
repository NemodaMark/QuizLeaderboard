public class Lesson
{
    public int Id { get; set; }

    public string Topic { get; set; } = "";     // "C# basics"
    public string Difficulty { get; set; } = ""; // "Easy"
    public string Language { get; set; } = "en"; // "hu", "en"

    public string Title { get; set; } = "";
    public string ContentHtml { get; set; } = ""; // rövid leírás, példa kódok
}