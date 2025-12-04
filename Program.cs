
using QuizLeaderboard.Components;
using Microsoft.EntityFrameworkCore;
using QuizLeaderboard.Data;
using QuizLeaderboard.Services;
using QuizLeaderboard.Models;
using QuizLeaderboard.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=leaderboard.db"));

builder.Services.AddScoped<LeaderboardService>();
builder.Services.AddSignalR();

builder.Services.AddScoped<UserSession>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddHttpClient<IQuestionGenerator, OpenAIQuestionGenerator>(client =>
{
    client.BaseAddress = new Uri("https://api.groq.com/");
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<LeaderboardHub>("/hubs/leaderboard");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // csak egyszer fut le, ha még nincs adat
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
            CompletedAt = DateTime.UtcNow
        });

        db.QuizResults.Add(new QuizResult
        {
            UserId = u2.Id,
            Score = 80,
            CompletedAt = DateTime.UtcNow
        });

        db.SaveChanges();
    }
}


app.Run();