using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;

namespace DayPlanner.ThirdPartyImports.Google_Calendar
{
    /// <summary>
    /// Service for interacting with Google Calendar to fetch appointments.
    /// </summary>
    public class GoogleCalendarService(IGoogleTokenProvider tokenProvider,
        IGoogleTokenService googleTokenService,
        IAppointmentsService appointmentService)
    {
        private readonly IGoogleTokenProvider _tokenProvider = tokenProvider;
        private readonly IGoogleTokenService _googleTokenService = googleTokenService;
        private readonly IAppointmentsService _appointmentService = appointmentService;
        /// <summary>Synchronizes appointments from Google Calendar.</summary>
        /// <param name="userId">The ID of the user whose calendar is being synchronized.</param>
        /// <param name="googleAccessToken">
        /// Optional. The Google access token for API calls. If not provided, it will be retrieved using the token provider.
        /// </param>
        /// <param name="syncToken">
        /// Optional. A token representing the last synchronization point. 
        /// If not provided, it will be retrieved using the token service or a full synchronization (past 1 year) will occur.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains the next synchronization token.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="userId"/> is null or empty.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a token cannot be retrieved for the specified <paramref name="userId"/>.
        /// </exception>
        public async Task<string> SyncAppointments(string userId, string googleAccessToken = "", string? syncToken = "")
        {
            ArgumentException.ThrowIfNullOrEmpty(userId, nameof(userId));

            //If token is not provided, get it from the token provider
            string accessToken = string.IsNullOrEmpty(googleAccessToken)
                        ? (await _tokenProvider.GetOrRefresh(userId))?.AccessToken
                            ?? throw new UnauthorizedAccessException($"Error receiving token for user with id: {userId}")
                        : googleAccessToken;

            var credential = GoogleCredential.FromAccessToken(accessToken);
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "DayPlanner",
            });

            var request = service.Events.List("primary");
            if (string.IsNullOrEmpty(syncToken))
                syncToken = await _googleTokenService.GetSyncToken(userId);
            if (!string.IsNullOrEmpty(syncToken))
                request.SyncToken = syncToken;
            else
            {
                request.TimeMinDateTimeOffset = DateTime.UtcNow.AddYears(-1);
                request.TimeMaxDateTimeOffset = DateTime.UtcNow;
            }
            request.SingleEvents = true;

            var events = await request.ExecuteAsync();
            List<Appointment> appointments = [];
            foreach (var @event in events.Items)
            {
                appointments.Add(new Appointment
                {
                    UserId = userId,
                    Start = @event.Start.DateTimeDateTimeOffset!.Value.DateTime,
                    End = @event.End.DateTimeDateTimeOffset!.Value.DateTime,
                    Summary = @event.Description,
                    CreatedAt = @event.CreatedDateTimeOffset!.Value.DateTime,
                    Title = @event.Summary,
                    Location = @event.Location,
                    Id = @event.Id,
                }); ;
            }
            if (appointments.Count > 0)
                await _appointmentService.ImportOrUpdateAppointments(userId, appointments);
            if (!syncToken!.Equals(events.NextSyncToken))
                await _googleTokenService.SaveSyncToken(userId, events.NextSyncToken);
            return events.NextSyncToken;

        }
    }
}
