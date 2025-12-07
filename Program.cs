using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

using QuizLeaderboard.Components;
using QuizLeaderboard.Data;
using QuizLeaderboard.Hubs;
using QuizLeaderboard.Models;
using QuizLeaderboard.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// ✅ ALL SERVICE REGISTRATIONS MUST OCCUR HERE
// ==========================================================

builder.Services.AddScoped<UserSession>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthService>();

// Blazor Servernél kell egy HttpClient is:
builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
});

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

// ÚJ: HttpClient és Controllers
builder.Services.AddHttpClient();
builder.Services.AddControllers();

// Custom services
builder.Services.AddScoped<UserSession>();
builder.Services.AddScoped<LeaderboardService>();
builder.Services.AddScoped<DuelService>();

// NEW CLIENT-SIDE SERVICE: Must be added for Blazor pages to inject
builder.Services.AddScoped<AuthService>(); 

// Question generator
builder.Services.AddHttpClient<IQuestionGenerator, OpenAIQuestionGenerator>();

// ==========================================================
// ❌ END OF SERVICE REGISTRATIONS
// ==========================================================

var app = builder.Build();

// ==========================================================
// ✅ MIDDLEWARE AND ENDPOINT MAPPINGS
// ==========================================================

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Blazor host
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ÚJ: API Controllers
app.MapControllers();

// SignalR hub
app.MapHub<LeaderboardHub>("/hubs/leaderboard");

// Seed demo data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    
    // Seed data (ensure QuizResult and QuizMode types are available)
    if (!db.Users.Any())
    {
        var u1 = new User { DisplayName = "Alice" };
        var u2 = new User { DisplayName = "Bob" };
        
        db.Users.AddRange(u1, u2);
        db.SaveChanges();
        
        // Assuming QuizResult and QuizMode are correctly defined in your Models
        // Note: You may want to hash the password for these seed users in a real app.
        db.QuizResults.Add(new QuizResult
        {
            UserId = u1.Id,
            Score = 50,
            CompletedAt = DateTime.UtcNow,
            Mode = (QuizMode)0, // Replace with actual QuizMode enum value
            Topic = "Seed",
            Difficulty = "Easy"
        });
        
        db.QuizResults.Add(new QuizResult
        {
            UserId = u2.Id,
            Score = 80,
            CompletedAt = DateTime.UtcNow,
            Mode = (QuizMode)0, // Replace with actual QuizMode enum value
            Topic = "Seed",
            Difficulty = "Easy"
        });
        
        db.SaveChanges();
    }
}

app.Run();