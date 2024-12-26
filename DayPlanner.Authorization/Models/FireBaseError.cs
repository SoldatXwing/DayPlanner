using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Authorization.Models
{
    internal class FireBaseError
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("errors")]
        public List<FirebaseErrorDetail> Errors { get; set; }
    }
    public class FirebaseErrorDetail
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }
}
