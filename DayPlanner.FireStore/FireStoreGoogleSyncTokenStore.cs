using DayPlanner.Abstractions.Stores;
using FirebaseAdmin.Auth;
using FirebaseAdmin;
using Google.Cloud.Firestore;
using DayPlanner.FireStore.Models;

namespace DayPlanner.FireStore;

public class FireStoreGoogleSyncTokenStore(FirestoreDb db, FirebaseApp app) : IGoogleSyncTokenStore
{
    private readonly FirebaseAuth _firebaseAuth = FirebaseAuth.GetAuth(app) ?? throw new ArgumentNullException(nameof(app), "The Firebase app cannot be null.");
    private readonly FirestoreDb _fireStoreDb = db;

    private const string _collectionName = "googleSyncTokens";

    public async Task<string?> Get(string userId)
    {
        var docRef = _fireStoreDb.Collection(_collectionName).Document(userId);
        var snapshot = await docRef.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            return snapshot.ConvertTo<FirestoreGoogleSyncToken>().SyncToken;
        }

        return null;
    }

    public async Task Save(string userId, string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentException.ThrowIfNullOrEmpty(token);

        try
        {
            _ = await _firebaseAuth.GetUserAsync(userId);
        }
        catch (FirebaseAuthException ex)
        {
            if (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
            {
                throw new ArgumentException($"Unable to find user with id '{userId}'.", ex);
            }

            throw;
        }

        DocumentReference refreshTokenRef = _fireStoreDb.Collection(_collectionName).Document(userId);
        FirestoreGoogleSyncToken tokenModel = new() { UserId = userId, SyncToken = token };
        await refreshTokenRef.SetAsync(tokenModel);
    }

    public async Task Delete(string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);

        var docRef = _fireStoreDb.Collection(_collectionName).Document(userId);
        await docRef.DeleteAsync();
    }
}