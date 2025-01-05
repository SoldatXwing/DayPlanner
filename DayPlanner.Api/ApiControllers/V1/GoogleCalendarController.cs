using Asp.Versioning;
using DayPlanner.Abstractions.Exceptions;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace DayPlanner.Api.ApiControllers.V1
{
    /// <summary>
    /// Manages Google Calendar API
    /// </summary>
    [ApiController]
    [ApiVersion(1)]
    [Route("v{version:apiVersion}/googlecalendar")]
    public class GoogleCalendarController : Controller
    {
        /// <summary>
        /// Redirects to Google OAuth2 login
        /// </summary>
        /// <param name="config"></param>
        /// <returns>The url to Google OAuth2 login</returns>
        [HttpGet("login")]
        [ProducesResponseType(302)]
        [Authorize]
        public IActionResult Login([FromServices] IConfiguration config)
        {
            var userId = HttpContext.User.Claims.Single(c => c.Type == "user_id").Value; //TODO: Implement extension method for userid
            var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth" +
                          $"?client_id={config["GoogleCalendar:client_Id"]}" +
                          $"&redirect_uri={config["GoogleCalendar:redirect_uri"]}" +
                          $"&response_type=code" +
                          $"&scope=email%20https://www.googleapis.com/auth/calendar.readonly" +
                          $"&access_type=offline" +
                          $"&state={userId}";
            return Redirect(authUrl);

        }/// <summary>
         /// Callback endpoint for handling the OAuth2 response from Google.
         /// </summary>
         /// <param name="code">The authorization code received from Google.</param>
         /// <param name="state">The user ID of the user who initiated the login.</param>
         /// <param name="config">The configuration settings containing Google client details.</param>
         /// <returns>The access token if successful, or an error message if unsuccessful.</returns>
        [HttpGet("callback")]
        [ProducesResponseType<ApiErrorModel>(400)]
        [ProducesResponseType<ApiErrorModel>(404)]
        [AllowAnonymous] // Reason: Google sends the request to this endpoint, not the user
        public async Task<IActionResult> Callback([FromQuery] string code,
            [FromQuery] string state, //state is the user id
            [FromServices] IConfiguration config,
            [FromServices] IGoogleRefreshTokenService googleRefreshTokenService)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(new ApiErrorModel { Message = "Error recieving callback", Error = "Code is null or empty" });
            }
            try
            {
                var tokenResponse = await ExchangeCodeForToken(code, config);
                //First time login
                if (tokenResponse!.TryGetValue("refresh_token", out var refreshToken) &&
                    !string.IsNullOrEmpty(refreshToken?.ToString()))
                {
                    ArgumentException.ThrowIfNullOrEmpty(state);
                    await googleRefreshTokenService.Create(state, refreshToken.ToString());
                }

                return Ok(new { Token = tokenResponse["access_token"]!.ToString(), ExpiresIn = tokenResponse["expires_in"]!.ToString() });
            }

            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorModel { Message = "Invalid code", Error = ex.Message });
            }
            catch (BadCredentialsException ex)
            {
                return NotFound(new ApiErrorModel { Message = "User not found", Error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiErrorModel { Message = "Error while exchanging code for token", Error = ex.Message });
            }

        }
        /// <summary>
        /// Exchanges the authorization code for an access token from Google's OAuth2 endpoint.
        /// </summary>
        /// <param name="code">The authorization code.</param>
        /// <param name="config">The configuration settings containing Google client details.</param>
        /// <returns>A JObject containing the access token and other response data.</returns>
        private static async Task<JObject?> ExchangeCodeForToken(string code,
            IConfiguration config)
        {
            using var client = new HttpClient();
            var request = new
            {
                code,
                client_id = config["GoogleCalendar:client_Id"]!,
                client_secret = config["GoogleCalendar:client_Secret"]!,
                redirect_uri = config["GoogleCalendar:redirect_uri"]!,
                grant_type = "authorization_code"
            };

            var response = await client.PostAsJsonAsync("https://oauth2.googleapis.com/token", request);

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
    }
}
