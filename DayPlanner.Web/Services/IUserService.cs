using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;

namespace DayPlanner.Web.Services
{
    public interface IUserService
    {
        Task<(User?, ApiErrorModel?)> UpdateUserAsync(UpdateUserRequest request);
    }
}
