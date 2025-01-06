using FirebaseAdmin.Auth;

namespace DayPlanner.Abstractions.Stores
{

    /// <summary>
    /// Interface for storing and retrieving user records.
    /// </summary>
    public interface IUserStore
    {
        /// <summary>
        /// Creates a new user record asynchronously.
        /// </summary>
        /// <param name="args">An object containing the arguments needed to create the user record.</param>
        /// <returns>The created <see cref="UserRecord"/> object.</returns>
        Task<UserRecord> CreateAsync(UserRecordArgs args);

        /// <summary>
        /// Retrieves a user record by its unique identifier (UID).
        /// </summary>
        /// <param name="uid">The unique identifier of the user to retrieve.</param>
        /// <returns>The <see cref="UserRecord"/> object associated with the specified UID.</returns>
        Task<UserRecord> GetByIdAsync(string uid);
    }



}
