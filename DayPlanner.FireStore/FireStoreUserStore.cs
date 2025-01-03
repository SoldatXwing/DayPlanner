using DayPlanner.Abstractions.Stores;
using FirebaseAdmin;
using FirebaseAdmin.Auth;

namespace DayPlanner.FireStore
{
    public class FireStoreUserStore(FirebaseApp app) : IUserStore
    {
        private readonly FirebaseAuth _firebaseAuth = FirebaseAuth.GetAuth(app);

        public async Task<UserRecord> CreateAsync(UserRecordArgs args)
        {
            ArgumentNullException.ThrowIfNull(args);
            return await _firebaseAuth.CreateUserAsync(args).ConfigureAwait(false);
        }

        public async Task<UserRecord> GetByIdAsync(string uid)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(uid);
            return await _firebaseAuth.GetUserAsync(uid).ConfigureAwait(false);
        }
    }

}
