using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using System.Globalization;

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
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains the next synchronization token.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="userId"/> is null or empty.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a token cannot be retrieved for the specified <paramref name="userId"/>.
        /// </exception>
        public async Task<string> SyncAppointments(string userId)
        {
            ArgumentException.ThrowIfNullOrEmpty(userId, nameof(userId));

            string accessToken = (await _tokenProvider.GetOrRefresh(userId))?.AccessToken
                ?? throw new UnauthorizedAccessException($"Error receiving token for user with id: {userId}");

            var credential = GoogleCredential.FromAccessToken(accessToken);
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "DayPlanner",
            });
            var request = service.Events.List("primary");
            string? syncToken = await _googleTokenService.GetSyncToken(userId)!;
            if (!string.IsNullOrEmpty(syncToken))
                request.SyncToken = syncToken;
            else
            {
                request.TimeMinDateTimeOffset = DateTime.UtcNow.AddMonths(-1);
                request.TimeMaxDateTimeOffset = DateTime.UtcNow.AddYears(1);
            }
            request.SingleEvents = true;

            var events = await request.ExecuteAsync();
            List<Appointment> appointments = [];
            foreach (var @event in events.Items)
            {
                DateTime startDateTime = @event.Start?.DateTimeDateTimeOffset?.DateTime ??
                    DateTime.ParseExact(@event.Start?.Date ?? string.Empty, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                DateTime endDateTime = @event.End?.DateTimeDateTimeOffset?.DateTime ??
                    DateTime.ParseExact(@event.End?.Date ?? string.Empty, "yyyy-MM-dd", CultureInfo.InvariantCulture);



                appointments.Add(new Appointment
                {

                    UserId = userId,
                    Start = startDateTime,
                    End = endDateTime,
                    Summary = @event.Description ?? "",
                    CreatedAt = @event.CreatedDateTimeOffset!.Value.DateTime,
                    Title = @event.Summary,
                    Location = @event.Location ?? "",
                    Id = @event.Id,
                });
            }
            if (appointments.Count > 0)
                await _appointmentService.ImportOrUpdateAppointments(userId, appointments, Abstractions.Enums.CalendarOrigin.GoogleCalendar);
            if (!string.IsNullOrEmpty(events.NextSyncToken) && (syncToken is null || !syncToken.Equals(events.NextSyncToken)))
                await _googleTokenService.SaveSyncToken(userId, events.NextSyncToken);
            return events.NextSyncToken;

        }
        /// <summary>
        /// Unsyncs the user from Google Calendar.
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="deleteImportedAppointments">If true, appointments with calendar origin GoogleCalendar are getting deleted, otherwise not</param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if a error occurred while receiving token</exception>
        public async Task UnSync(string userId, bool deleteImportedAppointments = false)
        {
            ArgumentException.ThrowIfNullOrEmpty(userId, nameof(userId));
            string accessToken = (await _tokenProvider.GetOrRefresh(userId))?.AccessToken
                ?? throw new UnauthorizedAccessException($"Error receiving token for user with id: {userId}");
            await _tokenProvider.RevokeToken(accessToken);
            if (deleteImportedAppointments)
                await _appointmentService.DeleteAppointmentsByCalendarOrigin(userId, Abstractions.Enums.CalendarOrigin.GoogleCalendar);

            if (_googleTokenService.GetSyncToken(userId) is not null)
                await _googleTokenService.DeleteSyncToken(userId);

            await _googleTokenService.DeleteRefreshToken(userId);
        }
    }
}
