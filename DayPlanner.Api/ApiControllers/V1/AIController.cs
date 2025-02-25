using Asp.Versioning;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Stores;
using DayPlanner.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
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
    public sealed class AiController(
        ILogger<AppointmentController> logger) : ControllerBase
    {
        [HttpGet("suggestion")]
        public async Task<IActionResult> GetAiResponse([FromQuery] string input, [FromQuery] DateTime start, [FromQuery] DateTime end, [FromServices] IChatClient chatClient, [FromServices] IAppointmentStore appointmentStore)
        {
            if(string.IsNullOrWhiteSpace(input))
            {
                logger.LogWarning("Invalid input: Input is empty.");
                return BadRequest(new ApiErrorModel { Message = "Invalid input", Error = "Input cannot be empty." });
            }
            var userId = HttpContext.User.GetUserId()!;
            var existingAppointments = await appointmentStore.GetUsersAppointments(userId, start, end);
            string existingAppointmentsJson = JsonSerializer.Serialize(existingAppointments);

            var prompt = $@"
                Current date range: from {start} to {end}

                User input: {input}

                User current appointments: {existingAppointmentsJson} if this string is empty, the user has no appointments.


                You are an AI assistant helping with appointment scheduling. Your task is to generate a conflict-free appointment suggestion based on the user's input, considering their existing appointments. Return a JSON object with:
                - Title: A brief description of the appointment (string).
                - Start: Start time in ISO 8601 format with time zone (e.g., '2025-02-26T18:00:00-08:00').
                - End: End time in ISO 8601 format with time zone.
                - Prompt: A message explaining the suggestion or any issues with the input (string).

                Rules:
                1. If the input is a valid appointment request (e.g., 'dinner tomorrow'), provide a suggestion with reasonable assumptions:
                    - Default duration: 1 hour unless specified.
                    - Default times: Use common sense (e.g., dinner at 6 PM).
                    - Handle relative dates ('today', 'tomorrow', 'next week') based on the current date and time.
                2. If the input is unclear or ambiguous (e.g., 'meeting'), set Title, Start, and End to null, and explain the issue in Prompt.
                3. If the input specifies a past date (e.g., 'dinner yesterday'), set Title to a suggestion, Start and End to null, and suggest a future date in Prompt.
                4. If the input is not an appointment request (e.g., 'What's the weather?'), set Title, Start, and End to null, and explain in Prompt.
                5. Ensure Start is before End and both are valid ISO 8601 strings.

                Return the response as a JSON string. Examples:

                Input: 'dinner tomorrow'
                Response: {{
                    'Title': 'Dinner',
                    'Start': '2025-02-26T18:00:00-08:00',
                    'End': '2025-02-26T20:00:00-08:00',
                    'Description': Evening dinner,
                    'PromptMessage': 'I’ve scheduled a dinner appointment for you from 6 PM to 8 PM tomorrow.'
                }}

                Input: 'meeting'
                Response: {{
                    'Title': null,
                    'Start': null,
                    'End': null,
                    'Description': null,
                    'PromptMessage': 'Please specify the date and time for the meeting.'
                }}

                Input: 'dinner yesterday'
                Response: {{
                    'Title': 'Dinner',
                    'Start': null,
                    'End': null,
                    'Description': null,
                    'PromptMessage': 'The date you specified is in the past. Would you like to schedule dinner for a future date?'
                }}

                Now, generate the appointment suggestion for the user input.
                ";
            try
            {
                var aiResponse = await chatClient.CompleteAsync(prompt);

                string jsonPattern = @"```json\s*(.*?)\s*```";
                var match = Regex.Match(aiResponse.Message.Text!, jsonPattern, RegexOptions.Singleline);
                if (!match.Success)
                {
                    logger.LogWarning("AI response did not contain a valid JSON block: {Response}", aiResponse);
                    return BadRequest(new ApiErrorModel { Message = "Invalid AI response", Error = "AI response is invalid." });
                }

                string jsonBlock = match.Groups[1].Value;

                // Deserialize the JSON into AppointmentSuggestion
                var suggestion = JsonSerializer.Deserialize<AppointmentSuggestion>(jsonBlock);

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
    }
}
