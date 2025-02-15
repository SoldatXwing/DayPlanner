using Asp.Versioning;
using DayPlanner.Abstractions.Exceptions;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Services;
using DayPlanner.Abstractions.Stores;
using DayPlanner.Api.Extensions;
using DayPlanner.Authorization.Services;
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
        /// <param name="googleOAuthService">Service to interact with google OAuth</param>
        /// <response code="200">Success - Returns the authorization url for the user</response>
        [HttpGet("login")]
        [ProducesResponseType<string>(200)]
        [Authorize]
        public IActionResult Login([FromServices] GoogleOAuthService googleOAuthService)
        {
            var userId = HttpContext.User.GetUserId()!;
            var authUrl = googleOAuthService.GenerateCalendarAuthUrl(userId); //Provide user id as state
            return Ok(authUrl);

        }/// <summary>
         /// Callback endpoint for handling the OAuth2 response from Google.
         /// </summary>
         /// <param name="code">The authorization code received from Google.</param>
         /// <param name="state">The user ID of the user who initiated the login.</param>
         /// <param name="googleOAuthService">Service to interact with google OAuth</param>
         /// <param name="googleRefreshTokenStore">The service used to store the refresh token for the user.</param>
         /// <returns>The access token if successful, or an error message if unsuccessful.</returns>
        [HttpGet("callback")]
        [ProducesResponseType<ApiErrorModel>(400)]
        [ProducesResponseType<ApiErrorModel>(404)]
        [AllowAnonymous] // Reason: Google sends the request to this endpoint, not the user
        public async Task<IActionResult> Callback([FromQuery] string code,
            [FromQuery] string state, //state is the user id
            [FromServices] GoogleOAuthService googleOAuthService,
            [FromServices] IGoogleRefreshTokenStore googleRefreshTokenStore)
        {
            if (string.IsNullOrEmpty(code))
            {
                _Logger.LogWarning("Error recieving callback: Code is null or empty");
                return BadRequest(new ApiErrorModel { Message = "Error recieving callback", Error = "Code is null or empty" });
            }
            try
            {
                var tokenResponse = await googleOAuthService.AuthenticateCalendar(code);
                //First time login
                if (tokenResponse!.TryGetValue("refresh_token", out var refreshToken) &&
                    !string.IsNullOrEmpty(refreshToken?.ToString()))
                {
                    if (string.IsNullOrEmpty(state))
                    {
                        _Logger.LogWarning("State (userId) is null or empty");
                        ArgumentException.ThrowIfNullOrEmpty(state);
                    }
                    await googleRefreshTokenStore.Create(state, refreshToken.ToString());
                }

                return NoContent();
            }

            catch (InvalidOperationException ex)
            {
                _Logger.LogWarning(ex, "Invalid Google callback code provided");
                return BadRequest(new ApiErrorModel { Message = "Invalid code", Error = ex.Message });
            }
            catch (BadCredentialsException ex)
            {
                _Logger.LogWarning(ex, "User not found for id: {UserId}", state);
                return NotFound(new ApiErrorModel { Message = "User not found", Error = ex.Message });
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Unexpected error while exchanging code for token");
                return BadRequest(new ApiErrorModel { Message = "Error while exchanging code for token", Error = ex.Message });
            }

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
        public async Task<IActionResult> SyncAppointments([FromServices] GoogleCalendarService googleCalendarService)
        {
            var userId = HttpContext.User.GetUserId()!;
            try
            {
                await googleCalendarService.SyncAppointments(userId);
                _Logger.LogInformation("Google appointments synchronized for user with uid {UserId}", userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                _Logger.LogWarning("Unauthorized access attempt by user with uid {UserId}", userId);
                return Forbid();
            }
            catch (InvalidOperationException)
            {
                _Logger.LogWarning("Error recieving refresh token for user with id {UserId}", userId);
                return Forbid();
            }
        }
        /// <summary>
        /// Disconnects the Google account from the user account.
        /// </summary>
        /// <param name="googleCalendarService">Service to unsync the user</param>
        /// <param name="deleteImportedAppointments">Indicates if imported appointments should be removed</param>
        /// <returns></returns>
        [HttpPost("disconnect")]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> DisconnectGoogleAccount(
            [FromServices] GoogleCalendarService googleCalendarService,
            [FromQuery] bool deleteImportedAppointments)
        {

            var userId = HttpContext.User.GetUserId()!;
            try
            {
                await googleCalendarService.UnSync(userId, deleteImportedAppointments);

                _Logger.LogInformation("Google account disconnected for user with ID {UserId}.", userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                _Logger.LogWarning("Unauthorized access attempt by user with uid {UserId}", userId);
                return Forbid();
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error disconnecting Google account for user with ID {UserId}.", userId);
                return Forbid();
            }
        }
    }
}
