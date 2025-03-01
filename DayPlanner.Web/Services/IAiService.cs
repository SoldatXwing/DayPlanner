using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;

namespace DayPlanner.Web.Services
{
    internal interface IAiService
    {
        Task<(AppointmentSuggestion?, ApiErrorModel?)> GetAppointmentSuggestionAsync(string input, DateTime startContext, DateTime endContext, string timeZone, string cultureCode);
    }
}
