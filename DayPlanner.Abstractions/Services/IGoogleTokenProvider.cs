using DayPlanner.Abstractions.Models.Backend;

namespace DayPlanner.Abstractions.Services;

/// <summary>
/// Provider for Google tokens
/// </summary>
public interface IGoogleTokenProvider
{
    /// <summary>
    /// Retrieves the access token for the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>The current Google token, or null if no valid token exists.</returns>
    Task<GoogleTokenResponse?> GetOrRefresh(string userId);

    /// <summary>
    /// Checks if the current access token is still valid for the specified user.
    /// </summary>
    /// <param name="token">Token from the user</param>
    /// <returns>True if the token is valid; otherwise, false.</returns>
    Task<bool> IsTokenValid(GoogleTokenResponse token);
}