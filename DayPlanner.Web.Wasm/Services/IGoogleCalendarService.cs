namespace DayPlanner.Web.Wasm.Services
{
    internal interface IGoogleCalendarService
    {
        Task<bool> IsConnected();
        Task<string> GetAuthUrlAsync();
        Task DisconnectAsync(bool deleteImportedAppointments);
    }
}
