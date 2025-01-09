using DayPlanner.Abstractions.Models.Backend;

namespace DayPlanner.Abstractions.Services;

public interface IGoogleTokenService
{
    Task<GoogleRefreshToken?> GetRefreshToken(string userId);

    Task CreateRefreshToken(string userId, string token);

    Task DeleteRefreshToken(string userId);

    Task<string?> GetSyncToken(string userId);

    Task DeleteSyncToken(string userId);

    Task SaveSyncToken(string userId, string token);
}