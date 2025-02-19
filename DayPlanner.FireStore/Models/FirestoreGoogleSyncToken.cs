using Google.Cloud.Firestore;

namespace DayPlanner.FireStore.Models;

[FirestoreData]
public class FirestoreGoogleSyncToken
{
    [FirestoreProperty("userId")]
    public required string UserId { get; set; }

    [FirestoreProperty("syncToken")]
    public required string SyncToken { get; set; }
}
