using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;

namespace DayPlanner.ThirdPartyImports.Google_Calendar
{
    public class GoogleCalendarService(IGoogleTokenProvider tokenProvider, HttpClient client) : IExternalAppointmentService
    {
        private readonly IGoogleTokenProvider _tokenProvider = tokenProvider;
        private readonly HttpClient _httpClient = client;
        public async Task<List<Appointment>> GetAppointments(string userId, DateTime start, DateTime end)
        {
            ArgumentException.ThrowIfNullOrEmpty(userId, nameof(userId));
            try
            {
                var token = await _tokenProvider.GetOrRefresh(userId) ?? throw new InvalidOperationException($"Error recieving token from user with id: {userId}");
                var credential = GoogleCredential.FromAccessToken(token.AccessToken);
                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "DayPlanner",
                });

                var request = service.Events.List("primary");
                request.TimeMinDateTimeOffset = start;
                request.TimeMaxDateTimeOffset = end;
                request.SingleEvents = true;
                request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;


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
                        Location = @event.Location
                    }); ;
                }
                return appointments;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }
    }
}
