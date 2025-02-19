using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Services;
using DayPlanner.Abstractions.Stores;
using DayPlanner.FireStore.Models;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;

namespace DayPlanner.FireStore;

public class FireStoreGoogeRefreshTokenStore(FirestoreDb fireStoreDb, IUserStore userStore) : IGoogleRefreshTokenStore
{

    private const string _collectionName = "googleRefreshTokens";

    public async Task<GoogleRefreshToken?> Get(string userId)
    {
        var docRef = fireStoreDb.Collection(_collectionName).Document(userId);
        var snapshot = await docRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            FirestoreGoogleRefreshToken token = snapshot.ConvertTo<FirestoreGoogleRefreshToken>();
            return new() { UserId = token.UserId, RefreshToken = token.RefreshToken };
        }

        return null;
    }

    public async Task<GoogleRefreshToken> Create(string userId, string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentException.ThrowIfNullOrEmpty(token);

        try
        {
            _ = await userStore.GetByIdAsync(userId) ?? throw new ArgumentException($"Unable to find user with id '{userId}'.");
        }
        catch (FirebaseAuthException ex)
        {
            if (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
            {
                throw new ArgumentException($"Unable to find user with id '{userId}'.", ex);
            }

        }

        DocumentReference refreshTokenRef = fireStoreDb.Collection(_collectionName).Document(userId);
        FirestoreGoogleRefreshToken refreshToken = new() { UserId = userId, RefreshToken = token };
        await refreshTokenRef.SetAsync(refreshToken);

        return new() { UserId = refreshToken.UserId, RefreshToken = refreshToken.RefreshToken };
    }

    public async Task Delete(string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);

        var docRef = fireStoreDb.Collection(_collectionName).Document(userId);
        await docRef.DeleteAsync();
    }
}