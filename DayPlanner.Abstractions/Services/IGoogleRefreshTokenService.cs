using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Stores;

namespace DayPlanner.Abstractions.Services
{
    public interface IGoogleRefreshTokenService
    {
        Task<GoogleRefreshToken> Get(string userId);
        Task Create(string userId, string token);
        Task Delete(string userId);
    }
    public class GoogleRefreshTokenService(IGoogleRefreshTokenStore store) : IGoogleRefreshTokenService
    {
        private readonly IGoogleRefreshTokenStore _store = store ?? throw new ArgumentNullException(nameof(store), "Store cannot be null.");
        public async Task Delete(string userId) => await _store.Delete(string.IsNullOrEmpty(userId) ? throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.") : userId);

        public async Task<GoogleRefreshToken> Get(string userId) => await _store.Get(string.IsNullOrEmpty(userId) ? throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.") : userId);

        public async Task Create(string userId, string token)
        {
            ArgumentException.ThrowIfNullOrEmpty(token);
            ArgumentException.ThrowIfNullOrEmpty(userId);
            await _store.Create(userId, token);
        }
    }

}