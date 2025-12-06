using System;
using System.ComponentModel.DataAnnotations;

namespace QuizLeaderboard.Models;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [EmailAddress]
    public string? Email { get; set; }

    public string? PasswordHash { get; set; }

    [Required, MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? BannerUrl { get; set; }
}