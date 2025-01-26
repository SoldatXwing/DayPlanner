using DayPlanner.Abstractions.Exceptions;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Services;
using DayPlanner.Authorization.Errors;
using DayPlanner.Authorization.Exceptions;
using Google.Apis.Http;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using Microsoft.Extensions.Http;

namespace DayPlanner.Authorization.Services
{
    /// <summary>
    /// JWT provider to get JWT token from Firebase.
    /// </summary>
    public partial class JwtProvider(System.Net.Http.IHttpClientFactory httpClientFactory) : IJwtProvider
    {
        private readonly HttpClient _authClient = httpClientFactory.CreateClient("AuthTokenClient");
        private readonly HttpClient _refreshClient = httpClientFactory.CreateClient("RefreshTokenClient");

        /// <summary>
        /// Gets JWT token for the given email and password.
        /// </summary>
        /// <param name="email">Email of the user</param>
        /// <param name="password">Password of the User</param>
        /// <returns></returns>
        /// <exception cref="BadCredentialsException">Throws if invalid login credentials are passed</exception>
        /// <exception cref="InvalidEmailException">Throws if no account with passed email exists</exception>
        /// <exception cref="Exception">Standart exception</exception>
        public async Task<(string token, string refreshToken)> GetForCredentialsAsync(string email, string password)
        {
            var request = new
            {
                email,
                password,
                returnSecureToken = true
            };
            var response = await _authClient.PostAsJsonAsync("", request);
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                FireBaseError? error = JObject.Parse(await response.Content.ReadAsStringAsync())["error"]!.ToObject<FireBaseError>();

                if (error != null)
                {
                    if (error.Message == "INVALID_LOGIN_CREDENTIALS")
                    {
                        throw new BadCredentialsException(error.Message);
                    }
                    else if (error.Message == "INVALID_EMAIL")
                    {
                        throw new InvalidEmailException(error.Message);
                    }
                    else
                    {
                        throw new Exception(error.Message);
                    }
                }
                else
                {
                    throw new Exception("Unknown error occurred while processing the request.");
                }
            }
            var authToken = await response.Content.ReadFromJsonAsync<AuthToken>();
            return (authToken!.IdToken, authToken.RefreshToken);
        }
        /// <summary>
        /// Refreshes the current login token.
        /// </summary>
        /// <param name="refreshToken">Refresh token of the user</param>
        /// <returns>Renewed token</returns>
        /// <exception cref="BadCredentialsException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<string> RefreshIdTokenAsync(string refreshToken)
        {
            var request = new
            {
                grant_type = "refresh_token",
                refresh_token = refreshToken
            };
            var response = await _refreshClient.PostAsJsonAsync("", request);
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                FireBaseError? error = JObject.Parse(await response.Content.ReadAsStringAsync())["error"]!.ToObject<FireBaseError>();

                if (error is not null)
                {
                    if (error.Message == "INVALID_REFRESH_TOKEN")
                        throw new BadCredentialsException(error.Message);
                    else
                        throw new Exception(error.Message);
                }
                else
                    throw new Exception("Unknown error occurred while processing the refresh token.");
            }
            string accessToken = JObject.Parse(await response.Content.ReadAsStringAsync())["access_token"]!.ToString();
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new Exception("Failed to retrieve a valid ID token from the refresh token response.");
            }

            return accessToken;
        }
    }
}