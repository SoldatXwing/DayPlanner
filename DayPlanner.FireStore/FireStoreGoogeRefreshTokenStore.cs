using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Stores;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.FireStore
{
    public class FireStoreGoogeRefreshTokenStore(FirestoreDb db) : IGoogleRefreshTokenStore
    {
        private readonly FirestoreDb _fireStoreDb = db;
        public async Task<GoogleRefreshToken> Create(string userId, string token)
        {
            ArgumentException.ThrowIfNullOrEmpty(userId);
            ArgumentException.ThrowIfNullOrEmpty(token);
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
