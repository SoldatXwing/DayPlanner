using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Abstractions.Models.Backend
{
    [FirestoreData]
    public class Appointment
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
        public GeoLocation? Location { get; set; } = null;

        [FirestoreProperty("startDate")]
        public DateTime Start { get; set; }

        [FirestoreProperty("endDate")]
        public DateTime End { get; set; }

        [FirestoreProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    [FirestoreData]
    public class GeoLocation
    {
        [FirestoreProperty("Latitude")]
        public double Latitude { get; set; }

        [FirestoreProperty("Longitude")]
        public double Longitude { get; set; }
    }
}
