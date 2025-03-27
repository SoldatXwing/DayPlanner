using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;

namespace DayPlanner.Authorization.Services
{
    /// <summary>
    /// Service to handle Google OAuth2 authentication.
    /// </summary>
    /// <param name="config"></param>
    public partial class GoogleOAuthService(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        private readonly IConfiguration Config = config;
        private readonly HttpClient _client = httpClientFactory.CreateClient("IdpClient");

        public string GenerateCalendarAuthUrl(string state)
        {
            string url = $"{Config["GoogleConfig:auth_url"]}" +
                         $"?client_id={Config["google_client_id"]}" +
                         $"&redirect_uri={Config["GoogleConfig:Calendar:redirect_uri"]}" +
                         $"&response_type=code" +
                         $"&scope=https://www.googleapis.com/auth/calendar.readonly" +
                         $"&access_type=offline" +
                         $"&state={state}";
            return url;
        }
        public string GenerateAccountAuthUrl(string os)
        {
            string url = $"{Config["GoogleConfig:auth_url"]}" +
                         $"?client_id={Config["google_client_id"]}" +
                         $"&redirect_uri={Config["GoogleConfig:AccountLogin:redirect_uri"]}" +
                         $"&response_type=code" +
                         $"&scope={Uri.EscapeDataString("email profile")}" +
                         $"&access_type=offline" +
                         $"&state={os}";
            return url;
        }
        /// <summary>
        /// Exchanges the authorization code for an access token from Google's OAuth2 endpoint.
        /// </summary>
        /// <param name="code">Code provided from google</param>
        /// <returns>The token</returns>
        public async Task<JObject?> AuthenticateCalendar(string code)
        {
            var redirectUri = Config["GoogleConfig:Calendar:redirect_uri"]!;
            return await ExchangeCodeForToken(code, redirectUri);
        }
        /// <summary>
        /// Exchanges the authorization code for an access token from Google's OAuth2 endpoint.
        /// </summary>
        /// <param name="code">Code provided from google</param>
        /// <returns>The token</returns>
        public async Task<JObject?> AuthenticateAccount(string code)
        {
            var redirectUri = Config["GoogleConfig:AccountLogin:redirect_uri"]!;
            return await ExchangeCodeForToken(code, redirectUri);
        }
        /// <summary>
        /// Exchanges the authorization code for an access token from Google's OAuth2 endpoint.
        /// </summary>
        /// <param name="code">The authorization code.</param>
        /// <param name="config">The configuration settings containing Google client details.</param>
        /// <returns>A JObject containing the access token and other response data.</returns>
        private async Task<JObject?> ExchangeCodeForToken(string code,
            string redirectUri)
        {
            using var client = new HttpClient();
            var request = new
            {
                code,
                client_id = Config["google_client_id"]!,
                client_secret = Config["google_client_secret"]!,
                redirect_uri = redirectUri,
                grant_type = "authorization_code"
            };

            var response = await client.PostAsJsonAsync(Config["GoogleConfig:TokenUri"], request);

            if (!response.IsSuccessStatusCode)
            {
                var error = JObject.Parse(await response.Content.ReadAsStringAsync());
                if (error["error_description"]!.ToString() == "Malformed auth code.")
                {
                    throw new InvalidOperationException("Invalid code provided");
                }
                throw new Exception("Error while exchanging code for token");
            }
            var content = JObject.Parse(await response.Content.ReadAsStringAsync());
            return content;
        }
        public async Task<JObject?> AuthenticateAccountWithFirebaseViaIdp(string requestUri, string idToken)
        {
            if (Uri.TryCreate(requestUri, UriKind.Absolute, out _))
            {
                var request = new
                {
                    postBody = $"id_token={idToken}&providerId=google.com",
                    requestUri,
                    returnSecureToken = true
                };
                var response = await _client.PostAsJsonAsync("", request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = JObject.Parse(await response.Content.ReadAsStringAsync());
                    if (error["error"]!.ToString() == "INVALID_IDP_RESPONSE")
                    {
                        throw new InvalidOperationException("Invalid IDP response");
                    }
                    throw new Exception("Error while exchanging code for token");
                }
                var content = JObject.Parse(await response.Content.ReadAsStringAsync());
                return content;
            }
            else
            {
                throw new InvalidOperationException("Invalid request URI");
            }
        }
    }
}