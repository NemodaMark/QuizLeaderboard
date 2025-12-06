using Microsoft.EntityFrameworkCore;
using QuizLeaderboard.Components;
using QuizLeaderboard.Data;
using QuizLeaderboard.Hubs;
using QuizLeaderboard.Models;
using QuizLeaderboard.Services;

var builder = WebApplication.CreateBuilder(args);

// Blazor Razor Components – NINCS InteractiveServer, csak sima komponensek
builder.Services.AddRazorComponents();

// EF Core + SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=leaderboard.db"));

// HttpContext eléréséhez (cookie-s auth miatt kell)
builder.Services.AddHttpContextAccessor();

// SignalR
builder.Services.AddSignalR();

// Saját szolgáltatások
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserSession>();
builder.Services.AddScoped<LeaderboardService>();
builder.Services.AddScoped<DuelService>();

// OpenAI kérdésgenerátor HTTP klienssel
builder.Services.AddHttpClient<IQuestionGenerator, OpenAIQuestionGenerator>();

var app = builder.Build();

// Hibakezelés
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Blazor 8 komponenses hostolás – NINCS rendermode
app.MapRazorComponents<App>();

// SignalR hub a leaderboardhoz
app.MapHub<LeaderboardHub>("/hubs/leaderboard");

// Demo seed adatok
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        var u1 = new User { DisplayName = "Alice", Email = "alice@example.com" };
        var u2 = new User { DisplayName = "Bob", Email = "bob@example.com" };

        db.Users.AddRange(u1, u2);
        db.SaveChanges();

        db.QuizResults.Add(new QuizResult
        {
            UserId = u1.Id,
            Score = 50,
            CompletedAt = DateTime.UtcNow,
            Mode = QuizMode.Casual,
            Topic = "Seed",
            Difficulty = "Easy"
        });

        db.QuizResults.Add(new QuizResult
        {
            UserId = u2.Id,
            Score = 80,
            CompletedAt = DateTime.UtcNow,
            Mode = QuizMode.Casual,
            Topic = "Seed",
            Difficulty = "Easy"
        });

        db.SaveChanges();
    }
}

app.Run();
