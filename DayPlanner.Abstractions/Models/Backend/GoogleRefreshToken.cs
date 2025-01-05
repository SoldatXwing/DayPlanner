using Google.Cloud.Firestore;

namespace DayPlanner.Abstractions.Models.Backend
{
    [FirestoreData]
    public class GoogleRefreshToken
    {
        [FirestoreDocumentId]
        [FirestoreProperty("userId")]
        public required string UserId { get; set; }
        [FirestoreProperty("token")]
        public required string RefreshToken { get; set; }

    }
}
