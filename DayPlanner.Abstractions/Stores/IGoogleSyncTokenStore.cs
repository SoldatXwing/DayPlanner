using DayPlanner.Abstractions.Models.Backend;

namespace DayPlanner.Abstractions.Stores;

/// <summary>
/// A store interface for get, save and delete google sync user tokens.
/// </summary>
public interface IGoogleSyncTokenStore
{
    /// <summary>
    /// Retrieves the sync token of a certain user.
    /// </summary>
    /// <param name="userId">The id of the user to get the token from.</param>
    /// <returns>The sync token. If <c>null</c> the user doesn't has one yet.</returns>
    Task<string?> Get(string userId);

    /// <summary>
    /// Saves a sync token for a user.
    /// </summary>
    /// <param name="userId">The user to set the token for.</param>
    /// <param name="syncToken">The new token to save.</param>
    /// <returns>Awaits the async operation</returns>
    Task Save(string userId, string syncToken);

    /// <summary>
    /// Removes the sync token of a certain user.
    /// </summary>
    /// <param name="userId">The user to remove the token from.</param>
    /// <returns>Awaits the async operation</returns>
    Task Delete(string userId);
    /// <summary>
    /// Gets all sync tokens in the database. (Mostly used in background jobs)
    /// </summary>
    /// <returns>All sync tokens</returns>
    Task<IEnumerable<GoogleSyncToken>> GetAll();

}