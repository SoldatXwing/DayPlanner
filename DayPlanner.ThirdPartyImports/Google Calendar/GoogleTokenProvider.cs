using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Services;
using Google.Apis.Auth.OAuth2.Responses;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace DayPlanner.ThirdPartyImports.Google_Calendar
{
    public class GoogleTokenProvider(IGoogleRefreshTokenService googleRefreshTokenService, HttpClient client,
        string clientId,
        string clientSecret) : IGoogleTokenProvider
    {
        private HttpClient _httpClient = client;
        public async Task<GoogleTokenResponse?> GetOrRefresh(string userId)
        {
            ArgumentException.ThrowIfNullOrEmpty(userId);
            var refreshToken = await googleRefreshTokenService.Get(userId) ?? throw new InvalidOperationException($"Error recieving refresh token with userid: {userId}");

            var request = new
            {
                grant_type = "refresh_token",
                refresh_token = refreshToken.RefreshToken,
                client_id = clientId,
                client_secret = clientSecret
            };

            var response = await _httpClient.PostAsJsonAsync("", request);
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Error fetching refresh token for user with id: {userId}");

            var tokenData = JsonConvert.DeserializeObject<GoogleTokenResponse>(await response.Content.ReadAsStringAsync());
            return tokenData ?? throw new InvalidOperationException("Failed to renew access token");
        }

        public async Task<bool> IsTokenValid(GoogleTokenResponse token)
        {
            if (token is null)
                return false;

            if (token.ExpiresIn <= 0) 
                return false;

            try
            {
                var request = new
                {
                    grant_type = "authorization_code",
                    access_token = token.AccessToken,
                };
                var response = await _httpClient.PostAsJsonAsync("", request);

                if (response.IsSuccessStatusCode)
                    return true; // Token is valid

            }
            catch
            {
                return false;
            }

            return false; 
        }


    }
}
