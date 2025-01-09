using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayPlanner.Authorization.Errors;

public class FireBaseErrorDetail
{
    [JsonProperty("message")]
    public required string Message { get; set; }

    [JsonProperty("domain")]
    public required string Domain { get; set; }

    [JsonProperty("reason")]
    public required string Reason { get; set; }
}