using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;

namespace DayPlanner.Abstractions.Services;

public interface IUserService
{
    Task<User> GetUserByIdAsync(string uid);

    Task<User> CreateUserAsync(RegisterUserRequest args);
}