using Asp.Versioning;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Stores;
using DayPlanner.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DayPlanner.Api.ApiControllers.V1
{
    /// <summary>
    /// Controller to access the AI
    /// </summary>
    /// <param name="logger"></param>
    [ApiController]
    [ApiVersion(1)]
    [Route("v{version:apiVersion}/ai")]
    public sealed partial class AiController(ILogger<AppointmentController> logger) : ControllerBase
    {
        /// <summary>
        /// Get the AI response for a given input
        /// </summary>
        /// <param name="input">The users input</param>
        /// <param name="start">Start date as context</param>
        /// <param name="end">End date as context</param>
        /// <param name="appointmentStore">Appointment store</param>
        /// <param name="userTimezone">Time zone of the user</param>
        /// <param name="chatClient">Chat client</param>
        /// <param name="cultureCode">Culture code of the user</param>
        /// <returns>A object of type <see cref="AppointmentSuggestion"/></returns>
        [HttpGet("suggestion")]
        [ProducesResponseType<AppointmentSuggestion>(200)]
        [ProducesResponseType<ApiErrorModel>(400)]
        public async Task<IActionResult> GetAiResponse([FromQuery] string input,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end,
            [FromServices] IAppointmentStore appointmentStore,
            [FromServices] IChatClient chatClient,
            [FromQuery] string userTimezone = "UTC",
            [FromQuery] string cultureCode = "en-en")
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                logger.LogWarning("Invalid input: Input is empty.");
                return BadRequest(new ApiErrorModel { Message = "Invalid input", Error = "Input cannot be empty." });
            }
            var userId = HttpContext.User.GetUserId()!;

            var existingAppointments = await appointmentStore.GetUsersAppointments(userId, start.ToUniversalTime(), end.ToUniversalTime());
            string existingAppointmentsJson = JsonSerializer.Serialize(existingAppointments);

            var prompt = $@"
    Current date range: from {start} to {end}

    User input: {input}

    User current appointments: {existingAppointmentsJson} if this string is empty, the user has no appointments.

    User timezone: Use {userTimezone} for all times in the response, unless otherwise specified in the input. If the user specifies a different timezone in the input (e.g., 'PST', 'CET', '+01:00'), prioritize that timezone for Start and End times.

    User culture: Use culture code: {cultureCode} for all text responses (e.g., Title, PromptMessage). Respond in the language and regional format of this culture. If the user’s input is in a different language, prioritize the language of the input; otherwise, use culture code{cultureCode}.

    You are an AI assistant helping with appointment scheduling. Your task is to generate a conflict-free appointment suggestion based on the user's input, considering their existing appointments. Return a JSON object with:
    - Title: A brief description of the appointment (string).
    - Start: Start time in ISO 8601 format with the user’s specified or assumed timezone (e.g., '2025-02-26T18:00:00-08:00' for PST, or '2025-02-26T18:00:00Z' for UTC).
    - End: End time in ISO 8601 format with the same timezone as Start.
    - PromptMessage: A message explaining the suggestion, any issues with the input, or confirmation of the appointment (string).

    Rules:
    1. If the input is a valid appointment request (e.g., 'dinner tomorrow'), provide a suggestion with reasonable assumptions:
        - Default duration: 1 hour unless the user specifies otherwise (e.g., 'dinner for 2 hours').
        - Default times: Use common sense based on the appointment type (e.g., dinner at 6 PM, meeting at 10 AM, lunch at 12 PM) in the user’s timezone.
        - Handle relative dates ('today', 'tomorrow', 'next week') based on the current date and time, using the user’s timezone or UTC if unspecified.
        - If the user specifies a timezone (e.g., 'dinner tomorrow at 6 PM PST' or 'dinner tomorrow CET'), use that timezone for Start and End, ensuring consistency with the PromptMessage.
    2. If the input is unclear or ambiguous (e.g., 'meeting'), set Title, Start, and End to null, and explain the issue in PromptMessage.
    3. If the input specifies a past date (e.g., 'dinner yesterday'), set Title to a suggestion, Start and End to null, and suggest a future date in PromptMessage, using the user’s timezone or UTC.
    4. If the input is not an appointment request (e.g., 'What’s the weather?'), set Title, Start, and End to null, and explain in PromptMessage.
    5. Ensure Start is before End and both are valid ISO 8601 strings with the correct timezone offset.

    Language Handling:
    - Detect the language of the user’s input (e.g., English, Spanish, French) and respond in the same language for the PromptMessage, Title, and Description. If the language is unclear or mixed, use the user’s specified culture ({cultureCode}).
    - Do not throw errors for non-{cultureCode} inputs; instead, process them naturally and respond appropriately in the detected language or {cultureCode} if detection fails.

    Return the response as a JSON string. Examples:

    Input (English, culture en-US, timezone UTC): 'dinner tomorrow'
    Response: {{
        'Title': 'Dinner',
        'Start': '2025-02-27T18:00:00Z',
        'End': '2025-02-27T19:00:00Z',
        'PromptMessage': 'I’ve scheduled a dinner appointment for you from 6 PM to 7 PM tomorrow (UTC).'
    }}

    Input (Spanish, culture es-ES, timezone CET): 'cena mañana a las 6 PM en CET'
    Response: {{
        'Title': 'Cena',
        'Start': '2025-02-27T18:00:00+01:00',
        'End': '2025-02-27T19:00:00+01:00',
        'PromptMessage': 'He programado una cita para la cena de 6 PM a 7 PM mañana (CET).'
    }}

    Input (French, culture fr-FR, timezone UTC): 'réunion aujourd’hui'
    Response: {{
        'Title': 'Réunion',
        'Start': '2025-02-26T10:00:00Z',
        'End': '2025-02-26T11:00:00Z',
        'PromptMessage': 'J’ai planifié une réunion de 10h00 à 11h00 aujourd’hui (UTC).'
    }}

    Input (English, culture en-US, timezone PST): 'meeting'
    Response: {{
        'Title': null,
        'Start': null,
        'End': null,
        'PromptMessage': 'Please specify the date and time for the meeting.'
    }}

    Input (Spanish, culture es-ES, timezone UTC): 'cena ayer'
    Response: {{
        'Title': 'Cena',
        'Start': null,
        'End': null,
        'PromptMessage': 'La fecha que especificaste está en el pasado. ¿Te gustaría programar la cena para una fecha futura?'
    }}

    Now, generate the appointment suggestion for the user input.
";
            try
            {
                var aiResponse = await chatClient.CompleteAsync(prompt);

                AppointmentSuggestion? suggestion = TryDeserializeDirectly(aiResponse.Message.Text!);

                suggestion ??= TryDeserializeFromJsonBlock(aiResponse.Message.Text!);

                if (suggestion == null)
                {
                    logger.LogWarning("AI returned an invalid or empty response for input: {Input}", input);
                    return BadRequest(new ApiErrorModel { Message = "Invalid AI response", Error = "AI response is empty or invalid." });
                }

                return Ok(suggestion);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize AI response for input: {Input}", input);
                return BadRequest(new ApiErrorModel { Message = "Invalid AI response", Error = "AI response is empty or invalid." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error processing input: {Input}", input);
                return BadRequest(new ApiErrorModel { Message = "Invalid AI response", Error = "AI response is empty or invalid." });
            }
        }
        private AppointmentSuggestion? TryDeserializeDirectly(string responseText)
        {
            try
            {
                return JsonSerializer.Deserialize<AppointmentSuggestion>(responseText);
            }
            catch (JsonException)
            {
                return null; 
            }
        }

        private AppointmentSuggestion? TryDeserializeFromJsonBlock(string responseText)
        {
            var match = AiJsonFormat().Match(responseText);

            if (!match.Success)
            {
                logger.LogWarning("AI response did not contain a valid JSON block: {Response}", responseText);
                return null;
            }
            try
            {
                string jsonBlock = match.Groups[1].Value;
                return JsonSerializer.Deserialize<AppointmentSuggestion>(jsonBlock);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize JSON block from AI response");
                return null;
            }
        }

        [GeneratedRegex(@"```json\s*(.*?)\s*```", RegexOptions.Singleline)]
        private static partial Regex AiJsonFormat();
    }
}
