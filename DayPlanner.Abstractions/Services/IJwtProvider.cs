using DayPlanner.Abstractions.Models.Backend;

namespace DayPlanner.Abstractions.Services;

public interface IJwtProvider
{
    Task<TokenResponse> GetForCredentialsAsync(string email, string password);
    Task<string> RefreshIdTokenAsync(string refreshToken);
}