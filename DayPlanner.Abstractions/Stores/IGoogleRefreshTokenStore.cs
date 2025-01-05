using DayPlanner.Abstractions.Models.Backend;

namespace DayPlanner.Abstractions.Stores
{
    public interface IGoogleRefreshTokenStore
    {
        Task<GoogleRefreshToken?> Get(string userId);
        Task<GoogleRefreshToken> Create(string userId, string token);
        Task Delete(string userId);

    }


}
