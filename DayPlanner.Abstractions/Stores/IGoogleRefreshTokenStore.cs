using DayPlanner.Abstractions.Models.Backend;

namespace DayPlanner.Abstractions.Stores;

/// <summary>
/// Interface for storing and retrieving Google refresh tokens for users.
/// </summary>
public interface IGoogleRefreshTokenStore
{
    /// <summary>
    /// Retrieves the stored Google refresh token for a user.
    /// </summary>
    /// <param name="userId">The ID of the user whose refresh token is to be retrieved.</param>
    /// <returns>The stored <see cref="GoogleRefreshToken"/> if found, or <c>null</c> if no token exists for the user.</returns>
    Task<GoogleRefreshToken?> Get(string userId);

    /// <summary>
    /// Creates and stores a new Google refresh token for a user.
    /// </summary>
    /// <param name="userId">The ID of the user for whom the refresh token is being created.</param>
    /// <param name="token">The refresh token to be stored.</param>
    /// <returns>The newly created <see cref="GoogleRefreshToken"/> object.</returns>
    Task<GoogleRefreshToken> Create(string userId, string token);

    /// <summary>
    /// Deletes the stored Google refresh token for a user.
    /// </summary>
    /// <param name="userId">The ID of the user whose refresh token is to be deleted.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task Delete(string userId);
}