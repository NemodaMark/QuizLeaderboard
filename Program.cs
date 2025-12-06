using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using QuizLeaderboard.Components;
using QuizLeaderboard.Data;
using QuizLeaderboard.Hubs;
using QuizLeaderboard.Models;
using QuizLeaderboard.Services;

var builder = WebApplication.CreateBuilder(args);

// Blazor components – interactive server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// EF Core + SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=leaderboard.db"));

// SignalR
builder.Services.AddSignalR();

// HttpContext access for AuthService
builder.Services.AddHttpContextAccessor();

// Custom services
builder.Services.AddScoped<UserSession>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<LeaderboardService>();
builder.Services.AddScoped<DuelService>();

// Question generator (keep your current implementation)
builder.Services.AddHttpClient<IQuestionGenerator, OpenAIQuestionGenerator>();

var app = builder.Build();

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Single Blazor host – NO MapGet/MapPost("/login"), NO MapPost("/register")
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// SignalR hub for leaderboard
app.MapHub<LeaderboardHub>("/hubs/leaderboard");

// Seed demo data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        var u1 = new User { DisplayName = "Alice" };
        var u2 = new User { DisplayName = "Bob" };

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
