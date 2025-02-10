using Newtonsoft.Json;

namespace DayPlanner.Abstractions.Models.Backend;

public class TokenResponse
{
    [JsonProperty("token")]
    public string Token { get; set; } = default!;

    [JsonProperty("refreshToken")]
    public string RefreshToken { get; set; } = default!;
}

