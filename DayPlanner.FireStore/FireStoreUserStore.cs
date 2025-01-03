using DayPlanner.Abstractions.Stores;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace DayPlanner.FireStore
{
    public class FireStoreUserStore(FirebaseApp app) : IUserStore
    {
        private readonly FirebaseAuth _firebaseAuth = FirebaseAuth.GetAuth(app) ?? throw new ArgumentNullException(nameof(app), "The Firebase app cannot be null.");

        public async Task<UserRecord> CreateAsync(UserRecordArgs args) => args is not null ? await _firebaseAuth.CreateUserAsync(args) : throw new ArgumentNullException(nameof(args), "The user record arguments cannot be null.");

        public async Task<UserRecord> GetByIdAsync(string uid) => uid is not "" ? await _firebaseAuth.GetUserAsync(uid) : throw new ArgumentNullException(nameof(uid), "The user uid cannot be empty.");
    }

}
