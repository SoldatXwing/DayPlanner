using FirebaseAdmin.Auth;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayPlanner.Abstractions.Services;

namespace DayPlanner.Authorization.Services
{
    public class AuthService : IAuthService
    {
        private readonly FirebaseAuth _firebaseAuth;

        public AuthService(FirebaseApp app)
        {
            if(app is null)
                throw new ArgumentNullException(nameof(app), "The Firebase app cannot be null.");
            
            _firebaseAuth = FirebaseAuth.GetAuth(app);
        }

        public async Task<FirebaseToken> VerifyTokenAsync(string idToken)
        {
            try
            {
                return await _firebaseAuth.VerifyIdTokenAsync(idToken);
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
                return await _firebaseAuth.GetUserAsync(uid);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve user with UID {uid}.", ex);
            }
        }

        public async Task<UserRecord> CreateUserAsync(UserRecordArgs args) => await _firebaseAuth.CreateUserAsync(args);
    }
}
