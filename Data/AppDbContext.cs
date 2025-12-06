using Microsoft.EntityFrameworkCore;
using QuizLeaderboard.Models;

namespace QuizLeaderboard.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<QuizResult> QuizResults => Set<QuizResult>();

    public DbSet<DuelMatch> Duels => Set<DuelMatch>();
    public DbSet<DuelQuestion> DuelQuestions => Set<DuelQuestion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // DuelQuestion.Options string[] -> string
        modelBuilder.Entity<DuelQuestion>()
            .Property(q => q.Options)
            .HasConversion(
                v => string.Join("|||", v),
                v => v.Split(new[] { "|||"}, StringSplitOptions.None)
            );

        // Kapcsolat DuelMatch <-> DuelQuestion
        modelBuilder.Entity<DuelQuestion>()
            .HasOne(q => q.DuelMatch)
            .WithMany(d => d.Questions)
            .HasForeignKey(q => q.DuelMatchId);
    }
}