using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Web.Wasm.Refit;
using Refit;
using System.Net;

namespace DayPlanner.Web.Wasm.Services.Implementations
{
    internal class ApiUserService(IDayPlannerApi api) : IUserService
    {
        public async Task<(User?, ApiErrorModel?)> UpdateUserAsync(UpdateUserRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            try
            {
               
                return (await api.UpdateCurrentUserAsync(request), null);
            }
            catch(Exception ex)
            {
                return (null, null);
            }
            //catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            //{
            //    ApiErrorModel? errorModel = await ex.GetContentAsAsync<ApiErrorModel>();
            //    return (null, errorModel);
            //}
        }
    }
}
