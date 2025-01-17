using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Stores;

namespace DayPlanner.Abstractions.Services.Implementations;
/// <summary>
/// Service for managing user-related operations.
/// </summary>
/// <param name="userRepository">The user repository for managing user data.</param>

public class UserService(IUserStore userRepository) : IUserService
{
    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="uid">The unique identifier of the user.</param>
    /// <returns>The user corresponding to the identifier, or null if not found.</returns>
    public async Task<User?> GetUserByIdAsync(string uid) => await userRepository.GetByIdAsync(uid);
    /// <summary>
    /// Creates a new user based on the provided registration request.
    /// </summary>
    /// <param name="args">The registration request containing user details.</param>
    /// <returns>The created user.</returns>
    public async Task<User> CreateUserAsync(RegisterUserRequest args) => await userRepository.CreateAsync(args);
}