using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Stores;

namespace DayPlanner.Abstractions.Services.Implementations;

/// <summary>
/// Service for managing Google tokens, including refresh tokens and sync tokens.
/// </summary>
/// <param name="store">The store for managing Google refresh tokens.</param>
/// <param name="syncStore">The store for managing Google sync tokens.</param>
public class GoogleTokenService(IGoogleRefreshTokenStore store, IGoogleSyncTokenStore syncStore) : IGoogleTokenService
{
    private readonly IGoogleRefreshTokenStore _store = store;
    private readonly IGoogleSyncTokenStore _syncStore = syncStore;

    /// <summary>
    /// Deletes the refresh token for the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user whose refresh token is to be deleted.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="userId"/> is null or empty.</exception>
    public async Task DeleteRefreshToken(string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        await _store.Delete(userId);
    }

    /// <summary>
    /// Retrieves the refresh token for the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user whose refresh token is to be retrieved.</param>
    /// <returns>The refresh token for the user, or null if not found.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="userId"/> is null or empty.</exception>
    public async Task<GoogleRefreshToken?> GetRefreshToken(string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        return await _store.Get(userId);
    }

    /// <summary>
    /// Creates a refresh token for the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user for whom the refresh token is to be created.</param>
    /// <param name="token">The refresh token to be created.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="userId"/> or <paramref name="token"/> is null or empty.</exception>
    public async Task CreateRefreshToken(string userId, string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        ArgumentException.ThrowIfNullOrEmpty(userId);

        await _store.Create(userId, token);
    }

    /// <summary>
    /// Retrieves the sync token for the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user whose sync token is to be retrieved.</param>
    /// <returns>The sync token for the user, or null if not found.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="userId"/> is null or empty.</exception>
    public async Task<string?> GetSyncToken(string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        return await _syncStore.Get(userId);
    }

    /// <summary>
    /// Deletes the sync token for the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user whose sync token is to be deleted.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="userId"/> is null or empty.</exception>
    public async Task DeleteSyncToken(string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        await _syncStore.Delete(userId);
    }

    /// <summary>
    /// Saves a sync token for the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user for whom the sync token is to be saved.</param>
    /// <param name="token">The sync token to be saved.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="userId"/> or <paramref name="token"/> is null or empty.</exception>
    public async Task SaveSyncToken(string userId, string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        ArgumentException.ThrowIfNullOrEmpty(userId);

        await _syncStore.Save(userId, token);
    }
}
