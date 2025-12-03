using Microsoft.EntityFrameworkCore;
using QuizLeaderboard.Models;

namespace QuizLeaderboard.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) {}

    public DbSet<User> Users => Set<User>();
    public DbSet<QuizResult> QuizResults => Set<QuizResult>();
}