using DayPlanner.Web.Wasm.Refit;

namespace DayPlanner.Web.Wasm.Services.Implementations
{
    internal class ApiGoogleCalendarService(IDayPlannerApi api) : IGoogleCalendarService
    {

        public async Task DisconnectAsync(bool deleteImportedAppointments) => await api.DisconnectGoogleCalendarAsync(deleteImportedAppointments);

        public async Task<string> GetAuthUrlAsync() => await api.GoogleLoginAsync();

        public async Task<bool> IsConnected() => await api.IsConnectedWithGoogleCalendarAsync();
    }
}
