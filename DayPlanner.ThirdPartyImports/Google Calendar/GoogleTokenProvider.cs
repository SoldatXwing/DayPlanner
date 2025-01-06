using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Services;
using Google.Apis.Auth.OAuth2.Responses;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace DayPlanner.ThirdPartyImports.Google_Calendar
{
    /// <summary>
    /// Provider for retrieving and refreshing tokens from Google.
    /// </summary>
    /// <param name="googleRefreshTokenService">The service used to retrieve stored refresh tokens for users.</param>
    /// <param name="client">The <see cref="HttpClient"/> instance used to make HTTP requests to Google's token endpoint.</param>
    /// <param name="clientId">The client ID used for authenticating with Google's API.</param>
    /// <param name="clientSecret">The client secret used for authenticating with Google's API.</param>
    public class GoogleTokenProvider(IGoogleRefreshTokenService googleRefreshTokenService, HttpClient client,
        string clientId,
        string clientSecret) : IGoogleTokenProvider
    {
        private HttpClient _httpClient = client;
        /// <summary>
        /// Retrieves a new access token from Google, valid for 1 hour, for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user for whom the token is being retrieved or refreshed.</param>
        /// <returns>
        /// A <see cref="GoogleTokenResponse"/> object containing the access token and its metadata,
        /// or <c>null</c> if the token cannot be retrieved.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="userId"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the token retrieval fails or if there is an error during the refresh process.
        /// </exception>
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
        /// <summary>
        /// Validates if the given Token is valid via fetching google api
        /// </summary>
        /// <param name="token">Token to be checked</param>
        /// <returns>True if valid, false if invalid</returns>
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
