using AutoMapper;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Stores;
using DayPlanner.FireStore.Models;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;

namespace DayPlanner.FireStore;

public class FireStoreGoogleSyncTokenStore(FirestoreDb fireStoreDb,
    IUserStore userStore,
    IMapper mapper) : IGoogleSyncTokenStore
{
    private const string _collectionName = "googleSyncTokens";

    public async Task<string?> Get(string userId)
    {
        var docRef = fireStoreDb.Collection(_collectionName).Document(userId);
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
            _ = await userStore.GetByIdAsync(userId) ?? throw new ArgumentException($"Unable to find user with id '{userId}'."); ;
        }
        catch (FirebaseAuthException ex)
        {
            if (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
            {
                throw new ArgumentException($"Unable to find user with id '{userId}'.", ex);
            }

            throw;
        }

        DocumentReference refreshTokenRef = fireStoreDb.Collection(_collectionName).Document(userId);
        FirestoreGoogleSyncToken tokenModel = new() { UserId = userId, SyncToken = token };
        await refreshTokenRef.SetAsync(tokenModel);
    }

    public async Task Delete(string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);

        var docRef = fireStoreDb.Collection(_collectionName).Document(userId);
        await docRef.DeleteAsync();
    }

    public async Task<IEnumerable<GoogleSyncToken>> GetAll()
    {
        var docRefs = fireStoreDb.Collection(_collectionName).ListDocumentsAsync();
        var syncTokens = new List<GoogleSyncToken>();

        await foreach (var docRef in docRefs)
        {
            var snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                syncTokens.Add(mapper.Map<GoogleSyncToken>(snapshot.ConvertTo<FirestoreGoogleSyncToken>()));
            }
        }

        return syncTokens;
    }

}