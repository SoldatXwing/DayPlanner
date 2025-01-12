using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Stores;

namespace DayPlanner.Abstractions.Services.Implementations;

public class UserService(IUserStore userRepository) : IUserService
{
    public async Task<User?> GetUserByIdAsync(string uid) => await userRepository.GetByIdAsync(uid);

    public async Task<User> CreateUserAsync(RegisterUserRequest args) => await userRepository.CreateAsync(args);
}