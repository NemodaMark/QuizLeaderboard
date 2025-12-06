using Microsoft.EntityFrameworkCore;
using QuizLeaderboard.Models;
using System;

namespace QuizLeaderboard.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<QuizResult> QuizResults => Set<QuizResult>();

    public DbSet<Duel> Duels => Set<Duel>();
    public DbSet<DuelQuestion> DuelQuestions => Set<DuelQuestion>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);

        // DuelQuestion.Options: string[] ↔ string az adatbázisban
        model.Entity<DuelQuestion>()
            .Property(q => q.Options)
            .HasConversion(
                v => string.Join("|||", v),
                v => v.Split(new[] { "|||" }, StringSplitOptions.None)
            );
    }
}