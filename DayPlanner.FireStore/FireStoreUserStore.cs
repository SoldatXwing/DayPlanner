using DayPlanner.Abstractions.Stores;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace DayPlanner.FireStore
{
    public class FireStoreUserStore : IUserStore
    {
        private readonly FirebaseAuth _firebaseAuth;
        public FireStoreUserStore(string serviceAccountPath)
        {
            ArgumentException.ThrowIfNullOrEmpty(serviceAccountPath);
            var app = FirebaseApp.DefaultInstance ?? FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(serviceAccountPath)
            });

            _firebaseAuth = FirebaseAuth.GetAuth(app);
        }
        public async Task<UserRecord> CreateAsync(UserRecordArgs args) => args is not null ? await _firebaseAuth.CreateUserAsync(args) : throw new ArgumentNullException(nameof(args), "The user record arguments cannot be null.");

        public async Task<UserRecord> GetByIdAsync(string uid) => uid is not "" ? await _firebaseAuth.GetUserAsync(uid) : throw new ArgumentNullException(nameof(uid), "The user uid cannot be empty.");
    }

}
