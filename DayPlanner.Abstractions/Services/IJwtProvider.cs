namespace DayPlanner.Abstractions.Services;

public interface IJwtProvider
{
    Task<(string token, string refreshToken)> GetForCredentialsAsync(string email, string password);
    Task<string> RefreshIdTokenAsync(string refreshToken);
}