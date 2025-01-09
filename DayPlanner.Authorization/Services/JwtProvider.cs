using DayPlanner.Abstractions.Exceptions;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Services;
using DayPlanner.Authorization.Errors;
using DayPlanner.Authorization.Exceptions;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;

namespace DayPlanner.Authorization.Services
{
    /// <summary>
    /// JWT provider to get JWT token from Firebase.
    /// </summary>
    /// <param name="client"></param>
    public partial class JwtProvider(HttpClient client) : IJwtProvider
    {
        private readonly HttpClient _httpClient = client;
        /// <summary>
        /// Gets JWT token for the given email and password.
        /// </summary>
        /// <param name="email">Email of the user</param>
        /// <param name="password">Password of the User</param>
        /// <returns></returns>
        /// <exception cref="BadCredentialsException">Throws if invalid login credentials are passed</exception>
        /// <exception cref="InvalidEmailException">Throws if no account with passed email exists</exception>
        /// <exception cref="Exception">Standart exception</exception>
        public async Task<string> GetForCredentialsAsync(string email, string password)
        {
            var request = new
            {
                email,
                password,
                returnSecureToken = true
            };
            var response = await _httpClient.PostAsJsonAsync("", request);
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
            return authToken!.IdToken;
        }
    }
}