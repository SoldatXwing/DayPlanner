using Asp.Versioning;
using DayPlanner.Abstractions.Exceptions;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Services;
using DayPlanner.Api.Extensions;
using DayPlanner.ThirdPartyImports.Google_Calendar;
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
    public class GoogleCalendarController(ILogger<GoogleCalendarController> logger) : ControllerBase
    {
        private ILogger<GoogleCalendarController> _Logger { get; } = logger;

        /// <summary>
        /// Redirects to Google OAuth2 login
        /// </summary>
        /// <param name="config"></param>
        /// <response code="200">Success - Returns the authorization url for the user</response>
        [HttpGet("login")]
        [ProducesResponseType<string>(200)]
        [Authorize]
        public IActionResult Login([FromServices] IConfiguration config)
        {
            var userId = HttpContext.User.GetUserId()!;
            var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth" +
                          $"?client_id={config["GoogleCalendar:client_Id"]}" +
                          $"&redirect_uri={config["GoogleCalendar:redirect_uri"]}" +
                          $"&response_type=code" +
                          $"&scope=https://www.googleapis.com/auth/calendar.readonly" +
                          $"&access_type=offline" +
                          $"&state={userId}";
            return Ok(authUrl);

        }/// <summary>
         /// Callback endpoint for handling the OAuth2 response from Google.
         /// </summary>
         /// <param name="code">The authorization code received from Google.</param>
         /// <param name="state">The user ID of the user who initiated the login.</param>
         /// <param name="config">The configuration settings containing Google client details.</param>
         /// <param name="googleRefreshTokenService">The service used to store the refresh token for the user.</param>
         /// <returns>The access token if successful, or an error message if unsuccessful.</returns>
        [HttpGet("callback")]
        [ProducesResponseType<ApiErrorModel>(400)]
        [ProducesResponseType<ApiErrorModel>(404)]
        [AllowAnonymous] // Reason: Google sends the request to this endpoint, not the user
        public async Task<IActionResult> Callback([FromQuery] string code,
            [FromQuery] string state, //state is the user id
            [FromServices] IConfiguration config,
            [FromServices] IGoogleTokenService googleRefreshTokenService)
        {
            if (string.IsNullOrEmpty(code))
            {
                _Logger.LogWarning("Error recieving callback: Code is null or empty");
                return BadRequest(new ApiErrorModel { Message = "Error recieving callback", Error = "Code is null or empty" });
            }
            try
            {
                var tokenResponse = await ExchangeCodeForToken(code, config);
                //First time login
                if (tokenResponse!.TryGetValue("refresh_token", out var refreshToken) &&
                    !string.IsNullOrEmpty(refreshToken?.ToString()))
                {
                    if (string.IsNullOrEmpty(state))
                    {
                        _Logger.LogWarning("State (userId) is null or empty");
                        ArgumentException.ThrowIfNullOrEmpty(state);
                    }
                    await googleRefreshTokenService.CreateRefreshToken(state, refreshToken.ToString());
                }

                return Ok(new { Token = tokenResponse["access_token"]!.ToString(), ExpiresIn = tokenResponse["expires_in"]!.ToString() });
            }

            catch (InvalidOperationException ex)
            {
                _Logger.LogWarning("Invalid google callback code provided");
                return BadRequest(new ApiErrorModel { Message = "Invalid code", Error = ex.Message });
            }
            catch (BadCredentialsException ex)
            {
                _Logger.LogWarning($"User not with id: {state} not found");
                return NotFound(new ApiErrorModel { Message = "User not found", Error = ex.Message });
            }
            catch (Exception ex)
            {
                _Logger.LogWarning($"Error while exchanging code for token. Ex: {ex.Message}");
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

        /// <summary>Synchronizes appointments from Google Calendar for the authenticated user.</summary>
        /// <param name="googleCalendarService">
        /// The service that interacts with the Google Calendar API to sync appointments.
        /// </param>
        /// <param name="tokenService">The google token service to use.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains an HTTP response:
        /// - 200 OK with the next sync token if the sync was successful.
        /// - 403 Forbidden if the user is not authorized or if no refresh token is found for the user.
        /// </returns>
        /// <response code="204">
        /// The synchronization were successfully.
        /// </response>
        /// <response code="403">
        /// Returns a Forbidden status if the user is not authorized or a required token is missing.
        /// </response>
        [HttpPost("sync")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> SyncAppointments([FromServices] GoogleCalendarService googleCalendarService, [FromServices] IGoogleTokenService tokenService)
        { 
            var userId = HttpContext.User.GetUserId()!;
            try
            {
                await googleCalendarService.SyncAppointments(userId);
                _Logger.LogInformation($"Google appointments synchronized for user with uid {userId}");
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                _Logger.LogInformation($"Unauthorized access attempt by user with uid {userId}");
                return Forbid();
            }
        }
    }
}
