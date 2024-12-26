using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Abstractions.Models.Backend
{
    public class FireBaseError
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public required string Message { get; set; }

        [JsonProperty("errors")]
        public List<FirebaseErrorDetail>? Errors { get; set; }
    }
    public class FirebaseErrorDetail
    {
        [JsonProperty("message")]
        public required string Message { get; set; }

        [JsonProperty("domain")]
        public required string Domain { get; set; }

        [JsonProperty("reason")]
        public required string Reason { get; set; }
    }
}
