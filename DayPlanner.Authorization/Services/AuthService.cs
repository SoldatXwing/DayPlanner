using FirebaseAdmin.Auth;
using FirebaseAdmin;
using DayPlanner.Abstractions.Services;

namespace DayPlanner.Authorization.Services
{
    public class AuthService(FirebaseApp app) : IAuthService
    {
        private readonly FirebaseAuth _firebaseAuth = FirebaseAuth.GetAuth(app);

        public async Task<FirebaseToken> VerifyTokenAsync(string idToken)
        {
            try
            {
                return await _firebaseAuth.VerifyIdTokenAsync(idToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException("Invalid Firebase ID Token.", ex);
            }
        }

        public async Task<UserRecord> GetUserByIdAsync(string uid)
        {
            try
            {              
                return await _firebaseAuth.GetUserAsync(uid).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve user with UID {uid}.", ex);
            }
        }

        public async Task<UserRecord> CreateUserAsync(UserRecordArgs args) => await _firebaseAuth.CreateUserAsync(args).ConfigureAwait(false);
    }
}
