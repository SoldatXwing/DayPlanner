using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Stores;

namespace DayPlanner.Abstractions.Services.Implementations;

public class GoogleTokenService(IGoogleRefreshTokenStore store, IGoogleSyncTokenStore syncStore) : IGoogleTokenService
{
    private readonly IGoogleRefreshTokenStore _store = store;
    private readonly IGoogleSyncTokenStore _syncStore = syncStore;

    public async Task DeleteRefreshToken(string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        await _store.Delete(userId);
    }

    public async Task<GoogleRefreshToken?> GetRefreshToken(string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        return await _store.Get(userId);
    }

    public async Task CreateRefreshToken(string userId, string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        ArgumentException.ThrowIfNullOrEmpty(userId);

        await _store.Create(userId, token);
    }

    public async Task<string?> GetSyncToken(string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        return await _syncStore.Get(userId);
    }

    public async Task DeleteSyncToken(string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        await _syncStore.Delete(userId);
    }

    public async Task SaveSyncToken(string userId, string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        ArgumentException.ThrowIfNullOrEmpty(userId);

        await _syncStore.Save(userId, token);
    }
}
