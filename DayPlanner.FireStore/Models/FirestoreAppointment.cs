using Google.Cloud.Firestore;

namespace DayPlanner.FireStore.Models;

[FirestoreData]
public class FirestoreAppointment
{
    [FirestoreDocumentId]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("userId")]
    public required string UserId { get; set; }

    [FirestoreProperty("title")]
    public string Title { get; set; } = string.Empty;

    [FirestoreProperty("summary")]
    public string Summary { get; set; } = string.Empty;

    [FirestoreProperty("location")]
    public string Location { get; set; } = string.Empty;

    [FirestoreProperty("startDate")]
    public DateTime Start { get; set; }

    [FirestoreProperty("endDate")]
    public DateTime End { get; set; }

    [FirestoreProperty("createdAt")]
    public DateTime CreatedAt { get; set; }
}
