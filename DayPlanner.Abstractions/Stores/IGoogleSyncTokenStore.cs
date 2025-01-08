using DayPlanner.Abstractions.Models.Backend;

namespace DayPlanner.Abstractions.Stores
{
    public interface IGoogleSyncTokenStore
    {
        Task<string?> Get(string userId);
        Task Save(string userId, string syncToken);
        Task Delete(string userId);
    }



}
