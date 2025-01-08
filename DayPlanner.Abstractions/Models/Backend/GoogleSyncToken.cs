using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Abstractions.Models.Backend
{
    [FirestoreData]
    public class GoogleSyncToken
    {
        [FirestoreProperty("userId")]
        public required string UserId { get; set; }
        [FirestoreProperty("syncToken")]
        public required string SyncToken { get; set; }
    }
}
