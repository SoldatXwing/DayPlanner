using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Stores;

namespace DayPlanner.Abstractions.Services
{
    public interface IGoogleTokenService
    {
        Task<GoogleRefreshToken?> GetRefreshToken(string userId);
        Task CreateRefreshToken(string userId, string token);
        Task DeleteRefreshToken(string userId);

        Task<string?> GetSyncToken(string userId);
        Task DeleteSyncToken(string userId);
        Task SaveSyncToken(string userId, string token);
    }
    public class GoogleRefreshTokenService(IGoogleRefreshTokenStore store, IGoogleSyncTokenStore syncStore) : IGoogleTokenService
    {
        private readonly IGoogleRefreshTokenStore _store = store ?? throw new ArgumentNullException(nameof(store), "Store cannot be null.");
        private readonly IGoogleSyncTokenStore _syncStore = syncStore ?? throw new ArgumentNullException(nameof(syncStore), "Sync store cannot be null.");
        public async Task DeleteRefreshToken(string userId) => await _store.Delete(string.IsNullOrEmpty(userId) ? throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.") : userId);
        public async Task<GoogleRefreshToken?> GetRefreshToken(string userId) => await _store.Get(string.IsNullOrEmpty(userId) ? throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.") : userId);
        public async Task CreateRefreshToken(string userId, string token)
        {
            ArgumentException.ThrowIfNullOrEmpty(token);
            ArgumentException.ThrowIfNullOrEmpty(userId);
            await _store.Create(userId, token);
        }
        public async Task<string?> GetSyncToken(string userId) => await _syncStore.Get(string.IsNullOrEmpty(userId) ? throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.") : userId);
        public async Task DeleteSyncToken(string userId) => await _syncStore.Delete(string.IsNullOrEmpty(userId) ? throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.") : userId);
        public async Task SaveSyncToken(string userId, string token)
        {
            ArgumentException.ThrowIfNullOrEmpty(token);
            ArgumentException.ThrowIfNullOrEmpty(userId);
            await _syncStore.Save(userId, token);
        }
    }

}