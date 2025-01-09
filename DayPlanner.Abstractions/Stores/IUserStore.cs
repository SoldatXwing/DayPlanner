using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.DTO;

namespace DayPlanner.Abstractions.Stores;

/// <summary>
/// Interface for storing and retrieving user records.
/// </summary>
public interface IUserStore
{
    /// <summary>
    /// Creates a new user record asynchronously.
    /// </summary>
    /// <param name="args">An object containing the arguments needed to create the user record.</param>
    /// <returns>The created <see cref="User"/> object.</returns>
    Task<User> CreateAsync(RegisterUserRequest args);

    /// <summary>
    /// Retrieves a user record by its unique identifier (UID).
    /// </summary>
    /// <param name="uid">The unique identifier of the user to retrieve.</param>
    /// <returns>The <see cref="User"/> object associated with the specified UID.</returns>
    Task<User> GetByIdAsync(string uid);
}
