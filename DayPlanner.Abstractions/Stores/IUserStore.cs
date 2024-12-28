using FirebaseAdmin.Auth;

namespace DayPlanner.Abstractions.Stores
{

    public interface IUserStore
    {
        Task<UserRecord> CreateAsync(UserRecordArgs args);
        Task<UserRecord> GetByIdAsync(string uid);
    }


}
