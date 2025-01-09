using Google.Cloud.Firestore;

namespace DayPlanner.Abstractions.Models.Backend
{
    [FirestoreData]
    public class FirestoreGoogleRefreshToken
    {
        [FirestoreProperty("userId")]
        public required string UserId { get; set; }

        [FirestoreProperty("refreshToken")]
        public required string RefreshToken { get; set; }
    }
}
