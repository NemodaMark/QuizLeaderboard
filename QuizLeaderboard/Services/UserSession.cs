namespace QuizLeaderboard.Services;

using QuizLeaderboard.Models;

public class UserSession
{
    public User? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser != null;

    public event Action? OnChange;

    public void SetUser(User? user)
    {
        CurrentUser = user;
        OnChange?.Invoke();
    }
}