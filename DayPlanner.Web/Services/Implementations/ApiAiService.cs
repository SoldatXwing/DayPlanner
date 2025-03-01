using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Web.Refit;
using Refit;
using System.Net;

namespace DayPlanner.Web.Services.Implementations
{
    internal class ApiAiService(IDayPlannerApi api) : IAiService
    {
        public async Task<(AppointmentSuggestion?, ApiErrorModel?)> GetAppointmentSuggestionAsync(string input, DateTime startContext, DateTime endContext, string timeZone, string cultureCode)
        {
            ArgumentException.ThrowIfNullOrEmpty(input);
            ArgumentException.ThrowIfNullOrEmpty(timeZone);
            ArgumentException.ThrowIfNullOrEmpty(cultureCode);

            try
            {
                var suggestion = await api.GetAiSuggestionAsync(input, startContext, endContext, timeZone, cultureCode);
                return (suggestion, null);
            }
            catch (ApiException ex)
                when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                ApiErrorModel? errorModel = await ex.GetContentAsAsync<ApiErrorModel>();
                return (null, errorModel);
            }

        }
    }
}
