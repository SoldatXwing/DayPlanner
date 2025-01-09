using Newtonsoft.Json;

namespace DayPlanner.Authorization.Errors;

public class FireBaseError
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("message")]
    public required string Message { get; set; }

    [JsonProperty("errors")]
    public List<FireBaseErrorDetail>? Errors { get; set; }
}
