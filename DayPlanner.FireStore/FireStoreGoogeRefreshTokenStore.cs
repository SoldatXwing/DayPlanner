using DayPlanner.Abstractions.Exceptions;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Stores;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.FireStore
{
    public class FireStoreGoogeRefreshTokenStore(FirestoreDb db, FirebaseApp app) : IGoogleRefreshTokenStore
    {
        private readonly FirestoreDb _fireStoreDb = db;
        private readonly FirebaseAuth _firebaseAuth = FirebaseAuth.GetAuth(app) ?? throw new ArgumentNullException(nameof(app), "The Firebase app cannot be null.");
        public async Task<GoogleRefreshToken> Create(string userId, string token)
        {
            ArgumentException.ThrowIfNullOrEmpty(userId);
            ArgumentException.ThrowIfNullOrEmpty(token);
            try
            {
                
                UserRecord _ = await _firebaseAuth.GetUserAsync(userId);
            }
            catch (FirebaseAuthException ex)
            {
                if (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
                {
                    throw new BadCredentialsException($"No user found with ID {userId}");
                }
                throw; 
            }
            DocumentReference refreshTokenRef = _fireStoreDb.Collection("googleRefreshTokens").Document();
            var request = new
            {
                userId,
                refreshToken = token
            };
            await refreshTokenRef.SetAsync(request);
            return new() { RefreshToken = token, UserId =  refreshTokenRef.Id};
        }

        public async Task Delete(string userId)
        {
            ArgumentException.ThrowIfNullOrEmpty(userId);
            var docRef = _fireStoreDb.Collection("googleRefreshTokens").Document(userId);
            await docRef.DeleteAsync();
        }

        public async Task<GoogleRefreshToken?> Get(string userId)
        {
            var docRef = _fireStoreDb.Collection("googleRefreshTokens").Document(userId);
            var snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<GoogleRefreshToken>();
            }

            return null;
        }
    }
}
