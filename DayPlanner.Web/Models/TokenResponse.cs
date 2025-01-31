using Newtonsoft.Json;

namespace DayPlanner.Web.Models;

internal class TokenResponse
{
    [JsonProperty("token")]
    public string Token { get; set; } = default!;

    [JsonProperty("refreshToken")]
    public string RefreshToken { get; set; } = default!;
}

