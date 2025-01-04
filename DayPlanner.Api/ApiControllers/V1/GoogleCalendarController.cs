using Asp.Versioning;
using DayPlanner.Abstractions.Models.Backend;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
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
        /// <returns></returns>
        [HttpGet("login")]
        [ProducesResponseType(302)]
        [AllowAnonymous]
        public IActionResult Login([FromServices] IConfiguration config)
        {

            var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth" +
                          $"?client_id={config["GoogleCalendar:client_Id"]}" +
                          $"&redirect_uri={config["GoogleCalendar:redirect_uri"]}" +
                          $"&response_type=code" +
                          $"&scope=https://www.googleapis.com/auth/calendar.readonly" +
                          $"&access_type=offline";
            return Redirect(authUrl);

        }
        [HttpGet("callback")]
        [ProducesResponseType<ApiErrorModel>(400)]
        [AllowAnonymous]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromServices] IConfiguration config)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(new ApiErrorModel { Message = "Error recieving callback", Error = "Code is null or empty" });
            }
            try
            {
                var tokenResponse = await ExchangeCodeForToken(code, config);

                var accessToken = tokenResponse["access_token"]!.ToString();
                return Ok(new { Token = accessToken });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorModel { Message = "Invalid code", Error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiErrorModel { Message = "Error while exchanging code for token", Error = ex.Message });
            }

        }
        private static async Task<JObject?> ExchangeCodeForToken(string code, IConfiguration config)
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

            return JObject.Parse(await response.Content.ReadAsStringAsync());
        }
    }
}
