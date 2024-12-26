using Newtonsoft.Json;

namespace DayPlanner.Abstractions.Models.Backend
{
    public class AuthToken
    {
        [JsonProperty("kind")]
        public required string Kind { get; set; }
        [JsonProperty("localId")]
        public required string LocalId { get; set; }
        [JsonProperty("email")]
        public required string Email { get; set; }
        [JsonProperty("displayName")]
        public required string DisplayName { get; set; }
        [JsonProperty("idToken")]
        public required string IdToken { get; set; }
        [JsonProperty("registered")]
        public bool Registered { get; set; }
        [JsonProperty("refreshToken")]
        public required string RefreshToken { get; set; }
        [JsonProperty("expiresIn")]
        public long ExpiresIn { get; set; }
    }
}